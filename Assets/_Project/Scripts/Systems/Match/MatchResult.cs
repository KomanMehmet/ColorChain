using System.Collections.Generic;
using _Project.Scripts.Core.Enums;
using UnityEngine;

namespace _Project.Scripts.Systems.Match
{
    public class MatchResult
    {
        /// <summary>
        /// Eşleşen pozisyonlar
        /// List çünkü sıralı ve dinamik
        /// </summary>
        public List<Vector2Int> MatchedPositions { get; private set; }

        /// <summary>
        /// Eşleşen renk
        /// </summary>
        public BallColor MatchedColor { get; private set; }

        /// <summary>
        /// Match tipi (horizontal, vertical, vs.)
        /// </summary>
        public MatchType MatchType { get; private set; }

        /// <summary>
        /// Kaç ball eşleşti?
        /// </summary>
        public int MatchCount => MatchedPositions.Count;

        /// <summary>
        /// Constructor
        /// </summary>
        public MatchResult(List<Vector2Int> positions, BallColor color, MatchType type)
        {
            MatchedPositions = positions;
            MatchedColor = color;
            MatchType = type;
        }

        /// <summary>
        /// Debug için string representation
        /// </summary>
        public override string ToString()
        {
            return $"MatchResult: {MatchCount} x {MatchedColor} ({MatchType})";
        }
    }
}