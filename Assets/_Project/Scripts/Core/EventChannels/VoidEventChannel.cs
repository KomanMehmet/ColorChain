using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace _Project.Scripts.Core.EventChannels
{
    [CreateAssetMenu(fileName = "VoidEventChannel", menuName = "ColorChain/Events/Void Event Channel")]
    public class VoidEventChannel : ScriptableObject
    {
        private readonly List<UnityAction> _listeners = new List<UnityAction>();
        
        public void Register(UnityAction listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void Unregister(UnityAction listener)
        {
            _listeners.Remove(listener);
        }
        
        public void RaiseEvent()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke();
            }
        }

        public void Clear()
        {
            _listeners.Clear();
        }
    }
}