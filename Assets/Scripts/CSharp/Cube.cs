using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSharp
{
    public class Cube
    {
        private Block[] blocks = new Block[27];

        public Block[] Blocks
        {
            get => blocks;
            set => blocks = value;
        }

        private List<Vector3> stepList = new List<Vector3>();

        public List<Vector3> StepList
        {
            get => stepList;
            set => stepList = value;
        }

        public bool DoRotate(Vector3 vector)
        {
            stepList.Add(vector);
            var direction = 1;
            var x = Convert.ToInt32(vector.x);
            var y = Convert.ToInt32(vector.y);
            var z = Convert.ToInt32(vector.z);
            if (Mathf.Abs(x) == 2 || Mathf.Abs(y) == 2 || Mathf.Abs(z) == 2)
            {
                direction = -1;
                x /= 2;
                y /= 2;
                z /= 2;
            }

            for (var i = 0; i < 27; i++)
            {
                var curBlock = blocks[i];
                if (Mathf.Abs(x) > 0 && Math.Abs(curBlock.Position.x - x) < 0.1 ||
                    Mathf.Abs(y) > 0 && Math.Abs(curBlock.Position.y - y) < 0.1 ||
                    Mathf.Abs(z) > 0 && Math.Abs(curBlock.Position.y - z) < 0.1)
                {
                    curBlock.Position = ChangeAngle(curBlock.Position, vector, vector * direction);
                    curBlock.Rotate = ChangeRotate(curBlock.Rotate, vector, direction);
                }
            }

            for (var i = 0; i < 27; i++)
            {
                var curBlock = blocks[i];
                if (curBlock.Position != curBlock.InitPos || curBlock.Rotate != new Vector3(0, 0, 0))
                {
                    return false;
                }
            }

            return true;
        }

        private Vector3 ChangeAngle(Vector3 vector, Vector3 point, Vector3 axis)
        {
            Vector3 vector3 = Quaternion.AngleAxis(90, axis) * (vector - point);

            return point + vector3;
        }

        private Vector3 ChangeRotate(Vector3 vector, Vector3 point, int direction)
        {
            if (Mathf.Abs(point.x) > 0)
            {
                vector.x = Mathf.Abs(vector.x + 90 * direction) % 360;
            }

            if (Mathf.Abs(point.y) > 0)
            {
                vector.y = Mathf.Abs(vector.y + 90 * direction) % 360;
            }

            if (Mathf.Abs(point.z) > 0)
            {
                vector.z = Mathf.Abs(vector.z + 90 * direction) % 360;
            }

            return vector;
        }

        public Cube Clone()
        {
            Cube newCube = new Cube();
            for (int i = 0; i < 27; i++)
            {
                newCube.Blocks[i] = new Block
                {
                    InitPos = Blocks[i].InitPos,
                    Position = Blocks[i].Position,
                    Rotate = Blocks[i].Rotate
                };
            }

            foreach (var step in StepList)
            {
                newCube.StepList.Add(step);
            }

            return newCube;
        }
    }
}