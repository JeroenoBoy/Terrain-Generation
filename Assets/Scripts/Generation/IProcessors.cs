using JetBrains.Annotations;



namespace Generation
{
    
    [NotNull]
    public interface Processors
    {
        public void Process(CreateChunkJob jobData);
    }
}
