using System;
using _Project.Scripts.Core.Enums;
using _Project.Scripts.Core.EventChannels;
using _Project.Scripts.Data;
using _Project.Scripts.Systems.UI.Core;
using _Project.Scripts.Systems.UI.Panels;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Systems.Level
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }
        
        [Header("Current Level")]
        [SerializeField] private LevelData currentLevel;

        [Header("Event Channels")] // ✅ Event channels
        [SerializeField] private MatchEventChannel matchEventChannel;
        [SerializeField] private ScoreEventChannel scoreEventChannel;
        [SerializeField] private GridProcessingCompleteEventChannel  gridProcessingCompleteEventChannel;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Level State
        private int _remainingMoves;
        private int _currentScore;
        private LevelState _currentState;
        
        private bool _isProcessing = false;
        
        // Events
        public event Action<int> OnMovesChanged;
        public event Action<int> OnLevelCompleted;
        public event Action OnLevelFailed;
        
        // Properties (read-only)
        public int RemainingMoves => _remainingMoves;
        public int CurrentScore => _currentScore;
        public int TargetScore => currentLevel != null ? currentLevel.TargetScore : 0;
        public int MaxMoves => currentLevel != null ? currentLevel.MaxMoves : 0;
        public LevelState CurrentState => _currentState;
        
        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (matchEventChannel != null)
            {
                matchEventChannel.Register(OnMatchEvent);
            }
            
            if (gridProcessingCompleteEventChannel != null)
            {
                gridProcessingCompleteEventChannel.Register(OnGridProcessingComplete);
            }
        }

        private void OnDisable()
        {
            if (matchEventChannel != null)
            {
                matchEventChannel.Unregister(OnMatchEvent);
            }
            
            if (gridProcessingCompleteEventChannel != null)
            {
                gridProcessingCompleteEventChannel.Unregister(OnGridProcessingComplete);
            }
        }
        
        public void StartLevel(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("[LevelManager] LevelData is null!");
                return;
            }

            currentLevel = levelData;

            _remainingMoves = currentLevel.MaxMoves;
            _currentScore = 0;
            _currentState = LevelState.Playing;
            _isProcessing = false;

            OnMovesChanged?.Invoke(_remainingMoves);

            if (scoreEventChannel != null)
            {
                ScoreEventData scoreData = new ScoreEventData(0, _currentScore);
                scoreEventChannel.RaiseEvent(scoreData);
            }

            if (showDebugLogs)
            {
                Debug.Log($"[LevelManager] Level started: {currentLevel.LevelName}");
                Debug.Log($"[LevelManager] Target: {currentLevel.TargetScore} | Moves: {currentLevel.MaxMoves}");
            }
        }

        public void OnMoveCompleted()
        {
            if (_currentState != LevelState.Playing)
            {
                return;
            }

            _remainingMoves--;
            OnMovesChanged?.Invoke(_remainingMoves);

            if (showDebugLogs)
            {
                Debug.Log($"[LevelManager] Move completed. Remaining: {_remainingMoves}");
            }
        }
        
        private void OnMatchEvent(MatchEventData data)
        {
            if (_currentState != LevelState.Playing)
            {
                return;
            }

            _isProcessing = true;
            
            //Skor hesapla
            int points = CalculateScore(data.matchCount, data.matchType);
            int previousScore = _currentScore;
            _currentScore += points;
            
            if (scoreEventChannel != null)
            {
                ScoreEventData scoreData = new ScoreEventData(points, _currentScore);
                scoreEventChannel.RaiseEvent(scoreData);
            }

            if (showDebugLogs)
            {
                Debug.Log($"[LevelManager] Match! +{points} points (Total: {_currentScore}/{currentLevel.TargetScore})");
            }
        }
        
        private void OnGridProcessingComplete(GridProcessingCompleteEventData data)
        {
            if (_currentState != LevelState.Playing)
            {
                return;
            }
            
            _isProcessing = false;

            if (showDebugLogs)
            {
                Debug.Log($"[LevelManager] Processing complete: {data}");
                Debug.Log($"[LevelManager] Final score: {_currentScore}, Remaining moves: {_remainingMoves}");
            }

            CheckLevelStatus();
        }
        
        private int CalculateScore(int matchCount, MatchType matchType)
        {
            const int BASE_POINTS = 10;
            const int COMBO_BONUS = 5;
            const int MIN_MATCH = 3;
            
            int baseScore = matchCount * BASE_POINTS;
            
            float multiplier = 1f;
            switch (matchType)
            {
                case MatchType.Horizontal:
                case MatchType.Vertical:
                    multiplier = 1f;
                    break;
                case MatchType.Cross:
                    multiplier = 1.5f;
                    break;
            }

            int score = Mathf.RoundToInt(baseScore * multiplier);
            
            if (matchCount > MIN_MATCH)
            {
                int extraBalls = matchCount - MIN_MATCH;
                score += extraBalls * COMBO_BONUS;
            }

            return score;
        }
        
        private void CheckLevelStatus()
        {
            if (_currentState != LevelState.Playing)
            {
                return;
            }
            
            if (_isProcessing)
            {
                if (showDebugLogs)
                {
                    Debug.Log("[LevelManager] Still processing, level check deferred");
                }
                return;
            }

            if (_currentScore >= currentLevel.TargetScore)
            {
                CompleteLevelSuccess();
                return;
            }
            
            if (_remainingMoves <= 0)
            {
                CompleteLevelFailed();
                return;
            }
        }
        
        private void CompleteLevelSuccess()
        {
            _currentState = LevelState.Completed;

            int stars = CalculateStars();

            if (showDebugLogs)
            {
                Debug.Log($"[LevelManager] LEVEL COMPLETED! Score: {_currentScore} | Stars: {stars}⭐");
            }
            
            OnLevelCompleted?.Invoke(stars);
            
            ShowLevelCompletePanel(stars).Forget();
        }
        
        private void CompleteLevelFailed()
        {
            _currentState = LevelState.Failed;

            if (showDebugLogs)
            {
                Debug.Log($"[LevelManager] LEVEL FAILED! Score: {_currentScore}/{currentLevel.TargetScore}");
            }

            // Local event
            OnLevelFailed?.Invoke();

            // UI Panel göster
            ShowLevelFailedPanel().Forget();
        }
        
        private async UniTaskVoid ShowLevelCompletePanel(int stars)
        {
            await UniTask.Delay(500);

            if (UIManager.Instance == null) return;

            var completePanel = UIManager.Instance.GetPanel<LevelCompletePanel>();
            if (completePanel != null)
            {
                await completePanel.ShowWithStars(stars, _currentScore);
            }
        }
        
        private async UniTaskVoid ShowLevelFailedPanel()
        {
            await UniTask.Delay(500);

            if (UIManager.Instance == null) return;

            var failedPanel = UIManager.Instance.GetPanel<LevelFailedPanel>();
            if (failedPanel != null)
            {
                await failedPanel.ShowWithScore(_currentScore, currentLevel.TargetScore);
            }
        }
        
        private int CalculateStars()
        {
            if (currentLevel == null) return 1;

            float scorePercentage = (float)_currentScore / currentLevel.TargetScore;

            if (scorePercentage >= 1.5f)
            {
                return 3;
            }
            else if (scorePercentage >= 1.2f)
            {
                return 2;
            }
            else
            {
                return 1; 
            }
        }

        public void RestartLevel()
        {
            if (currentLevel == null)
            {
                Debug.LogError("[LevelManager] No level to restart!");
                return;
            }

            StartLevel(currentLevel);

            // Grid'i de restart et
            if (Grid.GridManager.Instance != null)
            {
                Grid.GridManager.Instance.Initialize(currentLevel);
            }

            if (showDebugLogs)
            {
                Debug.Log("[LevelManager] Level restarted");
            }
        }
        
        public void LoadNextLevel()
        {
            // TODO: Level listesi
            Debug.Log("[LevelManager] Next level not implemented yet");
        }

        private void OnDestroy()
        {
            if (matchEventChannel != null)
            {
                matchEventChannel.Unregister(OnMatchEvent);
            }
            
            OnMovesChanged = null;
            OnLevelCompleted = null;
            OnLevelFailed = null;
        }
    }
}