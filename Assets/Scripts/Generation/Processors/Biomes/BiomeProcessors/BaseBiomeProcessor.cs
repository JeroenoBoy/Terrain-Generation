using UnityEngine;



namespace Generation.Processors.Biomes.BiomeProcessors
{
    public abstract class BaseBiomeProcessor : ScriptableObject
    {
        public CreateChunkJob jobData { get; set; }
        public BiomeProcessor biomeProcessor { get; set; }

        public abstract BiomeGenerator CreateInstance(CreateChunkJob job, BiomeProcessor biomeProcessor);
    }
}
