using JUtils.Attributes;
using UnityEngine;



namespace Generation
{
    [CreateAssetMenu]
    public class Block : ScriptableObject
    {
        [SerializeField] private BlockData _blockData = new ()
        {
            top      = new Vector2(-1, -1),
            bottom   = new Vector2(-1, -1),
            forward  = new Vector2(-1, -1),
            backward = new Vector2(-1, -1),
            right    = new Vector2(-1, -1),
            left     = new Vector2(-1, -1),
        };

        public BlockData data => _blockData;
    }

    
    
    [System.Serializable]
    public struct BlockData
    {
        public BlockId id;
        public Vector2 default_texture;
        public Vector2 top;
        public Vector2 bottom;
        public Vector2 forward;
        public Vector2 backward;
        public Vector2 right;
        public Vector2 left;
    }
    
    

    [System.Serializable]
    public struct UvSidePositions
    {
        public Optional<Vector2> top;
        public Optional<Vector2> bottom;
        public Optional<Vector2> forward;
        public Optional<Vector2> backward;
        public Optional<Vector2> right;
        public Optional<Vector2> left;
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
