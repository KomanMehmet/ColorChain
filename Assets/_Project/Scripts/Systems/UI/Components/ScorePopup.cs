using System;
using System.Threading;
using _Project.Scripts.Systems.UI.Enums;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Systems.UI.Components
{
    public class ScorePopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Animation Settings")]
        [SerializeField] private float duration = 1f;
        [SerializeField] private float moveDistance = 2f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;
        
        [Header("Color Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color bonusColor = Color.yellow;
        [SerializeField] private Color comboColor = Color.cyan;
        
        private CancellationTokenSource _cancellationTokenSource;
        private Vector3 _startPosition;

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }
        
        public void Show(int score, Vector3 worldPosition, ScorePopupType type = ScorePopupType.Normal)
        {
            ShowAsync(score, worldPosition, type).Forget();
        }
        
        public async UniTask ShowAsync(int score, Vector3 worldPosition, ScorePopupType type = ScorePopupType.Normal)
        {
            if (scoreText == null || canvasGroup == null) return;

            // Önceki animasyonu iptal et
            DOTween.Kill(transform);
            DOTween.Kill(canvasGroup);
            DOTween.Kill(scoreText);

            // Text güncelle
            scoreText.text = $"+{score}";

            // Renk seç
            Color targetColor = GetColorForType(type);
            scoreText.color = targetColor;

            // Pozisyon ayarla (world'den screen'e çevir)
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            transform.position = screenPos;
            _startPosition = transform.position;

            // Başlangıç durumu
            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one * 0.5f;

            try
            {
                // Animasyon sequence
                Sequence sequence = DOTween.Sequence();

                // Scale popup (büyüme)
                sequence.Append(transform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack));

                // Yukarı uçma + fade out (paralel)
                Vector3 targetPos = _startPosition + Vector3.up * moveDistance * 100f; // UI space
                sequence.Append(transform.DOMove(targetPos, duration).SetEase(moveEase));
                sequence.Join(canvasGroup.DOFade(0f, duration).SetEase(Ease.InQuad));

                // Manuel await
                sequence.Play();

                while (sequence != null && sequence.IsActive() && sequence.IsPlaying())
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    await UniTask.Yield(_cancellationTokenSource.Token);
                }

                // Animasyon bitti, pool'a geri dön
                ReturnToPool();
            }
            catch (OperationCanceledException)
            {
                // İptal edildi
                ReturnToPool();
            }
        }
        
        /// <summary>
        /// Renk seç
        /// </summary>
        private Color GetColorForType(ScorePopupType type)
        {
            switch (type)
            {
                case ScorePopupType.Bonus:
                    return bonusColor;
                case ScorePopupType.Combo:
                    return comboColor;
                default:
                    return normalColor;
            }
        }
        
        private void ReturnToPool()
        {
            if (ScorePopupManager.Instance != null)
            {
                ScorePopupManager.Instance.ReturnToPool(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            DOTween.Kill(transform);
            DOTween.Kill(canvasGroup);
            DOTween.Kill(scoreText);
        }
    }
}