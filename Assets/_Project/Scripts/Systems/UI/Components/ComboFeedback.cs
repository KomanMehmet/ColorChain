using System;
using System.Threading;
using _Project.Scripts.Core.EventChannels;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Systems.UI.Components
{
    public class ComboFeedback : MonoBehaviour
    {
        public static ComboFeedback Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Event Channels")]
        [SerializeField] private ComboEventChannel comboEventChannel;

        [Header("Animation Settings")]
        [SerializeField] private float showDuration = 1f;
        [SerializeField] private float scaleMultiplier = 1.5f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;

        [Header("Combo Colors")]
        [SerializeField] private Color combo2Color = Color.yellow;
        [SerializeField] private Color combo3Color = Color.green;
        [SerializeField] private Color combo4Color = Color.cyan;
        [SerializeField] private Color combo5PlusColor = Color.magenta;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private CancellationTokenSource _cancellationTokenSource;
        private Vector3 _originalScale;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _cancellationTokenSource = new CancellationTokenSource();

            if (comboText != null)
            {
                _originalScale = comboText.transform.localScale;
            }

            HideImmediate();
        }

        private void OnEnable()
        {
            if (comboEventChannel != null)
            {
                comboEventChannel.Register(OnComboEvent);
            }
        }

        private void OnDisable()
        {
            if (comboEventChannel != null)
            {
                comboEventChannel.Unregister(OnComboEvent);
            }
        }

        private void OnComboEvent(ComboEventData data)
        {
            if (data.comboCount < 2) return;

            ShowCombo(data.comboCount);

            if (showDebugLogs)
            {
                Debug.Log($"[ComboFeedback] Showing combo: x{data.comboCount}");
            }
        }

        public void ShowCombo(int comboCount)
        {
            ShowComboAsync(comboCount).Forget();
        }

        /// <summary>
        /// Combo göster (async) - Manuel DOTween yöntemi
        /// </summary>
        public async UniTask ShowComboAsync(int comboCount)
        {
            if (comboText == null || canvasGroup == null) return;

            // Önceki animasyonu iptal et
            DOTween.Kill(comboText.transform);
            DOTween.Kill(canvasGroup);

            // Text güncelle
            comboText.text = $"COMBO x{comboCount}!";

            // Renk seç
            Color targetColor = GetComboColor(comboCount);
            comboText.color = targetColor;

            // Scale hesapla (combo arttıkça büyür)
            float scale = 1f + (comboCount * 0.1f);
            scale = Mathf.Clamp(scale, 1f, 2f);

            if (showDebugLogs)
            {
                Debug.Log($"[ComboFeedback] Showing combo: x{comboCount} (Scale: {scale})");
            }

            try
            {
                // Başlangıç durumu
                comboText.transform.localScale = Vector3.zero;
                canvasGroup.alpha = 0f;

                // ✅ Manuel sequence oluştur
                Sequence sequence = DOTween.Sequence();

                // Fade in + Scale
                sequence.Append(canvasGroup.DOFade(1f, 0.2f));
                sequence.Join(comboText.transform.DOScale(_originalScale * scale, 0.3f).SetEase(showEase));

                // Pulse animasyonu
                sequence.Append(comboText.transform.DOScale(_originalScale * scale * 1.1f, 0.15f).SetEase(Ease.InOutSine));
                sequence.Append(comboText.transform.DOScale(_originalScale * scale, 0.15f).SetEase(Ease.InOutSine));

                // Bekle
                sequence.AppendInterval(showDuration * 0.5f);

                // Fade out
                sequence.Append(canvasGroup.DOFade(0f, 0.3f));
                sequence.Join(comboText.transform.DOScale(Vector3.zero, 0.3f).SetEase(hideEase));

                // ✅ Manuel await (sequence bitene kadar bekle)
                sequence.Play();

                while (sequence != null && sequence.IsActive() && sequence.IsPlaying())
                {
                    // Cancellation check
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    
                    // Bir frame bekle
                    await UniTask.Yield(_cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // İptal edildi, animasyonu durdur
                DOTween.Kill(comboText.transform);
                DOTween.Kill(canvasGroup);
                HideImmediate();
            }
        }

        private Color GetComboColor(int comboCount)
        {
            if (comboCount >= 5) return combo5PlusColor;
            if (comboCount == 4) return combo4Color;
            if (comboCount == 3) return combo3Color;
            return combo2Color;
        }

        private void HideImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (comboText != null)
            {
                comboText.transform.localScale = Vector3.zero;
            }
        }

        public void Hide()
        {
            DOTween.Kill(comboText?.transform);
            DOTween.Kill(canvasGroup);
            HideImmediate();
        }

        private void OnDestroy()
        {
            if (comboEventChannel != null)
            {
                comboEventChannel.Unregister(OnComboEvent);
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            DOTween.Kill(comboText?.transform);
            DOTween.Kill(canvasGroup);
        }

#if UNITY_EDITOR
        [ContextMenu("Test Combo x2")]
        private void TestCombo2() => ShowCombo(2);

        [ContextMenu("Test Combo x3")]
        private void TestCombo3() => ShowCombo(3);

        [ContextMenu("Test Combo x5")]
        private void TestCombo5() => ShowCombo(5);

        [ContextMenu("Test Combo x10")]
        private void TestCombo10() => ShowCombo(10);
#endif
    }
}