using Generation.Generators.Helpers;
using UnityEngine;



namespace Generation.Processors.Biomes
{
    [System.Serializable]
    public class PlainsBiome : IBiomeGenerator
    {
        public CreateChunkJob jobData        { get; set; }
        public BiomeProcessor biomeProcessor { get; set; }

        [SerializeField] private Octaves _octaves;
        
        //  Instance

        private OctavesSampler    _sampler;
        

        public IBiomeGenerator CreateInstance(CreateChunkJob job, BiomeProcessor biomeProcessor)
        {
            System.Random random = new (job.seed);
            
            return new PlainsBiome
            {
                _sampler = new OctavesSampler(
                    random, _octaves.octaves, 
                    job.x * job.chunkSize,
                    job.z * job.chunkSize)
            };
        }


        public int SampleMapPoint(int x, int z)
        {
            return (int)(biomeProcessor.baseHeight + _sampler.Sample(x, z));
        }


        public BlockId SampleBlock(int x, int y, int z, int heightValue)
        {
            if (y > heightValue) return BlockId.Air;
            if (y == heightValue) return BlockId.Grass;
            if (y > heightValue - 3) return BlockId.Dirt;
            
            return BlockId.Stone;
        }
    }
}
