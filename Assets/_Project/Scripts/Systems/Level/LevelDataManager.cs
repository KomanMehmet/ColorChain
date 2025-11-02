using System.Collections.Generic;
using _Project.Scripts.Data;
using UnityEngine;

namespace _Project.Scripts.Systems.Level
{
    public class LevelDataManager : MonoBehaviour
    {
        public static LevelDataManager Instance { get; private set; }
        
        [Header("Level Data")]
        [SerializeField] private LevelData[] allLevels;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private Dictionary<int, LevelProgress> _levelProgressMap = new Dictionary<int, LevelProgress>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadAllLevels();
        }
        
        private void LoadAllLevels()
        {
            if (allLevels == null || allLevels.Length == 0)
            {
                allLevels = Resources.LoadAll<LevelData>("Data/Levels");
            }
            
            for (int i = 0; i < allLevels.Length; i++)
            {
                LevelProgress progress = SaveSystem.LoadLevelProgress(i);
                _levelProgressMap[i] = progress;
            }

            if (showDebugLogs)
            {
                Debug.Log($"[LevelDataManager] Loaded {allLevels.Length} levels");
            }
        }
        
        public LevelData GetLevelData(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= allLevels.Length)
            {
                Debug.LogError($"[LevelDataManager] Invalid level index: {levelIndex}");
                return null;
            }

            return allLevels[levelIndex];
        }
        
        public LevelProgress GetLevelProgress(int levelIndex)
        {
            if (_levelProgressMap.ContainsKey(levelIndex))
            {
                return _levelProgressMap[levelIndex];
            }

            return new LevelProgress(levelIndex);
        }
        
        public void CompleteLevel(int levelIndex, int stars, int score)
        {
            if (!_levelProgressMap.ContainsKey(levelIndex))
            {
                _levelProgressMap[levelIndex] = new LevelProgress(levelIndex);
            }

            LevelProgress progress = _levelProgressMap[levelIndex];
            progress.UpdateProgress(stars, score);
            
            SaveSystem.SaveLevelProgress(progress);
            
            if (stars >= 1 && levelIndex < allLevels.Length - 1)
            {
                UnlockLevel(levelIndex + 1);
            }

            if (showDebugLogs)
            {
                Debug.Log($"[LevelDataManager] Level {levelIndex} completed: {stars} stars, {score} score");
            }
        }
        
        public void UnlockLevel(int levelIndex)
        {
            if (!_levelProgressMap.ContainsKey(levelIndex))
            {
                _levelProgressMap[levelIndex] = new LevelProgress(levelIndex);
            }

            _levelProgressMap[levelIndex].isUnlocked = true;
            SaveSystem.UnlockLevel(levelIndex);

            if (showDebugLogs)
            {
                Debug.Log($"[LevelDataManager] Level {levelIndex} unlocked!");
            }
        }
        
        public bool IsLevelUnlocked(int levelIndex)
        {
            if (levelIndex == 0) return true;

            if (_levelProgressMap.ContainsKey(levelIndex))
            {
                return _levelProgressMap[levelIndex].isUnlocked;
            }

            return false;
        }
        
        public int GetTotalLevels()
        {
            return allLevels != null ? allLevels.Length : 0;
        }
        
        [ContextMenu("Clear All Progress")]
        public void ClearAllProgress()
        {
            SaveSystem.ClearAllProgress();
            _levelProgressMap.Clear();
            LoadAllLevels();

            if (showDebugLogs)
            {
                Debug.Log("[LevelDataManager] All progress cleared!");
            }
        }
        
        public LevelData[] AllLevels => allLevels;
    }
}