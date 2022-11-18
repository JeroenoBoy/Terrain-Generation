using UnityEngine;



namespace Generation.Generators
{
    [System.Serializable]
    public struct TerrainProcessors : IProcessors
    {
        [SerializeField] private int _baseLevel;
        [SerializeField] private TerrainLayer[] _layers;

        [SerializeField] private int _seedOffset;
        

        public void Process(CreateChunkJob jobData)
        {
            int chunkX = jobData.x;
            int chunkY = jobData.z;
            int size   = jobData.chunkSize;
            
            float[,] map = CreateMap(chunkX, chunkY, size, jobData.seed);

            //  Adding to chunk and filling with stone

            for (int x = size; x-- > 0;) {
                for (int z = size; z-- > 0;) {
                    int height = _baseLevel + Mathf.RoundToInt(map[x,z]);
                    
                    for (int y = height; y-- > 0;) {
                        if (y == height-1)      jobData.blocks[x, y, z] = BlockId.Grass;
                        else if (y >= height-4) jobData.blocks[x, y, z] = BlockId.Dirt;
                        else                    jobData.blocks[x, y, z] = BlockId.Stone;
                    }
                }
            }
        }


        private float[,] CreateMap(int chunkX, int chunkY, int size, int seed)
        {
            System.Random random = new (seed * _seedOffset);
            
            //  Generating map

            float[,] map = new float[size, size];
            
            for (int i = _layers.Length; i --> 0;) {
                TerrainLayer layer = _layers[i];
                
                float offsetX = chunkX * size + (float)(layer.offsetX * random.NextDouble());
                float offsetZ = chunkY * size + (float)(layer.offsetZ * random.NextDouble());
                
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

        

        #region Test
        
        
        // [Header("Debug")] [SerializeField] private int x, y;
        // private ObjectPool<GameObject> _pool;
        // public ObjectPool<GameObject> pool => _pool ?? new ObjectPool<GameObject>(CreateFunc, OnGet, 
        // OnRelease, OnDestroy, true, 256, 512);
        // private List<GameObject> _activeObjects = new ();
        // public void TestGenerate()
        // {
        //     foreach (GameObject activeObject in _activeObjects) {
        //         pool.Release(activeObject);
        //     }
        //     
        //     _activeObjects.Clear();
        //
        //     int size = 64;
        //
        //     float[,] map = CreateMap(x, y,size, 0);
        //         
        //     for (int x = size; x --> 0;) {
        //         for (int z = size; z --> 0;) {
        //             GameObject obj = pool.Get();
        //             obj.transform.position = new Vector3(this.x * size + x, map[x,z], y * size + z);
        //             _activeObjects.Add(obj);
        //         }
        //     }
        // }


        // private GameObject CreateFunc() => Object.Instantiate(_cube);
        // private void OnRelease(GameObject gameObject) => gameObject.SetActive(false);
        // private void OnGet(GameObject gameObject) => gameObject.SetActive(true);
        // private void OnDestroy(GameObject gameObject) => Object.Destroy(gameObject);
        
        
        #endregion
        

        
        [System.Serializable]
        private class TerrainLayer
        {
            public float weight;
            public float noiseScale;
            public float offsetX;
            public float offsetY;
            public float offsetZ;
        }
    }
}
