using UnityEngine;
using Random = System.Random;



namespace Generation.Generators.Helpers
{
    public struct PerlinNoiseOffset
    {
        private readonly int _offsetX;
        private readonly int _offsetZ;
        
        public PerlinNoiseOffset(CreateChunkJob job, int offsetXMulti, int offsetZMulti)
        {
            Random random = new (job.seed);
            
            _offsetX = (int)(job.x * job.chunkSize + offsetXMulti * (float)random.NextDouble());
            _offsetZ = (int)(job.z * job.chunkSize + offsetZMulti * (float)random.NextDouble());
        }

        public float PerlinNoise(int x, int z)
        {
            return Mathf.PerlinNoise(x, z);
        }
    }
}
