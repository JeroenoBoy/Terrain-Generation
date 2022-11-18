using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;



namespace Generation.Generators.Helpers
{
    public class ChunkCreatorThread
    {
        private readonly List<GeneratorContext> _chunksToGenerate;
        private readonly List<GeneratorContext> _chunksToReduce;
        private readonly Thread _thread;
        private bool _stayAlive = false;


        /// <summary>
        /// Create a new chunk generator thread
        /// </summary>
        public ChunkCreatorThread()
        {
            _chunksToGenerate = new List<GeneratorContext>();
            _chunksToReduce   = new List<GeneratorContext>();
            
            _thread = new Thread(Spawn);
            _thread.Start();
        }


        /// <summary>
        /// Spawn the thread
        /// </summary>
        private void Spawn()
        {
            if (_stayAlive) throw new Exception("Thread is already alive");
            _stayAlive = true;

            try {
                while (_stayAlive) {
                    while (_chunksToGenerate.Count > 0) ExecuteCreateChunkJob(_chunksToGenerate.First());
                    while (_chunksToReduce.Count > 0)   ExecuteReduceMesh(_chunksToReduce.First());

                    Thread.Sleep(100);
                }
            }
            catch (Exception e) {
                _stayAlive = false;
                Debug.Log("Thread got an uncaught exception and died");
                throw e;
            }
        }
        

        /// <summary>
        /// Tell the thread to generate a new chunk
        /// </summary>
        public IEnumerator GenerateChunk(CreateChunkJob job)
        {
            if (!_stayAlive) throw new Exception("Thread is not alive");

            bool hasFinished = false;
            GeneratorContext ctx = new (job, () => hasFinished = true);
            _chunksToGenerate.Add(ctx);

            yield return new WaitUntil(() => hasFinished);
        }
        

        /// <summary>
        /// Tell the thread to generate a new chunk
        /// </summary>
        public IEnumerator ReduceMesh(CreateChunkJob job)
        {
            if (!_stayAlive) throw new Exception("Thread is not alive");

            bool hasFinished = false;
            GeneratorContext ctx = new (job, () => hasFinished = true);
            _chunksToReduce.Add(ctx);

            yield return new WaitUntil(() => hasFinished);
        }


        private void ExecuteCreateChunkJob(GeneratorContext ctx)
        {
            try {
                ctx.job.Execute();
            }
            catch (Exception e) {
                Debug.LogError("Job exited with exception");
                Debug.LogError(e);
            }
            _chunksToGenerate.Remove(ctx);
            ctx.finishAction.Invoke();
        }


        private void ExecuteReduceMesh(GeneratorContext ctx)
        {
            try {
                ctx.job.ReduceMesh();
            }
            catch (Exception e) {
                Debug.LogError("Job exited with exception");
                Debug.LogError(e);
            }
            _chunksToReduce.Remove(ctx);
            ctx.finishAction.Invoke();
        }


        /// <summary>
        /// Dispose the thread
        /// </summary>
        public void Dispose()
        {
            if (!_stayAlive) {
                Debug.LogWarning("Thread is not alive");
            }

            _stayAlive = false;
            _thread.Abort();
        }
        
        
        
        private class GeneratorContext
        {
            public readonly CreateChunkJob job;
            public readonly Action finishAction;


            public GeneratorContext(CreateChunkJob job, Action finishAction)
            {
                this.job = job;
                this.finishAction = finishAction;
            }
        }
    }
}
