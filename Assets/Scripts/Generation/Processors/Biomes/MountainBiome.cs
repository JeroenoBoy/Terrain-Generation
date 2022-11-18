using Generation.Generators.Helpers;
using UnityEngine;



namespace Generation.Processors.Biomes
{
    [System.Serializable]
    public class MountainBiome : IBiomeGenerator
    {
        public CreateChunkJob jobData        { get; set; }
        public BiomeProcessor biomeProcessor { get; set; }

        [SerializeField] private AnimationCurve _continentalnessCurve;
        [SerializeField] private Octaves _octaves;
     
        //  Instance

        private OctavesSampler _sampler;
        private float[,] _continentalness;

        private float _continentalnessOffset;
        private float _continentalnessMulti;
            

        public IBiomeGenerator CreateInstance(CreateChunkJob job, BiomeProcessor biomeProcessor)
        {
            System.Random random = new (job.seed);
            
            return new MountainBiome
            {
                _sampler = new OctavesSampler(random, _octaves.octaves, 
                    job.x * job.chunkSize, 
                    job.z * job.chunkSize),
                
                _continentalness       = job.continentalness,
                _continentalnessCurve  = _continentalnessCurve, 
                _continentalnessOffset = biomeProcessor.mountainStart,
                _continentalnessMulti  = 1/(1-biomeProcessor.mountainStart)
            };
        }


        public float SampleContinentalnessHeight(int x, int z)
        {
            return _continentalnessCurve.Evaluate((_continentalness[x, z] - _continentalnessOffset) * _continentalnessMulti);
        }


        public int SampleMapPoint(int x, int z)
        {
            return (int)(SampleContinentalnessHeight(x,z) + biomeProcessor.baseHeight + _sampler.Sample(x, z));
        }


        public BlockId SampleBlock(int x, int y, int z, int heightValue)
        {
            return y > heightValue ? BlockId.Air : BlockId.Stone;
        }
    }
}
