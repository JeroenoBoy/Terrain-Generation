using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JUtils.Attributes;
using UnityEngine;



namespace Generation.Generators
{
    public class ChunkGenerator : BaseGenerator
    {
        [Header("Generator options")]
        [SerializeField] private int _chunksX;
        [SerializeField] private int _chunksY;


        [Button]
        public void GenerateBase()
        {
            chunks = new Dictionary<Vector2Int, Chunk>();
            for (int x = _chunksX; x --> 0;) {
                for (int z = _chunksY; z --> 0;) {
                    GenerateChunk(x, z);
                }
            }
        }


        private void GenerateChunk(int x, int z)
        {
            //  Running generator

            CreateChunkJob job = GetCreateChunkJob(x, z);
            job.Execute();
            
            //  Creating chunk
            
            CreateChunk(job);
        }
    }
}
