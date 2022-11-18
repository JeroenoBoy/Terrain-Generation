using System;
using System.Linq;
using Generation.Generators;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;



namespace Generation
{
    public class CreateChunkJob
    {
        public readonly int seed;
        
        public readonly int x;
        public readonly int z;
        
        public readonly int chunkSize;
        public readonly int chunkHeight;
        public BlockId[,,] blocks { get; private set; }

        public readonly BlockData[] blockData;

        public float[,] continentalness;
        public float[,] heightMap;
        
        public IProcessors[] processors;
        public BaseGenerator.ComputeShaderData shader;
        
        public MeshData meshData;

        public BaseGenerator generator;

        public CreateChunkJob(BaseGenerator generator, int x, int z)
        {
            this.x = x;
            this.z = z;

            seed = generator.seed;
            
            chunkSize   = generator.chunkWidth;
            chunkHeight = generator.chunkHeight;

            blockData = generator.blocks.Select(t => t.data).ToArray();

            blocks = null;
            processors = default;
            
            this.generator = generator;
        }
        
        
        public void Execute()
        {
            blocks = new BlockId[chunkSize,chunkHeight,chunkSize];

            for (int i = 0; i < processors.Length; i++)
                processors[i].Process(this);
        }


        public void ReduceMesh()
        {
            generator.meshCreator.ReduceMesh(this);
        }


        public T GetProcessor<T>() where T : IProcessors
        {
            return (T)processors.First(t => t is T);
        }
    }



    public struct MeshData
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector2[] uvs;
        public int3[] triangles;
        public int[] indices;
    }
}
