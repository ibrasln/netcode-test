using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NetcodeTest.Networking.Client
{
    public class ClientSingleton : MonoBehaviour
    {
        private static ClientSingleton _instance;

        public ClientGameManager GameManager { get; private set; }
        
        public static ClientSingleton Instance
        {
            get
            {
                if (_instance is not null) return _instance;

                _instance = FindObjectOfType<ClientSingleton>();

                if (_instance == null)
                {
                    return null;
                }

                return _instance;
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public async Task<bool> CreateClient()
        {
            GameManager = new ClientGameManager();

            return await GameManager.InitAsync();
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}