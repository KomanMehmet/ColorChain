using _Project.Scripts.Data;
using _Project.Scripts.Systems.Grid;
using _Project.Scripts.Systems.Level;
using UnityEngine;

namespace _Project.Scripts._Development
{
    public class LevelStarter : MonoBehaviour
    {
        [Header("Level Settings")]
        [SerializeField] private LevelData testLevelData;
        [SerializeField] private bool autoStart = true;


        private void Start()
        {
            if (autoStart)
            {
                StartLevel();
            }
        }

        public void StartLevel()
        {
            if (testLevelData == null)
            {
                Debug.LogError("[LevelStarter] No level data assigned!");
                return;
            }

            Debug.Log($"Starting level: {testLevelData.LevelName}");

            // GridManager'ı initialize et
            if (GridManager.Instance != null)
            {
                GridManager.Instance.Initialize(testLevelData);
            }

            // ✅ LevelManager'ı başlat
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.StartLevel(testLevelData);
            }
        }
    }
}