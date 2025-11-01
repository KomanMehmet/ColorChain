using UnityEngine;

namespace _Project.Scripts.Systems.Audio
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "ColorChain/Audio/Audio Settings", order = 0)]
    public class AudioSettings : ScriptableObject
    {
        [Header("Volume Settings")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float sfxVolume = 1f;
        
        [Header("Mute Settings")]
        public bool isMuted = false;
        
        public void ResetToDefaults()
        {
            masterVolume = 1f;
            musicVolume = 0.7f;
            sfxVolume = 1f;
            isMuted = false;
        }
        
#if UNITY_EDITOR
        [ContextMenu("Reset To Defaults")]
        private void DebugResetToDefaults()
        {
            ResetToDefaults();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}