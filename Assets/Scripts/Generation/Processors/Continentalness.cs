using UnityEngine;



namespace Generation.Processors
{
    [System.Serializable]
    public struct Continentalness : IProcessors
    {
        public float offsetX;
        public float offsetZ;
        public float frequency;


        public void Process(CreateChunkJob jobData)
        {
            int chunkX = jobData.x;
            int chunkY = jobData.z;
            int size   = jobData.chunkSize;

            System.Random random = new (jobData.seed);

            float cOffsetX = chunkX * size + offsetX * (float)random.NextDouble();
            float cOffsetZ = chunkY * size + offsetZ * (float)random.NextDouble();

            float[,] map = new float[size, size];
            
            for (int x = 0; x < size; x++) {
                float pointX = (x + cOffsetX) * frequency;
                for (int z = 0; z < size; z++) {
                    float pointY = (z + cOffsetZ) * frequency;
                    
                    map[x, z] = Mathf.PerlinNoise(pointX, pointY);
                }
            }

            jobData.continentalness = map;
        }
    }
}
