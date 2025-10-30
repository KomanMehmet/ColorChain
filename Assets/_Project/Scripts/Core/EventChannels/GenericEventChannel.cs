using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace _Project.Scripts.Core.EventChannels
{
    public abstract  class GenericEventChannel<T> : ScriptableObject
    {
        private readonly List<UnityAction<T>> _listeners = new List<UnityAction<T>>();
        
        public void Register(UnityAction<T> listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }
        
        public void Unregister(UnityAction<T> listener)
        {
            _listeners.Remove(listener);
        }
        
        public void RaiseEvent(T value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke(value);
            }
        }
        
        public void Clear()
        {
            _listeners.Clear();
        }
    }
}