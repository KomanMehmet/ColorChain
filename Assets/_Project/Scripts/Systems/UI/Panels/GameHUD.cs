using _Project.Scripts.Systems.Level;
using _Project.Scripts.Systems.UI.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Systems.UI.Panels
{
    public class GameHUD : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text levelNameText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text movesText;
        [SerializeField] private Button pauseButton;
        
        [Header("Settings")]
        [SerializeField] private bool animateScoreChange = true;
        [SerializeField] private bool animateMovesChange = true;
        
        protected override void Awake()
        {
            base.Awake();

            // Pause button
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
            }
        }
        
        protected override void OnShown()
        {
            base.OnShown();

            // LevelManager event'lerine subscribe ol
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnScoreChanged += OnScoreChanged;
                LevelManager.Instance.OnMovesChanged += OnMovesChanged;
            }

            // İlk değerleri göster
            RefreshUI();
        }
        
        protected override void OnHidden()
        {
            base.OnHidden();

            // Event'lerden unsubscribe ol
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnScoreChanged -= OnScoreChanged;
                LevelManager.Instance.OnMovesChanged -= OnMovesChanged;
            }
        }
        
        /// <summary>
        /// UI'ı güncelle
        /// </summary>
        private void RefreshUI()
        {
            if (LevelManager.Instance == null) return;

            // Level name
            if (levelNameText != null)
            {
                levelNameText.text = $"LEVEL 1"; // Placeholder
            }

            // Score
            UpdateScoreText(LevelManager.Instance.CurrentScore, false);

            // Moves
            UpdateMovesText(LevelManager.Instance.RemainingMoves, false);
        }
        
        /// <summary>
        /// Skor değiştiğinde
        /// </summary>
        private void OnScoreChanged(int newScore)
        {
            UpdateScoreText(newScore, animateScoreChange);
        }
        
        /// <summary>
        /// Hamle değiştiğinde
        /// </summary>
        private void OnMovesChanged(int newMoves)
        {
            UpdateMovesText(newMoves, animateMovesChange);
        }
        
        /// <summary>
        /// Skor text'ini güncelle
        /// </summary>
        private void UpdateScoreText(int score, bool animate)
        {
            if (scoreText == null) return;

            int targetScore = LevelManager.Instance != null ? LevelManager.Instance.TargetScore : 0;
            scoreText.text = $"{score} / {targetScore}";

            if (animate)
            {
                // ✅ DOTween ile text büyüme animasyonu
                scoreText.transform.DOKill(); // Önceki animasyonu durdur
                scoreText.transform.localScale = Vector3.one * 1.2f;
                scoreText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            }
        }
        
        /// <summary>
        /// Hamle text'ini güncelle
        /// </summary>
        private void UpdateMovesText(int moves, bool animate)
        {
            if (movesText == null) return;

            int maxMoves = LevelManager.Instance != null ? LevelManager.Instance.MaxMoves : 0;
            movesText.text = $"{moves} / {maxMoves}";

            if (animate)
            {
                // ✅ DOTween ile text büyüme animasyonu
                movesText.transform.DOKill();
                movesText.transform.localScale = Vector3.one * 1.2f;
                movesText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);

                // ✅ DOTween ile renk değişimi
                if (moves <= 5)
                {
                    movesText.DOColor(Color.red, 0.3f);
                }
                else
                {
                    movesText.DOColor(Color.white, 0.3f);
                }
            }
        }
        
        /// <summary>
        /// Pause butonuna tıklandı
        /// </summary>
        private void OnPauseButtonClicked()
        {
            // TODO: Pause panel'i aç
            Debug.Log("[GameHUD] Pause button clicked");
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // DOTween cleanup
            if (scoreText != null) scoreText.transform.DOKill();
            if (movesText != null) movesText.transform.DOKill();

            // Button listener temizliği
            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
            }
        }
    }
}