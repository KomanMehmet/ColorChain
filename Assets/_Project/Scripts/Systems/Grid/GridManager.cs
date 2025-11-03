using System.Collections.Generic;
using _Project.Scripts.Core.Enums;
using _Project.Scripts.Core.EventChannels;
using _Project.Scripts.Data;
using _Project.Scripts.Gameplay.Ball;
using _Project.Scripts.Systems.Match;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Systems.Grid
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
        
        private bool _isDestroying = false;

        [Header("Grid Settings")]
        [SerializeField] private int gridSize = 8;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;

        [Header("Ball Settings")]
        [SerializeField] private GameObject ballPrefab;
        [SerializeField] private BallData[] ballDataArray;

        [Header("References")]
        [SerializeField] private Transform ballContainer;

        [Header("Event Channels")]
        [SerializeField] private MatchEventChannel matchEventChannel;
        [SerializeField] private ComboEventChannel comboEventChannel;
        [SerializeField] private BallPopEventChannel ballPopEventChannel;
        [SerializeField] private GridProcessingCompleteEventChannel gridProcessingCompleteEventChannel;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private MatchChecker _matchChecker;
        private GridCell[,] _grid;
        
        private int _currentCombo = 0;
        private bool _isProcessing = false;
        private int _totalMatchesThisMove = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Application.targetFrameRate = 60;

            Instance = this;
        }

        public void Initialize(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("[GridManager] LevelData is null!");
                return;
            }

            ClearGrid();

            gridSize = levelData.GridSize;
            ballDataArray = GetBallDataFromColors(levelData.AvailableColors);

            _matchChecker = new MatchChecker(this);

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

            if (showDebugLogs)
            {
                Debug.Log($"[GridManager] Initialized: {gridSize}x{gridSize} grid");
            }
        }
        
        public void ClearGrid()
        {
            if (_isDestroying) return;

            if (showDebugLogs)
            {
                Debug.Log("[GridManager] Clearing grid...");
            }
            
            if (_grid != null)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        GridCell cell = _grid[x, y];
                
                        if (cell.OccupyingBall != null)
                        {
                            Destroy(cell.OccupyingBall.gameObject);
                            cell.ClearBall();
                        }
                    }
                }
            }
            
            if (ballContainer != null)
            {
                List<GameObject> childrenToDestroy = new List<GameObject>();
        
                foreach (Transform child in ballContainer)
                {
                    childrenToDestroy.Add(child.gameObject);
                }

                foreach (var child in childrenToDestroy)
                {
                    if (child != null)
                    {
                        Destroy(child);
                    }
                }
            }
            
            _grid = null;

            if (showDebugLogs)
            {
                Debug.Log("[GridManager] Grid cleared!");
            }
        }

        private Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
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
                    await SpawnBallWithoutMatch(pos);
                    await UniTask.Delay(50);
                }
            }

            if (showDebugLogs)
            {
                Debug.Log("[GridManager] Initial balls spawned");
            }
        }

        private async UniTask SpawnBallWithoutMatch(Vector2Int gridPos)
        {
            if (!IsValidPosition(gridPos))
            {
                return;
            }

            List<BallColor> bannedColors = new List<BallColor>();

            Vector2Int leftPos = gridPos + Vector2Int.left;
            if (IsValidPosition(leftPos))
            {
                GridCell leftCell = GetCell(leftPos);
                if (leftCell.OccupyingBall != null)
                {
                    Vector2Int leftLeft = leftPos + Vector2Int.left;
                    if (IsValidPosition(leftLeft))
                    {
                        GridCell leftLeftCell = GetCell(leftLeft);
                        if (leftLeftCell.OccupyingBall != null)
                        {
                            if (leftCell.OccupyingBall.Color == leftLeftCell.OccupyingBall.Color)
                            {
                                bannedColors.Add(leftCell.OccupyingBall.Color);
                            }
                        }
                    }
                }
            }

            Vector2Int downPos = gridPos + Vector2Int.down;
            if (IsValidPosition(downPos))
            {
                GridCell downCell = GetCell(downPos);
                if (downCell.OccupyingBall != null)
                {
                    Vector2Int downDown = downPos + Vector2Int.down;
                    if (IsValidPosition(downDown))
                    {
                        GridCell downDownCell = GetCell(downDown);
                        if (downDownCell.OccupyingBall != null)
                        {
                            if (downCell.OccupyingBall.Color == downDownCell.OccupyingBall.Color)
                            {
                                bannedColors.Add(downCell.OccupyingBall.Color);
                            }
                        }
                    }
                }
            }

            BallData ballData = GetRandomBallDataExcluding(bannedColors);
            GridCell cell = GetCell(gridPos);
            GameObject ballObj = Instantiate(ballPrefab, cell.WorldPosition, Quaternion.identity, ballContainer);
            Ball ball = ballObj.GetComponent<Ball>();

            ball.Initialize(gridPos, ballData);
            _grid[gridPos.x, gridPos.y].SetBall(ball);

            await UniTask.Yield();
        }

        private BallData GetRandomBallDataExcluding(List<BallColor> excludedColors)
        {
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

            if (availableData.Count == 0)
            {
                return ballDataArray[Random.Range(0, ballDataArray.Length)];
            }

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

        public List<MatchResult> CheckAllMatches()
        {
            if (_matchChecker == null)
            {
                Debug.LogWarning("[GridManager] MatchChecker not initialized!");
                return new List<MatchResult>();
            }

            return _matchChecker.CheckAllMatches();
        }

        public async UniTask ProcessMove(Vector2Int pos1, Vector2Int pos2)
        {
            if (_isDestroying) return;

            if (showDebugLogs)
            {
                Debug.Log($"[GridManager] Processing move: {pos1} <-> {pos2}");
            }
            
            _isProcessing = true;
            _totalMatchesThisMove = 0;
            
            List<MatchResult> matches = new List<MatchResult>();

            MatchResult match1 = CheckMatchAt(pos1);
            if (match1 != null)
            {
                matches.Add(match1);
                _totalMatchesThisMove++;
            }

            MatchResult match2 = CheckMatchAt(pos2);
            if (match2 != null && !matches.Contains(match2))
            {
                matches.Add(match2);
                _totalMatchesThisMove++;
            }

            if (matches.Count > 0)
            {
                await ProcessMatches(matches);

                NotifyProcessingComplete();
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.Log("[GridManager] No match found, reverting swap");
                }

                SwapCells(pos1, pos2);

                Ball ball1 = GetCell(pos1).OccupyingBall;
                Ball ball2 = GetCell(pos2).OccupyingBall;

                if (ball1 != null)
                {
                    await ball1.MoveTo(pos1, GetCell(pos1).WorldPosition);
                }

                if (ball2 != null)
                {
                    await ball2.MoveTo(pos2, GetCell(pos2).WorldPosition);
                }
                
                NotifyProcessingComplete();
            }
        }
        
        public async UniTask ProcessMatches(List<MatchResult> matches)
        {
            if (_isDestroying) return;
            
            if (matches == null || matches.Count == 0)
            {
                return;
            }

            if (showDebugLogs)
            {
                Debug.Log($"[GridManager] Processing {matches.Count} match(es)");
            }
            
            foreach (var match in matches)
            {
                GridCell cell = GetCell(match.MatchedPositions[0]);
                BallColor matchColor = cell.OccupyingBall != null 
                    ? cell.OccupyingBall.Color 
                    : BallColor.Red;

                if (matchEventChannel != null)
                {
                    MatchEventData data = new MatchEventData(
                        match.MatchCount,
                        match.MatchType,
                        match.MatchedPositions.ToArray(),
                        matchColor
                    );
                    
                    matchEventChannel.RaiseEvent(data);
                }
            }
            
            List<UniTask> popTasks = new List<UniTask>();

            foreach (var match in matches)
            {
                foreach (var pos in match.MatchedPositions)
                {
                    GridCell cell = GetCell(pos);
                    if (cell.OccupyingBall != null)
                    {
                        Ball ball = cell.OccupyingBall;

                        if (ballPopEventChannel != null)
                        {
                            BallPopEventData popData = new BallPopEventData(
                                ball.transform.position,
                                ball.GetComponent<SpriteRenderer>()?.color ?? Color.white,
                                match.MatchCount
                            );
                            
                            ballPopEventChannel.RaiseEvent(popData);
                        }

                        UniTask popTask = ball.Pop();
                        popTasks.Add(popTask);
                        ClearCell(pos);
                    }
                }
            }

            await UniTask.WhenAll(popTasks);
            await UniTask.Delay(200);

            await ApplyGravity();
            await FillEmptyCells();
            await CheckAndProcessChainReactions();

            if (showDebugLogs)
            {
                Debug.Log("[GridManager] Matches processed!");
            }
        }

        private async UniTask ApplyGravity()
        {
            if (_isDestroying) return;
            
            bool hasMoved;
            int maxIterations = gridSize;
            int iteration = 0;

            do
            {
                hasMoved = false;
                iteration++;

                for (int y = 0; y < gridSize - 1; y++)
                {
                    for (int x = 0; x < gridSize; x++)
                    {
                        Vector2Int currentPos = new Vector2Int(x, y);
                        Vector2Int abovePos = new Vector2Int(x, y + 1);

                        GridCell currentCell = GetCell(currentPos);
                        GridCell aboveCell = GetCell(abovePos);

                        if (currentCell.IsEmpty && aboveCell.IsOccupied)
                        {
                            Ball ball = aboveCell.OccupyingBall;

                            ClearCell(abovePos);
                            SetBallToCell(currentPos, ball);

                            ball.MoveTo(currentPos, currentCell.WorldPosition).Forget();

                            hasMoved = true;
                        }
                    }
                }

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

        private async UniTask FillEmptyCells()
        {
            if (_isDestroying) return;
            
            List<UniTask> spawnTasks = new List<UniTask>();

            for (int y = gridSize - 1; y >= 0; y--)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    GridCell cell = GetCell(pos);

                    if (cell.IsEmpty)
                    {
                        spawnTasks.Add(SpawnBallWithoutMatch(pos));
                    }
                }
            }

            await UniTask.WhenAll(spawnTasks);
        }

        private async UniTask CheckAndProcessChainReactions()
        {
            if (_isDestroying) return;
            
            int chainCount = 0;
            int maxChains = 10;
            int totalMatches = 0;

            _currentCombo = 0;

            while (chainCount < maxChains)
            {
                if (_isDestroying) return;

                await UniTask.Delay(300);

                List<MatchResult> chainMatches = CheckAllMatches();

                if (chainMatches == null || chainMatches.Count == 0)
                {
                    if (showDebugLogs)
                    {
                        Debug.Log($"[GridManager] No more chain reactions. Total chains: {chainCount}");
                    }

                    if (_currentCombo > 1 && comboEventChannel != null)
                    {
                        ComboEventData comboData = new ComboEventData(
                            _currentCombo,
                            totalMatches,
                            Vector3.zero
                        );
                        
                        comboEventChannel.RaiseEvent(comboData);

                        if (showDebugLogs)
                        {
                            Debug.Log($"[GridManager] Combo x{_currentCombo} completed!");
                        }
                    }

                    break;
                }

                chainCount++;
                _currentCombo++;
                totalMatches += chainMatches.Count;
                _totalMatchesThisMove += chainMatches.Count;

                if (showDebugLogs)
                {
                    Debug.Log($"[GridManager] Chain reaction {chainCount}! New matches: {chainMatches.Count} (Combo: {_currentCombo})");
                }

                foreach (var match in chainMatches)
                {
                    GridCell cell = GetCell(match.MatchedPositions[0]);
                    BallColor matchColor = cell.OccupyingBall != null 
                        ? cell.OccupyingBall.Color 
                        : BallColor.Red;

                    if (matchEventChannel != null)
                    {
                        MatchEventData data = new MatchEventData(
                            match.MatchCount,
                            match.MatchType,
                            match.MatchedPositions.ToArray(),
                            matchColor
                        );
                        
                        matchEventChannel.RaiseEvent(data);
                    }

                    List<UniTask> popTasks = new List<UniTask>();

                    foreach (var pos in match.MatchedPositions)
                    {
                        GridCell matchCell = GetCell(pos);
                        if (matchCell.OccupyingBall != null)
                        {
                            Ball ball = matchCell.OccupyingBall;

                            if (ballPopEventChannel != null)
                            {
                                BallPopEventData popData = new BallPopEventData(
                                    ball.transform.position,
                                    ball.GetComponent<SpriteRenderer>()?.color ?? Color.white,
                                    match.MatchCount
                                );
                                
                                ballPopEventChannel.RaiseEvent(popData);
                            }

                            UniTask popTask = ball.Pop();
                            popTasks.Add(popTask);
                            ClearCell(pos);
                        }
                    }

                    await UniTask.WhenAll(popTasks);
                }

                await UniTask.Delay(200);

                await ApplyGravity();
                await FillEmptyCells();
            }

            if (chainCount >= maxChains)
            {
                Debug.LogWarning($"[GridManager] Max chain limit reached ({maxChains})!");
            }
        }
        
        private void NotifyProcessingComplete()
        {
            if (gridProcessingCompleteEventChannel != null)
            {
                GridProcessingCompleteEventData data = new GridProcessingCompleteEventData(
                    _totalMatchesThisMove,
                    0, // Score GridManager'da tutulmuyor, LevelManager'da
                    _currentCombo > 0
                );

                gridProcessingCompleteEventChannel.RaiseEvent(data);

                if (showDebugLogs)
                {
                    Debug.Log($"[GridManager] Processing complete: {_totalMatchesThisMove} matches, Combo: {_currentCombo > 0}");
                }
            }

            _isProcessing = false;
            _totalMatchesThisMove = 0;
        }

        public bool IsValidPosition(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.x < gridSize &&
                   gridPos.y >= 0 && gridPos.y < gridSize;
        }

        public GridCell GetCell(Vector2Int gridPos)
        {
            if (!IsValidPosition(gridPos))
            {
                return default;
            }

            return _grid[gridPos.x, gridPos.y];
        }

        public void ClearCell(Vector2Int gridPos)
        {
            if (!IsValidPosition(gridPos))
            {
                return;
            }

            _grid[gridPos.x, gridPos.y].ClearBall();
        }

        public void SetBallToCell(Vector2Int gridPos, Ball ball)
        {
            if (!IsValidPosition(gridPos))
            {
                return;
            }

            _grid[gridPos.x, gridPos.y].SetBall(ball);
        }

        public void SwapCells(Vector2Int pos1, Vector2Int pos2)
        {
            if (!IsValidPosition(pos1) || !IsValidPosition(pos2))
            {
                return;
            }

            Ball ball1 = _grid[pos1.x, pos1.y].OccupyingBall;
            Ball ball2 = _grid[pos2.x, pos2.y].OccupyingBall;

            _grid[pos1.x, pos1.y].SetBall(ball2);
            _grid[pos2.x, pos2.y].SetBall(ball1);
        }

        public int GetGridSize() => gridSize;

        private BallData[] GetBallDataFromColors(BallColor[] colors)
        {
            BallData[] allBallData = Resources.LoadAll<BallData>("Data/Balls");
            List<BallData> result = new List<BallData>();

            foreach (var color in colors)
            {
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

        private void OnDestroy()
        {
            _isDestroying = true;
            
            if (_grid != null)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        GridCell cell = _grid[x, y];
                
                        if (cell.OccupyingBall != null)
                        {
                            Destroy(cell.OccupyingBall.gameObject);
                        }
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_grid == null) return;

            Gizmos.color = Color.gray;

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