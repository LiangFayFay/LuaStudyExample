using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class Login : LuaScriptBase
    {
        protected override void Awake()
        {
            SetSelf(this);
            base.Awake();
        }
    }
}