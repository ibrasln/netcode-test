using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NetcodeTest.UI
{
    public class NameSelector : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button connectButton;
        [SerializeField] private int minNameLength = 1;
        [SerializeField] private int maxNameLength = 12;

        public const string PLAYER_NAME_KEY = "PlayerName";
        
        private void Start()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                return;
            }
            
            nameInputField.text = PlayerPrefs.GetString(PLAYER_NAME_KEY, string.Empty);
            HandleNameChanged();
        }

        public void HandleNameChanged()
        {
            connectButton.interactable = nameInputField.text.Length >= minNameLength && nameInputField.text.Length <= maxNameLength;
        }

        public void Connect()
        {
            PlayerPrefs.SetString(PLAYER_NAME_KEY, nameInputField.text);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}