namespace _Project.Scripts.Systems.Level
{
    [System.Serializable]
    public class LevelProgress
    {
        public int levelIndex;
        public bool isUnlocked;
        public int starsEarned;
        public int highScore;

        public LevelProgress(int index)
        {
            levelIndex = index;
            isUnlocked = index == 0;
            starsEarned = 0;
            highScore = 0;
        }

        public LevelProgress(int index, bool unlocked, int stars, int score)
        {
            levelIndex = index;
            isUnlocked = unlocked;
            starsEarned = stars;
            highScore = score;
        }

        public void UpdateProgress(int stars, int score)
        {
            if (stars > starsEarned)
            {
                starsEarned = stars;
            }

            if (score > highScore)
            {
                highScore = score;
            }
        }
        
        public override string ToString()
        {
            return $"Level {levelIndex}: Unlocked={isUnlocked}, Stars={starsEarned}, HighScore={highScore}";
        }
    }
}