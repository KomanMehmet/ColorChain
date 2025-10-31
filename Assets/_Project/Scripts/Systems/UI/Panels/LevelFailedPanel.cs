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

            // Button listeners
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        /// <summary>
        /// Panel'i göster ve bilgileri güncelle
        /// </summary>
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

            // Panel'i göster
            await Show();
        }

        /// <summary>
        /// Restart butonuna tıklandı
        /// </summary>
        private void OnRestartClicked()
        {
            Debug.Log("[LevelFailedPanel] Restart clicked");

            // Panel'i kapat
            Hide().Forget();

            // Level'ı yeniden başlat
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RestartLevel();
            }

            // Grid'i yeniden oluştur
            if (GridManager.Instance != null)
            {
                // TODO: GridManager.Instance.RestartGrid();
            }
        }
        
        /// <summary>
        /// Main Menu butonuna tıklandı
        /// </summary>
        private void OnMainMenuClicked()
        {
            Debug.Log("[LevelFailedPanel] Main Menu clicked");

            // Panel'i kapat
            Hide().Forget();

            // TODO: Ana menüye dön
            // SceneManager.LoadScene("MainMenu");
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