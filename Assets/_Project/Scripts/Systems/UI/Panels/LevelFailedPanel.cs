using _Project.Scripts.Systems.Grid;
using _Project.Scripts.Systems.Level;
using _Project.Scripts.Systems.UI.Core;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Systems.UI.Panels
{
    public class LevelFailedPanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        public async UniTask ShowWithScore(int currentScore, int targetScore)
        {
            // Mesajı güncelle
            if (messageText != null)
            {
                messageText.text = "Out of moves!";
            }

            // Skoru güncelle
            if (scoreText != null)
            {
                scoreText.text = $"Score: {currentScore} / {targetScore}";
            }
            
            await Show();
        }
        
        private void OnRestartClicked()
        {
            Debug.Log("[LevelFailedPanel] Restart clicked");
            
            Hide().Forget();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RestartLevel();
            }

            if (GridManager.Instance != null)
            {
                // TODO: GridManager.Instance.RestartGrid();
            }
        }
        
        private void OnMainMenuClicked()
        {
            Debug.Log("[LevelFailedPanel] Main Menu clicked");
            
            Hide().Forget();

            if (SceneManagement.SceneManager.Instance != null)
            {
                SceneManagement.SceneManager.Instance.LoadMainMenu();
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Button listener temizliği
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            }
        }
    }
}