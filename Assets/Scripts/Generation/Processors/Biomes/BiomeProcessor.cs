using System;
using Generation.Processors.Biomes.Blenders;
using Generation.Processors.Biomes.Generators;
using UnityEngine;



namespace Generation.Processors.Biomes
{
    [System.Serializable]
    public class BiomeProcessor : IProcessors
    {
        [SerializeField] private BiomeBlender _masterBlender;
        [SerializeField] private int          _baseHeight;

        public int baseHeight => _baseHeight;
        
        
        public void Process(CreateChunkJob jobData)
        {
            System.Random random = new (jobData.seed);
            IBiomeGenerator generator = _masterBlender.CreateInstance(random, jobData, this, null);

            int size   = jobData.chunkSize;
            int height = jobData.chunkHeight;

            BlockId[,,] blocks = jobData.blocks;

            //  Sampling points
            
            for (int x = 0; x < size; x++) {
                for (int z = 0; z < size; z++) {
                    int heightValue = generator.SampleMapPoint(x, z);

                    for (int y = 0; y < height; y++) {
                         blocks[x,y,z] = generator.SampleBlock(x,y,z,heightValue);
                    }
                }
            }
        }
    }
}
