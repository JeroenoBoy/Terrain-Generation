using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = Unity.Mathematics.Random;



namespace Generation.Generators.Helpers
{
    [System.Serializable]
    public struct Octaves
    {
        public Octave[] octaves;

        

        public float[,] Calculate(int startX, int startZ, int size)
        {
            float[,] map = new float[size,size];
            Calculate(startX, startZ, ref map);
            return map;
        }
        

        public float[,] Calculate(Random randomizer, int startX, int startZ, int size)
        {
            float[,] map = new float[size,size];
            Calculate(randomizer, startX, startZ, size, ref map);
            return map;
        }


        public void Calculate(int startX, int startZ, ref float[,] map)
        {
            Calculate(new Random(0), startX, startZ, map.GetLength(0), ref map);
        }

        
        public void Calculate(Random random, int startX, int startZ, int size, ref float[,] map)
        {
            for (int i = 0; i < octaves.Length; i++)
            {
                octaves[i].Calculate(random, startX, startZ, size, ref map);
            }
        }
        


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
    public struct Octave
    {
        public float scale;
        public float frequency;
        public float offsetX;
        public float offsetY;
        public float offsetZ;


        public void Calculate(Random random, int startX, int startZ, int size, ref float[,] map)
        {
            int baseX = startX + (int)(offsetX * random.NextFloat());
            int baseZ = startZ + (int)(offsetZ * random.NextFloat());
            
            for (int x = 0; x < size; x++) {
                for (int z = 0; z < size; z++) {
                    map[x, z] += Sample(baseX + x, baseZ + z);
                }
            }
        }


        public float Sample(int x, int y)
        {
            float cX = x * frequency;
            float cY = y * frequency;
            return offsetY + noise.cnoise(new float2(cX, cY)) * scale;
        }
    }



    public readonly struct OctavesSampler
    {
        private readonly Octave[] _octaves;
        private readonly int2[]   _positions;
        
        public OctavesSampler(Random random, Octave[] octaves, int startX, int startZ)
        {
            _octaves   = octaves;
            _positions = new int2[octaves.Length];
            
            for (int i = 0; i < octaves.Length; i++) {
                _positions[i] = new int2(
                    (int)(startX + _octaves[i].offsetX * random.NextFloat()),
                    (int)(startZ + _octaves[i].offsetZ * random.NextFloat())
                );
            }
        }


        public float Sample(int x, int z)
        {
            float height = 0;

            for (int i = 0; i < _octaves.Length; i++) {
                height += _octaves[i].Sample(x + _positions[i].x, z + _positions[i].y);
            }

            return height;
        }
    }
}
