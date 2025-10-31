using UnityEngine;

namespace _Project.Scripts.Runtime.Data.Settings
{
    [CreateAssetMenu(fileName = "GameData", menuName = "ColorChain/Data/Game Data")]
    public class GameData : ScriptableObject
    {
        [Header("Level Info")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int targetScore = 1000;
        [SerializeField] private int maxMoves = 20;
        
        [Header("Current Stats")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int currentMoves = 0;
        [SerializeField] private int combo = 0;
        
        public int CurrentLevel => currentLevel;
        public int TargetScore => targetScore;
        public int MaxMoves => maxMoves;
        public int CurrentScore => currentScore;
        public int CurrentMoves => currentMoves;
        public int Combo => combo;
        public int RemainingMoves => maxMoves - currentMoves;
        
        public void SetLevel(int level)
        {
            currentLevel = level;
            targetScore = 1000 + (level - 1) * 500; // Her level 500 puan artış
            maxMoves = 20 - Mathf.Min(level / 2, 5); // Levellar zorlaştıkça move azalır
        }
        
        public void AddScore(int score)
        {
            currentScore += score;
        }
        
        public void UseMove()
        {
            currentMoves++;
        }
        
        public void ResetStats()
        {
            currentScore = 0;
            currentMoves = 0;
            combo = 0;
        }
        
        public void IncrementCombo()
        {
            combo++;
        }
        
        public void ResetCombo()
        {
            combo = 0;
        }
        
        public bool IsLevelComplete()
        {
            return currentScore >= targetScore;
        }
        
        public bool IsLevelFailed()
        {
            return currentMoves >= maxMoves && currentScore < targetScore;
        }
    }
}