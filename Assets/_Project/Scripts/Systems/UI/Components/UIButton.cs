using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Scripts.Systems.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Animation Settings")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float clickScale = 0.95f;
        [SerializeField] private float animationDuration = 0.2f;
        [SerializeField] private Ease hoverEase = Ease.OutBack;
        [SerializeField] private Ease clickEase = Ease.InOutQuad;
        
        [Header("Sound")]
        [SerializeField] private bool playHoverSound = true;
        [SerializeField] private bool playClickSound = true;
        
        private Button _button;
        private Vector3 _originalScale;
        private Tweener _currentTween;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _originalScale = transform.localScale;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            // ✅ Hover animasyonu
            _currentTween?.Kill();
            _currentTween = transform.DOScale(_originalScale * hoverScale, animationDuration)
                .SetEase(hoverEase);

            // TODO: Hover ses efekti
            if (playHoverSound)
            {
                // AudioManager.Instance.PlaySFX("button_hover");
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            // ✅ Normal boyuta dön
            _currentTween?.Kill();
            _currentTween = transform.DOScale(_originalScale, animationDuration)
                .SetEase(hoverEase);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            // ✅ Basma animasyonu
            _currentTween?.Kill();
            _currentTween = transform.DOScale(_originalScale * clickScale, animationDuration)
                .SetEase(clickEase);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            // ✅ Normal boyuta dön
            _currentTween?.Kill();
            _currentTween = transform.DOScale(_originalScale * hoverScale, animationDuration)
                .SetEase(clickEase);

            // TODO: Click ses efekti
            if (playClickSound)
            {
                // AudioManager.Instance.PlaySFX("button_click");
            }
        }
        
        private void OnDisable()
        {
            // ✅ Disable olunca normal boyuta dön
            _currentTween?.Kill();
            transform.localScale = _originalScale;
        }
        
        private void OnDestroy()
        {
            _currentTween?.Kill();
        }
    }
}