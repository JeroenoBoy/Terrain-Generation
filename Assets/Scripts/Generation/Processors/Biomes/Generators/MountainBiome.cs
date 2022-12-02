using System;
using System.Linq;
using Generation.Generators.Helpers;
using Generation.Processors.Biomes.Blenders;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;



namespace Generation.Processors.Biomes.Generators
{
    public enum CalculationType { Linier, Quadratic, Root }



    [CreateAssetMenu(menuName = "Biomes/MountainBiome")]
    public class MountainBiome : BaseBiomeProcessor
    {
        [SerializeField] private int _minContinentalnessHeight;
        [SerializeField] private int _maxContinentalnessHeight;
        [SerializeField] private CalculationType _calculation;
     
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
                
                continentalness          = job.continentalness,
                continentalnessOffset    = data.heightValue - data.smoothing*.5f,
                continentalnessMulti     = 1/(nextValue - (data.heightValue - data.smoothing*.5f)),
                minContinentalnessHeight = _minContinentalnessHeight,
                maxContinentalnessHeight = _maxContinentalnessHeight,
                calculation              = _calculation,
            };
        }
    }
    
    
    public class MountainBiomeGenerator : IBiomeGenerator
    {
        public int baseHeight;

        
        public OctavesSampler sampler;
        public float[,] continentalness;

        public float continentalnessOffset;
        public float continentalnessMulti;

        public int minContinentalnessHeight;
        public int maxContinentalnessHeight;
        
        public CalculationType calculation;

        
        public float SampleContinentalnessHeight(int x, int z)
        {
            float v = (continentalness[x, z] - continentalnessOffset) * continentalnessMulti;
            
            return minContinentalnessHeight + (maxContinentalnessHeight - minContinentalnessHeight) * calculation switch
            {
                CalculationType.Linier => v,
                CalculationType.Quadratic => math.pow(v, 2),
                CalculationType.Root => math.sqrt(v),
                _ => throw new ArgumentOutOfRangeException()
            };
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
