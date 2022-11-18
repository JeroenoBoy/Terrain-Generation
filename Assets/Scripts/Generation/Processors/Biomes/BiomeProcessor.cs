using Generation.Biomes;
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
        [SerializeField] private int _baseHeight;

        public int baseHeight => _baseHeight;
        
        
        public void Process(CreateChunkJob jobData)
        {
            IBiomeGenerator plains   = _plainsBiome.CreateInstance(jobData);
            IBiomeGenerator mountain = _mountainBiome.CreateInstance(jobData);

            plains.jobData = jobData;
            mountain.jobData = jobData;

            plains.biomeProcessor = this;
            mountain.biomeProcessor = this;

            int size = jobData.chunkSize;
            int chunkHeight = jobData.chunkHeight;
            
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
                        float weight = (value - _mountainStart) / _smoothing;
                        height = (int)(weight * mountain.SampleMapPoint(x, z) + (1 - weight) * plains.SampleMapPoint(x, z));
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
