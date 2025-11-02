using _Project.Scripts.Core.EventChannels;
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
        [SerializeField] private Button menuButton;

        [Header("Event Channels")]
        [SerializeField] private ScoreEventChannel scoreEventChannel;
        
        [Header("Settings")]
        [SerializeField] private bool animateScoreChange = true;
        [SerializeField] private bool animateMovesChange = true;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OnMenuClicked);
            }
        }

        protected override void OnShown()
        {
            base.OnShown();
            
            if (scoreEventChannel != null)
            {
                scoreEventChannel.Register(OnScoreEvent);
            }
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnMovesChanged += OnMovesChanged;
            }

            RefreshUI();
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            
            if (scoreEventChannel != null)
            {
                scoreEventChannel.Unregister(OnScoreEvent);
            }
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnMovesChanged -= OnMovesChanged;
            }
        }

        private void RefreshUI()
        {
            if (LevelManager.Instance == null) return;

            // Level name
            if (levelNameText != null)
            {
                levelNameText.text = $"LEVEL 1";
            }
            
            UpdateScoreText(LevelManager.Instance.CurrentScore, false);

            UpdateMovesText(LevelManager.Instance.RemainingMoves, false);
        }
        
        private void OnScoreEvent(ScoreEventData data)
        {
            UpdateScoreText(data.totalScore, animateScoreChange);
            
            if (data.deltaScore > 0 && animateScoreChange)
            {
                ShowScoreDelta(data.deltaScore);
            }
        }
        
        private void OnMovesChanged(int newMoves)
        {
            UpdateMovesText(newMoves, animateMovesChange);
        }
        
        private void UpdateScoreText(int score, bool animate)
        {
            if (scoreText == null) return;

            int targetScore = LevelManager.Instance != null ? LevelManager.Instance.TargetScore : 0;
            scoreText.text = $"{score} / {targetScore}";

            if (animate)
            {
                scoreText.transform.DOKill();
                scoreText.transform.localScale = Vector3.one * 1.2f;
                scoreText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            }
        }
        private void UpdateMovesText(int moves, bool animate)
        {
            if (movesText == null) return;

            int maxMoves = LevelManager.Instance != null ? LevelManager.Instance.MaxMoves : 0;
            movesText.text = $"{moves} / {maxMoves}";

            if (animate)
            {
                movesText.transform.DOKill();
                movesText.transform.localScale = Vector3.one * 1.2f;
                movesText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
                
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
        
        private void OnMenuClicked()
        {
            if (SceneManagement.SceneManager.Instance != null)
            {
                SceneManagement.SceneManager.Instance.LoadMainMenu();
            }
        }
        
        private void ShowScoreDelta(int delta)
        {
            if (scoreText == null) return;
            
            Debug.Log($"[GameHUD] Score +{delta}");
        }
        

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (scoreEventChannel != null)
            {
                scoreEventChannel.Unregister(OnScoreEvent);
            }

            if (scoreText != null) scoreText.transform.DOKill();
            if (movesText != null) movesText.transform.DOKill();
            
            if (menuButton != null)
            {
                menuButton.onClick.RemoveListener(OnMenuClicked);
            }
        }
    }
}