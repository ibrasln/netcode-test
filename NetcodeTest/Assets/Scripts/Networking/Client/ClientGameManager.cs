using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine.SceneManagement;

namespace NetcodeTest.Networking.Client
{
    public class ClientGameManager
    {
        private const string MENU_SCENE_NAME = "Menu";
        
        public async Task<bool> InitAsync()
        {
            await UnityServices.InitializeAsync();

            AuthState authState = await AuthenticationWrapper.Authenticate();

            return authState == AuthState.Authenticated;
        }

        public void GoToMenu()
        {
            SceneManager.LoadScene(MENU_SCENE_NAME);
        }
    }
}