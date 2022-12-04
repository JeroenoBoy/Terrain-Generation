namespace Generation.Processors.Population
{
    public interface IPopulator
    {
        public void Execute(CreateChunkJob job);
    }
}
