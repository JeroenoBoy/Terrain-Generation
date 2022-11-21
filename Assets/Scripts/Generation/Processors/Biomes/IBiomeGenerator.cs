namespace Generation.Processors.Biomes
{
    public interface IBiomeGenerator
    {
        public int SampleMapPoint(int x, int z);
        public BlockId SampleBlock(int x, int y, int z, int heightValue);
    }
}
