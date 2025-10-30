using _Project.Scripts.Core.Enums;
using UnityEngine;

namespace _Project.Scripts.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "ColorChain/Data/Level Data", order = 1)]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        [SerializeField] private int levelNumber = 1;
        [SerializeField] private string levelName = "Level 1";
        
        [Header("Grid Settings")]
        [Range(6, 10)]
        [SerializeField] private int gridSize = 8;
        
        [Header("Gameplay Settings")]
        [SerializeField] private int maxMoves = 20;
        [SerializeField] private int targetScore = 1000;
        [SerializeField] private int twoStarScore = 1500;
        [SerializeField] private int threeStarScore = 2000;
        
        [Header("Ball Settings")]
        [SerializeField] private BallColor[] availableColors = 
        {
            BallColor.Red,
            BallColor.Blue,
            BallColor.Green
        };
        
        [Header("PowerUps")]
        [SerializeField] private bool powerUpsEnabled = false;
        [SerializeField] private int powerUpSpawnRate = 5;
        
        [Header("Special Blocks")]
        [Tooltip("Percentage of locked blocks (0-100)")]
        [Range(0, 100)]
        [SerializeField] private int lockedBlockPercentage = 0;
        
        public int LevelNumber => levelNumber;
        public string LevelName => levelName;
        public int GridSize => gridSize;
        public int MaxMoves => maxMoves;
        public int TargetScore => targetScore;
        public int TwoStarScore => twoStarScore;
        public int ThreeStarScore => threeStarScore;
        public BallColor[] AvailableColors => availableColors;
        public bool PowerUpsEnabled => powerUpsEnabled;
        public int PowerUpSpawnRate => powerUpSpawnRate;
        public int LockedBlockPercentage => lockedBlockPercentage;
        
        public int GetStarRating(int score)
        {
            if (score >= threeStarScore) return 3;
            if (score >= twoStarScore) return 2;
            if (score >= targetScore) return 1;
            return 0;
        }
        
        private void OnValidate()
        {
            // Level number pozitif olmalı
            if (levelNumber < 1)
            {
                levelNumber = 1;
            }

            // Grid size çift sayı olmalı (optional ama güzel görünür)
            if (gridSize % 2 != 0)
            {
                gridSize = Mathf.Clamp(gridSize + 1, 6, 10);
            }

            // Max moves en az 5 olmalı
            if (maxMoves < 5)
            {
                maxMoves = 5;
            }

            // Skor thresholdları mantıklı mı?
            if (twoStarScore <= targetScore)
            {
                twoStarScore = targetScore + 500;
                Debug.LogWarning("[LevelData] Two-star score must be higher than target score!");
            }

            if (threeStarScore <= twoStarScore)
            {
                threeStarScore = twoStarScore + 500;
                Debug.LogWarning("[LevelData] Three-star score must be higher than two-star score!");
            }

            // En az 2 renk olmalı
            if (availableColors == null || availableColors.Length < 2)
            {
                availableColors = new BallColor[] { BallColor.Red, BallColor.Blue, BallColor.Green };
                Debug.LogWarning("[LevelData] At least 2 colors required!");
            }
        }

        public override string ToString()
        {
            return $"Level {levelNumber}: {levelName} | Grid: {gridSize}x{gridSize} | Moves: {maxMoves} | Target: {targetScore}";
        }
    }
}