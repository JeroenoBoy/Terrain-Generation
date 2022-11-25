using Generation.Generators.Helpers;
using Generation.Processors.Biomes.Blenders;
using UnityEngine;
using Random = Unity.Mathematics.Random;



namespace Generation.Processors.Biomes.Generators
{
    [CreateAssetMenu(menuName = "Biomes/PlainsBiome")]
    public class PlainsBiome : BaseBiomeProcessor
    {
        [SerializeField] private Octaves _octaves;
        
        public override IBiomeGenerator CreateInstance(Random random, CreateChunkJob job, BiomeProcessor biomeProcessor, BiomeBlender parentBlender)
        {
            return new PlainsBiomeGenerator
            {
                baseHeight = biomeProcessor.baseHeight,
                sampler = new OctavesSampler(random, _octaves.octaves, job.x * job.chunkSize, job.z * job.chunkSize)
            };
        }
    }
    
    
    [System.Serializable]
    public class PlainsBiomeGenerator : IBiomeGenerator
    {
        public int baseHeight;
        public OctavesSampler sampler;
        

        public int SampleMapPoint(int x, int z)
        {
            return (int)(baseHeight + sampler.Sample(x, z));
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
