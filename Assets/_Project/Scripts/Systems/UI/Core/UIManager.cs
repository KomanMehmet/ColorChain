using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Systems.UI.Core
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool hideAllOnStart = false;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Tüm paneller (type → instance mapping)
        private Dictionary<Type, UIPanel> _panels = new Dictionary<System.Type, UIPanel>();
        
        // Panel stack (sıralama için)
        private Stack<UIPanel> _panelStack = new Stack<UIPanel>();
        
        // Şu an gösterilen paneller
        private HashSet<UIPanel> _visiblePanels = new HashSet<UIPanel>();
        
        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Tüm child panelleri kaydet
            RegisterAllPanels();

            if (hideAllOnStart)
            {
                HideAllPanelsImmediate();
            }
        }
        
        /// <summary>
        /// Tüm child panelleri otomatik kaydet
        /// </summary>
        private void RegisterAllPanels()
        {
            UIPanel[] panels = GetComponentsInChildren<UIPanel>(true);

            foreach (var panel in panels)
            {
                System.Type panelType = panel.GetType();
                
                if (!_panels.ContainsKey(panelType))
                {
                    _panels.Add(panelType, panel);
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"[UIManager] Registered panel: {panelType.Name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[UIManager] Duplicate panel type: {panelType.Name}");
                }
            }

            if (showDebugLogs)
            {
                Debug.Log($"[UIManager] Total panels registered: {_panels.Count}");
            }
        }
        
        /// <summary>
        /// Panel göster (generic)
        /// </summary>
        public async UniTask ShowPanel<T>() where T : UIPanel
        {
            Type panelType = typeof(T);

            if (!_panels.TryGetValue(panelType, out UIPanel panel))
            {
                Debug.LogError($"[UIManager] Panel not found: {panelType.Name}");
                return;
            }

            if (panel.IsVisible)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[UIManager] Panel already visible: {panelType.Name}");
                }
                return;
            }

            // Stack'e ekle
            _panelStack.Push(panel);
            _visiblePanels.Add(panel);

            if (showDebugLogs)
            {
                Debug.Log($"[UIManager] Showing panel: {panelType.Name}");
            }

            await panel.Show();
        }
        
        /// <summary>
        /// Panel göster (özel animasyon süresiyle)
        /// </summary>
        public async UniTask ShowPanel<T>(float animationDuration) where T : UIPanel
        {
            System.Type panelType = typeof(T);

            if (!_panels.TryGetValue(panelType, out UIPanel panel))
            {
                Debug.LogError($"[UIManager] Panel not found: {panelType.Name}");
                return;
            }

            if (panel.IsVisible)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[UIManager] Panel already visible: {panelType.Name}");
                }
                return;
            }

            // Stack'e ekle
            _panelStack.Push(panel);
            _visiblePanels.Add(panel);

            if (showDebugLogs)
            {
                Debug.Log($"[UIManager] Showing panel: {panelType.Name} (custom duration: {animationDuration}s)");
            }

            await panel.Show(animationDuration);
        }
        
        /// <summary>
        /// Panel gizle (generic)
        /// </summary>
        public async UniTask HidePanel<T>() where T : UIPanel
        {
            Type panelType = typeof(T);

            if (!_panels.TryGetValue(panelType, out UIPanel panel))
            {
                Debug.LogError($"[UIManager] Panel not found: {panelType.Name}");
                return;
            }

            if (!panel.IsVisible)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[UIManager] Panel already hidden: {panelType.Name}");
                }
                return;
            }

            // Stack'ten çıkar
            if (_panelStack.Count > 0 && _panelStack.Peek() == panel)
            {
                _panelStack.Pop();
            }
            _visiblePanels.Remove(panel);

            if (showDebugLogs)
            {
                Debug.Log($"[UIManager] Hiding panel: {panelType.Name}");
            }

            await panel.Hide();
        }

        /// <summary>
        /// Panel gizle (özel animasyon süresiyle)
        /// </summary>
        public async UniTask HidePanel<T>(float animationDuration) where T : UIPanel
        {
            Type panelType = typeof(T);

            if (!_panels.TryGetValue(panelType, out UIPanel panel))
            {
                Debug.LogError($"[UIManager] Panel not found: {panelType.Name}");
                return;
            }

            if (!panel.IsVisible)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[UIManager] Panel already hidden: {panelType.Name}");
                }
                return;
            }

            // Stack'ten çıkar
            if (_panelStack.Count > 0 && _panelStack.Peek() == panel)
            {
                _panelStack.Pop();
            }
            _visiblePanels.Remove(panel);

            if (showDebugLogs)
            {
                Debug.Log($"[UIManager] Hiding panel: {panelType.Name} (custom duration: {animationDuration}s)");
            }

            await panel.Hide(animationDuration);
        }
        
        
        /// <summary>
        /// Panel referansını al
        /// </summary>
        public T GetPanel<T>() where T : UIPanel
        {
            Type panelType = typeof(T);

            if (_panels.TryGetValue(panelType, out UIPanel panel))
            {
                return panel as T;
            }

            Debug.LogError($"[UIManager] Panel not found: {panelType.Name}");
            return null;
        }
        
        /// <summary>
        /// Panel visible mı?
        /// </summary>
        public bool IsPanelVisible<T>() where T : UIPanel
        {
            Type panelType = typeof(T);

            if (_panels.TryGetValue(panelType, out UIPanel panel))
            {
                return panel.IsVisible;
            }

            return false;
        }
        
        /// <summary>
        /// Tüm panelleri gizle
        /// </summary>
        public async UniTask HideAllPanels()
        {
            List<UniTask> hideTasks = new List<UniTask>();

            foreach (var panel in _visiblePanels)
            {
                hideTasks.Add(panel.Hide());
            }

            await UniTask.WhenAll(hideTasks);

            _visiblePanels.Clear();
            _panelStack.Clear();
        }
        
        /// <summary>
        /// Tüm panelleri anında gizle (animasyonsuz)
        /// </summary>
        public void HideAllPanelsImmediate()
        {
            foreach (var panel in _panels.Values)
            {
                panel.HideImmediate();
            }

            _visiblePanels.Clear();
            _panelStack.Clear();
        }

        /// <summary>
        /// En üstteki paneli kapat (back button için)
        /// </summary>
        public async UniTask HideTopPanel()
        {
            if (_panelStack.Count == 0)
            {
                if (showDebugLogs)
                {
                    Debug.Log("[UIManager] No panel to hide (stack empty)");
                }
                return;
            }

            UIPanel topPanel = _panelStack.Pop();
            _visiblePanels.Remove(topPanel);

            if (showDebugLogs)
            {
                Debug.Log($"[UIManager] Hiding top panel: {topPanel.GetType().Name}");
            }

            await topPanel.Hide();
        }
        
        /// <summary>
        /// Kaç panel visible?
        /// </summary>
        public int GetVisiblePanelCount()
        {
            return _visiblePanels.Count;
        }
    }
}