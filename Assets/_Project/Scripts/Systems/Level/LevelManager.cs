using System;
using _Project.Scripts.Core.Enums;
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
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Level State
        private int _remainingMoves;
        private int _currentScore;
        private LevelState _currentState;
        
        // Events
        public event Action<int> OnScoreChanged;
        public event Action<int> OnMovesChanged;
        public event Action<int> OnLevelCompleted;
        public event Action OnLevelFailed;     
        
        // Properties (read-only)
        public int RemainingMoves => _remainingMoves;
        public int CurrentScore => _currentScore;
        public int TargetScore => currentLevel.TargetScore;
        public int MaxMoves => currentLevel.MaxMoves;
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
        
        public void StartLevel(LevelData levelData)
        {
            currentLevel = levelData;

            // State'i sıfırla
            _remainingMoves = currentLevel.MaxMoves;
            _currentScore = 0;
            _currentState = LevelState.Playing;

            // Event'leri tetikle (UI güncellensin)
            OnMovesChanged?.Invoke(_remainingMoves);
            OnScoreChanged?.Invoke(_currentScore);

            if (showDebugLogs)
            {
                Debug.Log($"[LevelManager] Level started: {currentLevel.LevelName}");
                Debug.Log($"[LevelManager] Target Score: {currentLevel.TargetScore} | Max Moves: {currentLevel.MaxMoves}");
            }
        }
        
        public void OnMoveCompleted()
        {
            if (_currentState != LevelState.Playing)
            {
                return; // Level bittiyse hamle sayma
            }

            _remainingMoves--;
            OnMovesChanged?.Invoke(_remainingMoves);

            if (showDebugLogs)
            {
                Debug.Log($"[LevelManager] Move completed. Remaining: {_remainingMoves}");
            }

            // Hamle bittikten sonra kontrol et
            CheckLevelStatus();
        }

        public void OnMatchCompleted(int matchCount, _Project.Scripts.Core.Enums.MatchType matchType)
        {
            if (_currentState != LevelState.Playing)
            {
                return;
            }

            // Skor hesapla
            int points = CalculateScore(matchCount, matchType);
            _currentScore += points;

            OnScoreChanged?.Invoke(_currentScore);

            if (showDebugLogs)
            {
                Debug.Log($"[LevelManager] Match! +{points} points (Total: {_currentScore}/{currentLevel.TargetScore})");
            }

            // Match sonrası kontrol et
            CheckLevelStatus();
        }
        
        private int CalculateScore(int matchCount, _Project.Scripts.Core.Enums.MatchType matchType)
        {
            const int BASE_POINTS = 10;
            const int COMBO_BONUS = 5;
            const int MIN_MATCH = 3;

            // Base score
            int baseScore = matchCount * BASE_POINTS;

            // Match type multiplier
            float multiplier = 1f;
            switch (matchType)
            {
                case _Project.Scripts.Core.Enums.MatchType.Horizontal:
                case _Project.Scripts.Core.Enums.MatchType.Vertical:
                    multiplier = 1f;
                    break;
                case _Project.Scripts.Core.Enums.MatchType.Cross:
                    multiplier = 1.5f;
                    break;
            }

            int score = Mathf.RoundToInt(baseScore * multiplier);

            // Combo bonus (3'ten fazla ball varsa)
            if (matchCount > MIN_MATCH)
            {
                int extraBalls = matchCount - MIN_MATCH;
                score += extraBalls * COMBO_BONUS;
            }

            return score;
        }
        
        private void CheckLevelStatus()
        {
            // Zaten bittiyse kontrol etme
            if (_currentState != LevelState.Playing)
            {
                return;
            }

            // KAZANMA KONTROLÜ: Hedef skora ulaştı mı?
            if (_currentScore >= currentLevel.TargetScore)
            {
                CompleteLevelSuccess();
                return;
            }

            // KAYBETME KONTROLÜ: Hamle kalmadı mı?
            if (_remainingMoves <= 0)
            {
                CompleteLevelFailed();
                return;
            }

            // Devam ediyor
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

            // ✅ UI Panel'i otomatik göster
            ShowLevelCompletePanel(stars).Forget();
        }
        
        private void CompleteLevelFailed()
        {
            _currentState = LevelState.Failed;

            if (showDebugLogs)
            {
                Debug.Log($"[LevelManager] LEVEL FAILED! Score: {_currentScore}/{currentLevel.TargetScore}");
            }

            OnLevelFailed?.Invoke();

            // ✅ UI Panel'i otomatik göster
            ShowLevelFailedPanel().Forget();
        }

        private async UniTaskVoid ShowLevelCompletePanel(int stars)
        {
            await UniTask.Delay(500); // Biraz bekle (son animasyonlar bitsin)

            var completePanel = UIManager.Instance.GetPanel<LevelCompletePanel>();
            if (completePanel != null)
            {
                await completePanel.ShowWithStars(stars, _currentScore);
            }
        }
        
        private async UniTaskVoid ShowLevelFailedPanel()
        {
            await UniTask.Delay(500);

            var failedPanel = UIManager.Instance.GetPanel<LevelFailedPanel>();
            if (failedPanel != null)
            {
                await failedPanel.ShowWithScore(_currentScore, currentLevel.TargetScore);
            }
        }
        
        private int CalculateStars()
        {
            float scorePercentage = (float)_currentScore / currentLevel.TargetScore;

            if (scorePercentage >= 1.5f)
            {
                return 3; // ⭐⭐⭐
            }
            else if (scorePercentage >= 1.2f)
            {
                return 2; // ⭐⭐
            }
            else
            {
                return 1; // ⭐
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
        }
        
        public void LoadNextLevel()
        {
            // TODO: Level listesi olacak, sıradakini yükle
            Debug.Log("[LevelManager] Next level not implemented yet");
        }
        
        private void OnDestroy()
        {
            // Event'leri temizle (memory leak önleme)
            OnScoreChanged = null;
            OnMovesChanged = null;
            OnLevelCompleted = null;
            OnLevelFailed = null;
        }
    }
}