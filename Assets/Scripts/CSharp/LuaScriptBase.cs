using System;
using UnityEngine;
using XLua;

namespace DefaultNamespace
{
    [LuaCallCSharp]
    public class LuaScriptBase : MonoBehaviour
    {
        [SerializeField] private TextAsset luaScript;
        [SerializeField] private MyStruct[] myStructs;
        
        private static float lastGCTime = 0;
        private const float GCInterval = 1;
        private LuaEnv luaEnv = LuaEnvManager.Instance.GetEnv();
        private LuaTable luaTable;
        private Action luaAwake;
        private Action luaStart;
        private Action luaUpdate;
        private Action luaOnDestroy;

        protected void SetSelf<TValue>(TValue value)
        {
            luaTable = luaEnv.NewTable();
            luaTable.Set("self",value);
        }

        protected virtual void Awake()
        {
            if (luaTable == null)
            {
                SetSelf(this);
            }
            var metaTable = luaEnv.NewTable();
            metaTable.Set("__index",luaEnv.Global);
            luaTable.SetMetaTable(metaTable);
            metaTable.Dispose();
            
            foreach (var item in myStructs)
            {
                luaTable.Set(item.Name,item.Value);
            }

            luaEnv.DoString(luaScript.text, "Error", luaTable);
            
            luaTable.Get("awake",out luaAwake);
            luaTable.Get("start",out luaStart);
            luaTable.Get("update",out luaUpdate);
            luaTable.Get("ondestroy",out luaOnDestroy);
            
            luaAwake?.Invoke();
        }

        public void Start()
        {
            luaStart?.Invoke();
        }

        public void Update()
        {
            luaUpdate?.Invoke();
            if (Time.time - lastGCTime > GCInterval)
            {
                luaEnv.Tick();
                lastGCTime = Time.time;
            }
        }

        public void OnDestroy()
        {
            luaOnDestroy?.Invoke();
            luaTable = null;
        }
    }
}