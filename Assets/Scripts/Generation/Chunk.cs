using System;
using System.Collections;
using System.Threading;
using Generation.Generators;
using Generation.Processors;
using JUtils.Attributes;
using JUtils.Extensions;
using Unity.Mathematics;
using UnityEngine;



namespace Generation
{
    public class Chunk : MonoBehaviour
    {
        [SerializeField] private int2 _location;

        public BaseGenerator generator { get; set; }
        public BlockId[,,] blocks { get; set; }
        public int size { get; set; }
        public int height { get; set; }
        
        public int2 location
        {
            get => _location;
            set {
                _location = value;
                transform.position = new Vector3(
                    _location.x * size,
                    0,
                    _location.y * size
                );
            }
        }


        private void Start()
        {
            RegenerateMesh();
        }


        private Coroutine _meshGenerator;
        [Button]
        private void RegenerateMesh()
        {
            IEnumerator Enumerator()
            {
                MeshData data = generator.meshCreator.Process(blocks, size, height);
                
                //  Reducing mesh

                MeshReduceObject obj = new () { data = data, creator = generator.meshCreator};
                Thread thread = new (obj.ReduceMesh);
                thread.Start();
                yield return new WaitWhile(() => thread.IsAlive);

                //  applying
                
                GetComponent<MeshFilter>().mesh = new Mesh
                {
                    vertices = obj.data.vertices, uv = obj.data.uvs, normals = obj.data.normals, triangles = obj.data.indices
                };

                _meshGenerator = null;
            }

            if (_meshGenerator != null) StopCoroutine(_meshGenerator);
            _meshGenerator = StartCoroutine(Enumerator());
        }


        private void OnDestroy()
        {
            generator.Delete(location);
        }



        public struct MeshReduceObject
        {
            public MeshData data;
            public MeshCreator creator;
            
            public void ReduceMesh()
            {
                data = creator.ReduceMesh(data);
            }
        }
    }
}
