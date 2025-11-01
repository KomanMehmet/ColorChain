using System.Collections.Generic;
using _Project.Scripts.Gameplay.Ball;
using _Project.Scripts.Systems.Grid;
using _Project.Scripts.Systems.Level;
using _Project.Scripts.Systems.Match;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Systems.Input
{
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler Instance { get; private set; }
        
        private bool _isDestroying = false;

        [Header("Input Settings")]
        [SerializeField] private float minSwipeDistance = 50f;
        [SerializeField] private float maxSwipeTime = 1f;

        [Header("Visual Feedback")]
        [SerializeField] private bool highlightSelectedBall = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private PlayerInputActions _inputActions;
        private Camera _mainCamera;
        private Ball _selectedBall;
        private Vector2 _touchStartPos;
        private float _touchStartTime;
        private bool _isTouching;
        private bool _inputEnabled = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogError("[InputHandler] Main Camera not found!");
            }

            _inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
            _inputActions.Gameplay.Touch.started += OnTouchStarted;
            _inputActions.Gameplay.Touch.canceled += OnTouchEnded;

            if (showDebugLogs)
            {
                Debug.Log("[InputHandler] Input Actions enabled and subscribed");
            }
        }

        private void OnDisable()
        {
            _inputActions.Gameplay.Touch.started -= OnTouchStarted;
            _inputActions.Gameplay.Touch.canceled -= OnTouchEnded;
            _inputActions.Disable();

            if (showDebugLogs)
            {
                Debug.Log("[InputHandler] Input Actions disabled and unsubscribed");
            }
        }

        private void OnDestroy()
        {
            _isDestroying = true;
            _inputActions?.Dispose();
        }

        private void OnTouchStarted(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;
            if (GridManager.Instance == null) return;

            Vector2 touchPosition = _inputActions.Gameplay.TouchPosition.ReadValue<Vector2>();
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(touchPosition);
            worldPos.z = 0f;

            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);

            if (hitCollider != null)
            {
                Ball ball = hitCollider.GetComponent<Ball>();

                if (ball != null)
                {
                    SelectBall(ball);
                    _touchStartPos = touchPosition;
                    _touchStartTime = Time.time;
                    _isTouching = true;

                    if (showDebugLogs)
                    {
                        Debug.Log($"[InputHandler] Touch started at {ball.GridPosition}");
                    }
                }
            }
        }

        private void OnTouchEnded(InputAction.CallbackContext context)
        {
            if (!_isTouching) return;

            _isTouching = false;

            if (_selectedBall == null) return;

            float touchDuration = Time.time - _touchStartTime;
            if (touchDuration > maxSwipeTime)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[InputHandler] Touch too slow ({touchDuration:F2}s), cancelled");
                }

                DeselectBall();
                return;
            }

            Vector2 touchEndPos = _inputActions.Gameplay.TouchPosition.ReadValue<Vector2>();
            Vector2 swipeDelta = touchEndPos - _touchStartPos;
            float swipeDistance = swipeDelta.magnitude;

            if (swipeDistance < minSwipeDistance)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[InputHandler] Tap detected (distance: {swipeDistance:F0}px), no move");
                }

                return;
            }

            Vector2Int direction = GetSwipeDirection(swipeDelta);

            if (showDebugLogs)
            {
                Debug.Log($"[InputHandler] Swipe detected: {direction} (distance: {swipeDistance:F0}px, time: {touchDuration:F2}s)");
            }

            ProcessMove(direction).Forget();
        }

        private Vector2Int GetSwipeDirection(Vector2 swipeDelta)
        {
            float absX = Mathf.Abs(swipeDelta.x);
            float absY = Mathf.Abs(swipeDelta.y);

            if (absX > absY)
            {
                return swipeDelta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                return swipeDelta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
        }

        /// <summary>
        /// ✅ GÜNCELLEME: GridManager.ProcessMove() kullan
        /// </summary>
        private async UniTaskVoid ProcessMove(Vector2Int direction)
        {
            if (_isDestroying) return;
            
            if (_selectedBall == null) return;

            Vector2Int currentPos = _selectedBall.GridPosition;
            Vector2Int targetPos = currentPos + direction;

            if (!GridManager.Instance.IsValidPosition(targetPos))
            {
                if (showDebugLogs)
                {
                    Debug.Log("[InputHandler] Target position out of bounds");
                }

                DeselectBall();
                return;
            }

            GridCell targetCell = GridManager.Instance.GetCell(targetPos);
            
            // ✅ Input'u kapat (processing bitene kadar)
            _inputEnabled = false;

            if (targetCell.IsOccupied)
            {
                Ball targetBall = targetCell.OccupyingBall;

                if (showDebugLogs)
                {
                    Debug.Log($"[InputHandler] Swapping balls: {currentPos} ↔ {targetPos}");
                }

                // ✅ Önce hamlede hamle sayısını azalt
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.OnMoveCompleted();
                }

                // Swap
                GridManager.Instance.SwapCells(currentPos, targetPos);

                // Animasyonlar
                var task1 = _selectedBall.MoveTo(targetPos, targetCell.WorldPosition);
                var task2 = targetBall.MoveTo(currentPos, GridManager.Instance.GetCell(currentPos).WorldPosition);
                await UniTask.WhenAll(task1, task2);

                // ✅ GridManager.ProcessMove() ile tüm işlemi yap
                await GridManager.Instance.ProcessMove(currentPos, targetPos);

                // ✅ Deselect
                DeselectBall();
            }
            else
            {
                // Hedef boş: Normal hareket
                if (showDebugLogs)
                {
                    Debug.Log($"[InputHandler] Moving to empty cell: {currentPos} → {targetPos}");
                }

                // ✅ Hamlede hamle sayısını azalt
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.OnMoveCompleted();
                }

                GridManager.Instance.ClearCell(currentPos);
                GridManager.Instance.SetBallToCell(targetPos, _selectedBall);

                await _selectedBall.MoveTo(targetPos, targetCell.WorldPosition);

                DeselectBall();

                // ✅ Match kontrol et
                var match = GridManager.Instance.CheckMatchAt(targetPos);
                if (match != null)
                {
                    List<MatchResult> matches = new List<MatchResult> { match };
                    
                    // ✅ Processing başlat
                    await GridManager.Instance.ProcessMatches(matches);
                    
                    // ✅ Processing complete event otomatik raise edilecek
                }
            }

            // ✅ Input'u aç
            _inputEnabled = true;

            if (showDebugLogs)
            {
                Debug.Log($"[InputHandler] Move completed: {currentPos} → {targetPos}");
            }
        }

        private void SelectBall(Ball ball)
        {
            if (_selectedBall != null)
            {
                _selectedBall.Deselect();
            }

            _selectedBall = ball;

            if (highlightSelectedBall)
            {
                _selectedBall.Select();
            }
        }

        private void DeselectBall()
        {
            if (_selectedBall != null)
            {
                _selectedBall.Deselect();
                _selectedBall = null;
            }
        }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;

            if (!enabled)
            {
                DeselectBall();
                _isTouching = false;
            }

            if (showDebugLogs)
            {
                Debug.Log($"[InputHandler] Input {(enabled ? "enabled" : "disabled")}");
            }
        }

        private void OnDrawGizmos()
        {
            if (!_isTouching) return;
            if (_mainCamera == null) return;

            Vector2 currentPos = _inputActions.Gameplay.TouchPosition.ReadValue<Vector2>();

            Vector3 startWorld = _mainCamera.ScreenToWorldPoint(new Vector3(_touchStartPos.x, _touchStartPos.y, 10f));
            Vector3 currentWorld = _mainCamera.ScreenToWorldPoint(new Vector3(currentPos.x, currentPos.y, 10f));

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startWorld, currentWorld);
            Gizmos.DrawSphere(startWorld, 0.1f);
            Gizmos.DrawSphere(currentWorld, 0.1f);
        }
    }
}