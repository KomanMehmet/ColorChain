using System;
using _Project.Scripts.Systems.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Systems.UI.Panels
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;

        [Header("UI Elements")] 
        [SerializeField] private TMP_Text versionText;
        [SerializeField] private string versionPrefix = "v";

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private void Awake()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
            
            if (versionText != null)
            {
                versionText.text = $"{versionPrefix}{Application.version}";
            }
            
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }
            
            if (showDebugLogs)
            {
                Debug.Log("[MainMenuUI] Main Menu initialized");
            }
        }

        private void OnPlayClicked()
        {
            if (showDebugLogs)
            {
                Debug.Log("[MainMenuUI] Play button clicked");
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("buttonclick");
            }

            /*if (SceneManagement.SceneManager.Instance != null)
            {
                SceneManagement.SceneManager.Instance.LoadGameplay();
            }*/
            
            if (SceneManagement.SceneManager.Instance != null)
            {
                SceneManagement.SceneManager.Instance.LoadScene("LevelSelection");
            }
        }

        private void OnSettingsClicked()
        {
            if (showDebugLogs)
            {
                Debug.Log("[MainMenuUI] Settings button clicked");
            }
            
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        private void OnQuitClicked()
        {
            if (showDebugLogs)
            {
                Debug.Log("[MainMenuUI] Quit button clicked");
            }
            
            if (SceneManagement.SceneManager.Instance != null)
            {
                SceneManagement.SceneManager.Instance.QuitGame();
            }
        }
        
        private void OnDestroy()
        {
            if (playButton != null)
            {
                playButton.onClick.RemoveListener(OnPlayClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(OnSettingsClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitClicked);
            }
        }
    }
}