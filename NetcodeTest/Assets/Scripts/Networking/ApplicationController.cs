using System;
using System.Threading.Tasks;
using NetcodeTest.Networking.Client;
using NetcodeTest.Networking.Host;
using NetcodeTest.Networking.Server;
using UnityEngine;

namespace NetcodeTest.Networking
{
    public class ApplicationController : MonoBehaviour
    {
        [SerializeField] private ClientSingleton clientPrefab;
        [SerializeField] private HostSingleton hostPrefab;
        [SerializeField] private ServerSingleton serverPrefab;

        private ApplicationData _applicationData;
        
        private async void Start()
        {
            DontDestroyOnLoad(gameObject);

            await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
        }

        private async Task LaunchInMode(bool isDedicatedServer)
        {
            if (isDedicatedServer)
            {
                _applicationData = new();
                
                ServerSingleton serverSingleton = Instantiate(serverPrefab);
                await serverSingleton.CreateServer();

                await serverSingleton.GameManager.StartGameServerAsync();
            }
            else
            {
                HostSingleton hostSingleton = Instantiate(hostPrefab);
                hostSingleton.CreateHost();
                
                ClientSingleton clientSingleton = Instantiate(clientPrefab);
                bool authenticated = await clientSingleton.CreateClient();

                if (authenticated)
                {
                    clientSingleton.GameManager.GoToMenu();
                }
            }
        }
    }
}