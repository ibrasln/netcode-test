using UnityEngine;

namespace NetcodeTest.Networking.Host
{
    public class HostSingleton : MonoBehaviour
    {
        private static HostSingleton _instance;

        public HostGameManager GameManager { get; private set; }
        
        public static HostSingleton Instance
        {
            get
            {
                if (_instance is not null) return _instance;

                _instance = FindObjectOfType<HostSingleton>();

                if (_instance == null)
                {
                    Debug.LogError("No HostSingleton in the scene!");
                    return null;
                }

                return _instance;
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void CreateHost()
        {
            GameManager = new HostGameManager();
        }
    }
}