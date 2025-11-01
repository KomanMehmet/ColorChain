namespace _Project.Scripts.Core.EventChannels
{
    /// <summary>
    /// Score event data
    /// </summary>
    [System.Serializable]
    public struct ScoreEventData
    {
        public int deltaScore;
        public int totalScore;

        public ScoreEventData(int delta, int total)
        {
            deltaScore = delta;
            totalScore = total;
        }

        public override string ToString()
        {
            return $"Score: +{deltaScore} (Total: {totalScore})";
        }
    }
}