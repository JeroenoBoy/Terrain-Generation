using Unity.Mathematics;
using UnityEditor;
using UnityEngine;



namespace Generation.Generators.Helpers
{    [System.Serializable]
    public struct Octaves3d
    {
        public Octave3d[] octaves;


        public Octave3dSampler CreateSampler(System.Random random, int startX, int startY, int startZ)
            => new (random, octaves, startX, startY, startZ);


#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(Octaves))]
        public class OctavesEditor : PropertyDrawer
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("octaves"));
            }


            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.PropertyField(position, property.FindPropertyRelative("octaves"), label);
            }
        }
#endif
        
    }
    
    [System.Serializable]
    public struct Octave3d
    {
        public float scale;
        public float frequency;
        public float offsetX;
        public float offsetY;
        public float offsetZ;


        public float Sample(int x, int y, int z)
        {
            float cX = x * frequency;
            float cY = y * frequency;
            float cZ = z * frequency;
            
            return noise.cnoise(new float3(cX, cY, cZ)) * scale;
        }

        private static float Perlin3dFixed(float a, float b) => Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a, b));
    }



    public readonly struct Octave3dSampler
    {
        private readonly Octave3d[] _octaves;
        private readonly int3[]     _positions;


        public Octave3dSampler(System.Random random, Octaves3d octaves, int startX, int startY, int startZ)
            : this(random, octaves.octaves, startX, startY, startZ) {}
        
        public Octave3dSampler(System.Random random, Octave3d[] octaves, int startX, int startY, int startZ)
        {
            _octaves   = octaves;
            _positions = new int3[octaves.Length];
            
            for (int i = 0; i < octaves.Length; i++) {
                _positions[i] = new int3(
                    (int)(startX + _octaves[i].offsetX * random.NextDouble()),
                    (int)(startY + _octaves[i].offsetY * random.NextDouble()),
                    (int)(startZ + _octaves[i].offsetZ * random.NextDouble())
                );
            }
        }


        public float Sample(int x, int y, int z)
        {
            float height = 0;

            for (int i = 0; i < _octaves.Length; i++) {
                height += _octaves[i].Sample(x + _positions[i].x, y + _positions[i].y, z + _positions[i].z);
            }

            return height;
        }
    }
}
