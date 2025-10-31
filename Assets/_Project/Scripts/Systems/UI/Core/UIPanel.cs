using _Project.Scripts.Systems.UI.Enums;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Systems.UI.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract  class UIPanel : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] protected string panelName;
        [SerializeField] protected bool hideOnAwake = true;
        
        [Header("Animation Settings")]
        [SerializeField] protected float animationDuration = 0.3f;
        [SerializeField] protected Ease showEase = Ease.OutBack;
        [SerializeField] protected Ease hideEase = Ease.InBack;
        [SerializeField] protected PanelAnimationType animationType = PanelAnimationType.Scale;
        
        protected CanvasGroup canvasGroup;
        protected RectTransform rectTransform;
        protected bool isVisible;
        
        // Animasyon için başlangıç değerleri
        private Vector3 _originalScale;
        private Vector2 _originalPosition;
        
        public string PanelName => panelName;
        public bool IsVisible => isVisible;
        
        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();

            // Orijinal değerleri kaydet
            _originalScale = rectTransform.localScale;
            _originalPosition = rectTransform.anchoredPosition;

            // Başlangıçta gizle
            if (hideOnAwake)
            {
                HideImmediate();
            }
        }
        
        public virtual async UniTask Show()
        {
            if (isVisible) return;

            gameObject.SetActive(true);
            isVisible = true;

            // Show animasyonu
            await PlayShowAnimation();

            // Alt sınıfların override edebileceği callback
            OnShown();
        }
        
        /// <summary>
        /// Paneli göster (özel animasyon süresiyle)
        /// </summary>
        public virtual async UniTask Show(float customDuration)
        {
            if (isVisible) return;

            // Geçici olarak animasyon süresini değiştir
            float originalDuration = animationDuration;
            animationDuration = customDuration;

            gameObject.SetActive(true);
            isVisible = true;

            // Show animasyonu
            await PlayShowAnimation();

            // Orijinal süreye geri dön
            animationDuration = originalDuration;

            // Callback
            OnShown();
        }
        
        public virtual async UniTask Hide()
        {
            if (!isVisible) return;

            isVisible = false;

            // Alt sınıfların override edebileceği callback
            OnHiding();

            // Hide animasyonu
            await PlayHideAnimation();

            gameObject.SetActive(false);

            // Alt sınıfların override edebileceği callback
            OnHidden();
        }
        
        /// <summary>
        /// Paneli gizle (özel animasyon süresiyle)
        /// </summary>
        public virtual async UniTask Hide(float customDuration)
        {
            if (!isVisible) return;

            // Geçici olarak animasyon süresini değiştir
            float originalDuration = animationDuration;
            animationDuration = customDuration;

            isVisible = false;

            // Alt sınıfların override edebileceği callback
            OnHiding();

            // Hide animasyonu
            await PlayHideAnimation();

            gameObject.SetActive(false);

            // Orijinal süreye geri dön
            animationDuration = originalDuration;

            // Alt sınıfların override edebileceği callback
            OnHidden();
        }
        
        public virtual void ShowImmediate()
        {
            gameObject.SetActive(true);
            isVisible = true;
            
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            
            rectTransform.localScale = _originalScale;
            rectTransform.anchoredPosition = _originalPosition;

            OnShown();
        }
        
        public virtual void HideImmediate()
        {
            isVisible = false;
            gameObject.SetActive(false);
            
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            OnHidden();
        }
        
        private async UniTask PlayShowAnimation()
        {
            // Animasyon başlamadan önce hazırlık
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;

            Sequence showSequence = DOTween.Sequence();

            switch (animationType)
            {
                case PanelAnimationType.Fade:
                    canvasGroup.alpha = 0f;
                    showSequence.Append(canvasGroup.DOFade(1f, animationDuration).SetEase(showEase));
                    break;

                case PanelAnimationType.Scale:
                    rectTransform.localScale = Vector3.zero;
                    canvasGroup.alpha = 1f;
                    showSequence.Append(rectTransform.DOScale(_originalScale, animationDuration).SetEase(showEase));
                    break;

                case PanelAnimationType.SlideFromTop:
                    rectTransform.anchoredPosition = _originalPosition + Vector2.up * 1000f;
                    canvasGroup.alpha = 1f;
                    showSequence.Append(rectTransform.DOAnchorPos(_originalPosition, animationDuration).SetEase(showEase));
                    break;

                case PanelAnimationType.SlideFromBottom:
                    rectTransform.anchoredPosition = _originalPosition + Vector2.down * 1000f;
                    canvasGroup.alpha = 1f;
                    showSequence.Append(rectTransform.DOAnchorPos(_originalPosition, animationDuration).SetEase(showEase));
                    break;

                case PanelAnimationType.FadeAndScale:
                    rectTransform.localScale = Vector3.zero;
                    canvasGroup.alpha = 0f;
                    showSequence.Append(rectTransform.DOScale(_originalScale, animationDuration).SetEase(showEase));
                    showSequence.Join(canvasGroup.DOFade(1f, animationDuration));
                    break;
            }

            await showSequence.AsyncWaitForCompletion();

            // Animasyon bitince interactable yap
            canvasGroup.interactable = true;
        }
        
        private async UniTask PlayHideAnimation()
        {
            // Animasyon başlamadan önce interactable kapat
            canvasGroup.interactable = false;

            Sequence hideSequence = DOTween.Sequence();

            switch (animationType)
            {
                case PanelAnimationType.Fade:
                    hideSequence.Append(canvasGroup.DOFade(0f, animationDuration).SetEase(hideEase));
                    break;

                case PanelAnimationType.Scale:
                    hideSequence.Append(rectTransform.DOScale(Vector3.zero, animationDuration).SetEase(hideEase));
                    break;

                case PanelAnimationType.SlideFromTop:
                    hideSequence.Append(rectTransform.DOAnchorPos(_originalPosition + Vector2.up * 1000f, animationDuration).SetEase(hideEase));
                    break;

                case PanelAnimationType.SlideFromBottom:
                    hideSequence.Append(rectTransform.DOAnchorPos(_originalPosition + Vector2.down * 1000f, animationDuration).SetEase(hideEase));
                    break;

                case PanelAnimationType.FadeAndScale:
                    hideSequence.Append(rectTransform.DOScale(Vector3.zero, animationDuration).SetEase(hideEase));
                    hideSequence.Join(canvasGroup.DOFade(0f, animationDuration));
                    break;
            }

            await hideSequence.AsyncWaitForCompletion();

            canvasGroup.blocksRaycasts = false;
        }
        
        protected virtual void OnShown()
        {
            // Override edilebilir
        }
        
        protected virtual void OnHiding()
        {
            // Override edilebilir
        }
        
        protected virtual void OnHidden()
        {
            // Override edilebilir
        }
        
        protected virtual void OnDestroy()
        {
            // DOTween cleanup
            rectTransform?.DOKill();
            canvasGroup?.DOKill();
        }
    }
}