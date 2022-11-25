using UnityEngine;
using Random = Unity.Mathematics.Random;



namespace Generation.Generators.Helpers
{
    public readonly struct PerlinNoiseOffset
    {
        private readonly int _offsetX;
        private readonly int _offsetZ;
        
        public PerlinNoiseOffset(CreateChunkJob job, int offsetXMulti, int offsetZMulti)
        {
            Random random = new (job.seed);
            
            _offsetX = (int)(job.x * job.chunkSize + offsetXMulti * random.NextFloat());
            _offsetZ = (int)(job.z * job.chunkSize + offsetZMulti * random.NextFloat());
        }

        public float PerlinNoise(int x, int z)
        {
            return Mathf.PerlinNoise(x, z);
        }
    }
}
