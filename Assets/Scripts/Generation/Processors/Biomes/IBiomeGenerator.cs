using Generation.Processors;



namespace Generation.Biomes
{
    public interface IBiomeGenerator
    {
        public CreateChunkJob jobData        { get; set; }
        public BiomeProcessor biomeProcessor { get; set; }

        public IBiomeGenerator CreateInstance(CreateChunkJob job);

        public int SampleMapPoint(int x, int z);
        public BlockId SampleBlock(int x, int y, int z, int heightValue);
    }
}
