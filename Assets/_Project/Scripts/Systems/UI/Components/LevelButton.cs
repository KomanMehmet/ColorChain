using _Project.Scripts.Systems.Level;
using _Project.Scripts.Systems.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Systems.UI.Components
{
    public class LevelButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text levelNumberText;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject[] stars; // 3 yıldız

        [Header("Colors")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = Color.gray;

        private int _levelIndex;
        private bool _isUnlocked;
        
        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
        }
        
        public void Setup(int levelIndex, LevelProgress progress)
        {
            _levelIndex = levelIndex;
            _isUnlocked = progress.isUnlocked;

            // Level numarası
            if (levelNumberText != null)
            {
                levelNumberText.text = $"{levelIndex + 1}";
            }
            
            if (lockIcon != null)
            {
                lockIcon.SetActive(!_isUnlocked);
            }
            
            UpdateStars(progress.starsEarned);
            
            if (button != null)
            {
                button.interactable = _isUnlocked;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
                
                var colors = button.colors;
                colors.normalColor = _isUnlocked ? unlockedColor : lockedColor;
                button.colors = colors;
            }
        }
        
        private void UpdateStars(int earnedStars)
        {
            if (stars == null || stars.Length == 0) return;

            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] != null)
                {
                    stars[i].SetActive(i < earnedStars);
                }
            }
        }
        
        private void OnButtonClicked()
        {
            if (!_isUnlocked) return;
            
            if (LevelManager.Instance != null && LevelDataManager.Instance != null)
            {
                var levelData = LevelDataManager.Instance.GetLevelData(_levelIndex);
                
                if (levelData != null)
                {
                    if (SceneManager.Instance != null)
                    {
                        SceneManager.Instance.LoadGameplay();

                        LevelManager.Instance.StartLevel(levelData);
                    }
                }
            }
        }
    }
}