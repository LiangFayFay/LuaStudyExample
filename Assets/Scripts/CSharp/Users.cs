using System.Collections.Generic;
using XLua;
namespace DefaultNamespace
{
    [LuaCallCSharp]
    public class Users
    {
        public static List<UserStruct> MyUsers = new List<UserStruct>();
    }

    [LuaCallCSharp]
    public class UserStruct
    {
        public string Username;
        public string Password;
    }
}