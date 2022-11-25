using System;
using System.Globalization;
using Generation.Generators.Helpers;
using JUtils.Attributes;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;



namespace Generation.Processors
{
    [System.Serializable]
    public struct CaveProcessor : IProcessors
    {
        [Header("Generators")]
        [SerializeField] private Optional<BubbleCave[]> _bubbleCaves;
        [SerializeField] private Optional<LongCaveGenerator[]> _longCaveGenerator;
        
        
        public void Process(CreateChunkJob jobData)
        {
            Random random = new (jobData.seed);

            if (_bubbleCaves.enabled) {
                BubbleCave[] caves = _bubbleCaves.value;
                for (int i = 0; i < caves.Length; i++) {
                    caves[i].Process(jobData, random);
                }
            }

            if (_longCaveGenerator.enabled) {
                LongCaveGenerator[] caves = _longCaveGenerator.value;
                for (int i = 0; i < caves.Length; i++) {
                    caves[i].Process(jobData, random);
                }
            }
            
        }
        
        
        
        [System.Serializable]
        public struct BubbleCave
        {
            [SerializeField] private float randomPositionOffset;
            
            [Header("Shape Height map")]
            [SerializeField] private float mapFrequency;
            [Tooltip("The perlin noise map value from which it should generate"), Range(0,1)]
            [SerializeField] private float mapDelta;
            [SerializeField] private AnimationCurve heightCurve;

            [Space]
            [SerializeField] private Octaves octaves;

            [Header("Y map")]
            [SerializeField] private float heightMapFrequency;
            [SerializeField] private int minY;
            [SerializeField] private int maxY;
            
            
            
            public void Process(CreateChunkJob data, Random random)
            {
                //  Pre calculating values
                
                int shapeX = data.x * data.chunkSize + (int)(randomPositionOffset * random.NextFloat());
                int shapeZ = data.z * data.chunkSize + (int)(randomPositionOffset * random.NextFloat());
                
                int heightMapX = data.x * data.chunkSize + (int)(randomPositionOffset * random.NextFloat());
                int heightMapZ = data.z * data.chunkSize + (int)(randomPositionOffset * random.NextFloat());

                //  Calculating octaves

                float[,] heightMap = octaves.Calculate(
                    random,
                    data.x * data.chunkSize,
                    data.z * data.chunkSize,
                    data.chunkSize
                );
                
                //  Generating cave
                
                for (int x = data.chunkSize; x --> 0;) {
                    for (int z = data.chunkSize; z --> 0;) {
                        float smX   = (shapeX + x) * mapFrequency;
                        float smZ   = (shapeZ + z) * mapFrequency;
                        float value = noise.cnoise(new float2(smX, smZ)) + heightMap[x, z];
                        
                        //  Calculating height offset
                        
                        if (value < mapDelta) continue;
                        
                        //  Painting

                        float hmX = (heightMapX + x) * heightMapFrequency;
                        float hmZ = (heightMapZ + z) * heightMapFrequency;
                        float sy  = minY + (maxY - minY) * Mathf.PerlinNoise(hmX, hmZ);

                        float height = heightCurve.Evaluate((value - mapDelta) / (1 - mapDelta));

                        //  Adding height variation
                       
                        float halfHeight   = height * .5f;
                        int   targetHeight = (int)math.ceil(sy + halfHeight);

                        for (int y = (int)math.floor(sy - halfHeight); y < targetHeight; y++)
                            data.blocks[x, y, z] = BlockId.Air;
                    }
                }
            }
        }



        [System.Serializable]
        public struct LongCaveGenerator
        {
            [Header("Shape")]
            [Range(0, 1), SerializeField] private float _minIntensity;
            [Range(0, 1), SerializeField] private float _maxIntensity;
            [SerializeField] private int _minHeight;
            [SerializeField] private int _maxHeight;
            [Tooltip("The sampling size on the map to determine how high the cave should be")]
            [SerializeField] private int _sampleSize;
            [Range(0,1)]
            [SerializeField] private float _sampleWeight;
            [SerializeField] private Octaves _shapeMap;

            [Header("Y map")] [SerializeField] private int _minY;
            [SerializeField] private int _maxY;
            [SerializeField] private Octaves _yMap;


            [Header("Mask")]
            [SerializeField] private bool _maskEnabled;
            [Range(0, 1)]
            [SerializeField] private float _maskIntensity;
            [Range(0, 1)]
            [SerializeField] private float _startFade;
            [SerializeField] private Octave _mask;


            public void Process(CreateChunkJob data, Random random)
            {
                int size = data.chunkSize;
                
                OctavesSampler shapeSampler = new (random, _shapeMap.octaves, data.x * size, data.z * size);
                OctavesSampler ySampler     = new (random, _yMap.octaves,     data.x * size, data.z * size);
                OctavesSampler maskSampler  = new (random,new []{_mask},data.x * size, data.z * size);
                
                for (int x = 0; x < size; x++) {
                    for (int z = 0; z < size; z++) {
                        float value = shapeSampler.Sample(x, z);
                        float maskSample = 0;

                        //  Applying mask
                        
                        if (_maskEnabled) {
                            maskSample = maskSampler.Sample(x, z);
                            if (maskSample < _maskIntensity) continue;
                        }

                        if (value <= _minIntensity || value >= _maxIntensity) continue;

                        //  Getting height
                        
                        float averageHeight = _sampleWeight * AverageHeight(x,z,shapeSampler);
                        float currHeight = (1-_sampleWeight) * Normalize(value);
                        float height = _minHeight + (_maxHeight - _minHeight) * (averageHeight + currHeight);
                        
                        //  Adding mask fade

                        if (_maskEnabled && maskSample < _startFade) {
                            height -= _maxHeight * (1 - (maskSample - _maskIntensity) / (_startFade - _maskIntensity));
                        }
                        
                        //  Adding height variation

                        float sy = _minY + (_maxY - _minY) * ySampler.Sample(x, z);

                        //  Processing cave

                        float halfHeight = height * .5f;
                        int targetHeight = (int)math.ceil(sy + halfHeight);

                        for (int y = (int)math.floor(sy - halfHeight); y < targetHeight; y++) {
                            data.blocks[x, y, z] = BlockId.Air;
                        }
                    }
                }
            }


            private float AverageHeight(int x, int z, OctavesSampler sampler)
            {
                int hss = (int)(_sampleSize * .5f);

                float average = 0;

                for (int sx = x - hss; sx < x + hss; sx++) {
                    for (int sz = z - hss; sz < z + hss; sz++) {
                        float value = sampler.Sample(sx, sz);
                        if (value <= _minIntensity || value >= _maxIntensity) continue;
                        average += Normalize(value);
                    }
                }

                return average / (_sampleSize * _sampleSize);
            }


            private float Normalize(float value) =>  Mathf.Sin(Mathf.PI * ((value - _minIntensity) / (_maxIntensity - _minIntensity)));
        }
    }
}
