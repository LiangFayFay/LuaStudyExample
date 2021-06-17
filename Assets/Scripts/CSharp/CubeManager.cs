using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;
using UnityEngine.UI;
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
        public Button disrupt;
        public Button restore;
        public Button confirm;
        public Transform dialog;

        private const float Angle = 90f;
        private const float Speed = 0.02f;

        private int len = 0;
        private int direction = 1;
        private bool isKeyDown = false;
        private bool doRotate = false;
        private bool doAutoRotate = false;
        private bool isBusy = false;
        private Vector3 rotateVector;
        private float curAngle = 0f;
        private bool success = false;
        private int colorIndex = 0; //0-红；1-绿；2-橙；3-蓝
        private CFOP cfop = CFOP.None;
        private bool thirdCrossFinish = false;
        private bool thirdCornerFinish = false;
        private bool thirdColorFinish = false;
        private bool thirdPosFinish = false;
        private bool thirdCrossPosFinish = false;
        private bool thirdCornerPosFinish = false;

        private CubeArray[] cubeArray = new CubeArray[27];
        private CubeArray[] rotateArray = new CubeArray[9];
        private List<Vector3> orderList = new List<Vector3>();

        private readonly Vector3[] keyCodes =
        {
            new Vector3(0, 1, 0), //KeyCode.U,
            new Vector3(0, 0, -1), //KeyCode.F,
            new Vector3(1, 0, 0), //KeyCode.R,
            new Vector3(0, 0, 1), //KeyCode.B,
            new Vector3(-1, 0, 0), //KeyCode.L,
            new Vector3(0, -1, 0), //KeyCode.D,

            //6-9 废弃
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,

            new Vector3(0, 1, 0) * 2, //KeyCode.U2,
            new Vector3(0, 0, -1) * 2, //KeyCode.F2,
            new Vector3(1, 0, 0) * 2, //KeyCode.R2,
            new Vector3(0, 0, 1) * 2, //KeyCode.B2,
            new Vector3(-1, 0, 0) * 2, //KeyCode.L2,
            new Vector3(0, -1, 0) * 2 //KeyCode.D2,
        };

        private readonly Vector3[] firstLayerCrossList =
        {
            new Vector3(0, -1, -1), //红白
            new Vector3(1, -1, 0), //绿白
            new Vector3(0, -1, 1), //橙白
            new Vector3(-1, -1, 0) //蓝白
        };

        private readonly Vector3[] firstLayerCornerList =
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

        private readonly Vector3[] thirdLayerCrossList =
        {
            new Vector3(0, 1, -1), //红黄
            new Vector3(1, 1, 0), //绿黄
            new Vector3(0, 1, 1), //橙黄
            new Vector3(-1, 1, 0), //蓝黄
        };

        private readonly Vector3[] thirdLayerCornerList =
        {
            new Vector3(1, 1, -1), //红绿黄
            new Vector3(1, 1, 1), //绿橙黄
            new Vector3(-1, 1, 1), //橙蓝黄
            new Vector3(-1, 1, -1), //蓝红黄
        };

        private void Init()
        {
            isKeyDown = false;
            doRotate = false;
            doAutoRotate = false;
            isBusy = false;
            curAngle = 0f;
            success = false;
            colorIndex = 0; //0-红；1-绿；2-橙；3-蓝
            cfop = CFOP.None;
            thirdCrossFinish = false;
            thirdCornerFinish = false;
            thirdColorFinish = false;
            thirdPosFinish = false;
            thirdCrossPosFinish = false;
            thirdCornerPosFinish = false;
            orderList.Clear();
        }

        private void Awake()
        {
            confirm.onClick.AddListener(ConfirmClick);
            disrupt.onClick.AddListener(DisruptClick);
            restore.onClick.AddListener(RestoreClick);

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
                        var rotate = ChangeRotate(myCube.transform.localRotation);
                        var vector = ChangePos(myCube.transform.localPosition);
                        var checkPos = vector == pos;
                        var checkRotate = rotate == new Vector3(0, 0, 0);
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

        private IEnumerator RandomRotate(int times)
        {
            var rand = new Random();
            for (var i = 1; i <= times; i++)
            {
                if (success)
                {
                    yield break;
                }

                var vector = keyCodes[rand.Next(0, 6)];
                RotationLocal(vector.x, vector.y, vector.z);
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
                            rotateVector.x * direction,
                            rotateVector.y * direction,
                            rotateVector.z * direction),
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
                            rotateVector.x * direction,
                            rotateVector.y * direction,
                            rotateVector.z * direction),
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
                        isBusy = false;
                    }
                }
            }

            if (!isBusy)
            {
                if (cfop == CFOP.Cross)
                {
                    Cross();
                }
                else if (cfop == CFOP.FirstLayer)
                {
                    FirstLayer();
                }
                else if (cfop == CFOP.SecondLayer)
                {
                    SecondLayer();
                }
                else if (cfop == CFOP.ThirdLayer)
                {
                    if (!thirdColorFinish)
                    {
                        ThirdLayerColor();
                    }
                    else if (!thirdPosFinish)
                    {
                        ThirdLayerPos();
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
                DisruptClick();
                return;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                cfop = CFOP.Cross;
                return;
            }
        }

        private void DisruptClick()
        {
            Init();
            StartCoroutine(RandomRotate(50));
        }

        private void ConfirmClick()
        {
            HideDialog();
        }

        private void RestoreClick()
        {
            if (cfop == CFOP.None) cfop = CFOP.Cross;
        }

        private void ShowDialog()
        {
            dialog.localScale = Vector3.one;   
        }

        private void HideDialog()
        {
            dialog.localScale = Vector3.zero;
        }

        private int ChangeKey(int key)
        {
            //顺时针处理
            if (key > 4 && key < 8)
            {
                return key - 4;
            }

            //逆时针处理
            if (key > 14 && key < 18)
            {
                return key - 4;
            }

            return key;
        }

        //十字
        private void Cross()
        {
            isBusy = true;
            if (colorIndex > 3)
            {
                colorIndex = 0;
                cfop = CFOP.FirstLayer;
                isBusy = false;
                return;
            }

            var curCube = firstLayerCrossList[colorIndex];
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i].Cube;
                var pos = cubeArray[i].Pos;
                var curPos = ChangePos(myCube.transform.localPosition);
                if (pos == curCube)
                {
                    if (curPos == pos) //前下
                    {
                        if (ChangeRotate(myCube.transform.localRotation) != new Vector3(0, 0, 0))
                        {
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F2 + colorIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                        }
                        else
                        {
                            colorIndex++;
                            isBusy = false;
                            return;
                        }
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, -1, 0))) //左下
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U2]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, -1, 0))) //右下
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(0, -1, 1))) //后下
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 0, -1))) //中左前
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F2 + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 0, 1))) //中左后
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U2]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L2 + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 0, -1))) //中右前
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 0, 1))) //中右后
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(0, 1, -1))) //上前
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(0, 1, 1))) //上后
                    {
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 1, 0))) //上左
                    {
                        orderList.Add(keyCodes[(int) RotateKey.U2]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 1, 0))) //上右
                    {
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                    }

                    StartAutoRotate();
                    break;
                }
            }
        }

        //底层
        private void FirstLayer()
        {
            isBusy = true;
            if (colorIndex > 3)
            {
                colorIndex = 0;
                cfop = CFOP.SecondLayer;
                isBusy = false;
                return;
            }

            var curCube = firstLayerCornerList[colorIndex];
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i].Cube;
                var pos = cubeArray[i].Pos;
                var curPos = ChangePos(myCube.transform.localPosition);
                if (pos == curCube)
                {
                    if (curPos == pos) //右下前
                    {
                        if (ChangeRotate(myCube.transform.localRotation) != new Vector3(0, 0, 0))
                        {
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                            orderList.Add(keyCodes[(int) RotateKey.U2]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                            orderList.Add(keyCodes[(int) RotateKey.U2]);
                            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
                        }
                        else
                        {
                            colorIndex++;
                            isBusy = false;
                            return;
                        }
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, -1, -1))) //左前下
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F2 + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, -1, 1))) //左后下
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B2 + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + colorIndex)]);

                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, -1, 1))) //右后下
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B2 + colorIndex)]);

                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 1, -1))) //左上前
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U2]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 1, 1))) //左上后
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 1, 1))) //右上后
                    {
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U2]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 1, -1))) //右上前
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
                    }

                    StartAutoRotate();
                    break;
                }
            }
        }

        //中层
        private void SecondLayer()
        {
            isBusy = true;
            if (colorIndex > 3)
            {
                colorIndex = 0;
                cfop = CFOP.ThirdLayer;
                isBusy = false;
                return;
            }

            var curCube = secondLayerList[colorIndex];
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i].Cube;
                var pos = cubeArray[i].Pos;
                var curPos = ChangePos(myCube.transform.localPosition);
                if (pos == curCube)
                {
                    if (curPos == pos) //中右前
                    {
                        if (ChangeRotate(myCube.transform.localRotation) != new Vector3(0, 0, 0))
                        {
                            SecondLayerExp1();
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                            SecondLayerExp1();
                        }
                        else
                        {
                            colorIndex++;
                            isBusy = false;
                            return;
                        }
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 0, -1))) //中左前
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U2]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.F2 + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U2]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L2 + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + colorIndex)]);
                        SecondLayerExp2();
                    }
                    else if (curPos == ChangeAngle(new Vector3(-1, 0, 1))) //中左后
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U2]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.L2 + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U2]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B2 + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + colorIndex)]);
                        SecondLayerExp1();
                    }
                    else if (curPos == ChangeAngle(new Vector3(1, 0, 1))) //中右后
                    {
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U2]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.B2 + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U2]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
                        orderList.Add(keyCodes[(int) RotateKey.U]);
                        orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
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

        //顶层
        private void ThirdLayerColor()
        {
            isBusy = true;

            void Cb()
            {
                StartCoroutine(ThirdLayerCornerChecker(() =>
                {
                    thirdColorFinish = true;
                    colorIndex = 0;
                    isBusy = false;
                }));
            }

            StartCoroutine(ThirdLayerCrossChecker(Cb));
        }

        private void ThirdLayerPos()
        {
            isBusy = true;
            if (!thirdCrossPosFinish)
            {
                if (colorIndex == 3)
                {
                    thirdCrossPosFinish = true;
                    colorIndex = 0;
                    isBusy = false;
                    return;
                }

                ThirdLayerCrossPos();
            }
            else if (!thirdCornerPosFinish)
            {
                ThirdLayerCornerPos();
            }
        }

        private void ThirdLayerCrossPos()
        {
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i].Cube;
                var pos = cubeArray[i].Pos;
                var curPos = ChangePos(myCube.transform.localPosition);

                if (pos == thirdLayerCrossList[colorIndex])
                {
                    if (curPos != pos)
                    {
                        if (colorIndex == 0)
                        {
                            orderList.Add(keyCodes[(int) RotateKey.U]);
                        }
                        else if (colorIndex == 1)
                        {
                            ThirdLayerExp3(2);
                        }
                        else if (colorIndex == 2)
                        {
                            ThirdLayerExp4(2);
                        }

                        break;
                    }

                    colorIndex++;
                    isBusy = false;
                    return;
                }
            }

            StartAutoRotate();
        }

        private void ThirdLayerCornerPos()
        {
            var correctIndex = 0;
            var thirdCornerCount = 0;
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i].Cube;
                var pos = cubeArray[i].Pos;
                var curPos = ChangePos(myCube.transform.localPosition);
                for (var j = 0; j <= 3; j++)
                {
                    var curCube = thirdLayerCornerList[j];
                    if (curPos == curCube && curPos == pos)
                    {
                        correctIndex = j;
                        thirdCornerCount++;
                        break;
                    }
                }
            }

            if (thirdCornerCount == 0)
            {
                ThirdLayerExp5(0);
            }
            else if (thirdCornerCount == 1)
            {
                var index = (correctIndex + 2) % 4;
                ThirdLayerExp5(index);
            }
            else if (thirdCornerCount == 4)
            {
                Complete();
                return;
            }

            StartAutoRotate();
        }

        private void Complete()
        {
            // doAutoRotate = false;
            cfop = CFOP.None;
            isBusy = false;
            thirdPosFinish = true;
            thirdCornerPosFinish = true;
            ShowDialog();
        }

        private bool CheckThirdCross()
        {
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i].Cube;
                var pos = cubeArray[i].Pos;
                var rotate = ChangeRotate(myCube.transform.localRotation);
                for (var j = 0; j <= 3; j++)
                {
                    var curCube = thirdLayerCrossList[j];
                    if (pos == curCube)
                    {
                        if (rotate.x == 0 && rotate.z == 0)
                        {
                            break;
                        }

                        return false;
                    }
                }
            }

            thirdCrossFinish = true;
            return true;
        }

        private bool CheckThirdCorner()
        {
            for (var i = 0; i <= 26; i++)
            {
                var myCube = cubeArray[i].Cube;
                var rotate = ChangeRotate(myCube.transform.localRotation);
                var curPos = ChangePos(myCube.transform.localPosition);
                for (var j = 0; j <= 3; j++)
                {
                    if (curPos == new Vector3(1, 1, -1))
                    {
                        if (rotate.x == 0 && rotate.z == 0)
                        {
                            return true;
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        private void StartAutoRotate()
        {
            isKeyDown = true;
            AutoRotation(orderList[0]);
        }

        private void AutoRotation(Vector3 vector)
        {
            direction = 1;
            var x = Convert.ToInt32(vector.x);
            var y = Convert.ToInt32(vector.y);
            var z = Convert.ToInt32(vector.z);
            if (Mathf.Abs(x) == 2 || Mathf.Abs(y) == 2 || Mathf.Abs(z) == 2)
            {
                direction = -1;
                vector /= 2;
                x /= 2;
                y /= 2;
                z /= 2;
            }

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
            doAutoRotate = true;
        }

        private IEnumerator ThirdLayerCrossChecker(Action cb)
        {
            while (!thirdCrossFinish)
            {
                yield return new WaitForSeconds(0.1f);
                if (isKeyDown || CheckThirdCross()) continue;

                for (var i = 0; i <= new Random().Next(0, 2); i++)
                {
                    orderList.Add(keyCodes[(int) RotateKey.U]);
                }

                ThirdLayerExp1();
                StartAutoRotate();
            }

            cb();
        }

        private IEnumerator ThirdLayerCornerChecker(Action cb)
        {
            while (!thirdCornerFinish)
            {
                yield return new WaitForSeconds(0.1f);
                if (isKeyDown) continue;

                if (!CheckThirdCorner())
                {
                    ThirdLayerExp2();
                }
                else
                {
                    colorIndex++;
                    if (colorIndex == 4)
                    {
                        thirdCornerFinish = true;
                    }

                    orderList.Add(keyCodes[(int) RotateKey.U]);
                }

                StartAutoRotate();
            }

            cb();
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
            var angles = colorIndex * Angle * Mathf.PI / 180;
            // x1 = x0*cosB - y0*sinB
            // y1 = y0*cosB + x0*sinB
            result.x = Convert.ToInt32(vector.x * Mathf.Cos(angles) - vector.z * Mathf.Sin(angles));
            result.y = vector.y;
            result.z = Convert.ToInt32(vector.z * Mathf.Cos(angles) + vector.x * Mathf.Sin(angles));

            return result;
        }

        private void SecondLayerExp1()
        {
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U2]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U2]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F2 + colorIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
        }

        private void SecondLayerExp2()
        {
            orderList.Add(keyCodes[(int) RotateKey.U2]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F2 + colorIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + colorIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + colorIndex)]);
            orderList.Add(keyCodes[(int) RotateKey.U2]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + colorIndex)]);
        }

        private void ThirdLayerExp1()
        {
            orderList.Add(keyCodes[(int) RotateKey.F]);
            orderList.Add(keyCodes[(int) RotateKey.R]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.R2]);
            orderList.Add(keyCodes[(int) RotateKey.U2]);
            orderList.Add(keyCodes[(int) RotateKey.F2]);
        }

        private void ThirdLayerExp2()
        {
            orderList.Add(keyCodes[(int) RotateKey.R2]);
            orderList.Add(keyCodes[(int) RotateKey.D2]);
            orderList.Add(keyCodes[(int) RotateKey.R]);
            orderList.Add(keyCodes[(int) RotateKey.D]);
        }

        private void ThirdLayerExp3(int index)
        {
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
            orderList.Add(keyCodes[(int) RotateKey.U2]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
            orderList.Add(keyCodes[(int) RotateKey.U2]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + index)]);
            orderList.Add(keyCodes[(int) RotateKey.U2]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
        }

        private void ThirdLayerExp4(int index)
        {
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + index)]);
            orderList.Add(keyCodes[(int) RotateKey.U2]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.L2 + index)]);
            orderList.Add(keyCodes[(int) RotateKey.U]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + index)]);
            orderList.Add(keyCodes[(int) RotateKey.U2]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.L + index)]);
        }

        private void ThirdLayerExp5(int index)
        {
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.B2 + index)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + index)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + index)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R2 + index)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.B + index)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + index)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.F + index)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
            orderList.Add(keyCodes[ChangeKey((int) RotateKey.R + index)]);
        }
    }
}