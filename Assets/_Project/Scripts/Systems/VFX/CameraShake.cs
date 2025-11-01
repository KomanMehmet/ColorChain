using System;
using System.Threading;
using _Project.Scripts.Core.EventChannels;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Systems.VFX
{
    /// <summary>
    /// CAMERA SHAKE SYSTEM
    /// Kamera titreşimi efekti
    /// 
    /// EVENT-DRIVEN:
    /// - MatchEventChannel dinler → Match'e göre shake
    /// - ComboEventChannel dinler → Combo'ya göre shake
    /// 
    /// ÖZELLIKLER:
    /// - Trauma-based smooth shake
    /// - Multiple shake stacking
    /// - Configurable intensities
    /// - Settings integration ready
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }
        
        [System.Serializable]
        public class ShakeConfig
        {
            public ShakeIntensity intensity;
            public float duration = 0.3f;
            public float magnitude = 0.2f;
            public float roughness = 10f;
            public float fadeInTime = 0.1f;
            public float fadeOutTime = 0.1f;
        }
        
        [Header("Shake Configurations")]
        [SerializeField] private ShakeConfig[] shakeConfigs = new ShakeConfig[]
        {
            new ShakeConfig { intensity = ShakeIntensity.Light, duration = 0.15f, magnitude = 0.05f, roughness = 10f },
            new ShakeConfig { intensity = ShakeIntensity.Medium, duration = 0.3f, magnitude = 0.15f, roughness = 15f },
            new ShakeConfig { intensity = ShakeIntensity.Heavy, duration = 0.5f, magnitude = 0.3f, roughness = 20f },
            new ShakeConfig { intensity = ShakeIntensity.Explosion, duration = 0.7f, magnitude = 0.5f, roughness = 25f },
        };

        [Header("Event Channels")] // ✅ Event channels
        [SerializeField] private MatchEventChannel matchEventChannel;
        [SerializeField] private ComboEventChannel comboEventChannel;
        
        [Header("Settings")]
        [SerializeField] private bool enableShake = true;
        [SerializeField] private float shakeMultiplier = 1f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private Camera _camera;
        private Vector3 _originalPosition;
        private float _trauma = 0f;
        private CancellationTokenSource _cancellationTokenSource;
        
        // Shake stack
        private int _activeShakes = 0;

        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _camera = GetComponent<Camera>();
            _originalPosition = transform.localPosition;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void OnEnable()
        {
            // ✅ Event'lere subscribe ol
            if (matchEventChannel != null)
            {
                matchEventChannel.Register(OnMatchEvent);
            }

            if (comboEventChannel != null)
            {
                comboEventChannel.Register(OnComboEvent);
            }
        }

        private void OnDisable()
        {
            // ✅ Event'lerden unsubscribe ol
            if (matchEventChannel != null)
            {
                matchEventChannel.Unregister(OnMatchEvent);
            }

            if (comboEventChannel != null)
            {
                comboEventChannel.Unregister(OnComboEvent);
            }
        }

        /// <summary>
        /// ✅ Match event handler
        /// </summary>
        private void OnMatchEvent(MatchEventData data)
        {
            if (!enableShake) return;

            // Match sayısına göre shake intensity seç
            ShakeIntensity intensity;

            if (data.matchCount >= 5)
            {
                intensity = ShakeIntensity.Heavy;
            }
            else if (data.matchCount == 4)
            {
                intensity = ShakeIntensity.Medium;
            }
            else
            {
                intensity = ShakeIntensity.Light;
            }

            Shake(intensity);

            if (showDebugLogs)
            {
                Debug.Log($"[CameraShake] Match shake: {intensity} (Count: {data.matchCount})");
            }
        }

        /// <summary>
        /// ✅ Combo event handler
        /// </summary>
        private void OnComboEvent(ComboEventData data)
        {
            if (!enableShake) return;

            // Combo sayısına göre shake intensity seç
            ShakeIntensity intensity;

            if (data.comboCount >= 5)
            {
                intensity = ShakeIntensity.Explosion; // Çok güçlü!
            }
            else if (data.comboCount >= 3)
            {
                intensity = ShakeIntensity.Heavy;
            }
            else
            {
                intensity = ShakeIntensity.Medium;
            }

            Shake(intensity);

            if (showDebugLogs)
            {
                Debug.Log($"[CameraShake] Combo shake: {intensity} (Combo: x{data.comboCount})");
            }
        }

        private void LateUpdate()
        {
            // Trauma'yı azalt (doğal sönümleme)
            if (_trauma > 0f)
            {
                _trauma -= Time.deltaTime * 2f;
                _trauma = Mathf.Clamp01(_trauma);

                // Shake uygula
                if (_trauma > 0f)
                {
                    ApplyShake();
                }
                else
                {
                    // Trauma 0'a düştü, pozisyonu sıfırla
                    transform.localPosition = _originalPosition;
                }
            }
        }

        /// <summary>
        /// Shake uygula (trauma-based)
        /// </summary>
        private void ApplyShake()
        {
            float shake = _trauma * _trauma; // Quadratic easing

            // Perlin noise ile smooth random hareket
            float x = (Mathf.PerlinNoise(Time.time * 20f, 0f) - 0.5f) * 2f * shake * shakeMultiplier;
            float y = (Mathf.PerlinNoise(0f, Time.time * 20f) - 0.5f) * 2f * shake * shakeMultiplier;

            transform.localPosition = _originalPosition + new Vector3(x, y, 0f);
        }

        /// <summary>
        /// Shake efekti oynat (fire-and-forget)
        /// </summary>
        public void Shake(ShakeIntensity intensity = ShakeIntensity.Medium)
        {
            if (!enableShake) return;

            ShakeAsync(intensity).Forget();
        }

        /// <summary>
        /// Shake efekti oynat (async)
        /// </summary>
        public async UniTask ShakeAsync(ShakeIntensity intensity = ShakeIntensity.Medium)
        {
            if (!enableShake) return;

            ShakeConfig config = GetConfig(intensity);
            if (config == null) return;

            _activeShakes++;

            if (showDebugLogs)
            {
                Debug.Log($"[CameraShake] Shake started: {intensity} (Active: {_activeShakes})");
            }

            try
            {
                float elapsed = 0f;
                float targetMagnitude = config.magnitude;

                while (elapsed < config.duration)
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    elapsed += Time.deltaTime;

                    // Fade in/out için smooth curve
                    float intensity01;
                    if (elapsed < config.fadeInTime)
                    {
                        // Fade in
                        intensity01 = elapsed / config.fadeInTime;
                    }
                    else if (elapsed > config.duration - config.fadeOutTime)
                    {
                        // Fade out
                        float fadeOutProgress = (config.duration - elapsed) / config.fadeOutTime;
                        intensity01 = fadeOutProgress;
                    }
                    else
                    {
                        // Full intensity
                        intensity01 = 1f;
                    }

                    // Trauma'yı güncelle
                    _trauma = Mathf.Max(_trauma, targetMagnitude * intensity01);

                    await UniTask.Yield(_cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // İptal edildi
            }
            finally
            {
                _activeShakes--;

                if (_activeShakes <= 0)
                {
                    _activeShakes = 0;
                    _trauma = 0f;
                    transform.localPosition = _originalPosition;
                }

                if (showDebugLogs)
                {
                    Debug.Log($"[CameraShake] Shake ended: {intensity} (Active: {_activeShakes})");
                }
            }
        }

        /// <summary>
        /// Özel shake (custom parametreler)
        /// </summary>
        public async UniTask ShakeCustom(float duration, float magnitude, float roughness = 10f)
        {
            if (!enableShake) return;

            _activeShakes++;

            try
            {
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    elapsed += Time.deltaTime;
                    float progress = elapsed / duration;

                    float intensity01 = 1f - progress; // Linear fade out
                    _trauma = Mathf.Max(_trauma, magnitude * intensity01);

                    await UniTask.Yield(_cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // İptal edildi
            }
            finally
            {
                _activeShakes--;
                if (_activeShakes <= 0)
                {
                    _activeShakes = 0;
                    _trauma = 0f;
                    transform.localPosition = _originalPosition;
                }
            }
        }

        /// <summary>
        /// Tüm shake'leri durdur
        /// </summary>
        public void StopAllShakes()
        {
            _activeShakes = 0;
            _trauma = 0f;
            transform.localPosition = _originalPosition;

            if (showDebugLogs)
            {
                Debug.Log("[CameraShake] All shakes stopped");
            }
        }

        /// <summary>
        /// Config al
        /// </summary>
        private ShakeConfig GetConfig(ShakeIntensity intensity)
        {
            foreach (var config in shakeConfigs)
            {
                if (config.intensity == intensity)
                {
                    return config;
                }
            }

            Debug.LogWarning($"[CameraShake] Config not found for intensity: {intensity}");
            return null;
        }

        /// <summary>
        /// Shake'i aktif/deaktif et (settings için)
        /// </summary>
        public void SetShakeEnabled(bool enabled)
        {
            enableShake = enabled;

            if (!enabled)
            {
                StopAllShakes();
            }
        }

        /// <summary>
        /// Shake multiplier ayarla (settings için)
        /// </summary>
        public void SetShakeMultiplier(float multiplier)
        {
            shakeMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
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

            // Cleanup
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            transform.localPosition = _originalPosition;
        }

#if UNITY_EDITOR
        [ContextMenu("Test Light Shake")]
        private void TestLightShake() => Shake(ShakeIntensity.Light);

        [ContextMenu("Test Medium Shake")]
        private void TestMediumShake() => Shake(ShakeIntensity.Medium);

        [ContextMenu("Test Heavy Shake")]
        private void TestHeavyShake() => Shake(ShakeIntensity.Heavy);

        [ContextMenu("Test Explosion Shake")]
        private void TestExplosionShake() => Shake(ShakeIntensity.Explosion);

        [ContextMenu("Stop All Shakes")]
        private void TestStopAllShakes() => StopAllShakes();
#endif
    }
}