using _Project.Scripts.Systems.Grid;
using _Project.Scripts.Systems.Level;
using _Project.Scripts.Systems.UI.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Systems.UI.Panels
{
    public class LevelCompletePanel : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Image[] starImages;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button restartButton;
        
        [Header("Star Settings")]
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private Sprite starEmptySprite;
        [SerializeField] private float starAnimationDelay = 0.3f;
        
        private int _earnedStars;
        
        protected override void Awake()
        {
            base.Awake();

            // Button listeners
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
        }
        
        /// <summary>
        /// Panel'i göster ve yıldızları animasyonla doldur
        /// </summary>
        public async UniTask ShowWithStars(int stars, int finalScore)
        {
            _earnedStars = stars;

            // Skoru güncelle
            if (scoreText != null)
            {
                scoreText.text = $"Score: {finalScore}";
            }

            // Panel'i göster
            await Show();

            // Yıldız animasyonlarını başlat
            await AnimateStars();
        }
        
        // <summary>
        /// Yıldızları animasyonla göster
        /// </summary>
        private async UniTask AnimateStars()
        {
            if (starImages == null || starImages.Length == 0) return;

            // Başlangıçta tüm yıldızları boş yap
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].sprite = starEmptySprite;
                    starImages[i].transform.localScale = Vector3.zero;
                }
            }

            // Her yıldızı sırayla animasyonla doldur
            for (int i = 0; i < _earnedStars && i < starImages.Length; i++)
            {
                await UniTask.Delay((int)(starAnimationDelay * 1000));

                if (starImages[i] != null)
                {
                    // Yıldızı doldur
                    starImages[i].sprite = starFilledSprite;

                    // Scale animasyonu
                    starImages[i].transform.localScale = Vector3.zero;
                    starImages[i].transform.DOScale(Vector3.one, 0.5f)
                        .SetEase(Ease.OutElastic);

                    // Rotate animasyonu
                    starImages[i].transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360)
                        .SetEase(Ease.OutQuad);

                    // TODO: Ses efekti çal (ding!)
                }
            }
        }
        
        /// <summary>
        /// Next Level butonuna tıklandı
        /// </summary>
        private void OnNextLevelClicked()
        {
            Debug.Log("[LevelCompletePanel] Next Level clicked");

            // Panel'i kapat
            Hide().Forget();

            // TODO: Sonraki level'ı yükle
            // LevelManager.Instance.LoadNextLevel();
        }
        
        /// <summary>
        /// Restart butonuna tıklandı
        /// </summary>
        private void OnRestartClicked()
        {
            Debug.Log("[LevelCompletePanel] Restart clicked");

            // Panel'i kapat
            Hide().Forget();

            // Level'ı yeniden başlat
            if (LevelManager.Instance != null)
            {
                Level.LevelManager.Instance.RestartLevel();
            }

            // Grid'i yeniden oluştur
            if (GridManager.Instance != null)
            {
                // GridManager'ın restart methodu gerekecek
                // GridManager.Instance.RestartGrid();
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Button listener temizliği
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
            }
        }
    }
}