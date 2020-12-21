// using System;
// using UnityEngine;
// using UnityEngine.UI;
// using XLua;
//
// namespace DefaultNamespace
// {
//     [LuaCallCSharp]
//     public class NormalTip : MonoBehaviour
//     {
//         [SerializeField] private TextAsset luaScript;
//
//         private void Awake()
//         {
//             var luaEnv = LuaEnvManager.Instance.GetEnv();
//             var luaTable = luaEnv.NewTable();
//             var metaTable = luaEnv.NewTable();
//             metaTable.Set("__index",luaEnv.Global);
//             luaTable.SetMetaTable(metaTable);
//             metaTable.Dispose();
//             luaTable.Set("tip",this);
//             luaTable.Set("tipText",GetComponentInChildren<Text>());
//             
//             luaEnv.DoString(luaScript.text, "NormalTip", luaTable);
//         }
//     }
// }