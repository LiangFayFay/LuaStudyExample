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

        private LuaEnv m_LuaEnv;
        static  readonly List<string> m_SearchRoots = new List<string>(4);
        static  readonly List<string> m_SearchSubDirs = new List<string>(4);
        static  readonly List<string> m_SearchExts = new List<string>(4);
        
        private LuaEnv luaEnv = LuaEnvManager.Instance.GetEnv();

        private void Awake()
        {
            InitLuaEnv();
            
            var luaTable = luaEnv.NewTable();
            var metaTable = luaEnv.NewTable();
            metaTable.Set("__index",luaEnv.Global);
            luaTable.SetMetaTable(metaTable);
            metaTable.Dispose();
            
            foreach (var item in myStructs)
            {
                luaTable.Set(item.Name,item.Value);
            }

            luaEnv.DoString(luaScript.text, "Error", luaTable);
        }
        
        public void InitLuaEnv()
        {
            m_LuaEnv = LuaEnvManager.Instance.GetEnv();
            m_SearchRoots.Clear();
            m_SearchSubDirs.Clear();
            m_SearchExts.Clear();

            m_SearchRoots.Add(Application.streamingAssetsPath);
            m_SearchSubDirs.Add("Scripts");
            m_SearchExts.Add(".lua");

            m_LuaEnv.AddLoader(LuaCodeLoader);
        }
        
        private byte[] LuaCodeLoader(ref string filePath)
        {
            var relPath = filePath.Replace(".", "/");

            for (int i = m_SearchRoots.Count - 1; i >= 0; i--)
            {
                var root = m_SearchRoots[i];
                for (int j = m_SearchSubDirs.Count - 1; j >= 0; j--)
                {
                    for (int k = m_SearchExts.Count - 1; k >= 0; k--)
                    {
                        filePath = m_SearchSubDirs[j] + "/" + relPath + m_SearchExts[k];
                        var luafilePath = root + "/" + filePath;

                        if (File.Exists(luafilePath))
                        {
                            string s = File.ReadAllText(luafilePath);
                            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
                            return bytes;
                        }
                    }
                }
            }

            return null;
        }
    }
}