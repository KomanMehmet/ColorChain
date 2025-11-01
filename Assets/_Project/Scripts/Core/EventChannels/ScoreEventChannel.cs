using UnityEngine;

namespace _Project.Scripts.Core.EventChannels
{
    [CreateAssetMenu(fileName = "ScoreEventChannel", menuName = "ColorChain/Events/Score Event Channel")]
    public class ScoreEventChannel : GenericEventChannel<ScoreEventData>
    {
        
    }
}