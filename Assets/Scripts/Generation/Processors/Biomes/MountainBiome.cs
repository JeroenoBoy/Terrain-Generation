using Generation.Generators.Helpers;
using Generation.Processors;
using UnityEngine;



namespace Generation.Biomes
{
    [System.Serializable]
    public class MountainBiome : IBiomeGenerator
    {
        public CreateChunkJob jobData        { get; set; }
        public BiomeProcessor biomeProcessor { get; set; }

        [SerializeField] private int _offsetXMulti;
        [SerializeField] private int _offsetZMulti;
     
        //  Instance
        
        private PerlinNoiseOffset _pnOffset;
        

        public IBiomeGenerator CreateInstance(CreateChunkJob job)
        {
            return new MountainBiome
            {
                _pnOffset = new PerlinNoiseOffset(job, _offsetXMulti, _offsetZMulti)
            };
        }


        public int SampleMapPoint(int x, int z)
        {
            return 0;
        }


        public BlockId SampleBlock(int x, int y, int z, int heightValue)
        {
            return y > heightValue ? BlockId.Air : BlockId.Stone;
        }
    }
}
