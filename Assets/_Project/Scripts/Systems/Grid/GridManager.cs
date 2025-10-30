using System.Collections.Generic;
using _Project.Scripts.Core.Enums;
using _Project.Scripts.Data;
using _Project.Scripts.Gameplay.Ball;
using _Project.Scripts.Systems.Level;
using _Project.Scripts.Systems.Match;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Systems.Grid
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Grid Settings")] [SerializeField]
        private int gridSize = 8;

        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;

        [Header("Ball Settings")] [SerializeField]
        private GameObject ballPrefab;

        [SerializeField] private BallData[] ballDataArray;

        [Header("References")] [SerializeField]
        private Transform ballContainer;

        private MatchChecker _matchChecker;

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
            // LevelData'dan ayarları al
            gridSize = levelData.GridSize;
            ballDataArray = GetBallDataFromColors(levelData.AvailableColors);

            // ✅ MatchChecker oluştur
            _matchChecker = new MatchChecker(this);

            // Grid array'ini oluştur
            _grid = new GridCell[gridSize, gridSize];

            // Her hücreyi oluştur
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector3 worldPos = GridToWorldPosition(gridPos);
                    _grid[x, y] = new GridCell(gridPos, worldPos);
                }
            }

            // İlk topları spawn et
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
                    Vector2Int pos = new Vector2Int(x, y);

                    // ✅ Match oluşturmayan bir ball spawn et
                    await SpawnBallWithoutMatch(pos);

                    // Küçük delay (görsel olarak güzel durur)
                    await UniTask.Delay(50);
                }
            }
        }

        private async UniTask SpawnBallWithoutMatch(Vector2Int gridPos)
        {
            if (!IsValidPosition(gridPos))
            {
                return;
            }

            // Pozisyonu kontrol et: sol ve aşağı komşular
            List<BallColor> bannedColors = new List<BallColor>();

            // Sol komşuya bak (x-1)
            Vector2Int leftPos = gridPos + Vector2Int.left;
            if (IsValidPosition(leftPos))
            {
                GridCell leftCell = GetCell(leftPos);
                if (leftCell.OccupyingBall != null)
                {
                    // Sol komşunun soluna bak (x-2)
                    Vector2Int leftLeft = leftPos + Vector2Int.left;
                    if (IsValidPosition(leftLeft))
                    {
                        GridCell leftLeftCell = GetCell(leftLeft);
                        if (leftLeftCell.OccupyingBall != null)
                        {
                            // 2 aynı renk yan yana → 3. olmasın
                            if (leftCell.OccupyingBall.Color == leftLeftCell.OccupyingBall.Color)
                            {
                                bannedColors.Add(leftCell.OccupyingBall.Color);
                            }
                        }
                    }
                }
            }

            // Alt komşuya bak (y-1)
            Vector2Int downPos = gridPos + Vector2Int.down;
            if (IsValidPosition(downPos))
            {
                GridCell downCell = GetCell(downPos);
                if (downCell.OccupyingBall != null)
                {
                    // Alt komşunun altına bak (y-2)
                    Vector2Int downDown = downPos + Vector2Int.down;
                    if (IsValidPosition(downDown))
                    {
                        GridCell downDownCell = GetCell(downDown);
                        if (downDownCell.OccupyingBall != null)
                        {
                            // 2 aynı renk alt alta → 3. olmasın
                            if (downCell.OccupyingBall.Color == downDownCell.OccupyingBall.Color)
                            {
                                bannedColors.Add(downCell.OccupyingBall.Color);
                            }
                        }
                    }
                }
            }

            // Yasaklı olmayan bir renk seç
            BallData ballData = GetRandomBallDataExcluding(bannedColors);

            // Ball spawn et
            GridCell cell = GetCell(gridPos);
            GameObject ballObj = Instantiate(ballPrefab, cell.WorldPosition, Quaternion.identity, ballContainer);
            Ball ball = ballObj.GetComponent<Ball>();

            ball.Initialize(gridPos, ballData);
            _grid[gridPos.x, gridPos.y].SetBall(ball);

            await UniTask.Yield();
        }

        private BallData GetRandomBallDataExcluding(List<BallColor> excludedColors)
        {
            // Kullanılabilir data'ları filtrele
            List<BallData> availableData = new List<BallData>();
            float totalWeight = 0f;

            foreach (var data in ballDataArray)
            {
                if (!excludedColors.Contains(data.Color))
                {
                    availableData.Add(data);
                    totalWeight += data.SpawnWeight;
                }
            }

            // Eğer hiç kullanılabilir renk yoksa (çok nadir), herhangi birini ver
            if (availableData.Count == 0)
            {
                return ballDataArray[Random.Range(0, ballDataArray.Length)];
            }

            // Weighted random
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var data in availableData)
            {
                currentWeight += data.SpawnWeight;
                if (randomValue <= currentWeight)
                {
                    return data;
                }
            }

            return availableData[0];
        }

        public MatchResult CheckMatchAt(Vector2Int gridPos)
        {
            if (_matchChecker == null)
            {
                Debug.LogWarning("[GridManager] MatchChecker not initialized!");
                return null;
            }

            return _matchChecker.CheckMatchAt(gridPos);
        }

        /// <summary>
        /// Tüm grid'de match kontrol et
        /// </summary>
        /// <returns>Bulunan tüm matchler</returns>
        public List<MatchResult> CheckAllMatches()
        {
            if (_matchChecker == null)
            {
                Debug.LogWarning("[GridManager] MatchChecker not initialized!");
                return new List<MatchResult>();
            }

            return _matchChecker.CheckAllMatches();
        }

        public async UniTask ProcessMatches(List<MatchResult> matches)
        {
            if (matches == null || matches.Count == 0)
            {
                return;
            }

            Debug.Log($"[GridManager] Processing {matches.Count} match(es)");
            
            foreach (var match in matches)
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.OnMatchCompleted(match.MatchCount, match.MatchType);
                }
            }

            // Her match için
            foreach (var match in matches)
            {
                Debug.Log($"[GridManager] Match: {match}");

                List<UniTask> popTasks = new List<UniTask>();

                foreach (var pos in match.MatchedPositions)
                {
                    GridCell cell = GetCell(pos);
                    if (cell.OccupyingBall != null)
                    {
                        UniTask popTask = cell.OccupyingBall.Pop();
                        popTasks.Add(popTask);
                        ClearCell(pos);
                    }
                }

                await UniTask.WhenAll(popTasks);
            }

            await UniTask.Delay(200);

            // ✅ GRAVITY: Topları düşür
            await ApplyGravity();

            // ✅ Boş hücreleri doldur
            await FillEmptyCells();

            // ✅ Tekrar match kontrol et (chain reaction)
            await CheckAndProcessChainReactions();

            Debug.Log("[GridManager] Matches processed!");
        }

        /// <summary>
        /// Gravity uygula: Topları aşağı düşür
        /// </summary>
        private async UniTask ApplyGravity()
        {
            bool hasMoved;
            int maxIterations = gridSize; // Sonsuz döngü önleme
            int iteration = 0;

            do
            {
                hasMoved = false;
                iteration++;

                // Alttan üste doğru tara
                for (int y = 0; y < gridSize - 1; y++)
                {
                    for (int x = 0; x < gridSize; x++)
                    {
                        Vector2Int currentPos = new Vector2Int(x, y);
                        Vector2Int abovePos = new Vector2Int(x, y + 1);

                        GridCell currentCell = GetCell(currentPos);
                        GridCell aboveCell = GetCell(abovePos);

                        // Şu an boş ve üstte ball var mı?
                        if (currentCell.IsEmpty && aboveCell.IsOccupied)
                        {
                            Ball ball = aboveCell.OccupyingBall;

                            // Grid'i güncelle
                            ClearCell(abovePos);
                            SetBallToCell(currentPos, ball);

                            // Animasyon (paralel olabilir)
                            ball.MoveTo(currentPos, currentCell.WorldPosition).Forget();

                            hasMoved = true;
                        }
                    }
                }

                // Bir frame bekle (animasyonlar için)
                if (hasMoved)
                {
                    await UniTask.Delay(100);
                }
            } while (hasMoved && iteration < maxIterations);

            if (iteration >= maxIterations)
            {
                Debug.LogWarning("[GridManager] Gravity max iterations reached!");
            }
        }

        /// <summary>
        /// Boş hücreleri yukarıdan yeni toplarla doldur
        /// </summary>
        private async UniTask FillEmptyCells()
        {
            List<UniTask> spawnTasks = new List<UniTask>();

            // Yukarıdan aşağı tara
            for (int y = gridSize - 1; y >= 0; y--)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    GridCell cell = GetCell(pos);

                    if (cell.IsEmpty)
                    {
                        // Yeni ball spawn et (match oluşturmadan)
                        spawnTasks.Add(SpawnBallWithoutMatch(pos));
                    }
                }
            }

            await UniTask.WhenAll(spawnTasks);
        }

        /// <summary>
        /// Chain reaction: Gravity sonrası yeni match var mı kontrol et
        /// </summary>
        private async UniTask CheckAndProcessChainReactions()
        {
            int chainCount = 0;
            int maxChains = 10; // Sonsuz döngü önleme

            while (chainCount < maxChains)
            {
                // Tüm grid'de match ara
                List<MatchResult> newMatches = CheckAllMatches();

                if (newMatches.Count == 0)
                {
                    // Artık match yok, dur
                    break;
                }

                chainCount++;
                Debug.Log($"[GridManager] Chain reaction {chainCount}: {newMatches.Count} match(es) found!");

                // Yeni match'leri işle (recursive değil, aynı process)
                foreach (var match in newMatches)
                {
                    List<UniTask> popTasks = new List<UniTask>();

                    foreach (var pos in match.MatchedPositions)
                    {
                        GridCell cell = GetCell(pos);
                        if (cell.OccupyingBall != null)
                        {
                            UniTask popTask = cell.OccupyingBall.Pop();
                            popTasks.Add(popTask);
                            ClearCell(pos);
                        }
                    }

                    await UniTask.WhenAll(popTasks);
                }

                await UniTask.Delay(200);

                // Tekrar gravity ve fill
                await ApplyGravity();
                await FillEmptyCells();
            }

            if (chainCount >= maxChains)
            {
                Debug.LogWarning("[GridManager] Max chain reactions reached!");
            }
            else if (chainCount > 0)
            {
                Debug.Log($"[GridManager] Chain reaction completed after {chainCount} chains!");
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
            GridCell cell = GetCell(gridPos); // ✅ Kopya al
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
            _grid[gridPos.x, gridPos.y].SetBall(ball); // ✅ Bu şekilde değiştir

            await UniTask.Yield(); // Bir frame bekle
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
                return default; // Default GridCell (boş)
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

        public void ClearCell(Vector2Int gridPos)
        {
            if (!IsValidPosition(gridPos))
            {
                Debug.LogWarning($"[GridManager] Invalid position: {gridPos}");
                return;
            }

            _grid[gridPos.x, gridPos.y].ClearBall();
        }

        public void SetBallToCell(Vector2Int gridPos, Ball ball)
        {
            if (!IsValidPosition(gridPos))
            {
                Debug.LogWarning($"[GridManager] Invalid position: {gridPos}");
                return;
            }

            _grid[gridPos.x, gridPos.y].SetBall(ball);
        }

        public void SwapCells(Vector2Int pos1, Vector2Int pos2)
        {
            if (!IsValidPosition(pos1) || !IsValidPosition(pos2))
            {
                Debug.LogWarning($"[GridManager] Invalid swap positions: {pos1}, {pos2}");
                return;
            }

            // İki ball'ı al
            Ball ball1 = _grid[pos1.x, pos1.y].OccupyingBall;
            Ball ball2 = _grid[pos2.x, pos2.y].OccupyingBall;

            // Swap
            _grid[pos1.x, pos1.y].SetBall(ball2);
            _grid[pos2.x, pos2.y].SetBall(ball1);
        }
    }
}