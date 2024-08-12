using NetcodeTest.Networking.Shared;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.Networking.Server
{
    public class NetworkServer
    {
        private NetworkManager _networkManager;
        
        public NetworkServer(NetworkManager networkManager)
        {
            _networkManager = networkManager;

            _networkManager.ConnectionApprovalCallback += ApprovalCheck;
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
            UserData userData = JsonUtility.FromJson<UserData>(payload);
            
            Debug.Log(userData.Username);

            response.Approved = true;
            response.CreatePlayerObject = true;
        }
    }
}