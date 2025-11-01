using UnityEngine;

namespace _Project.Scripts.Core.EventChannels
{
    /// <summary>
    /// Combo event data
    /// </summary>
    [System.Serializable]
    public struct ComboEventData
    {
        public int comboCount;
        public int totalMatches;
        public Vector3 position;

        public ComboEventData(int combo, int matches, Vector3 pos)
        {
            comboCount = combo;
            totalMatches = matches;
            position = pos;
        }

        public override string ToString()
        {
            return $"Combo x{comboCount} ({totalMatches} matches)";
        }
    }
}