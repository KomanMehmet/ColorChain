using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Systems.SceneManagement
{
    public class SceneTransition : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image fadeImage;
        
        [Header("Settings")]
        [SerializeField] private Color fadeColor = Color.black;
        [SerializeField] private bool blockRaycasts = true;
        
        private CancellationTokenSource _cancellationTokenSource;
        
        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            
            if (fadeImage != null)
            {
                fadeImage.color = fadeColor;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
        }

        public async UniTask FadeOut(float duration)
        {
            if (canvasGroup == null) return;
            
            canvasGroup.blocksRaycasts = blockRaycasts;

            Tween tween = canvasGroup.DOFade(1f, duration).SetEase(Ease.InOutQuad);

            try
            {
                tween.Play();

                while (tween != null && tween.IsActive() && tween.IsPlaying())
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    await UniTask.Yield(_cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {

            }
        }
        
        public async UniTask FadeIn(float duration)
        {
            if (canvasGroup == null) return;

            Tween tween = canvasGroup.DOFade(0f, duration).SetEase(Ease.InOutQuad);

            try
            {
                tween.Play();

                while (tween != null && tween.IsActive() && tween.IsPlaying())
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    await UniTask.Yield(_cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {

            }

            canvasGroup.blocksRaycasts = false;
        }
        
        public void ShowImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = blockRaycasts;
            }
        }
        
        public void HideImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void OnDestroy()
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
            }
        }
    }
}