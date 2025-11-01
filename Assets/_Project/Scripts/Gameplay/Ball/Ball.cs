using System;
using _Project.Scripts.Core.Enums;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using _Project.Scripts.Systems.VFX;

namespace _Project.Scripts.Gameplay.Ball
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class Ball : MonoBehaviour, IPoolable
    {
        [Header("Animation Settings")]
        [SerializeField] private float spawnDuration = 0.3f;
        [SerializeField] private float moveDuration = 0.25f;
        [SerializeField] private float popDuration = 0.2f;   
        
        [Header("Bounce Settings")]
        [SerializeField] private float bounceAmount = 0.2f;
        
        private SpriteRenderer _spriteRenderer;
        private CircleCollider2D _circleCollider;
        
        private BallData _data;
        private Vector2Int _gridPosition;
        private bool _isSelected;
        private bool _isAnimating;
        
        // ✅ CancellationTokenSource ekle
        private CancellationTokenSource _cancellationTokenSource;
        
        public BallColor Color => _data.Color;
        public Vector2Int GridPosition => _gridPosition;
        public bool IsAnimating => _isAnimating;
        public bool IsSelected => _isSelected;
        
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _circleCollider = GetComponent<CircleCollider2D>();
            
            _circleCollider.isTrigger = true;
            
            // ✅ CancellationTokenSource oluştur
            _cancellationTokenSource = new CancellationTokenSource();
        }
        
        public void Initialize(Vector2Int gridPos, BallData ballData)
        {
            _gridPosition = gridPos;
            _data = ballData;
            
            _spriteRenderer.color = _data.BallColor;

            if (_data.BallSprite != null)
            {
                _spriteRenderer.sprite = _data.BallSprite;
            }
            
            transform.localScale = Vector3.zero;

            _isSelected = false;

            PlaySpawnAnimation().Forget();
        }
        
        private async UniTaskVoid PlaySpawnAnimation()
        {
            _isAnimating = true;
            
            if (ParticleManager.Instance != null && _data != null)
            {
                ParticleManager.Instance.PlayEffectOneShot(
                    ParticleType.Spawn,
                    transform.position, 
                    _data.BallColor,
                    0.5f // Daha küçük scale
                );
            }
            
            float elapsed = 0f;
            
            try
            {
                // ✅ CancellationToken ile while loop
                while (elapsed < spawnDuration)
                {
                    // ✅ Token iptal edildi mi kontrol et
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    
                    elapsed += Time.deltaTime;
                    float t = elapsed / spawnDuration;
                    
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    
                    float easeValue = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                    
                    transform.localScale = Vector3.one * easeValue * (1f + bounceAmount);
                    
                    await UniTask.Yield(_cancellationTokenSource.Token); // ✅ Token ekle
                }

                transform.localScale = Vector3.one;
            }
            catch (OperationCanceledException)
            {
                // Animasyon iptal edildi, sorun yok
            }
            finally
            {
                _isAnimating = false;
            }
        }
        
        public async UniTask MoveTo(Vector2Int newGridPos, Vector3 targetWorldPos)
        {
            _isAnimating = true;
            
            Vector3 startPos = transform.position;
            
            _gridPosition = newGridPos;
            
            float elapsed = 0f;

            try
            {
                // ✅ CancellationToken ile while loop
                while (elapsed < moveDuration)
                {
                    // ✅ Token iptal edildi mi kontrol et
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    
                    elapsed += Time.deltaTime;
                    float t = elapsed / moveDuration;
                    
                    float easeValue = t < 0.5f 
                        ? 2f * t * t  
                        : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
                    
                    transform.position = Vector3.Lerp(startPos, targetWorldPos, easeValue);
                    
                    await UniTask.Yield(_cancellationTokenSource.Token); // ✅ Token ekle
                }

                transform.position = targetWorldPos;
            }
            catch (OperationCanceledException)
            {
                // İptal edildi, sorun yok
            }
            finally
            {
                _isAnimating = false;
            }
        }
        
        public async UniTask Pop()
        {
            _isAnimating = true;
            
            Vector3 startScale = transform.localScale;
            
            float elapsed = 0f;
            
            if (ParticleManager.Instance != null && _data != null)
            {
                ParticleManager.Instance.PlayEffectOneShot(
                    ParticleType.Pop,
                    transform.position, 
                    _data.BallColor
                );
            }
            
            try
            {
                // ✅ CancellationToken ile while loop
                while (elapsed < popDuration)
                {
                    // ✅ Token iptal edildi mi kontrol et
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    
                    elapsed += Time.deltaTime;
                    float t = elapsed / popDuration;
                    
                    float scale;
                    
                    if (t < 0.5f)
                    {
                        float t1 = t * 2f;
                        scale = Mathf.Lerp(1f, 1.3f, t1);
                    }
                    else
                    {
                        float t2 = (t - 0.5f) * 2f;
                        scale = Mathf.Lerp(1.3f, 0f, t2);
                    }
                    
                    transform.localScale = startScale * scale;
                    
                    Color currentColor = _spriteRenderer.color;
                    currentColor.a = 1f - t; 
                    _spriteRenderer.color = currentColor;
                    
                    await UniTask.Yield(_cancellationTokenSource.Token); // ✅ Token ekle
                }
            }
            catch (OperationCanceledException)
            {
                // İptal edildi, hemen deaktif et
            }
            catch (System.Exception)
            {
                // Obje destroy edilmiş
            }
            finally
            {
                _isAnimating = false;
        
                if (this != null && gameObject != null)
                {
                    OnRelease();
                }
            }
        }
        
        public void Select()
        {
            _isSelected = true;
            
            transform.localScale = Vector3.one * 1.1f;
            
            _spriteRenderer.color = _data.BallColor * 1.2f;
        }
        
        public void Deselect()
        {
            _isSelected = false;
            
            transform.localScale = Vector3.one;
            
            _spriteRenderer.color = _data.BallColor;
        }
        
        public void OnSpawn()
        {
            gameObject.SetActive(true);
            
            Color color = _spriteRenderer.color;
            color.a = 1f;
            _spriteRenderer.color = color;
        }
        
        public void OnRelease()
        {
            Deselect();
            
            gameObject.SetActive(false);
        }
        
        // ✅ OnDestroy'da cancel et
        private void OnDestroy()
        {
            // Tüm async operasyonları iptal et
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _isSelected ? UnityEngine.Color.green : UnityEngine.Color.yellow;

            Gizmos.DrawWireSphere(transform.position, 0.5f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.7f, 
                $"Grid: {_gridPosition}\nColor: {_data?.Color}"
            );
#endif
        }
        
        public override string ToString()
        {
            return $"Ball [{_data?.Color}] at {_gridPosition} | Selected: {_isSelected} | Animating: {_isAnimating}";
        }
    }
}