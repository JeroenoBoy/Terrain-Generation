using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace Generation.Processors.Biomes.BiomeProcessors
{
    [CreateAssetMenu(menuName = "Biome/Blender")]
    public class BiomeBlender : BaseBiomeProcessor
    {
        private enum BlendMap { Continentalness }
        
        [SerializeField] private BlendMap _blendMap;
        [SerializeField] private List<BaseBiomeProcessor> _processors;


        public override BiomeGenerator CreateInstance(CreateChunkJob job, BiomeProcessor biomeProcessor)
        {
            return _blendMap switch
            {
                BlendMap.Continentalness => (BiomeGenerator)new ContinentalnessBiomeBlender
                {
                    subGenerators = _processors.Select(t => t.CreateInstance(job, biomeProcessor)).ToArray()
                },
                
                _ => throw new ArgumentOutOfRangeException() 
            };
        }
        
        
        public class ContinentalnessBiomeBlender : BiomeGenerator
        {
            public CreateChunkJob jobData { get; set; }
            public BiomeProcessor biomeProcessor { get; set; }


            public BiomeGenerator[] subGenerators;

            
            public int SampleMapPoint(int x, int z)
            {
            }


            public BlockId SampleBlock(int x, int y, int z, int heightValue)
            {
            }
        }
    }
}
