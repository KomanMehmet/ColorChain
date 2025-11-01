using System;
using System.Collections.Generic;
using System.Threading;
using _Project.Scripts.Core.EventChannels;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Systems.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [System.Serializable]
        public class SoundEffect
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0.5f, 2f)] public float pitch = 1f;
            public bool loop = false;
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private GameObject sfxSourcePrefab;

        [Header("Sound Effects")]
        [SerializeField] private SoundEffect matchPopSFX;
        [SerializeField] private SoundEffect swapSFX;
        [SerializeField] private SoundEffect comboSFX;
        [SerializeField] private SoundEffect scoreSFX;
        [SerializeField] private SoundEffect levelCompleteSFX;
        [SerializeField] private SoundEffect levelFailedSFX;

        [Header("Music")]
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip menuMusic;

        [Header("Event Channels")]
        [SerializeField] private MatchEventChannel matchEventChannel;
        [SerializeField] private ComboEventChannel comboEventChannel;
        [SerializeField] private ScoreEventChannel scoreEventChannel;

        [Header("Settings")]
        [SerializeField] private AudioSettings audioSettings;

        [Header("Pool Settings")]
        [SerializeField] private int sfxPoolSize = 10;
        [SerializeField] private Transform sfxContainer;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        // SFX Pool
        private Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
        private List<AudioSource> _activeSFX = new List<AudioSource>();

        // Volume
        private float _masterVolume = 1f;
        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;

        // Cancellation
        private CancellationTokenSource _cancellationTokenSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _cancellationTokenSource = new CancellationTokenSource();
            
            LoadSettings();

            InitializeSFXPool();

            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
        }

        private void OnEnable()
        {
            if (matchEventChannel != null)
            {
                matchEventChannel.Register(OnMatchEvent);
            }

            if (comboEventChannel != null)
            {
                comboEventChannel.Register(OnComboEvent);
            }

            if (scoreEventChannel != null)
            {
                scoreEventChannel.Register(OnScoreEvent);
            }
        }

        private void OnDisable()
        {
            if (matchEventChannel != null)
            {
                matchEventChannel.Unregister(OnMatchEvent);
            }

            if (comboEventChannel != null)
            {
                comboEventChannel.Unregister(OnComboEvent);
            }

            if (scoreEventChannel != null)
            {
                scoreEventChannel.Unregister(OnScoreEvent);
            }
        }
        
        private void OnMatchEvent(MatchEventData data)
        {
            PlaySFX(matchPopSFX);

            if (showDebugLogs)
            {
                Debug.Log($"[AudioManager] Match SFX played: {data.matchCount} balls");
            }
        }
        
        private void OnComboEvent(ComboEventData data)
        {
            PlaySFX(comboSFX);

            if (showDebugLogs)
            {
                Debug.Log($"[AudioManager] Combo SFX played: x{data.comboCount}");
            }
        }
        
        private void OnScoreEvent(ScoreEventData data)
        {
            if (data.deltaScore > 0)
            {
                PlaySFX(scoreSFX);
            }
        }

        private void InitializeSFXPool()
        {
            if (sfxSourcePrefab == null)
            {
                sfxSourcePrefab = new GameObject("SFXSource");
                sfxSourcePrefab.AddComponent<AudioSource>();
                sfxSourcePrefab.SetActive(false);
            }
            
            if (sfxContainer == null)
            {
                GameObject container = new GameObject("SFXContainer");
                container.transform.SetParent(transform);
                sfxContainer = container.transform;
            }

            for (int i = 0; i < sfxPoolSize; i++)
            {
                CreateSFXSource();
            }

            if (showDebugLogs)
            {
                Debug.Log($"[AudioManager] SFX pool initialized: {sfxPoolSize} sources");
            }
        }

        private AudioSource CreateSFXSource()
        {
            GameObject obj = Instantiate(sfxSourcePrefab, sfxContainer);
            obj.name = "SFXSource";
            obj.SetActive(false);

            AudioSource source = obj.GetComponent<AudioSource>();
            if (source != null)
            {
                source.playOnAwake = false;
                source.loop = false;
                _sfxPool.Enqueue(source);
            }

            return source;
        }

        public void PlaySFX(SoundEffect sfx)
        {
            if (sfx == null || sfx.clip == null) return;

            AudioSource source = GetSFXSource();
            if (source == null) return;

            source.clip = sfx.clip;
            source.volume = sfx.volume * _sfxVolume * _masterVolume;
            source.pitch = sfx.pitch;
            source.loop = sfx.loop;

            source.gameObject.SetActive(true);
            source.Play();

            _activeSFX.Add(source);

            if (!sfx.loop)
            {
                ReturnSFXToPoolAsync(source, sfx.clip.length).Forget();
            }
        }
        
        public void PlaySFX(string sfxName)
        {
            SoundEffect sfx = GetSFXByName(sfxName);
            if (sfx != null)
            {
                PlaySFX(sfx);
            }
        }

        private SoundEffect GetSFXByName(string sfxName)
        {
            switch (sfxName.ToLower())
            {
                case "match":
                case "pop":
                    return matchPopSFX;
                case "swap":
                    return swapSFX;
                case "combo":
                    return comboSFX;
                case "score":
                    return scoreSFX;
                case "levelcomplete":
                    return levelCompleteSFX;
                case "levelfailed":
                    return levelFailedSFX;
                default:
                    Debug.LogWarning($"[AudioManager] SFX not found: {sfxName}");
                    return null;
            }
        }
        
        private AudioSource GetSFXSource()
        {
            if (_sfxPool.Count > 0)
            {
                return _sfxPool.Dequeue();
            }

            if (showDebugLogs)
            {
                Debug.LogWarning("[AudioManager] SFX pool empty, creating new source");
            }

            return CreateSFXSource();
        }
        
        private async UniTaskVoid ReturnSFXToPoolAsync(AudioSource source, float delay)
        {
            if (source == null) return;

            try
            {
                // Delay kadar bekle
                await UniTask.Delay((int)(delay * 1000), cancellationToken: _cancellationTokenSource.Token);

                if (source != null)
                {
                    source.Stop();
                    source.gameObject.SetActive(false);
                    _activeSFX.Remove(source);
                    _sfxPool.Enqueue(source);
                }
            }
            catch (OperationCanceledException)
            {
                
            }
        }
        
        public void PlayMusic(AudioClip music, bool loop = true)
        {
            if (music == null || musicSource == null) return;

            musicSource.clip = music;
            musicSource.loop = loop;
            musicSource.volume = _musicVolume * _masterVolume;
            musicSource.Play();

            if (showDebugLogs)
            {
                Debug.Log($"[AudioManager] Music playing: {music.name}");
            }
        }
        public void PlayGameplayMusic()
        {
            if (gameplayMusic != null)
            {
                PlayMusic(gameplayMusic);
            }
        }

        public void PlayMenuMusic()
        {
            if (menuMusic != null)
            {
                PlayMusic(menuMusic);
            }
        }
        
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }
        
        public void PauseMusic()
        {
            if (musicSource != null)
            {
                musicSource.Pause();
            }
        }
        
        public void ResumeMusic()
        {
            if (musicSource != null)
            {
                musicSource.UnPause();
            }
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            
            if (musicSource != null)
            {
                musicSource.volume = _musicVolume * _masterVolume;
            }

            SaveSettings();
        }
        
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            
            if (musicSource != null)
            {
                musicSource.volume = _musicVolume * _masterVolume;
            }

            SaveSettings();
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            SaveSettings();
        }

        public void SetMute(bool mute)
        {
            AudioListener.volume = mute ? 0f : 1f;
            SaveSettings();
        }

        private void LoadSettings()
        {
            if (audioSettings != null)
            {
                _masterVolume = audioSettings.masterVolume;
                _musicVolume = audioSettings.musicVolume;
                _sfxVolume = audioSettings.sfxVolume;
            }
            else
            {
                _masterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1f);
                _musicVolume = PlayerPrefs.GetFloat("Audio_MusicVolume", 1f);
                _sfxVolume = PlayerPrefs.GetFloat("Audio_SFXVolume", 1f);
            }
        }
        
        private void SaveSettings()
        {
            if (audioSettings != null)
            {
                audioSettings.masterVolume = _masterVolume;
                audioSettings.musicVolume = _musicVolume;
                audioSettings.sfxVolume = _sfxVolume;
            }
            else
            {
                PlayerPrefs.SetFloat("Audio_MasterVolume", _masterVolume);
                PlayerPrefs.SetFloat("Audio_MusicVolume", _musicVolume);
                PlayerPrefs.SetFloat("Audio_SFXVolume", _sfxVolume);
                PlayerPrefs.Save();
            }
        }

        public void StopAllSFX()
        {
            foreach (var source in _activeSFX)
            {
                if (source != null)
                {
                    source.Stop();
                    source.gameObject.SetActive(false);
                }
            }

            _activeSFX.Clear();
        }

        private void OnDestroy()
        {
            // Event'lerden unsubscribe
            if (matchEventChannel != null)
            {
                matchEventChannel.Unregister(OnMatchEvent);
            }

            if (comboEventChannel != null)
            {
                comboEventChannel.Unregister(OnComboEvent);
            }

            if (scoreEventChannel != null)
            {
                scoreEventChannel.Unregister(OnScoreEvent);
            }
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            StopAllSFX();
        }
        
        public float MasterVolume => _masterVolume;
        public float MusicVolume => _musicVolume;
        public float SFXVolume => _sfxVolume;
        public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying;

#if UNITY_EDITOR
        [ContextMenu("Test Match SFX")]
        private void TestMatchSFX() => PlaySFX(matchPopSFX);

        [ContextMenu("Test Combo SFX")]
        private void TestComboSFX() => PlaySFX(comboSFX);

        [ContextMenu("Test Score SFX")]
        private void TestScoreSFX() => PlaySFX(scoreSFX);

        [ContextMenu("Play Gameplay Music")]
        private void TestGameplayMusic() => PlayGameplayMusic();

        [ContextMenu("Stop Music")]
        private void TestStopMusic() => StopMusic();
#endif
    }
}