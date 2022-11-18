using JUtils.Attributes;
using UnityEngine;



namespace Generation
{
    [CreateAssetMenu]
    public class Block : ScriptableObject
    {
        public BlockId id;
        public Vector2 defaultTexture;

        public Optional<BlockUvPosition> blocks;
    }


    [System.Serializable]
    public class BlockUvPosition
    {
        public Optional<BlockUvPosition> Top;
        public Optional<BlockUvPosition> Bottom;
        public Optional<BlockUvPosition> Forward;
        public Optional<BlockUvPosition> Backward;
        public Optional<BlockUvPosition> Right;
        public Optional<BlockUvPosition> Left;
    }
    
    
    public enum BlockId
    {
        Air,
        Grass,
        Dirt,
        Stone
    }
}
