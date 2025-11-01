using System;
using System.Collections.Generic;
using System.Threading;
using _Project.Scripts.Core.EventChannels;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Systems.VFX
{
    /// <summary>
    /// PARTICLE MANAGER
    /// Particle effect sistemini yönetir
    /// 
    /// EVENT-DRIVEN:
    /// - BallPopEventChannel dinler → Pop particle oynatır
    /// - ComboEventChannel dinler → Combo particle oynatır (opsiyonel)
    /// 
    /// ÖZELLIKLER:
    /// - Object pooling ile performans
    /// - UniTask ile async yönetim
    /// - Type-safe particle system
    /// - Auto-return to pool
    /// </summary>
    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance { get; private set; }
        
        [System.Serializable]
        public class ParticleConfig
        {
            public ParticleType type;
            public GameObject prefab;
            public int initialPoolSize = 10;
            public bool autoReturn = true;
            public float maxLifetime = 5f;
        }
        
        [Header("Particle Configurations")]
        [SerializeField] private List<ParticleConfig> particleConfigs = new List<ParticleConfig>();

        [Header("Event Channels")] // ✅ Event channels
        [SerializeField] private BallPopEventChannel ballPopEventChannel;
        [SerializeField] private ComboEventChannel comboEventChannel;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;
        
        // Pool dictionary
        private Dictionary<ParticleType, Queue<ParticleSystem>> _particlePools = new Dictionary<ParticleType, Queue<ParticleSystem>>();
        
        // Config dictionary
        private Dictionary<ParticleType, ParticleConfig> _configs = new Dictionary<ParticleType, ParticleConfig>();

        // Active particles
        private List<ParticleSystem> _activeParticles = new List<ParticleSystem>();
        
        // Cancellation token
        private CancellationTokenSource _cancellationTokenSource;
        
        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _cancellationTokenSource = new CancellationTokenSource();

            // Config'leri dictionary'e kaydet
            foreach (var config in particleConfigs)
            {
                _configs[config.type] = config;
            }

            // Pool'ları initialize et
            InitializePools();
        }

        private void OnEnable()
        {
            // ✅ Event'lere subscribe ol
            if (ballPopEventChannel != null)
            {
                ballPopEventChannel.Register(OnBallPopEvent);
            }

            if (comboEventChannel != null)
            {
                comboEventChannel.Register(OnComboEvent);
            }
        }

        private void OnDisable()
        {
            // ✅ Event'lerden unsubscribe ol
            if (ballPopEventChannel != null)
            {
                ballPopEventChannel.Unregister(OnBallPopEvent);
            }

            if (comboEventChannel != null)
            {
                comboEventChannel.Unregister(OnComboEvent);
            }
        }

        /// <summary>
        /// ✅ Ball pop event handler
        /// </summary>
        private void OnBallPopEvent(BallPopEventData data)
        {
            // Pop particle oynat
            PlayEffectOneShot(ParticleType.Pop, data.position, data.color);

            if (showDebugLogs)
            {
                Debug.Log($"[ParticleManager] Pop particle played at {data.position}");
            }
        }

        /// <summary>
        /// ✅ Combo event handler (opsiyonel)
        /// </summary>
        private void OnComboEvent(ComboEventData data)
        {
            // Combo particle oynat (eğer varsa)
            if (_configs.ContainsKey(ParticleType.Combo))
            {
                // Combo pozisyonu verilmişse onu kullan, yoksa ekran ortası
                Vector3 position = data.position != Vector3.zero 
                    ? data.position 
                    : Vector3.zero;

                // Combo sayısına göre scale
                float scale = 1f + (data.comboCount * 0.2f);
                scale = Mathf.Clamp(scale, 1f, 2.5f);

                PlayEffectOneShot(ParticleType.Combo, position, Color.yellow, scale);

                if (showDebugLogs)
                {
                    Debug.Log($"[ParticleManager] Combo particle played: x{data.comboCount}");
                }
            }
        }

        /// <summary>
        /// Pool'ları başlat
        /// </summary>
        private void InitializePools()
        {
            foreach (var config in particleConfigs)
            {
                if (config.prefab == null)
                {
                    Debug.LogWarning($"[ParticleManager] Prefab is null for type: {config.type}");
                    continue;
                }

                // Pool oluştur
                _particlePools[config.type] = new Queue<ParticleSystem>();

                // Initial particle'ları oluştur
                for (int i = 0; i < config.initialPoolSize; i++)
                {
                    CreateParticle(config);
                }

                if (showDebugLogs)
                {
                    Debug.Log($"[ParticleManager] Pool created: {config.type} (Size: {config.initialPoolSize})");
                }
            }
        }

        /// <summary>
        /// Yeni particle oluştur
        /// </summary>
        private ParticleSystem CreateParticle(ParticleConfig config)
        {
            GameObject obj = Instantiate(config.prefab, transform);
            obj.name = $"{config.type}_Particle";
            obj.SetActive(false);

            ParticleSystem ps = obj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                _particlePools[config.type].Enqueue(ps);
            }
            else
            {
                Debug.LogError($"[ParticleManager] No ParticleSystem component on prefab: {config.type}");
                Destroy(obj);
            }

            return ps;
        }

        /// <summary>
        /// Particle efekti oynat (async)
        /// </summary>
        public async UniTask PlayEffect(ParticleType type, Vector3 position, Color? color = null, float scale = 1f)
        {
            if (!_configs.ContainsKey(type))
            {
                Debug.LogError($"[ParticleManager] Particle type not configured: {type}");
                return;
            }

            ParticleSystem ps = GetParticle(type);
            if (ps == null) return;

            // Pozisyon ve scale ayarla
            ps.transform.position = position;
            ps.transform.localScale = Vector3.one * scale;

            // Renk ayarla (eğer verilmişse)
            if (color.HasValue)
            {
                var main = ps.main;
                main.startColor = color.Value;
            }

            // Oynat
            ps.gameObject.SetActive(true);
            _activeParticles.Add(ps);
            ps.Play();

            if (showDebugLogs)
            {
                Debug.Log($"[ParticleManager] Playing effect: {type} at {position}");
            }

            // Auto-return aktifse, otomatik olarak pool'a geri koy
            ParticleConfig config = _configs[type];
            if (config.autoReturn)
            {
                ReturnToPoolAfterPlay(ps, type, config.maxLifetime).Forget();
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// Particle efekti oynat (fire-and-forget)
        /// </summary>
        public void PlayEffectOneShot(ParticleType type, Vector3 position, Color? color = null, float scale = 1f)
        {
            PlayEffect(type, position, color, scale).Forget();
        }

        /// <summary>
        /// Pool'dan particle al
        /// </summary>
        private ParticleSystem GetParticle(ParticleType type)
        {
            if (!_particlePools.ContainsKey(type))
            {
                Debug.LogError($"[ParticleManager] Pool not found for type: {type}");
                return null;
            }

            Queue<ParticleSystem> pool = _particlePools[type];

            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }

            // Pool boş, yeni oluştur
            if (showDebugLogs)
            {
                Debug.LogWarning($"[ParticleManager] Pool empty for {type}, creating new particle");
            }

            return CreateParticle(_configs[type]);
        }

        /// <summary>
        /// Particle bitince pool'a geri koy
        /// </summary>
        private async UniTaskVoid ReturnToPoolAfterPlay(ParticleSystem ps, ParticleType type, float maxLifetime)
        {
            if (ps == null) return;

            try
            {
                // Particle bitene kadar bekle
                float elapsed = 0f;
                while (ps != null && ps.isPlaying && elapsed < maxLifetime)
                {
                    await UniTask.Yield(_cancellationTokenSource.Token);
                    elapsed += Time.deltaTime;
                }

                // Pool'a geri koy
                if (ps != null && ps.gameObject != null)
                {
                    ps.Stop();
                    ps.gameObject.SetActive(false);
                    _activeParticles.Remove(ps);

                    if (_particlePools.ContainsKey(type))
                    {
                        _particlePools[type].Enqueue(ps);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Manager destroy oldu
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ParticleManager] Error returning particle: {ex.Message}");
            }
        }

        /// <summary>
        /// Manuel pool'a geri koy
        /// </summary>
        public void ReturnToPool(ParticleSystem ps, ParticleType type)
        {
            if (ps == null) return;

            ps.Stop();
            ps.gameObject.SetActive(false);
            _activeParticles.Remove(ps);

            if (_particlePools.ContainsKey(type))
            {
                _particlePools[type].Enqueue(ps);
            }
        }

        /// <summary>
        /// Tüm particle'ları durdur
        /// </summary>
        public void StopAllParticles()
        {
            foreach (var ps in _activeParticles)
            {
                if (ps != null && ps.isPlaying)
                {
                    ps.Stop();
                    ps.gameObject.SetActive(false);
                }
            }

            _activeParticles.Clear();
        }

        /// <summary>
        /// Belirli tip particle'ları durdur
        /// </summary>
        public void StopParticlesOfType(ParticleType type)
        {
            for (int i = _activeParticles.Count - 1; i >= 0; i--)
            {
                var ps = _activeParticles[i];
                if (ps != null && ps.name.Contains(type.ToString()))
                {
                    ps.Stop();
                    ps.gameObject.SetActive(false);
                    _activeParticles.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Pool istatistikleri
        /// </summary>
        public void PrintPoolStats()
        {
            Debug.Log("=== Particle Pool Stats ===");
            foreach (var kvp in _particlePools)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value.Count} available");
            }
            Debug.Log($"Active particles: {_activeParticles.Count}");
        }

        private void OnDestroy()
        {
            // Event'lerden unsubscribe
            if (ballPopEventChannel != null)
            {
                ballPopEventChannel.Unregister(OnBallPopEvent);
            }

            if (comboEventChannel != null)
            {
                comboEventChannel.Unregister(OnComboEvent);
            }

            // Cleanup
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            
            StopAllParticles();
        }

#if UNITY_EDITOR
        [ContextMenu("Print Pool Stats")]
        private void DebugPrintStats()
        {
            PrintPoolStats();
        }
#endif
    }
}