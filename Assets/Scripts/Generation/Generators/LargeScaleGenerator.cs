using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Generation.Generators.Helpers;
using JUtils.Attributes;
using JUtils.Extensions;
using UnityEngine;



namespace Generation.Generators
{
    public class LargeScaleGenerator : BaseGenerator
    {
        [Header("Generator options")]
        [SerializeField] private int _chunksX;
        [SerializeField] private int _chunksY;
        [SerializeField] private int _threads;
        
        private bool _isRunning;
        private int  _activeGenerators;


        [Button]
        public IEnumerator GenerateChunks()
        {
            if (_isRunning) throw new Exception("Already running!");
            _isRunning = true;

            chunks = new Dictionary<Vector2Int, Chunk>();

            for (int x = _chunksX; x-- > 0;) {
                for (int y = _chunksY; y-- > 0;) {

                    if (_activeGenerators >= _threads) {
                        yield return new WaitUntil(() => _activeGenerators < _threads);
                    }
                    
                    StartCoroutine(GenerateChunk(x, y));
                }
            }
            
            _isRunning = false;
        }



        [Button]
        public void DestroyChunks()
        {
            foreach (Transform transform in chunkHolder) {
                Destroy(transform.gameObject);
            }
        }


        /**
         * Generate a new chunk async
         */
        private IEnumerator GenerateChunk(int x, int z)
        {
            _activeGenerators++;
            
            //  Running generator
            
            CreateChunkJob job = GetCreateChunkJob(x, z);

            Thread thread = new (job.Execute);
            thread.Start();
            yield return new WaitWhile(() => thread.IsAlive);
            
            //  Generating mesh
            
            meshCreator.Process(job);
            
            thread = new Thread(job.ReduceMesh);
            thread.Start();
            yield return new WaitWhile(() => thread.IsAlive);
            
            //  Creating chunk
            
            CreateChunk(job);
            _activeGenerators--;
        }
    }
}
