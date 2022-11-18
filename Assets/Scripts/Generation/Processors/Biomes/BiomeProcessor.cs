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
        [SerializeField] private float _smoothingFrequency;

        public int baseHeight => _baseHeight;
        public float mountainStart => _mountainStart;
        
        
        public void Process(CreateChunkJob jobData)
        {
            IBiomeGenerator plains   = _plainsBiome.CreateInstance(jobData, this);
            IBiomeGenerator mountain = _mountainBiome.CreateInstance(jobData, this);

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
                    }
                    else if (value > _mountainStart) {
                        
                        //  Noise based smoothing

                        float point      = Mathf.Clamp01(Mathf.PerlinNoise((x + chunkX)*_smoothing, (z + chunkZ)*_smoothing));
                        float weight     = (value - _mountainStart) / _smoothing;
                        float baseWeight = 1 - (2*weight - 1) * (2*weight - 1);
                        
                        height = (int)(point * baseWeight + (1-baseHeight) * (weight * mountain.SampleMapPoint(x, z) + (1 - weight) * plains.SampleMapPoint(x, z)));
                    }
                    else {
                        height = plains.SampleMapPoint(x, z);
                    }
                    
                    //  Sampling block

                    if (value > _mountainStart + _smoothing * .5f) {
                        for (int y = 0; y < chunkHeight; y++) {
                            jobData.blocks[x, y, z] = mountain.SampleBlock(x,y,z,height);
                        }
                    }
                    else {
                        for (int y = 0; y < chunkHeight; y++) {
                            jobData.blocks[x, y, z] = plains.SampleBlock(x, y, z, height);
                        }
                    }
                }
            }
        }
    }
}
