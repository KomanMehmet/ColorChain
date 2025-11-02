using _Project.Scripts.Data;
using UnityEngine;

namespace _Project.Scripts.Systems.Level
{
    public class GameplaySceneLoader : MonoBehaviour
    {
        public static LevelData PendingLevelData { get; set; }
        
        [Header("Prefabs")]
        [SerializeField] private GameObject gridManagerPrefab;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private void Start()
        {
            if (PendingLevelData != null)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[GameplaySceneLoader] Starting pending level: {PendingLevelData.LevelName}");
                }
                
                if (Grid.GridManager.Instance == null && gridManagerPrefab != null)
                {
                    Instantiate(gridManagerPrefab);
                }
                
                if (Grid.GridManager.Instance != null)
                {
                    Grid.GridManager.Instance.ClearGrid();
                    Grid.GridManager.Instance.Initialize(PendingLevelData);
                }

                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.StartLevel(PendingLevelData);
                }
                
                /*if (Grid.GridManager.Instance != null)
                {
                    Grid.GridManager.Instance.Initialize(PendingLevelData);
                }*/
                
                PendingLevelData = null;
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.LogWarning("[GameplaySceneLoader] No pending level data!");
                }
            }
        }
    }
}