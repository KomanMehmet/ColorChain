using _Project.Scripts.Data;
using _Project.Scripts.Systems.Grid;
using UnityEngine;

namespace _Project.Scripts._Development
{
    public class LevelStarter : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private LevelData testLevelData;
        
        [Header("Auto Start")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private float startDelay = 0.5f;

        private void Start()
        {
            if (autoStart)
            {
                Invoke(nameof(StartLevel), startDelay);
            }
        }



        [ContextMenu("Start Level")]
        private void StartLevel()
        {
            if (GridManager.Instance == null)
            {
                Debug.LogError("GridManager not found in scene!");
                return;
            }

            if (testLevelData == null)
            {
                Debug.LogError("Test Level Data not assigned!");
                return;
            }

            Debug.Log($"Starting level: {testLevelData.LevelName}");
            GridManager.Instance.Initialize(testLevelData);
        }
    }
}