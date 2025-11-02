using System.Collections.Generic;
using _Project.Scripts.Systems.Level;
using _Project.Scripts.Systems.SceneManagement;
using _Project.Scripts.Systems.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Systems.UI.Panels
{
    public class LevelSelectionUI : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject levelButtonPrefab;

        [Header("Container")]
        [SerializeField] private Transform levelButtonContainer;

        [Header("UI Elements")]
        [SerializeField] private Button backButton;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;
        
        private List<LevelButton> _levelButtons = new List<LevelButton>();
        
        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }
        }
        
        private void OnEnable()
        {
            RefreshLevelButtons();
        }
        
        private void Start()
        {
            CreateLevelButtons();
        }
        
        private void CreateLevelButtons()
        {
            if (LevelDataManager.Instance == null)
            {
                Debug.LogError("[LevelSelectionUI] LevelDataManager not found!");
                return;
            }

            if (levelButtonPrefab == null || levelButtonContainer == null)
            {
                Debug.LogError("[LevelSelectionUI] Missing prefab or container!");
                return;
            }
            foreach (var btn in _levelButtons)
            {
                if (btn != null)
                {
                    Destroy(btn.gameObject);
                }
            }
            _levelButtons.Clear();
            
            int totalLevels = LevelDataManager.Instance.GetTotalLevels();

            for (int i = 0; i < totalLevels; i++)
            {
                GameObject btnObj = Instantiate(levelButtonPrefab, levelButtonContainer);
                LevelButton levelButton = btnObj.GetComponent<LevelButton>();

                if (levelButton != null)
                {
                    LevelProgress progress = LevelDataManager.Instance.GetLevelProgress(i);
                    levelButton.Setup(i, progress);
                    _levelButtons.Add(levelButton);
                }
            }

            if (showDebugLogs)
            {
                Debug.Log($"[LevelSelectionUI] Created {totalLevels} level buttons");
            }
        }
        
        private void RefreshLevelButtons()
        {
            if (LevelDataManager.Instance == null) return;

            foreach (var btn in _levelButtons)
            {
                if (btn != null)
                {
                    int levelIndex = _levelButtons.IndexOf(btn);
                    LevelProgress progress = LevelDataManager.Instance.GetLevelProgress(levelIndex);
                    btn.Setup(levelIndex, progress);
                }
            }

            if (showDebugLogs)
            {
                Debug.Log("[LevelSelectionUI] Level buttons refreshed");
            }
        }
        
        private void OnBackClicked()
        {
            if (SceneManager.Instance != null)
            {
                SceneManager.Instance.LoadMainMenu();
            }
        }
        
        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
            }
        }
    }
}