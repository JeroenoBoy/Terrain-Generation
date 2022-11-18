using Generation.Processors.Biomes;
using UnityEngine;



namespace Generation.Processors
{
    [System.Serializable]
    public class BiomeProcessor : IProcessors
    {
        [SerializeField] private PlainsBiome _plainsBiome;
        [SerializeField] private MountainBiome _mountainBiome;

        [Header("Smoothing")]
        [Range(0,1)]
        [SerializeField] private float _mountainStart;
        [Range(0,.1f)]
        [SerializeField] private float _smoothing;
        [SerializeField] private int   _baseHeight;
        [Range(0,1)]
        [SerializeField] private float _smoothingFrequency;
        [Range(0,1)]
        [SerializeField] private float _smoothingScale;

        public int baseHeight => _baseHeight;
        public float mountainStart => _mountainStart;
        
        
        public void Process(CreateChunkJob jobData)
        {
            BiomeGenerator plains   = _plainsBiome.CreateInstance(jobData, this);
            BiomeGenerator mountain = _mountainBiome.CreateInstance(jobData, this);

            plains.jobData = jobData;
            mountain.jobData = jobData;

            plains.biomeProcessor = this;
            mountain.biomeProcessor = this;

            int size = jobData.chunkSize;
            int chunkHeight = jobData.chunkHeight;

            int chunkX = jobData.x * jobData.chunkSize;
            int chunkZ = jobData.z * jobData.chunkSize;
            
            float[,] continentalness = jobData.continentalness;

            //  Sampling points
            
            for (int x = 0; x < size; x++) {
                for (int z = 0; z < size; z++) {
                    float value = continentalness[x, z];
                    int height;

                    //  Getting target point
                    
                    if (value > _mountainStart + _smoothing) {
                        height = mountain.SampleMapPoint(x, z);
                        for (int y = 0; y < chunkHeight; y++) {
                            jobData.blocks[x, y, z] = mountain.SampleBlock(x, y, z, height);
                        }
                    }
                    else if (value > _mountainStart) {
                        float point = Mathf.Clamp01(Mathf.PerlinNoise((x + chunkX)*_smoothing, (z + chunkZ)*_smoothing));
                        
                        //  Noise based smoothing

                        float weight     = (value - _mountainStart) / _smoothing;
                        float baseWeight = (1 - Mathf.Pow(2* weight-1, 2))*_smoothingScale;
                        float multi      = weight * (1-baseWeight) + point * baseWeight;

                        float v1 = multi * mountain.SampleMapPoint(x, z);
                        float v2 = (1-multi) * plains.SampleMapPoint(x, z);

                        height = (int)(v1 + v2);
                        
                        //  Sampling block

                        if (multi > .5f) for (int y = 0; y < chunkHeight; y++) {
                            jobData.blocks[x, y, z] = mountain.SampleBlock(x, y, z, height);
                        }
                        else for (int y = 0; y < chunkHeight; y++) {
                            jobData.blocks[x, y, z] = plains.SampleBlock(x, y, z, height);
                        }
                        // height = (int)(baseWeight * point + (1-baseWeight) * (weight*mountain.SampleMapPoint(x, z) + (1-weight)*plains.SampleMapPoint(x, z)));
                    }
                    else {
                        height = plains.SampleMapPoint(x, z);
                        for (int y = 0; y < chunkHeight; y++) {
                            jobData.blocks[x, y, z] = plains.SampleBlock(x, y, z, height);
                        }
                    }
                }
            }
        }
    }
}
