using System;
using System.Collections.Generic;
using System.Linq;
using Generation.Generators.Helpers;
using Generation.Processors.Biomes.Generators;
using JUtils.Attributes;
using JUtils.Extensions;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = Unity.Mathematics.Random;



namespace Generation.Processors.Biomes.Blenders
{
    [CreateAssetMenu(menuName = "Biomes/Blender")]
    public class BiomeBlender : BaseBiomeProcessor
    {
        private enum BlendMap { Continentalness, Random }
        [SerializeField] private BlendMap _blendMap;
        [SerializeField] private Octaves  _randomOctaves;

        [Header("Perlin noise smoothing")]
        [SerializeField] private float _smoothingFrequency;
        [SerializeField] private float _smoothingScale;
        [SerializeField] private CalculationType _calculation;
        
        [Space, Space]
        [SerializeField] private SetupData[] _processors;

        public IReadOnlyList<SetupData> processors => _processors;


        private void OnValidate()
        {
            float current = -1;

            foreach (SetupData biomeBlendData in _processors) {
                if (biomeBlendData.heightValue < current) {
                    Debug.LogWarning("Please sort the blend map on height to avoid issues");
                    return;
                }

                current = biomeBlendData.heightValue;
            }
        }


        public SetupData FindSetupData(BaseBiomeProcessor processor)
        {
            return _processors.First(t => t.processor == processor);
        }


        public SetupData FindNext(BaseBiomeProcessor processor)
        {
            bool found = false;
            
            return _processors.FirstOrDefault(t => {
                if (found) return true;
                found = t.processor == processor;
                return false;
            });
        }


        public override IBiomeGenerator CreateInstance(Random random, CreateChunkJob job, BiomeProcessor biomeProcessor, BiomeBlender parentBlender)
        {
            if (_processors.Length == 0) throw new Exception($"No processors found on {name}");
            if (_processors.Length == 1) return _processors.First().processor.CreateInstance(random, job, biomeProcessor, this);

            int chunkSize = job.chunkSize;
            int x = job.x * chunkSize;
            int z = job.z * chunkSize;
            
            return (IBiomeGenerator)(_blendMap switch
            {
                BlendMap.Continentalness => new MapBiomeBlender
                {
                    blendMap = job.continentalness,
                    blendWeightMap = new float[chunkSize, chunkSize],
                    processorIndexes = new int[chunkSize, chunkSize],
                    chunkX = x,
                    chunkZ = z,
                    smoothingFrequency = _smoothingFrequency,
                    smoothingScale     = _smoothingScale,
                    calculation        = _calculation,
                    
                    subGenerators = _processors.Select(t => new BiomeBlendData
                    {
                        generator = t.processor.CreateInstance(random, job, biomeProcessor, this),
                        heightValue = t.heightValue,
                        smoothing = t.smoothing
                    }).ToArray()
                    
                },
                BlendMap.Random => new MapBiomeBlender
                {
                    blendMap         = _randomOctaves.Calculate(random, x, z, chunkSize),
                    blendWeightMap   = new float[chunkSize,chunkSize],
                    processorIndexes = new   int[chunkSize,chunkSize], 
                    
                    chunkX = x,
                    chunkZ = z,
                    
                    smoothingFrequency = _smoothingFrequency,
                    smoothingScale     = _smoothingScale,
                    calculation        = _calculation,
                    
                    subGenerators = _processors.Select(t => new BiomeBlendData
                    {
                        generator   = t.processor.CreateInstance(random, job, biomeProcessor, this),
                        heightValue = t.heightValue,
                        smoothing   = t.smoothing
                    }).ToArray()
                },
                _ => throw new ArgumentOutOfRangeException() 
            });
        }
        
        
        
        public struct MapBiomeBlender : IBiomeGenerator
        {
            public float[,] blendMap;
            public float[,] blendWeightMap;
            public   int[,] processorIndexes;
            
            public BiomeBlendData[] subGenerators;

            public float smoothingFrequency, smoothingScale;
            public CalculationType calculation;
            
