using UnityEngine;

namespace CSharp
{
    public class Cube : MonoBehaviour
    {
        private Vector3 position;
        private Vector3 rotate;

        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        public Vector3 Rotate
        {
            get => rotate;
            set => rotate = value;
        }
    }
}