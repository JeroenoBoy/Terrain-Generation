namespace Generation.Processors.Biomes
{
    public interface IBiomeGenerator
    {
        public CreateChunkJob jobData        { get; set; }
        public BiomeProcessor biomeProcessor { get; set; }

        public IBiomeGenerator CreateInstance(CreateChunkJob job, BiomeProcessor biomeProcessor);

        public int SampleMapPoint(int x, int z);
        public BlockId SampleBlock(int x, int y, int z, int heightValue);
    }
}
