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
        public int TeamIndex = -1;
        public GameInfo UserGamePreferences = new();
    }

    [Serializable]
    public class GameInfo
    {
        public Map Map;
        public GameMode GameMode;
        public GameQueue GameQueue;

        public string ToMultiplayQueue()
        {
            return GameQueue switch
            {
                GameQueue.Solo => "solo-queue",
                GameQueue.Team => "team-queue",
                _ => "solo-queue"
            };
        }
    }
}