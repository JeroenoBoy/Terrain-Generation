using JUtils.Attributes;
using Unity.Mathematics;
using UnityEngine;



namespace Generation
{
    [CreateAssetMenu]
    public class Block : ScriptableObject
    {
        [SerializeField] private BlockData _blockData = new ()
        {
            top      = new float2(-1, -1),
            bottom   = new float2(-1, -1),
            forward  = new float2(-1, -1),
            backward = new float2(-1, -1),
            right    = new float2(-1, -1),
            left     = new float2(-1, -1),
        };

        public BlockData data => _blockData;
    }

    
    
    [System.Serializable]
    public struct BlockData
    {
        public BlockId id;
        public float2 default_texture;
        public float2 top;
        public float2 bottom;
        public float2 forward;
        public float2 backward;
        public float2 right;
        public float2 left;
    }
    
    

    [System.Serializable]
    public struct UvSidePositions
    {
        public Optional<float2> top;
        public Optional<float2> bottom;
        public Optional<float2> forward;
        public Optional<float2> backward;
        public Optional<float2> right;
        public Optional<float2> left;
    }
    
    
    
    public enum BlockId
    {
        Air,
        Grass,
        Dirt,
        Stone,
        Snow
    }
}
