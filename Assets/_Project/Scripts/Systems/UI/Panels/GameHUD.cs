using _Project.Scripts.Core.EventChannels;
using _Project.Scripts.Systems.Level;
using _Project.Scripts.Systems.UI.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Systems.UI.Panels
{
    /// <summary>
    /// GAME HUD
    /// Oyun içi HUD (Score, Moves, Level name)
    /// 
    /// EVENT-DRIVEN:
    /// - ScoreEventChannel dinler → Skor günceller
    /// - LevelManager.OnMovesChanged dinler → Hamle günceller (local event)
    /// 
    /// SORUMLULUKLAR:
    /// - Score/Moves gösterimi
    /// - Animasyonlu güncellemeler
    /// - Pause button
    /// </summary>
    public class GameHUD : UIPanel
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text levelNameText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text movesText;
        [SerializeField] private Button pauseButton;

        [Header("Event Channels")] // ✅ Event channel
        [SerializeField] private ScoreEventChannel scoreEventChannel;
        
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

            // ✅ Score event channel'a subscribe
            if (scoreEventChannel != null)
            {
                scoreEventChannel.Register(OnScoreEvent);
            }

            // ✅ LevelManager'ın local event'ine subscribe (moves için)
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnMovesChanged += OnMovesChanged;
            }

            // İlk değerleri göster
            RefreshUI();
        }

        protected override void OnHidden()
        {
            base.OnHidden();

            // ✅ Score event channel'dan unsubscribe
            if (scoreEventChannel != null)
            {
                scoreEventChannel.Unregister(OnScoreEvent);
            }

            // ✅ LevelManager'dan unsubscribe
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnMovesChanged -= OnMovesChanged;
            }
        }

        /// <summary>
        /// UI'ı güncelle (ilk açılış)
        /// </summary>
        private void RefreshUI()
        {
            if (LevelManager.Instance == null) return;

            // Level name
            if (levelNameText != null)
            {
                levelNameText.text = $"LEVEL 1"; // TODO: Dinamik level numarası
            }

            // Score
            UpdateScoreText(LevelManager.Instance.CurrentScore, false);

            // Moves
            UpdateMovesText(LevelManager.Instance.RemainingMoves, false);
        }

        /// <summary>
        /// ✅ Score event handler (event channel'dan)
        /// </summary>
        private void OnScoreEvent(ScoreEventData data)
        {
            UpdateScoreText(data.totalScore, animateScoreChange);

            // ✅ Bonus: Delta score popup (opsiyonel)
            if (data.deltaScore > 0 && animateScoreChange)
            {
                ShowScoreDelta(data.deltaScore);
            }
        }

        /// <summary>
        /// Hamle değiştiğinde (local event)
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
                // DOTween ile text büyüme animasyonu
                scoreText.transform.DOKill();
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
                // DOTween ile text büyüme animasyonu
                movesText.transform.DOKill();
                movesText.transform.localScale = Vector3.one * 1.2f;
                movesText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);

                // Renk değişimi (az hamle kaldıysa kırmızı)
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
        /// ✅ Score delta popup göster (örnek: +50)
        /// </summary>
        private void ShowScoreDelta(int delta)
        {
            if (scoreText == null) return;

            // Basit popup (opsiyonel, daha sonra geliştirebilirsin)
            // TODO: Ayrı bir ScorePopup component'i yap
            Debug.Log($"[GameHUD] Score +{delta}");
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

            // Score event channel cleanup
            if (scoreEventChannel != null)
            {
                scoreEventChannel.Unregister(OnScoreEvent);
            }

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