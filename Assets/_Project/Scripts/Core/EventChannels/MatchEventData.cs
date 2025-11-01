using _Project.Scripts.Core.Enums;
using UnityEngine;

namespace _Project.Scripts.Core.EventChannels
{
    /// <summary>
    /// Match event data
    /// </summary>
    [System.Serializable]
    public struct MatchEventData
    {
        public int matchCount;
        public MatchType matchType;
        public Vector2Int[] positions;
        public BallColor color;

        public MatchEventData(int count, MatchType type, Vector2Int[] pos, BallColor ballColor)
        {
            matchCount = count;
            matchType = type;
            positions = pos;
            color = ballColor;
        }

        public override string ToString()
        {
            return $"Match: {matchCount}x {matchType} ({color})";
        }
    }
}