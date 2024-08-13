using NetcodeTest.Networking.Client;
using NetcodeTest.Networking.Host;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.UI
{
    public class GameHUD : MonoBehaviour
    {
        public void LeaveGame()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                HostSingleton.Instance.GameManager.Shutdown();
            }

            ClientSingleton.Instance.GameManager.Disconnect();
        }
    }
}