using System;
using _Project.Scripts.Systems.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Systems.UI.Panels
{
    public class SettingsPanel : MonoBehaviour
    {
        [Header("Volume Sliders")] 
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Volume Texts")] 
        [SerializeField] private TMP_Text masterVolumeText;
        [SerializeField] private TMP_Text musicVolumeText;
        [SerializeField] private TMP_Text sfxVolumeText;
        
        [Header("Buttons")]
        [SerializeField] private Button closeButton;

        [Header("Debugs")] 
        [SerializeField] private bool showDebugLogs = false;

        private void Awake()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }
        }
        
        private void OnEnable()
        {
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            if(AudioManager.Instance == null) return;

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
                UpdateMasterVolumeText(AudioManager.Instance.MasterVolume);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.MusicVolume;
                UpdateMusicVolumeText(AudioManager.Instance.MusicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.SFXVolume;
                UpdateSFXVolumeText(AudioManager.Instance.SFXVolume);
            }
            
            if (showDebugLogs)
            {
                Debug.Log("[SettingsPanel] Current settings loaded");
            }
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(value);
            }
            
            UpdateMasterVolumeText(value);
            
            if (showDebugLogs)
            {
                Debug.Log($"[SettingsPanel] Master volume: {value:F2}");
            }
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(value);
            }
            
            UpdateMusicVolumeText(value);

            if (showDebugLogs)
            {
                Debug.Log($"[SettingsPanel] Music volume: {value:F2}");
            }
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
            }

            UpdateSFXVolumeText(value);
            
            if (showDebugLogs)
            {
                Debug.Log($"[SettingsPanel] SFX volume: {value:F2}");
            }
        }

        private void UpdateMasterVolumeText(float value)
        {
            if (masterVolumeText != null)
            {
                masterVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }
        
        private void UpdateMusicVolumeText(float value)
        {
            if (musicVolumeText != null)
            {
                musicVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }
        
        private void UpdateSFXVolumeText(float value)
        {
            if (sfxVolumeText != null)
            {
                sfxVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }
        
        private void OnCloseClicked()
        {
            if (showDebugLogs)
            {
                Debug.Log("[SettingsPanel] Close button clicked");
            }
            
            gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            }

            // Close button
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
            }
        }
    }
}