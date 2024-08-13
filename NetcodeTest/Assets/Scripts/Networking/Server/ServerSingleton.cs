using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;

namespace NetcodeTest.Networking.Server
{
    public class ServerSingleton : MonoBehaviour
    {
        private static ServerSingleton _instance;

        public ServerGameManager GameManager { get; private set; }
        
        public static ServerSingleton Instance
        {
            get
            {
                if (_instance is not null) return _instance;

                _instance = FindObjectOfType<ServerSingleton>();

                if (_instance == null)
                {
                    Debug.LogError("No ServerSingleton in the scene!");
                    return null;
                }

                return _instance;
            }
        }
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public async Task CreateServer()
        {
            await UnityServices.InitializeAsync();
            GameManager = new ServerGameManager(
                ApplicationData.IP(), 
                ApplicationData.Port(), 
                ApplicationData.QPort(), 
                NetworkManager.Singleton);
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}