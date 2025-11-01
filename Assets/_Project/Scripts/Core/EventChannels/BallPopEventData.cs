using UnityEngine;

namespace _Project.Scripts.Core.EventChannels
{
    /// <summary>
    /// Ball pop event data (VFX için)
    /// </summary>
    [System.Serializable]
    public struct BallPopEventData
    {
        public Vector3 position;
        public Color color;
        public int matchCount;

        public BallPopEventData(Vector3 pos, Color col, int count)
        {
            position = pos;
            color = col;
            matchCount = count;
        }

        public override string ToString()
        {
            return $"Ball Pop at {position} (Match: {matchCount})";
        }
    }
}