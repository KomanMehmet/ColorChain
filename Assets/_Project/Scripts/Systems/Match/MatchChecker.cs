using System.Collections.Generic;
using _Project.Scripts.Core.Enums;
using _Project.Scripts.Systems.Grid;
using UnityEngine;

namespace _Project.Scripts.Systems.Match
{
    public class MatchChecker
    {
        private const int MIN_MATCH_COUNT = 3;
        
        private readonly GridManager _gridManager;
        
        public MatchChecker(GridManager gridManager)
        {
            _gridManager = gridManager;
        }
        
        public MatchResult CheckMatchAt(Vector2Int gridPos)
        {
            if (!_gridManager.IsValidPosition(gridPos))
            {
                return null;
            }

            GridCell cell = _gridManager.GetCell(gridPos);
            if (cell.OccupyingBall == null)
            {
                return null;
            }

            BallColor targetColor = cell.OccupyingBall.Color;

            // Yatay ve dikey çizgileri ayrı kontrol et
            List<Vector2Int> horizontalMatches = CheckLineMatch(gridPos, targetColor, Vector2Int.right);
            List<Vector2Int> verticalMatches = CheckLineMatch(gridPos, targetColor, Vector2Int.up);

            List<Vector2Int> matchedPositions = new List<Vector2Int>();
            MatchType matchType = MatchType.None;

            bool hasHorizontalMatch = horizontalMatches.Count >= MIN_MATCH_COUNT;
            bool hasVerticalMatch = verticalMatches.Count >= MIN_MATCH_COUNT;

            if (hasHorizontalMatch && hasVerticalMatch)
            {
                // Her iki yönde de eşleşme var → CROSS
                matchedPositions = new List<Vector2Int>(horizontalMatches);
                foreach (var pos in verticalMatches)
                {
                    if (!matchedPositions.Contains(pos))
                    {
                        matchedPositions.Add(pos);
                    }
                }
                matchType = MatchType.Cross;
            }
            else if (hasHorizontalMatch)
            {
                matchedPositions = horizontalMatches;
                matchType = MatchType.Horizontal;
            }
            else if (hasVerticalMatch)
            {
                matchedPositions = verticalMatches;
                matchType = MatchType.Vertical;
            }
            else
            {
                return null;
            }

            return new MatchResult(matchedPositions, targetColor, matchType);
        }
        
        private List<Vector2Int> CheckLineMatch(Vector2Int startPos, BallColor targetColor, Vector2Int direction)
        {
            List<Vector2Int> matchedPositions = new List<Vector2Int>();
            matchedPositions.Add(startPos);

            // İleri yönde ara
            Vector2Int currentPos = startPos + direction;
            while (_gridManager.IsValidPosition(currentPos))
            {
                GridCell cell = _gridManager.GetCell(currentPos);
        
                if (cell.OccupyingBall != null && cell.OccupyingBall.Color == targetColor)
                {
                    matchedPositions.Add(currentPos);
                    currentPos += direction;
                }
                else
                {
                    break;
                }
            }

            // Geri yönde ara
            currentPos = startPos - direction;
            while (_gridManager.IsValidPosition(currentPos))
            {
                GridCell cell = _gridManager.GetCell(currentPos);
        
                if (cell.OccupyingBall != null && cell.OccupyingBall.Color == targetColor)
                {
                    matchedPositions.Add(currentPos);
                    currentPos -= direction;
                }
                else
                {
                    break;
                }
            }

            return matchedPositions;
        }
        
        public List<MatchResult> CheckAllMatches()
        {
            List<MatchResult> allMatches = new List<MatchResult>();
            HashSet<Vector2Int> checkedPositions = new HashSet<Vector2Int>();

            int gridSize = _gridManager.GetGridSize();

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    if (checkedPositions.Contains(pos))
                    {
                        continue;
                    }

                    MatchResult match = CheckMatchAt(pos);

                    if (match != null)
                    {
                        allMatches.Add(match);

                        foreach (var matchPos in match.MatchedPositions)
                        {
                            checkedPositions.Add(matchPos);
                        }
                    }
                    else
                    {
                        checkedPositions.Add(pos);
                    }
                }
            }

            return allMatches;
        }
    }
}