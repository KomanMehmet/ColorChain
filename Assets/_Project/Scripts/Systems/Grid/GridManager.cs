using System.Collections.Generic;
using _Project.Scripts.Core.Enums;
using _Project.Scripts.Data;
using _Project.Scripts.Gameplay.Ball;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Systems.Grid
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
        
        [Header("Grid Settings")]
        [SerializeField] private int gridSize = 8;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        
        [Header("Ball Settings")]
        [SerializeField] private GameObject ballPrefab; 
        [SerializeField] private BallData[] ballDataArray;
        
        [Header("References")]
        [SerializeField] private Transform ballContainer;
        
        private GridCell[,] _grid;
        
        private Queue<Ball> _ballPool = new Queue<Ball>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public void Initialize(LevelData levelData)
        {
            gridSize = levelData.GridSize;
            ballDataArray = GetBallDataFromColors(levelData.AvailableColors);
            
            _grid = new GridCell[gridSize, gridSize];
            
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    
                    Vector3 worldPos = GridToWorldPosition(gridPos);
                    
                    _grid[x, y] = new GridCell(gridPos, worldPos);
                }
            }

            SpawnInitialBalls().Forget();
        }
        
        private Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            // Grid'i merkeze almak için offset hesapla
            float offsetX = (gridSize - 1) * cellSize * 0.5f;
            float offsetY = (gridSize - 1) * cellSize * 0.5f;

            float worldX = gridPos.x * cellSize - offsetX + gridOrigin.x;
            float worldY = gridPos.y * cellSize - offsetY + gridOrigin.y;

            return new Vector3(worldX, worldY, 0f);
        }
        
        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            float offsetX = (gridSize - 1) * cellSize * 0.5f;
            float offsetY = (gridSize - 1) * cellSize * 0.5f;

            int x = Mathf.RoundToInt((worldPos.x - gridOrigin.x + offsetX) / cellSize);
            int y = Mathf.RoundToInt((worldPos.y - gridOrigin.y + offsetY) / cellSize);

            return new Vector2Int(x, y);
        }
        
        private async UniTaskVoid SpawnInitialBalls()
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    // Random top spawn et
                    await SpawnBallAt(new Vector2Int(x, y));
                    
                    // Küçük delay (görsel olarak güzel durur)
                    await UniTask.Delay(50);  // 50ms
                }
            }
        }
        
        public async UniTask SpawnBallAt(Vector2Int gridPos)
        {
            // Pozisyon geçerli mi?
            if (!IsValidPosition(gridPos))
            {
                Debug.LogWarning($"Invalid grid position: {gridPos}");
                return;
            }

            // Hücre boş mu? (ref kullanmadan kontrol)
            GridCell cell = GetCell(gridPos);  // ✅ Kopya al
            if (!cell.IsEmpty)
            {
                Debug.LogWarning($"Cell already occupied: {gridPos}");
                return;
            }

            // Random BallData seç (spawn weight'e göre)
            BallData ballData = GetRandomBallData();

            // Ball prefab'ı instantiate et
            GameObject ballObj = Instantiate(ballPrefab, cell.WorldPosition, Quaternion.identity, ballContainer);
            Ball ball = ballObj.GetComponent<Ball>();

            // Ball'ı initialize et
            ball.Initialize(gridPos, ballData);

            // GridCell'e kaydet (direkt array erişimi)
            _grid[gridPos.x, gridPos.y].SetBall(ball);  // ✅ Bu şekilde değiştir

            await UniTask.Yield();  // Bir frame bekle
        }
        
        private BallData GetRandomBallData()
        {
            // Toplam weight hesapla
            float totalWeight = 0f;
            foreach (var data in ballDataArray)
            {
                totalWeight += data.SpawnWeight;
            }

            // Random değer
            float randomValue = Random.Range(0f, totalWeight);

            // Hangi data'yı seçeceğimizi bul
            float currentWeight = 0f;
            foreach (var data in ballDataArray)
            {
                currentWeight += data.SpawnWeight;
                if (randomValue <= currentWeight)
                {
                    return data;
                }
            }

            // Fallback (olmamalı ama safety)
            return ballDataArray[0];
        }
        
        public bool IsValidPosition(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.x < gridSize &&
                   gridPos.y >= 0 && gridPos.y < gridSize;
        }
        
        public ref GridCell GetCellRef(Vector2Int gridPos)
        {
            return ref _grid[gridPos.x, gridPos.y];
        }
        
        public GridCell GetCell(Vector2Int gridPos)
        {
            if (!IsValidPosition(gridPos))
            {
                return default;  // Default GridCell (boş)
            }

            return _grid[gridPos.x, gridPos.y];
        }
        
        private BallData[] GetBallDataFromColors(BallColor[] colors)
        {
            // Resources klasöründen BallData'ları yükle
            BallData[] allBallData = Resources.LoadAll<BallData>("Data/Balls");

            List<BallData> result = new List<BallData>();

            foreach (var color in colors)
            {
                // İlgili renkteki BallData'yı bul
                foreach (var data in allBallData)
                {
                    if (data.Color == color)
                    {
                        result.Add(data);
                        break;
                    }
                }
            }

            return result.ToArray();
        }
        
        public int GetGridSize() => gridSize;
        
        private void OnDrawGizmos()
        {
            if (_grid == null) return;

            Gizmos.color = Color.gray;

            // Grid çizgilerini çiz
            for (int x = 0; x <= gridSize; x++)
            {
                Vector3 start = GridToWorldPosition(new Vector2Int(x, 0)) - Vector3.up * cellSize * 0.5f;
                Vector3 end = GridToWorldPosition(new Vector2Int(x, gridSize - 1)) + Vector3.up * cellSize * 0.5f;
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= gridSize; y++)
            {
                Vector3 start = GridToWorldPosition(new Vector2Int(0, y)) - Vector3.right * cellSize * 0.5f;
                Vector3 end = GridToWorldPosition(new Vector2Int(gridSize - 1, y)) + Vector3.right * cellSize * 0.5f;
                Gizmos.DrawLine(start, end);
            }

            // Dolu hücreleri yeşil, boş hücreleri kırmızı göster
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    GridCell cell = _grid[x, y];
                    Gizmos.color = cell.IsOccupied ? Color.green : Color.red;
                    Gizmos.DrawWireCube(cell.WorldPosition, Vector3.one * cellSize * 0.8f);
                }
            }
        }
    }
}