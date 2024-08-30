using System;
using System.Collections;
using System.Threading.Tasks;
using NetcodeTest.Networking.Client;
using NetcodeTest.Networking.Host;
using NetcodeTest.Networking.Server;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetcodeTest.Networking
{
    public class ApplicationController : MonoBehaviour
    {
        [SerializeField] private ClientSingleton clientPrefab;
        [SerializeField] private HostSingleton hostPrefab;
        [SerializeField] private ServerSingleton serverPrefab;
        [SerializeField] private NetworkObject playerPrefab;

        private ApplicationData _applicationData;
        
        private const string GAME_SCENE_NAME = "Game";
        
        private async void Start()
        {
            DontDestroyOnLoad(gameObject);

            await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
        }

        private async Task LaunchInMode(bool isDedicatedServer)
        {
            if (isDedicatedServer)
            {
                Application.targetFrameRate = 60;
                
                _applicationData = new();
                
                ServerSingleton serverSingleton = Instantiate(serverPrefab);

                StartCoroutine(LoadGameSceneAsync(serverSingleton));
            }
            else
            {
                HostSingleton hostSingleton = Instantiate(hostPrefab);
                hostSingleton.CreateHost(playerPrefab);
                
                ClientSingleton clientSingleton = Instantiate(clientPrefab);
                bool authenticated = await clientSingleton.CreateClient();

                if (authenticated)
                {
                    clientSingleton.GameManager.GoToMenu();
                }
            }
        }

        private IEnumerator LoadGameSceneAsync(ServerSingleton serverSingleton)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GAME_SCENE_NAME);

            while (!asyncOperation.isDone)
            {
                yield return null;
            }
            
            Task createServerTask = serverSingleton.CreateServer(playerPrefab);

            yield return new WaitUntil(() => createServerTask.IsCompleted);
            
            Task startServerTask = serverSingleton.GameManager.StartGameServerAsync();
            
            yield return new WaitUntil(() => startServerTask.IsCompleted);
        }
    }
}