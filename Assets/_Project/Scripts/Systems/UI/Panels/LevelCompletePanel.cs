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
        [SerializeField] private Button menuButton;
        
        [Header("Star Settings")]
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private Sprite starEmptySprite;
        [SerializeField] private float starAnimationDelay = 0.3f;
        
        private int _earnedStars;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
            
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OnMenuClicked);
            }
        }

        public async UniTask ShowWithStars(int stars, int finalScore)
        {
            _earnedStars = stars;

            if (scoreText != null)
            {
                scoreText.text = $"Score: {finalScore}";
            }

            await Show();

            await AnimateStars();
        }
        
        private async UniTask AnimateStars()
        {
            if (starImages == null || starImages.Length == 0) return;
            
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].sprite = starEmptySprite;
                    starImages[i].transform.localScale = Vector3.zero;
                }
            }
            
            for (int i = 0; i < _earnedStars && i < starImages.Length; i++)
            {
                await UniTask.Delay((int)(starAnimationDelay * 1000));

                if (starImages[i] != null)
                {
                    starImages[i].sprite = starFilledSprite;
                    
                    starImages[i].transform.localScale = Vector3.zero;
                    starImages[i].transform.DOScale(Vector3.one, 0.5f)
                        .SetEase(Ease.OutElastic);

                    starImages[i].transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360)
                        .SetEase(Ease.OutQuad);

                    // TODO: Ses efekti çal (ding!)
                }
            }
        }
        
        private void OnNextLevelClicked()
        {
            Debug.Log("[LevelCompletePanel] Next Level clicked");
            
            Hide().Forget();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadNextLevel();
            }
        }

        private void OnRestartClicked()
        {
            Debug.Log("[LevelCompletePanel] Restart clicked");
            
            Hide().Forget();
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RestartLevel();
            }

            if (GridManager.Instance != null)
            {
                // GridManager'ın restart methodu gerekecek
                // GridManager.Instance.RestartGrid();
            }
        }
        
        private void OnMenuClicked()
        {
            Debug.Log("[LevelCompletePanel] MeinMenu clicked");
            
            Hide().Forget();
            
            if (SceneManagement.SceneManager.Instance != null)
            {
                SceneManagement.SceneManager.Instance.LoadMainMenu();
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
            }
            
            if (menuButton != null)
            {
                menuButton.onClick.RemoveListener(OnMenuClicked);
            }
        }
    }
}