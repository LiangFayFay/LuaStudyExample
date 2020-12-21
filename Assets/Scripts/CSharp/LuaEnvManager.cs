using XLua;

namespace DefaultNamespace
{
    public class LuaEnvManager
    {
        private static LuaEnvManager instance;
        private static LuaEnv luaEnv = new LuaEnv();

        static LuaEnvManager()
        {
            instance = new LuaEnvManager();
        }
        
        public static LuaEnvManager Instance
        {
            get => instance;
        }

        public LuaEnv GetEnv()
        {
            return luaEnv;
        }
    }
}