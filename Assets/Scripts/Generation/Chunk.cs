using UnityEngine;



namespace Generation
{
    public class Chunk : MonoBehaviour
    {
        [SerializeField] private Vector2Int _location;

        public BlockId[,,] blocks { get; set; }
        public int size { get; set; }
        public int height { get; set; }
        public Vector2Int location
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
    }
}
