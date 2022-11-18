using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Generation.Processors;
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
        [SerializeField] private MeshCreator _meshCreator;


        protected Dictionary<Vector2Int, Chunk> chunks;
        public MeshCreator meshCreator => _meshCreator;


        public int chunkWidth => _chunkWidth;
        public int chunkHeight => _chunkHeight;
        public Block[] blocks => _blocks;
        public Transform chunkHolder => _chunkHolder;


        private ComputeShaderData _shader;
        private ISerializationCallbackReceiver _serializationCallbackReceiverImplementation;


        protected virtual void OnEnable()
        {
            _shader = _meshCreator.CreateShader(this);
        }


        protected virtual void OnDisable()
        {
            _shader.Dispose();
            _shader = null;
        }


        protected CreateChunkJob GetCreateChunkJob(int x, int z) =>
            new (this, x, z)
            {
                shader = _shader,
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
            chunk.location = new Vector2Int(job.x, job.z);

            chunk.blocks = job.blocks;
            chunk.GetComponent<MeshFilter>().mesh = new Mesh
            {
                vertices  = job.meshData.vertices,
                normals   = job.meshData.normals,
                uv        = job.meshData.uvs,
                triangles = job.meshData.indices
            };
            
            chunks.Add(chunk.location, chunk);
            chunk.transform.parent = _chunkHolder;
        }


        #region OjbectPool


        public class ComputeShaderData
        {
            private ComputeShader _shader;
            public readonly int calculateVoxels;
            public readonly int clearData;

            public ComputeBuffer blocksBuffer;
            public ComputeBuffer uvMapBuffer;
            
            public ComputeBuffer verticesBuffer;
            public ComputeBuffer normalBuffer;
            public ComputeBuffer uvBuffer;
            public ComputeBuffer triangleBuffer;


            public ComputeShaderData(ComputeShader shader)
            {
                _shader = shader;
                calculateVoxels = shader.FindKernel("main");
                clearData = shader.FindKernel("clear");
            }


            public void Dispatch(int size, int height)
            {
                verticesBuffer.SetCounterValue(0);
                normalBuffer.SetCounterValue(0);
                uvBuffer.SetCounterValue(0);
                triangleBuffer.SetCounterValue(0);
                
                verticesBuffer.SetData(Array.Empty<Vector3>());
                normalBuffer.SetData(Array.Empty<Vector3>());
                uvBuffer.SetData(Array.Empty<Vector2>());
                triangleBuffer.SetData(Array.Empty<int3>());

                _shader.Dispatch(clearData, verticesBuffer.count / 128, 1, 1);
                _shader.Dispatch(calculateVoxels, size/8, height/8, size/8);
            }


            public void Dispose()
            {
                Destroy(_shader);
                blocksBuffer.Release();
                uvMapBuffer.Release();
                
                verticesBuffer.Release();
                normalBuffer.Release();
                uvBuffer.Release();
                triangleBuffer.Release();
            }
        }
        
        #endregion
    }
}
