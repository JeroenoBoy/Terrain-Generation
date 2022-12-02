using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;



namespace Generation.Generators
{
    public class RelativeChunkGenerator : BaseGenerator
    {
        [SerializeField] private Transform _target;
        [SerializeField] private int       _range;
        [SerializeField] private int       _threads;
        
        private int  _activeGenerators;
        private List<int2> _chunksToCreate;

        private readonly List<Thread> _activeThreads = new ();


        private void Start()
        {
            chunks = new Dictionary<int2, Chunk>();
            _chunksToCreate = new List<int2>();
        }


        private void FixedUpdate()
        {
            float2 playerUnroundedPos = ((float3)_target.position).xz / chunkWidth;
            int2          playerChunk = (int2)math.floor(playerUnroundedPos);

            int maxRange = _range;
            int halfRange = maxRange / 2;
            
            //  Getting chunks to destroy
            
            int2[] toDestroy = chunks
                .Select(c => c.Key)
                .Where(c => (math.abs(c.x - playerChunk.x) > halfRange) || (math.abs(c.y - playerChunk.y) > halfRange))
                .ToArray();
            
            //  Destroying out of range chunks
            
            foreach (int2 pos in toDestroy) {
                DestroyChunk(pos);
            }
            
            //  Getting positions of new chunks to generate
            
            for (int x = playerChunk.x - halfRange; x <= playerChunk.x + halfRange; x++) {
                for (int z = playerChunk.y - halfRange; z <= playerChunk.y + halfRange; z++) {
                    int2 newChunkPos = new (x, z);
                    if (chunks.ContainsKey(newChunkPos)) continue;
                    StartCoroutine(GenerateChunk(newChunkPos));
                }
            }
        }


        private void DestroyChunk(int2 pos)
        {
            if (!chunks.TryGetValue(pos, out Chunk chunk)) {
                Debug.Log($"Tried to get chunk at {pos} but chunk does not exist");
                return;
            }
        
            Destroy(chunk.gameObject);
            chunks.Remove(pos);
        }
    

        /**
         * Generate a new chunk async
         */
        private IEnumerator GenerateChunk(int2 chunkPos)
        {
            if (_chunksToCreate.Contains(chunkPos)) yield break;
            _chunksToCreate.Add(chunkPos);

            while (_activeGenerators >= _threads) yield return null;
            
            _activeGenerators++;
            
            int x = chunkPos.x, z = chunkPos.y;
        
            //  Running generator
        
            CreateChunkJob job = GetCreateChunkJob(x, z);

            Thread thread = new (job.Execute);
            _activeThreads.Add(thread);
            thread.Start();
            yield return new WaitWhile(() => thread.IsAlive);
            _activeThreads.Remove(thread);
            
            //  Creating chunk
        
            CreateChunk(job);
            _activeGenerators--;
            _chunksToCreate.Remove(chunkPos);
        }


        
        private void OnDestroy()
        {
            foreach (Thread activeThread in _activeThreads) {
                activeThread.Abort();
            }
        }
    }
}
