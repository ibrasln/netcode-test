using System;

namespace NetcodeTest.Networking.Shared
{
    public enum Map
    {
        Default
    }
    
    public enum GameMode
    {
        Default
    }
    
    public enum GameQueue
    {
        Solo,
        Team
    }
    
    [Serializable]
    public class UserData
    {
        public string Username;
        public string UserAuthId;
        public GameInfo UserGamePreferences;
    }

    [Serializable]
    public class GameInfo
    {
        public Map Map;
        public GameMode GameMode;
        public GameQueue GameQueue;

        public string ToMultiplayQueue()
        {
            return "";
        }
    }
}