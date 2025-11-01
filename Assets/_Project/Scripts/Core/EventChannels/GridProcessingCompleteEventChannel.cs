using UnityEngine;

namespace _Project.Scripts.Core.EventChannels
{
    [CreateAssetMenu(fileName = "GridProcessingCompleteEventChannel", menuName = "ColorChain/Events/Grid Processing Complete Event Channel")]
    public class GridProcessingCompleteEventChannel : GenericEventChannel<GridProcessingCompleteEventData>
    {
        
    }
}