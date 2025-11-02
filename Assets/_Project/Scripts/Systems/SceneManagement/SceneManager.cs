using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Systems.SceneManagement
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }
        
        [Header("Scene Name")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameplaySceneName = "Gameplay";
        
        [Header("Transition")]
        [SerializeField] private SceneTransition sceneTransition;
        [SerializeField] private float transitionDuration = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private string _currentSceneName;
        
        //Properties
        public string CurrentSceneName => _currentSceneName;
        public string MainMenuSceneName => mainMenuSceneName;
        public string GameplaySceneName => gameplaySceneName;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (showDebugLogs)
            {
                Debug.Log($"[SceneManager] Initialized in scene: {_currentSceneName}");
            }
        }

        public void LoadScene(string sceneName)
        {
            LoadSceneAsync(sceneName).Forget();
        }

        public async UniTask LoadSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneManager] Scene name is null or empty!");
                return;
            }

            if (showDebugLogs)
            {
                Debug.Log($"[SceneManager] Loading scene: {sceneName}");
            }

            if (sceneTransition != null)
            {
                await sceneTransition.FadeOut(transitionDuration);
            }

            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone)
            {
                await UniTask.Yield();
            }

            _currentSceneName = sceneName;

            if (sceneTransition != null)
            {
                await sceneTransition.FadeIn(transitionDuration);
            }
            
            if (showDebugLogs)
            {
                Debug.Log($"[SceneManager] Scene loaded: {sceneName}");
            }
        }

        public void LoadMainMenu()
        {
            LoadScene(mainMenuSceneName);
        }
        
        public void LoadGameplay()
        {
            LoadScene(gameplaySceneName);
        }

        public void ReloadCurrentScene()
        {
            LoadScene(_currentSceneName);
        }

        public void QuitGame()
        {
            if (showDebugLogs)
            {
                Debug.Log("[SceneManager] Quitting game...");
            }
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}