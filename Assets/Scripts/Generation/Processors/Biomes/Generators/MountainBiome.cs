using System;
using System.Linq;
using Generation.Generators.Helpers;
using Generation.Processors.Biomes.Blenders;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;



namespace Generation.Processors.Biomes.Generators
{
    public enum CalculationType { Linier, Quadratic, Root }



    [CreateAssetMenu(menuName = "Biomes/MountainBiome")]
    public class MountainBiome : BaseBiomeProcessor
    {
        [SerializeField] private int _minContinentalnessHeight;
        [SerializeField] private int _maxContinentalnessHeight;
        [SerializeField] private CalculationType _calculation;

        [Header("3dNoise")]
        [SerializeField] private float _3dNoiseTopFalloff;
        [SerializeField] private float _3dNoiseBottomFalloff;
        [SerializeField] private float _3dNoiseFrequency;
        [SerializeField] private float _3dNoiseMargin;
     
        [SerializeField] private Octaves _octaves;
        
        
        public override IBiomeGenerator CreateInstance(Random random, CreateChunkJob job, BiomeProcessor biomeProcessor, BiomeBlender parentBlender)
        {
            BiomeBlender.SetupData data = parentBlender.FindSetupData(this);
            BiomeBlender.SetupData next = parentBlender.FindNext(this);

            float nextValue = next.heightValue == 0 ? 1 : next.heightValue - next.smoothing * .5f;
            
            return new MountainBiomeGenerator
            {
                sampler = new OctavesSampler(random, _octaves.octaves, job.x * job.chunkSize, job.z * job.chunkSize),
                baseHeight = biomeProcessor.baseHeight,
                
                continentalness          = job.continentalness,
                continentalnessOffset    = data.heightValue - data.smoothing*.5f,
                continentalnessMulti     = 1/(nextValue - (data.heightValue - data.smoothing*.5f)),
                minContinentalnessHeight = _minContinentalnessHeight,
                maxContinentalnessHeight = _maxContinentalnessHeight,
                calculation              = _calculation,
                
                topFalloff       = _3dNoiseTopFalloff,
                bottomFalloff    = _3dNoiseBottomFalloff,
                frequency3dNoise = _3dNoiseFrequency,
                
                cx = job.x * job.chunkSize,
                cz = job.z * job.chunkSize
            };
        }
    }
    
    
    public class MountainBiomeGenerator : IBiomeGenerator
    {
        public int baseHeight;

        
        public OctavesSampler sampler;
        public float[,] continentalness;

        public float continentalnessOffset;
        public float continentalnessMulti;

        public int minContinentalnessHeight;
        public int maxContinentalnessHeight;

        public float topFalloff;
        public float frequency3dNoise;
        public float bottomFalloff;

        //  Chunk locations
        public int cx, cz;
        
        public CalculationType calculation;

        
        public float SampleContinentalnessHeight(int x, int z)
        {
            float v = (continentalness[x, z] - continentalnessOffset) * continentalnessMulti;
            
            return minContinentalnessHeight + (maxContinentalnessHeight - minContinentalnessHeight) * calculation switch
            {
                CalculationType.Linier => v,
                CalculationType.Quadratic => math.pow(v, 2),
                CalculationType.Root => math.sqrt(v),
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        public int SampleMapPoint(int x, int z)
        {
            return (int)(SampleContinentalnessHeight(x,z) + baseHeight + sampler.Sample(x, z));
        }


        public BlockId SampleBlock(int x, int y, int z, int heightValue)
        {
            float multi = y > heightValue
                ? 1 + (y-heightValue) * topFalloff
                : 1 - (heightValue-y) * bottomFalloff;
            
            float value = .5f+.5f*noise.cnoise(math.float3(x+cx,y,z+cz) * frequency3dNoise) + multi;

            return value > 1 ? BlockId.Air : BlockId.Stone;
        }
    }
}
