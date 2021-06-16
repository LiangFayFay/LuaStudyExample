using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace CSharp
{
    class CubeArray
    {
        public GameObject Cube;
        public Vector3 Pos;
    }

    public class CubeManager : MonoBehaviour
    {
        public GameObject cube;
        public Transform trans;
        private CubeArray[] cubeArray = new CubeArray[27];
        private int len = 0;
        private bool isKeyDown = false;
        private bool doRotate = false;
        private bool doAutoRotate = false;
        private CubeArray[] rotateArray = new CubeArray[9];
        private Vector3 rotateVector;
        private const float Angle = 90f;
        private const float Speed = 0.01f;
        private float curAngle = 0f;
        private bool success = false;
        private List<Vector3> orderList = new List<Vector3>();

        private readonly Vector3[] keyCodes =
        {
            new Vector3(0, 1, 0), //KeyCode.U,
            new Vector3(0, 0, -1), //KeyCode.F,
            new Vector3(1, 0, 0), //KeyCode.R,
            new Vector3(0, 0, 1), //KeyCode.B,
            new Vector3(-1, 0, 0), //KeyCode.L,
            new Vector3(0, -1, 0) //KeyCode.D,
        };

        private readonly Vector3[] crossCubeList =
        {
            new Vector3(0, -1, -1), //红白
            new Vector3(1, -1, 0), //绿白
            new Vector3(0, -1, 1), //橙白
            new Vector3(-1, -1, 0) //蓝白
        };

        private readonly Vector3[] firstLayerList =
        {
            new Vector3(1, -1, -1), //红白绿
            new Vector3(1, -1, 1), //绿白橙
            new Vector3(-1, -1, 1), //橙白蓝
            new Vector3(-1, -1, -1), //蓝白红
        };

        private readonly Vector3[] secondLayerList =
        {
            new Vector3(1, 0, -1), //红绿
            new Vector3(1, 0, 1), //绿橙
            new Vector3(-1, 0, 1), //橙蓝
            new Vector3(-1, 0, -1), //蓝红
        };

        private int crossIndex = 0;
        private CFOP progress = CFOP.None;

        private void Awake()
        {
            var index = 0;
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    for (var z = -1; z <= 1; z++)
                    {
                        var newCube = Instantiate(cube, trans);
                        len = (int) cube.transform.localScale.x;
                        var pos = new Vector3(x, y, z) * len;
                        newCube.transform.localPosition = pos;
                        newCube.SetActive(true);
                        newCube.GetComponent<Cube>().Position = new Vector3(x, y, z);
                        newCube.GetComponent<Cube>().Rotate = new Vector3(0, 0, 0);
                        cubeArray[index] = new CubeArray {Cube = newCube, Pos = new Vector3(x, y, z)};
                        index++;
                    }
                }
            }
        }

        private bool CheckSuccess()
        {
            var index = 0;
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    for (var z = -1; z <= 1; z++)
                    {
                        if (x == 0 && y == 0 || x == 0 && z == 0 || x == 0 && z == 0)
                        {
                            index++;
                            continue;
                        }

                        var myCube = cubeArray[index].Cube;
                        var pos = new Vector3(x, y, z) * len;
                        var rotate = myCube.transform.localRotation;
                        var vector = myCube.transform.localPosition;
                        var checkPos = Mathf.Abs(vector.x - pos.x) < 10 &&
                                       Mathf.Abs(vector.y - pos.y) < 10 &&
                                       Mathf.Abs(vector.z - pos.z) < 10;
                        var checkRotate = Mathf.Abs(rotate.x) < 10 &&
                                          Mathf.Abs(rotate.y) < 10 &&
                                          Mathf.Abs(rotate.z) < 10;
                        if (!checkPos || !checkRotate)
                        {
                            return false;
                        }

                        index++;
                    }
                }
            }

            return true;
        }

        IEnumerator RandomRotate(int times)
        {
            var rand = new Random();
            for (var i = 1; i <= times; i++)
            {
                if (success)
                {
                    yield break;
                }

                var vector = keyCodes[rand.Next(0, 5)];
                RotationLocal(vector.x, vector.y, vector.z);
                // yield return 0;
                yield return new WaitForSeconds(0.02f);
            }

            isKeyDown = false;
        }

        private void RotationLocal(float x, float y, float z)
        {
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i];
                if ((Mathf.Abs(x) > 0 && Math.Abs(myCube.Cube.transform.localPosition.x * x - len) < 10f) ||
                    (Mathf.Abs(y) > 0 && Math.Abs(myCube.Cube.transform.localPosition.y * y - len) < 10f) ||
                    (Mathf.Abs(z) > 0 && Math.Abs(myCube.Cube.transform.localPosition.z * z - len) < 10f))
                {
                    myCube.Cube.transform.RotateAround(
                        new Vector3(x, y, z) * len,
                        new Vector3(x, y, z),
                        90f);
                    if (CheckSuccess())
                    {
                        success = true;
                    }
                }
            }
        }

        private void Rotation(float x, float y, float z)
        {
            var index = 0;
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i];
                if ((Mathf.Abs(x) > 0 && Math.Abs(myCube.Cube.transform.localPosition.x * x - len) < 10f) ||
                    (Mathf.Abs(y) > 0 && Math.Abs(myCube.Cube.transform.localPosition.y * y - len) < 10f) ||
                    (Mathf.Abs(z) > 0 && Math.Abs(myCube.Cube.transform.localPosition.z * z - len) < 10f))
                {
                    rotateArray[index] = myCube;
                    index++;
                }
            }

            rotateVector = new Vector3(x, y, z);
            doRotate = true;
        }

        private void Update()
        {
            if (doRotate)
            {
                var angle = (Angle / Speed) * Time.deltaTime;
                curAngle += angle;
                if (curAngle >= Angle)
                {
                    angle -= curAngle - Angle;
                }

                for (var i = 0; i <= 8; i++)
                {
                    var myCube = rotateArray[i];
                    myCube.Cube.transform.RotateAround(
                        rotateVector * len,
                        new Vector3(
                            rotateVector.x,
                            rotateVector.y,
                            rotateVector.z),
                        angle);
                }

                if (curAngle >= Angle)
                {
                    doRotate = false;
                    isKeyDown = false;
                    curAngle = 0f;
                }
            }

            if (doAutoRotate)
            {
                var angle = (Angle / Speed) * Time.deltaTime;
                curAngle += angle;
                if (curAngle >= Angle)
                {
                    angle -= curAngle - Angle;
                }

                for (var i = 0; i <= 8; i++)
                {
                    var myCube = rotateArray[i];
                    myCube.Cube.transform.RotateAround(
                        rotateVector * len,
                        new Vector3(
                            rotateVector.x,
                            rotateVector.y,
                            rotateVector.z),
                        angle);
                }

                if (curAngle >= Angle)
                {
                    orderList.RemoveAt(0);
                    doAutoRotate = false;
                    curAngle = 0f;
                    if (orderList.Count > 0)
                    {
                        StartAutoRotate();
                    }
                    else
                    {
                        isKeyDown = false;
                        if (progress == CFOP.None)
                        {
                            doAutoRotate = false;
                            return;
                        }
                        else if (progress == CFOP.Cross)
                        {
                            Cross();
                        }
                        else if (progress == CFOP.FirstLayer)
                        {
                            FirstLayer();
                        }
                        else if (progress == CFOP.SecondLayer)
                        {
                            SecondLayer();
                        }
                        else if (progress == CFOP.ThirdLayer)
                        {
                            ThirdLayer();
                        }
                    }
                }
            }

            if (isKeyDown) return;
            if (Input.GetKeyDown(KeyCode.U))
            {
                isKeyDown = true;
                Rotation(0, 1, 0);
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                isKeyDown = true;
                Rotation(1, 0, 0);
                return;
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                isKeyDown = true;
                Rotation(-1, 0, 0);
                return;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                isKeyDown = true;
                Rotation(0, -1, 0);
                return;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                isKeyDown = true;
                Rotation(0, 0, -1);
                return;
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                isKeyDown = true;
                Rotation(0, 0, 1);
                return;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                isKeyDown = true;
                success = false;
                StartCoroutine(RandomRotate(100));
                return;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                progress = CFOP.Cross;
                Cross();
            }
        }

        private int ChangeKey(int key)
        {
            if (key > 4)
            {
                return key - 4;
            }

            return key;
        }

        //十字
        private void Cross()
        {
            if (crossIndex > 3)
            {
                crossIndex = 0;
                progress = CFOP.FirstLayer;
                FirstLayer();
                return;
            }

            var curCube = crossCubeList[crossIndex];
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i].Cube;
                var pos = cubeArray[i].Pos;
                var curPos = ChangePos(myCube.transform.localPosition);
                if (pos == curCube)
                {
                    if (curPos == pos) //前下
                    {
                        if (ChangeRotate(myCube.transform.rotation) != new Vector3(0, 0, 0))
                        {
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        }
                        else
                        {
                            crossIndex++;
                            Cross();
                            return;
                        }
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, -1, 0))) //左下
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, -1, 0))) //右下
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(0, -1, 1))) //后下
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 0, -1))) //中左前
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 0, 1))) //中左后
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 0, -1))) //中右前
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 0, 1))) //中右后
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(0, 1, -1))) //上前
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(0, 1, 1))) //上后
                    {
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 1, 0))) //上左
                    {
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 1, 0))) //上右
                    {
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                    }

                    StartAutoRotate();
                    break;
                }
            }
        }

        //底层
        private void FirstLayer()
        {
            if (crossIndex > 3)
            {
                crossIndex = 0;
                progress = CFOP.SecondLayer;
                SecondLayer();
                return;
            }

            var curCube = firstLayerList[crossIndex];
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i].Cube;
                var pos = cubeArray[i].Pos;
                var curPos = ChangePos(myCube.transform.localPosition);
                if (pos == curCube)
                {
                    if (curPos == pos) //右下前
                    {
                        if (ChangeRotate(myCube.transform.rotation) != new Vector3(0, 0, 0))
                        {
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        }
                        else
                        {
                            crossIndex++;
                            FirstLayer();
                            return;
                        }
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, -1, -1))) //左前下
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, -1, 1))) //左后下
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);

                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, -1, 1))) //右后下
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);

                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 1, -1))) //左上前
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 1, 1))) //左上后
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 1, 1))) //右上后
                    {
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 1, -1))) //右上前
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                    }

                    StartAutoRotate();
                    break;
                }
            }
        }

        //中层
        private void SecondLayer()
        {
            if (crossIndex > 3)
            {
                crossIndex = 0;
                progress = CFOP.ThirdLayer;
                ThirdLayerExp1();
                orderList.Add(keyCodes[(int) RotateKey.U]);
                ThirdLayerExp1();
                orderList.Add(keyCodes[(int) RotateKey.U]);
                ThirdLayerExp1();
                orderList.Add(keyCodes[(int) RotateKey.U]);
                ThirdLayerExp1();
                orderList.Add(keyCodes[(int) RotateKey.U]);
                ThirdLayerExp1();
                orderList.Add(keyCodes[(int) RotateKey.U]);
                StartAutoRotate();
                return;
            }

            var curCube = secondLayerList[crossIndex];
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i].Cube;
                var pos = cubeArray[i].Pos;
                var curPos = ChangePos(myCube.transform.localPosition);
                if (pos == curCube)
                {
                    if (curPos == pos) //中右前
                    {
                        if (ChangeRotate(myCube.transform.rotation) != new Vector3(0, 0, 0))
                        {
                            SecondLayerExp1();
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            SecondLayerExp1();
                        }
                        else
                        {
                            crossIndex++;
                            SecondLayer();
                            return;
                        }
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 0, -1))) //中左前
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        SecondLayerExp2();
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 0, 1))) //中左后
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        SecondLayerExp1();
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 0, 1))) //中右后
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        SecondLayerExp2();
                    }
                    else if (curPos == ChangeAngle(new Vector3(0, 1, -1))) //上前
                    {
                        SecondLayerExp1();
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 1, 0))) //上右
                    {
                        SecondLayerExp2();
                    }
                    else if (curPos == ChangeAngle(new Vector3(0, 1, 1))) //上后
                    {
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        SecondLayerExp2();
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 1, 0))) //上左
                    {
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        SecondLayerExp2();
                    }

                    StartAutoRotate();
                    break;
                }
            }
        }

        private void SecondLayerExp1()
        {
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
        }

        private void SecondLayerExp2()
        {
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
        }
        private void ThirdLayerExp1()
        {
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + crossIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + crossIndex)]);
        }

        //顶层
        private void ThirdLayer()
        {
             
        }

        private void StartAutoRotate()
        {
            isKeyDown = true;
            AutoRotation(orderList[0]);
        }

        private void AutoRotation(Vector3 vector)
        {
            var x = vector.x;
            var y = vector.y;
            var z = vector.z;
            var index = 0;
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i];
                if ((Mathf.Abs(x) > 0 && Math.Abs(myCube.Cube.transform.localPosition.x * x - len) < 10f) ||
                    (Mathf.Abs(y) > 0 && Math.Abs(myCube.Cube.transform.localPosition.y * y - len) < 10f) ||
                    (Mathf.Abs(z) > 0 && Math.Abs(myCube.Cube.transform.localPosition.z * z - len) < 10f))
                {
                    rotateArray[index] = myCube;
                    index++;
                }
            }

            rotateVector = vector;
            doAutoRotate = true;
        }

        private Vector3 ChangePos(Vector3 position)
        {
            var pos = position / len;
            pos.x = Convert.ToInt32(pos.x);
            pos.y = Convert.ToInt32(pos.y);
            pos.z = Convert.ToInt32(pos.z);

            return pos;
        }

        private Vector3 ChangeRotate(Quaternion rotation)
        {
            var rotate = rotation.eulerAngles / Angle;
            rotate.x = Convert.ToInt32(rotate.x);
            rotate.y = Convert.ToInt32(rotate.y);
            rotate.z = Convert.ToInt32(rotate.z);

            return rotate;
        }

        private Vector3 ChangeAngle(Vector3 vector)
        {
            var result = new Vector3();
            var angles = crossIndex * Angle * Mathf.PI / 180;
            // x1 = x0*cosB - y0*sinB
            // y1 = y0*cosB + x0*sinB
            result.x = Convert.ToInt32(vector.x * Mathf.Cos(angles) - vector.z * Mathf.Sin(angles));
            result.y = vector.y;
            result.z = Convert.ToInt32(vector.z * Mathf.Cos(angles) + vector.x * Mathf.Sin(angles));

            return result;
        }
    }
}