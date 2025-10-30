using _Project.Scripts.Gameplay.Ball;
using UnityEngine;

namespace _Project.Scripts.Systems.Grid
{
    public struct GridCell
    {
        public Vector2Int GridPosition;
        public Vector3 WorldPosition;
        public Ball OccupyingBall;
        public bool IsLocked;
        public bool IsSpecialBlock;
        
        public GridCell(Vector2Int gridPos, Vector3 worldPos)
        {
            GridPosition = gridPos;
            WorldPosition = worldPos;
            OccupyingBall = null;
            IsLocked = false;
            IsSpecialBlock = false;
        }
        
        public bool IsEmpty => OccupyingBall == null && !IsLocked && !IsSpecialBlock;
        
        public bool IsOccupied => OccupyingBall != null;
        
        public void SetBall(Ball ball)
        {
            OccupyingBall = ball;
        }
        
        public void ClearBall()
        {
            OccupyingBall = null;
        }
        
        public override string ToString()
        {
            string status = IsEmpty ? "Empty" : IsOccupied ? "Occupied" : "Blocked";
            return $"Cell [{GridPosition.x},{GridPosition.y}] - {status}";
        }
    }
}