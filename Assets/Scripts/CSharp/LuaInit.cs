using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

namespace DefaultNamespace
{
    [LuaCallCSharp]
    public class LuaInit : MonoBehaviour
    {
        [SerializeField] private TextAsset luaScript;
        [SerializeField] private MyStruct[] myStructs;
        [SerializeField] private MyStruct[] dialog;
        [SerializeField] private MyStruct[] theme;


        private void Awake()
        {
            var luaEnv = LuaEnvManager.Instance.GetEnv();
            luaEnv.AddLoader(LuaCodeLoader);
            var luaTable = luaEnv.NewTable();
            var metaTable = luaEnv.NewTable();
            metaTable.Set("__index", luaEnv.Global);
            luaTable.SetMetaTable(metaTable);
            metaTable.Dispose();

            foreach (var item in myStructs)
            {
                luaTable.Set(item.Name, item.Value);
            }

            foreach (var item in dialog)
            {
                luaTable.Set(item.Name, item.Value);
            }
            
            foreach (var item in theme)
            {
                luaTable.Set(item.Name, item.Value);
            }

            luaEnv.DoString(luaScript.text, "Error", luaTable);
        }

        private byte[] LuaCodeLoader(ref string filePath)
        {
            var relPath = filePath.Replace(".", "/");
            var s = File.ReadAllText(Application.dataPath + "/" + relPath + ".lua");
            return System.Text.Encoding.UTF8.GetBytes(s);
        }
    }
}