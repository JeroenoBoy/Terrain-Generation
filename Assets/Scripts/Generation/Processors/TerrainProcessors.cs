using Generation.Generators.Helpers;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;



namespace Generation.Processors
{ 
    [System.Serializable]
    public struct TerrainProcessors : IProcessors
    {
        [SerializeField] private int _baseLevel;
        
        [Header("Layers")]
        [SerializeField] private TerrainLayer[]  _perlinLayers;
        [SerializeField] private BlockLevels     _blockLevels;
        

        public void Process(CreateChunkJob jobData)
        {
            int chunkX = jobData.x;
            int chunkY = jobData.z;
            int size   = jobData.chunkSize;
            
            float[,] map = CreateMap(chunkX, chunkY, size, jobData.seed);
            
            CreateBlocks(chunkX, chunkY, size, map, jobData);
        }


        private float[,] CreateMap(int chunkX, int chunkY, int size, uint seed)
        {
            Random random = new (seed);
            
            float[,] map = new float[size,size];
            
            //  Perlin
            
            for (int i = _perlinLayers.Length; i --> 0;) {
                TerrainLayer layer = _perlinLayers[i];
                
                float offsetX = chunkX * size + layer.offsetX * random.NextFloat();
                float offsetZ = chunkY * size + layer.offsetZ * random.NextFloat();
                
                for (int x = 0; x < size; x++) {
                    for (int z = 0; z < size; z++) {
                        float pointX = (offsetX + x) * layer.noiseScale;
                        float pointZ = (offsetZ + z) * layer.noiseScale;
                        
                        map[x,z] += Mathf.PerlinNoise(pointX, pointZ) * layer.weight + layer.offsetY;
                    }
                }
            }

            return map;
        }



        private void CreateBlocks(int chunkX, int chunkY, int size, float[,] map, CreateChunkJob jobData)
        {
            //  Configuring snow map

            Random random = new (jobData.seed);
            float snowOffsetX = chunkX * size + _blockLevels.offsetX * random.NextFloat();
            float snowOffsetZ = chunkY * size + _blockLevels.offsetZ * random.NextFloat();
            
            //  Adding to chunk and filling with stone
            
            BlockLevels bLevel = _blockLevels;
                
            //  No added 3d perlin noise layer
            
            for (int x = size; x --> 0;) {
                for (int z = size; z --> 0;) {
                    int height = _baseLevel + (int)math.round(map[x,z]);

                    if (height >= bLevel.snowLevel) {
                        float pointX = (snowOffsetX + x) * bLevel.frequency;
                        float pointY = (snowOffsetZ + z) * bLevel.frequency;
                        float snowHeight = bLevel.snowLevel + noise.cnoise(new float2(pointX, pointY)) * bLevel.scale;
                        
                        jobData.blocks[x, height, z] = height >= snowHeight ? BlockId.Snow : BlockId.Grass;
                    }
                    else {
                        jobData.blocks[x, height, z] = BlockId.Grass;
                    }
                    
                    for (int y = height; y --> 0;) {
                        jobData.blocks[x, y, z] = y >= height - bLevel.dirtHeight ? BlockId.Dirt : BlockId.Stone;
                    }
                }
            }
        }
        
        
        
        [System.Serializable]
        private struct TerrainLayer
        {
            public float weight;
            public float noiseScale;
            public float offsetX;
            public float offsetY;
            public float offsetZ;
        }



        [System.Serializable]
        private struct BlockLevels
        {
            public int dirtHeight;
            
            [Header("Noise")]
            public float offsetX;
            public float offsetZ;
            public float frequency;
            public float scale;
            
            [Space]
            public int snowLevel;
        }
    }
}
