using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JUtils.Attributes;
using JUtils.Extensions;
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
        private List<Vector2Int> _chunksToCreate;


        private void Start()
        {
            chunks = new Dictionary<Vector2Int, Chunk>();
            _chunksToCreate = new List<Vector2Int>();
        }


        private void FixedUpdate()
        {
            Vector2 playerUnroundedPos = _target.position.XZToVector2() / chunkWidth;
            Vector2Int     playerChunk = Vector2Int.RoundToInt(playerUnroundedPos);

            int maxRange = _range;
            int halfRange = maxRange / 2;
            
            //  Getting chunks to destroy
            
            Vector2Int[] toDestroy = chunks
                .Select(c => c.Key)
                .Where(c => (Mathf.Abs(c.x - playerChunk.x) > halfRange) || (Mathf.Abs(c.y - playerChunk.y) > halfRange))
                .ToArray();
            
            //  Destroying out of range chunks
            
            foreach (Vector2Int pos in toDestroy) {
                DestroyChunk(pos);
            }
            
            //  Getting positions of new chunks to generate
            
            for (int x = playerChunk.x - halfRange; x <= playerChunk.x + halfRange; x++) {
                for (int z = playerChunk.y - halfRange; z <= playerChunk.y + halfRange; z++) {
                    if (chunks.ContainsKey(new Vector2Int(x, z))) continue;
                    Vector2Int newChunkPos = new (x, z);
                    StartCoroutine(GenerateChunk(newChunkPos));
                }
            }
        }


        private void DestroyChunk(Vector2Int pos)
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
        private IEnumerator GenerateChunk(Vector2Int chunkPos)
        {
            if (_chunksToCreate.Contains(chunkPos)) yield break;
            _chunksToCreate.Add(chunkPos);

            while (_activeGenerators >= _threads) yield return null;
            
            _activeGenerators++;
            
            int x = chunkPos.x, z = chunkPos.y;
        
            //  Running generator
        
            CreateChunkJob job = GetCreateChunkJob(x, z);

            Thread thread = new (job.Execute);
            thread.Start();
            yield return new WaitWhile(() => thread.IsAlive);
        
            //  Creating chunk
        
            CreateChunk(job);
            _activeGenerators--;
            _chunksToCreate.Remove(chunkPos);
        }
    }
}