            public float chunkX, chunkZ;


            public float ApplyCalculation(float x)
            {
                return calculation switch
                {
                    CalculationType.Linier => x,
                    CalculationType.Quadratic => x * x,
                    CalculationType.Root => math.sqrt(x),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }


            public int SampleMapPoint(int x, int z)
            {
                float value = blendMap[x, z];
                int   generatorsAmount = subGenerators.Length;
                
                BiomeBlendData previous = subGenerators[0];
                
                for (int i = 1; i < generatorsAmount; i++) {
                    BiomeBlendData data = subGenerators[i];
                    processorIndexes[x, z] = i-1;

                    float heightValue = data.heightValue;
                    float smoothing = data.smoothing;
                    
                    float minHeight = heightValue - smoothing * .5f;
                    float maxHeight = minHeight + smoothing;
                    
                    //  Simple contrast

                    if (value < minHeight) return previous.generator.SampleMapPoint(x, z);
                    if (value > maxHeight) {
                        previous = data;
                        continue;
                    }

                    float point = math.clamp(.5f+noise.cnoise(new float2((x + chunkX)*smoothingFrequency, (z + chunkZ)*smoothingFrequency))*.5f,0,1);

                    float weight     = ApplyCalculation((value - minHeight) / smoothing);
                    float baseWeight = (1 - math.pow(2* weight-1, 2))*smoothingScale;
                    float multi      = weight * (1-baseWeight) + point * baseWeight;
                    
                    //  Noise based smoothing

                    float g1 = multi * data.generator.SampleMapPoint(x, z);
                    float g2 = (1 - multi) * previous.generator.SampleMapPoint(x, z);

                    blendWeightMap[x, z] = multi;
                    return (int)(g1+g2);
                }
                
                //  For safety

                processorIndexes[x, z] = generatorsAmount-1;
                return previous.generator.SampleMapPoint(x, z);
            }


            public BlockId SampleBlock(int x, int y, int z, int heightValue)
            {
                float bwm = blendWeightMap[x, z];

                return bwm switch
                {
                    0     => subGenerators[processorIndexes[x, z]].generator.SampleBlock(x, y, z, heightValue),
                    > .5f => subGenerators[processorIndexes[x, z]+1].generator.SampleBlock(x, y, z, heightValue),
                    _     => subGenerators[processorIndexes[x, z]].generator.SampleBlock(x, y, z, heightValue)
                };
            }
        }
        
        
        
        public struct BiomeBlendData
        {
            public IBiomeGenerator generator;
            public float heightValue;
            public float smoothing;
        }



        [Serializable]
        public struct SetupData
        {
            public BaseBiomeProcessor processor;
            [Range(0,1)]
            public float heightValue;
            [Range(0,1)]
            public float smoothing;

            
#if UNITY_EDITOR
            [CustomPropertyDrawer(typeof(SetupData))]
            private class BiomeEditor : PropertyDrawer
            {
                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("processor")) + 2 +
                           EditorGUI.GetPropertyHeight(property.FindPropertyRelative("heightValue")) + 2 +
                           EditorGUI.GetPropertyHeight(property.FindPropertyRelative("smoothing"));
                }


                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    SerializedProperty processorProperty   = property.FindPropertyRelative("processor");
                    SerializedProperty heightValueProperty = property.FindPropertyRelative("heightValue");
                    SerializedProperty smoothingProperty   = property.FindPropertyRelative("smoothing");

                    position.height = EditorGUI.GetPropertyHeight(processorProperty);
                    EditorGUI.PropertyField(position, processorProperty);
                    position.y += position.height+2;
                    
                    position.height = EditorGUI.GetPropertyHeight(heightValueProperty);
                    EditorGUI.PropertyField(position, heightValueProperty);
                    position.y += position.height+2;
                    
                    position.height = EditorGUI.GetPropertyHeight(smoothingProperty);
                    EditorGUI.PropertyField(position, smoothingProperty);
                }
            }
#endif
        }
    }
}
