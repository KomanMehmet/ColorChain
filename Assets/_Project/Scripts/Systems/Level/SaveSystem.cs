using UnityEngine;

namespace _Project.Scripts.Systems.Level
{
    public static class SaveSystem
    {
        private const string LEVEL_UNLOCK_KEY = "Level_Unlocked_";
        private const string LEVEL_STARS_KEY = "Level_Stars_";
        private const string LEVEL_SCORE_KEY = "Level_Score_";
        
        public static void SaveLevelProgress(LevelProgress progress)
        {
            PlayerPrefs.SetInt(LEVEL_UNLOCK_KEY + progress.levelIndex, progress.isUnlocked ? 1 : 0);
            PlayerPrefs.SetInt(LEVEL_STARS_KEY + progress.levelIndex, progress.starsEarned);
            PlayerPrefs.SetInt(LEVEL_SCORE_KEY + progress.levelIndex, progress.highScore);
            PlayerPrefs.Save();
        }
        
        public static LevelProgress LoadLevelProgress(int levelIndex)
        {
            bool isUnlocked = PlayerPrefs.GetInt(LEVEL_UNLOCK_KEY + levelIndex, levelIndex == 0 ? 1 : 0) == 1;
            int stars = PlayerPrefs.GetInt(LEVEL_STARS_KEY + levelIndex, 0);
            int score = PlayerPrefs.GetInt(LEVEL_SCORE_KEY + levelIndex, 0);

            return new LevelProgress(levelIndex, isUnlocked, stars, score);
        }
        
        public static bool IsLevelUnlocked(int levelIndex)
        {
            if (levelIndex == 0) return true; // İlk level her zaman açık
            return PlayerPrefs.GetInt(LEVEL_UNLOCK_KEY + levelIndex, 0) == 1;
        }
        
        public static void UnlockLevel(int levelIndex)
        {
            PlayerPrefs.SetInt(LEVEL_UNLOCK_KEY + levelIndex, 1);
            PlayerPrefs.Save();
        }
        
        public static void ClearAllProgress()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
        
        public static int GetLevelStars(int levelIndex)
        {
            return PlayerPrefs.GetInt(LEVEL_STARS_KEY + levelIndex, 0);
        }
        
        public static int GetLevelHighScore(int levelIndex)
        {
            return PlayerPrefs.GetInt(LEVEL_SCORE_KEY + levelIndex, 0);
        }
    }
}