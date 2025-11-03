using _Project.Scripts.Data;
using _Project.Scripts.Systems.UI.Core;
using _Project.Scripts.Systems.UI.Panels;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Systems.Level
{
    public class GameplaySceneLoader : MonoBehaviour
    {
        public static LevelData PendingLevelData { get; set; }

        [Header("Development Settings")]
        [SerializeField] private LevelData defaultLevelForDevelopment;
        [SerializeField] private bool useDevelopmentMode = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private async void Start()
        {
            if (showDebugLogs)
            {
                Debug.Log("[GameplaySceneLoader] Start called");
            }

            LevelData levelToLoad = null;

            // Production mode: PendingLevelData varsa kullan
            if (PendingLevelData != null)
            {
                levelToLoad = PendingLevelData;
                
                if (showDebugLogs)
                {
                    Debug.Log($"[GameplaySceneLoader] Production mode - Loading: {levelToLoad.LevelName}");
                }
                
                PendingLevelData = null; // Temizle
            }
            // Development mode: Default level kullan
            else if (useDevelopmentMode && defaultLevelForDevelopment != null)
            {
                levelToLoad = defaultLevelForDevelopment;
                
                if (showDebugLogs)
                {
                    Debug.Log($"[GameplaySceneLoader] Development mode - Loading: {levelToLoad.LevelName}");
                }
            }
            else
            {
                Debug.LogError("[GameplaySceneLoader] No level to load! Set PendingLevelData or assign defaultLevelForDevelopment");
                return;
            }

            // Level'ı yükle
            await LoadLevel(levelToLoad);
        }

        private async UniTask LoadLevel(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("[GameplaySceneLoader] LevelData is null!");
                return;
            }

            // 1. Grid'i temizle ve initialize et
            if (Grid.GridManager.Instance != null)
            {
                if (showDebugLogs)
                {
                    Debug.Log("[GameplaySceneLoader] Clearing and initializing grid...");
                }

                Grid.GridManager.Instance.ClearGrid();
                Grid.GridManager.Instance.Initialize(levelData);
            }
            else
            {
                Debug.LogError("[GameplaySceneLoader] GridManager.Instance is NULL!");
            }

            // 2. LevelManager'ı başlat
            if (LevelManager.Instance != null)
            {
                if (showDebugLogs)
                {
                    Debug.Log("[GameplaySceneLoader] Starting level...");
                }

                LevelManager.Instance.StartLevel(levelData);
            }
            else
            {
                Debug.LogError("[GameplaySceneLoader] LevelManager.Instance is NULL!");
            }

            // 3. GameHUD'ı göster
            if (UIManager.Instance != null)
            {
                await UIManager.Instance.ShowPanel<GameHUD>(0f);
            }
            else
            {
                Debug.LogError("[GameplaySceneLoader] UIManager.Instance is NULL!");
            }

            if (showDebugLogs)
            {
                Debug.Log("[GameplaySceneLoader] Level loading complete!");
            }
        }
    }
}