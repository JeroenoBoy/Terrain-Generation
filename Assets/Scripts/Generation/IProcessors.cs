using JetBrains.Annotations;



namespace Generation
{
    
    public interface IProcessors
    {
        public void Process(CreateChunkJob jobData);
    }
}
