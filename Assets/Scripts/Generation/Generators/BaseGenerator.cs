using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Generation.Processors;
using Generation.Processors.Biomes;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;



namespace Generation.Generators
{
    public abstract class BaseGenerator : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private Transform _chunkHolder;
        [SerializeField] private GameObject _chunkPrefab;
        
        [Header("Generator data")]
        [SerializeField] private int _chunkWidth;
        [SerializeField] private int _chunkHeight;
        [field: SerializeField] public int seed { get; private set; }

        [Space]
        [SerializeField] private Block[] _blocks;

        [Header("Setup options")]

        [Header("Generators")]
        [SerializeField] private Continentalness _continentalness;
        [SerializeField] private BiomeProcessor _biomeProcessor;
        [SerializeField] private CaveProcessor _caveProcessor;
        [SerializeField] private ComputeShader _shader;

        private MeshCreator _meshCreator;

        protected Dictionary<int2, Chunk> chunks;
        public MeshCreator meshCreator => _meshCreator;

        public int chunkWidth => _chunkWidth;
        public int chunkHeight => _chunkHeight;
        public Block[] blocks => _blocks;
        public Transform chunkHolder => _chunkHolder;
        
        
        protected virtual void OnEnable()
        {
            _meshCreator = new MeshCreator(this, _shader);
        }


        protected virtual void OnDisable()
        {
            _meshCreator.Dispose();
        }


        protected CreateChunkJob GetCreateChunkJob(int x, int z) =>
            new (this, x, z)
            {
                processors = new IProcessors[]
                {
                    _continentalness,
                    _biomeProcessor,
                    _caveProcessor
                }
            };


        /// <summary>
        /// Spawns the chunk in the world
        /// </summary>
        protected void CreateChunk(CreateChunkJob job)
        {
            Chunk chunk    = Instantiate(_chunkPrefab.gameObject, transform).GetComponent<Chunk>();
            chunk.size     = _chunkWidth;
            chunk.height   = _chunkHeight;
            chunk.location = new int2(job.x, job.z);

            chunk.blocks = job.blocks;
            chunk.generator = this;
            
            chunks.Add(chunk.location, chunk);
            chunk.transform.parent = _chunkHolder;
        }


        /// <summary>
        /// Tries to delete a chunk if it exists
        /// </summary>
        public void Delete(int2 location)
        {
            if (chunks is null || !chunks.ContainsKey(location)) return;

            Chunk chunk = chunks[location];
            if (chunk.isActiveAndEnabled) Destroy(chunk);
            chunks.Remove(location);
        }
    }
}
