using System.Collections.Generic;
using _Project.Scripts.Core.EventChannels;
using _Project.Scripts.Systems.UI.Enums;
using UnityEngine;

namespace _Project.Scripts.Systems.UI.Components
{
    public class ScorePopupManager : MonoBehaviour
    {
        public static ScorePopupManager Instance { get; private set; }
        
        [Header("Prefab")]
        [SerializeField] private GameObject scorePopupPrefab;
        
        [Header("Event Channels")]
        [SerializeField] private MatchEventChannel matchEventChannel;
        
        [Header("Pool Settings")]
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private Transform popupContainer;
        
        [Header("Score Settings")]
        [SerializeField] private int baseScore = 10;
        [SerializeField] private int comboBonus = 5;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;
        
        private Queue<ScorePopup> _pool = new Queue<ScorePopup>();
        private List<ScorePopup> _activePopups = new List<ScorePopup>();
        
        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Pool'u oluştur
            InitializePool();
        }
        
        private void OnEnable()
        {
            if (matchEventChannel != null)
            {
                matchEventChannel.Register(OnMatchEvent);
            }
        }
        
        private void OnDisable()
        {
            // ✅ Match event'ten unsubscribe ol
            if (matchEventChannel != null)
            {
                matchEventChannel.Unregister(OnMatchEvent);
            }
        }
        
        private void OnMatchEvent(MatchEventData data)
        {
            Vector3 centerWorldPosition = CalculateCenterPosition(data.positions);
            
            int score = CalculateScore(data.matchCount);
            
            ScorePopupType type = GetPopupType(data.matchCount);
            
            SpawnPopup(score, centerWorldPosition, type);

            if (showDebugLogs)
            {
                Debug.Log($"[ScorePopupManager] Spawned {data.positions.Length} popups for match");
            }
        }
        
        private Vector3 CalculateCenterPosition(Vector2Int[] positions)
        {
            if (positions == null || positions.Length == 0)
            {
                return Vector3.zero;
            }

            Vector3 sum = Vector3.zero;
            foreach (var pos in positions)
            {
                Vector3 worldPos = GridPosToWorldPos(pos);
                sum += worldPos;
            }

            return sum / positions.Length;
        }
        
        private void InitializePool()
        {
            if (scorePopupPrefab == null)
            {
                Debug.LogError("[ScorePopupManager] Score popup prefab is null!");
                return;
            }

            if (popupContainer == null)
            {
                GameObject container = new GameObject("PopupContainer");
                container.transform.SetParent(transform);
                popupContainer = container.transform;
            }

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePopup();
            }

            if (showDebugLogs)
            {
                Debug.Log($"[ScorePopupManager] Pool initialized with {initialPoolSize} popups");
            }
        }
        
        private ScorePopup CreatePopup()
        {
            GameObject obj = Instantiate(scorePopupPrefab, popupContainer);
            obj.SetActive(false);

            ScorePopup popup = obj.GetComponent<ScorePopup>();
            if (popup != null)
            {
                _pool.Enqueue(popup);
            }
            else
            {
                Debug.LogError("[ScorePopupManager] Prefab doesn't have ScorePopup component!");
                Destroy(obj);
            }

            return popup;
        }
        
        public void SpawnPopup(int score, Vector3 worldPosition, ScorePopupType type = ScorePopupType.Normal)
        {
            ScorePopup popup = GetPopup();
            if (popup == null) return;

            popup.gameObject.SetActive(true);
            _activePopups.Add(popup);

            popup.Show(score, worldPosition, type);
        }
        
        private ScorePopup GetPopup()
        {
            if (_pool.Count > 0)
            {
                return _pool.Dequeue();
            }

            // Pool boş, yeni oluştur
            if (showDebugLogs)
            {
                Debug.LogWarning("[ScorePopupManager] Pool empty, creating new popup");
            }

            return CreatePopup();
        }
        
        public void ReturnToPool(ScorePopup popup)
        {
            if (popup == null) return;

            popup.gameObject.SetActive(false);
            _activePopups.Remove(popup);
            _pool.Enqueue(popup);
        }
        
        private int CalculateScore(int matchCount)
        {
            int score = matchCount * baseScore;
            
            if (matchCount >= 4)
            {
                int extra = matchCount - 3;
                score += extra * comboBonus;
            }

            return score;
        }
        
        private ScorePopupType GetPopupType(int matchCount)
        {
            if (matchCount >= 5)
            {
                return ScorePopupType.Combo;
            }
            else if (matchCount >= 4)
            {
                return ScorePopupType.Bonus;
            }
            else
            {
                return ScorePopupType.Normal;
            }
        }
        
        private Vector3 GridPosToWorldPos(Vector2Int gridPos)
        {
            // GridManager'dan pozisyon al
            if (Grid.GridManager.Instance != null)
            {
                var cell = Grid.GridManager.Instance.GetCell(gridPos);
                return cell.WorldPosition;
            }

            return Vector3.zero;
        }

        private void OnDestroy()
        {
            // Event'ten unsubscribe
            if (matchEventChannel != null)
            {
                matchEventChannel.Unregister(OnMatchEvent);
            }
        }
    }
}