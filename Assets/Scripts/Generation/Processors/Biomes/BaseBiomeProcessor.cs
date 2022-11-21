using Generation.Processors.Biomes.Blenders;
using UnityEngine;



namespace Generation.Processors.Biomes
{
    public abstract class BaseBiomeProcessor : ScriptableObject
    {
        public CreateChunkJob jobData { get; set; }
        public BiomeProcessor biomeProcessor { get; set; }
        protected BiomeBlender _blender;


        public abstract IBiomeGenerator CreateInstance(System.Random random, CreateChunkJob job, BiomeProcessor biomeProcessor, BiomeBlender parentBlender);
    }
}
