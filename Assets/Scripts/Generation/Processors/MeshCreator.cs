using System;
using System.Collections.Generic;
using Generation.Generators;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;



namespace Generation.Processors
{
    [System.Serializable]
    public class MeshCreator : IProcessors
    {
        [SerializeField] private Material _material;
        [SerializeField] private ComputeShader _shader;


        public BaseGenerator.ComputeShaderData CreateShader(BaseGenerator generator)
        {
            ComputeShader baseShader = _shader;
            ComputeShader shader = Object.Instantiate(baseShader);

            int size = generator.chunkHeight * generator.chunkWidth * generator.chunkWidth;

            ComputeBuffer blocksBuffer = new (size, sizeof(int));
            ComputeBuffer uvMapBuffer = new (size, sizeof(int) + sizeof(float) * 14);

            ComputeBuffer verticesBuffer = new (
                size,
                sizeof(float) * 3,
                ComputeBufferType.Counter
            );

            ComputeBuffer normalBuffer = new (
                size,
                sizeof(float) * 3,
                ComputeBufferType.Counter
            );

            ComputeBuffer uvBuffer = new (
                size,
                sizeof(float) * 2,
                ComputeBufferType.Counter
            );

            ComputeBuffer triangleBuffer = new (
                size * 2,
                sizeof(int) * 3,
                ComputeBufferType.Counter
            );

            int mainIndex = shader.FindKernel("main");
            int clearIndex = shader.FindKernel("clear");

            shader.SetInt("size", generator.chunkWidth);
            shader.SetInt("height", generator.chunkHeight);

            shader.SetBuffer(mainIndex, "blocks", blocksBuffer);
            shader.SetBuffer(mainIndex, "block_uvs", uvMapBuffer);
            shader.SetBuffer(mainIndex, "vertices", verticesBuffer);
            shader.SetBuffer(mainIndex, "normals", normalBuffer);
            shader.SetBuffer(mainIndex, "uvs", uvBuffer);
            shader.SetBuffer(mainIndex, "triangles", triangleBuffer);

            shader.SetBuffer(clearIndex, "vertices", verticesBuffer);
            shader.SetBuffer(clearIndex, "normals", normalBuffer);
            shader.SetBuffer(clearIndex, "uvs", uvBuffer);
            shader.SetBuffer(clearIndex, "triangles", triangleBuffer);
            
            Debug.Log("New shader made");

            return new BaseGenerator.ComputeShaderData(shader)
            {
                blocksBuffer = blocksBuffer,
                uvMapBuffer = uvMapBuffer,
                verticesBuffer = verticesBuffer,
                normalBuffer = normalBuffer,
                uvBuffer = uvBuffer,
                triangleBuffer = triangleBuffer,
            };
        }


        public void Process(CreateChunkJob jobData)
        {
            //  Creating buffers

            BaseGenerator.ComputeShaderData shader = jobData.shader;

            shader.blocksBuffer.SetData(jobData.blocks);
            shader.uvMapBuffer.SetData(jobData.blockData);

            //  Running the shader

            shader.Dispatch(jobData.chunkSize, jobData.chunkHeight);

            //  Creating the mesh

            Vector3[] vertices  = new Vector3[shader.verticesBuffer.count];
            Vector3[] normals   = new Vector3[shader.normalBuffer.count];
            Vector2[] uvs       = new Vector2[shader.uvBuffer.count];
            int3[]    triangles = new    int3[shader.triangleBuffer.count];

            shader.verticesBuffer.GetData(vertices);
            shader.normalBuffer.GetData(normals);
            shader.uvBuffer.GetData(uvs);
            shader.triangleBuffer.GetData(triangles);

            jobData.meshData = new MeshData
            {
                vertices = vertices, normals = normals, uvs = uvs, triangles = triangles
            };
        }
        

        public void ReduceMesh(CreateChunkJob job)
        {
            int3[] triangles = job.meshData.triangles;
            Stack<int> triangleStack = new (triangles.Length*2);

            for (int i = 0; i < triangles.Length; i++) {
                int3 quad = triangles[i];

                if (quad.Equals(default)) break;

                triangleStack.Push(quad.x);
                triangleStack.Push(quad.y);
                triangleStack.Push(quad.z);
            }

            int size = triangleStack.Count / 6 * 4;
            Array.Resize(ref job.meshData.vertices, size);
            Array.Resize(ref job.meshData.normals, size);
            Array.Resize(ref job.meshData.uvs, size);

            job.meshData.triangles = null;
            job.meshData.indices = triangleStack.ToArray();
        }



        // public struct Triangle
        // {
        //     public uint x, y, z;
        //
        //
        //     public Triangle(uint x, uint y, uint z)
        //     {
        //         this.x = x;
        //         this.y = y;
        //         this.z = z;
        //     }
        //
        //
        //     public static readonly Triangle Zero = new ();
        //
        //     public override bool Equals(object obj)
        //         => obj is Triangle t && t.x == y && t.y == y && t.z == z;
        //
        //
        //     public override int GetHashCode() => HashCode.Combine(x, y, z);
        // }



        public struct Quad
        {
            public int4 indices;
            public Vector3 normal;


            public static readonly Quad zero = new() { indices = int4.zero, normal = Vector3.zero};
            

            public static bool operator ==(Quad a, Quad b) => a.indices.Equals(b.indices) && a.normal.Equals(b.normal);
            public static bool operator !=(Quad a, Quad b) => !(a == b);
            
            
            public bool Equals(Quad other)
            {
                return indices.Equals(other.indices) && normal.Equals(other.normal);
            }


            public override bool Equals(object obj)
            {
                return obj is Quad other && Equals(other);
            }


            public override int GetHashCode()
            {
                return HashCode.Combine(indices, normal);
            }
        }
    }
}
