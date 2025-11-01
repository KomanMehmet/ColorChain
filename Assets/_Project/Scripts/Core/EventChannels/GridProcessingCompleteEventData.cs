namespace _Project.Scripts.Core.EventChannels
{
    [System.Serializable]
    public struct GridProcessingCompleteEventData
    {
        public int totalMatches;
        public int totalScore;
        public bool hasChainReaction;
        
        public GridProcessingCompleteEventData(int matches, int score, bool chainReaction)
        {
            totalMatches = matches;
            totalScore = score;
            hasChainReaction = chainReaction;
        }

        public override string ToString()
        {
            return $"Processing Complete: {totalMatches} matches, {totalScore} score, Chain: {hasChainReaction}";
        }
    }
}