using System.Linq;
using Generation.Generators.Helpers;
using Generation.Processors.Biomes.Blenders;
using UnityEngine;
using Random = System.Random;



namespace Generation.Processors.Biomes.Generators
{
    [CreateAssetMenu(menuName = "Biomes/MountainBiome")]
    public class MountainBiome : BaseBiomeProcessor
    {
        [SerializeField] private AnimationCurve _continentalnessCurve;
        [SerializeField] private Octaves _octaves;
        
        
        public override IBiomeGenerator CreateInstance(Random random, CreateChunkJob job, BiomeProcessor biomeProcessor, BiomeBlender parentBlender)
        {
            BiomeBlender.SetupData data = parentBlender.FindSetupData(this);
            BiomeBlender.SetupData next = parentBlender.FindNext(this);

            float nextValue = next.heightValue == 0 ? 1 : next.heightValue - next.smoothing * .5f;
            
            return new MountainBiomeGenerator
            {
                sampler = new OctavesSampler(random, _octaves.octaves, job.x * job.chunkSize, job.z * job.chunkSize),
                baseHeight = biomeProcessor.baseHeight,
                
                continentalness       = job.continentalness,
                continentalnessCurve  = _continentalnessCurve,
                continentalnessOffset = data.heightValue - data.smoothing*.5f,
                continentalnessMulti  = 1/(nextValue - (data.heightValue - data.smoothing*.5f))
            };
        }
    }
    
    
    public class MountainBiomeGenerator : IBiomeGenerator
    {
        public int baseHeight;
     
        //  Instance

        public OctavesSampler sampler;
        public float[,] continentalness;

        public AnimationCurve continentalnessCurve;
        public float continentalnessOffset;
        public float continentalnessMulti;


        public float SampleContinentalnessHeight(int x, int z)
        {
            return continentalnessCurve.Evaluate((continentalness[x, z] - continentalnessOffset) * continentalnessMulti);
        }


        public int SampleMapPoint(int x, int z)
        {
            return (int)(SampleContinentalnessHeight(x,z) + baseHeight + sampler.Sample(x, z));
        }


        public BlockId SampleBlock(int x, int y, int z, int heightValue)
        {
            return y > heightValue ? BlockId.Air : BlockId.Stone;
        }
    }
}
