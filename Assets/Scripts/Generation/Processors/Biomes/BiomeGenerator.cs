namespace Generation.Processors.Biomes
{
    public abstract class BiomeGenerator
    {
        public CreateChunkJob jobData        { get; set; }
        public BiomeProcessor biomeProcessor { get; set; }

        public abstract int SampleMapPoint(int x, int z);
        public abstract BlockId SampleBlock(int x, int y, int z, int heightValue);
    }
}
