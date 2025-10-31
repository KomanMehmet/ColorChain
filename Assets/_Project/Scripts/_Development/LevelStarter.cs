using _Project.Scripts.Data;
using _Project.Scripts.Systems.Grid;
using _Project.Scripts.Systems.Level;
using _Project.Scripts.Systems.UI.Core;
using _Project.Scripts.Systems.UI.Panels;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts._Development
{
    public class LevelStarter : MonoBehaviour
    {
        [Header("Level Settings")]
        [SerializeField] private LevelData testLevelData;
        [SerializeField] private bool autoStart = true;


        private async void Start()
        {
            if (autoStart)
            {
                await StartLevel();
            }
        }

        public async UniTask StartLevel()
        {
            if (testLevelData == null)
            {
                Debug.LogError("[LevelStarter] No level data assigned!");
                return;
            }

            Debug.Log($"Starting level: {testLevelData.LevelName}");

            // ✅ 1. LevelManager'ı başlat
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.StartLevel(testLevelData);
            }

            // ✅ 2. GameHUD'ı instant göster (0 saniye, async pattern)
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPanel<GameHUD>(0f).Forget();
            }

            // ✅ 3. GridManager'ı initialize et (arka planda spawn)
            if (GridManager.Instance != null)
            {
                GridManager.Instance.Initialize(testLevelData);
            }

            await UniTask.Yield();
        }
    }
}