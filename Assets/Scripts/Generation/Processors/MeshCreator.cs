using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Object = UnityEngine.Object;



namespace Generation.Generators
{
    [System.Serializable]
    public struct MeshCreator : IProcessors
    {
        [SerializeField] private Material _material;
        [SerializeField] private ComputeShader _shader;


        public void Process(CreateChunkJob jobData)
        {
            ComputeShader shader = Object.Instantiate(_shader);

            //  Creating buffers

            ComputeBuffer blocksBuffer = new (jobData.blocks.Length, sizeof(int));
            ComputeBuffer uvMapBuffer  = new (jobData.blocks.Length, sizeof(int) + sizeof(float) * 14);
            
            ComputeBuffer verticesBuffer = new (
                jobData.blocks.Length,
                sizeof(float) * 3,
                ComputeBufferType.Counter
            );
            
            ComputeBuffer normalBuffer = new (
                jobData.blocks.Length,
                sizeof(float) * 3,
                ComputeBufferType.Counter
            );
            
            ComputeBuffer uvBuffer = new (
                jobData.blocks.Length,
                sizeof(float) * 2,
                ComputeBufferType.Counter
            );
            
            ComputeBuffer triangleBuffer = new (
                jobData.blocks.Length * 2,
                sizeof(int) * 3,
                ComputeBufferType.Append
            );
            

            blocksBuffer.SetData(jobData.blocks);
            uvMapBuffer.SetData(jobData.blockData);

            //  Running the shader

            int kernelIndex = shader.FindKernel("main");

            shader.SetInt("size", jobData.chunkSize);
            shader.SetInt("height", jobData.chunkHeight);

            shader.SetBuffer(kernelIndex, "blocks", blocksBuffer);
            shader.SetBuffer(kernelIndex, "block_uvs", uvMapBuffer);
            shader.SetBuffer(kernelIndex, "vertices", verticesBuffer);
            shader.SetBuffer(kernelIndex, "normals", normalBuffer);
            shader.SetBuffer(kernelIndex, "uvs", uvBuffer);
            shader.SetBuffer(kernelIndex, "triangles", triangleBuffer);

            verticesBuffer.SetCounterValue(0);
            normalBuffer.SetCounterValue(0);
            uvBuffer.SetCounterValue(0);
            triangleBuffer.SetCounterValue(0);

            shader.Dispatch(kernelIndex, jobData.chunkSize / 8, jobData.chunkHeight / 8, jobData.chunkSize / 8);

            //  Creating the mesh

            Vector3[] vertices  = new Vector3[verticesBuffer.count];
            Vector3[] normals   = new Vector3[normalBuffer.count];
            Vector2[] uvs       = new Vector2[uvBuffer.count];
            int3[]    triangles = new int3[triangleBuffer.count];

            verticesBuffer.GetData(vertices);
            normalBuffer.GetData(normals);
            uvBuffer.GetData(uvs);
            triangleBuffer.GetData(triangles);

            Mesh mesh = CreateMesh(vertices, triangles, normals, uvs);
            jobData.mesh = mesh;
            
            //  Releasing buffers

            blocksBuffer.Release();
            uvMapBuffer.Release();
            verticesBuffer.Release();
            normalBuffer.Release();
            triangleBuffer.Release();
            uvBuffer.Release();
            Object.Destroy(shader);
        }
        

        public Mesh CreateMesh(Vector3[] vertices, int3[] triangles, Vector3[] normals, Vector2[] uvs)
        {
            Stack<int> triangleStack = new (triangles.Length*2);

            for (int i = 0; i < triangles.Length; i++) {
                int3 quad = triangles[i];

                if (quad.Equals(default)) break;

                triangleStack.Push(quad.x);
                triangleStack.Push(quad.y);
                triangleStack.Push(quad.z);
            }

            int size = triangleStack.Count / 6 * 4;
            Array.Resize(ref vertices, size);
            Array.Resize(ref normals, size);
            Array.Resize(ref uvs, size);

            return new Mesh
            {
                vertices = vertices,
                triangles = triangleStack.ToArray(),
                normals = normals,
                uv = uvs
            };
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
        }
    }
}
