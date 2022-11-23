using System;
using System.Collections.Generic;
using System.Linq;
using Generation.Generators;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;



namespace Generation.Processors
{
    [System.Serializable]
    public class MeshCreator
    {
        private ComputeShaderData _computeShader;
        private BaseGenerator     _generator;
        

        public MeshCreator(BaseGenerator generator, ComputeShader baseShader)
        {
            _generator = generator;
            
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

            _computeShader = new ComputeShaderData(shader)
            {
                blocksBuffer = blocksBuffer,
                uvMapBuffer = uvMapBuffer,
                verticesBuffer = verticesBuffer,
                normalBuffer = normalBuffer,
                uvBuffer = uvBuffer,
                triangleBuffer = triangleBuffer,
            };
        }


        public MeshData Process(BlockId[,,] blocks, int chunkSize, int chunkHeight)
        {
            //  Creating buffers

            ComputeShaderData shader = _computeShader;

            shader.blocksBuffer.SetData(blocks);
            shader.uvMapBuffer.SetData(_generator.blocks.Select(b => b.data).ToArray());

            //  Running the shader

            shader.Dispatch(chunkSize, chunkHeight);

            //  Creating the mesh

            Vector3[] vertices  = new Vector3[shader.verticesBuffer.count];
            Vector3[] normals   = new Vector3[shader.normalBuffer.count];
            Vector2[] uvs       = new Vector2[shader.uvBuffer.count];
            int3[]    triangles = new    int3[shader.triangleBuffer.count];

            shader.verticesBuffer.GetData(vertices);
            shader.normalBuffer.GetData(normals);
            shader.uvBuffer.GetData(uvs);
            shader.triangleBuffer.GetData(triangles);

            return new MeshData
            {
                vertices = vertices, normals = normals, uvs = uvs, triangles = triangles
            };
        }
        

        public MeshData ReduceMesh(MeshData meshData)
        {
            int3[] triangles = meshData.triangles;
            Stack<int> triangleStack = new (triangles.Length*2);

            for (int i = 0; i < triangles.Length; i++) {
                int3 quad = triangles[i];

                if (quad.Equals(default)) break;

                triangleStack.Push(quad.x);
                triangleStack.Push(quad.y);
                triangleStack.Push(quad.z);
            }

            int size = triangleStack.Count / 6 * 4;
            Array.Resize(ref meshData.vertices, size);
            Array.Resize(ref meshData.normals, size);
            Array.Resize(ref meshData.uvs, size);

            meshData.triangles = null;
            meshData.indices = triangleStack.ToArray();

            return meshData;
        }


        public void Dispose()
        {
            _computeShader.Dispose();
        }



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
                Object.Destroy(_shader);
                blocksBuffer.Release();
                uvMapBuffer.Release();
                
                verticesBuffer.Release();
                normalBuffer.Release();
                uvBuffer.Release();
                triangleBuffer.Release();
            }
        }
    }
}
