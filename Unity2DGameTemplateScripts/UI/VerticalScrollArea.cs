using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Unity2DGameTemplate.UI
{
    public class VerticalScrollArea : MonoBehaviour
    {
        [SerializeField, Tooltip("Contentのサイズが最小を下回るときにスクロールバーを隠すかどうか")]
        private bool _isMinToHideScrollBar = true;

        [SerializeField]
        private float _mouseScrollSpeed = 30f;

        [Header("以下のパラメータはそのままにしておいてください。")]
        [SerializeField]
        private RectTransform _scrollBarAreaRectTransform;

        [SerializeField]
        private RectTransform _scrollBarCurrentMaskRectTransform;

        [SerializeField]
        private RectTransform _contentRectTransform;

        [SerializeField]
        private RectTransform _contentAreaMaskRectTransform;

        [SerializeField]
        private Mask _contentAreaMask;

        [SerializeField]
        private ScrollBarCurrent _scrollBarCurrent;

        private float _contentAreaRatio;

        private bool _isScrolling;

        private float _currentScrollRatio;

        private float _maxContentScroll;

        private float _originalDiff;

        private void Init()
        {
            _contentAreaRatio = Mathf.Min(1f, _contentAreaMaskRectTransform.rect.height / _contentRectTransform.rect.height);

            if (_contentAreaRatio >= 1f && _isMinToHideScrollBar)
            {
                _scrollBarAreaRectTransform.gameObject.SetActive(false);
            }

            Vector2 contentAnchoredPos = _contentRectTransform.anchoredPosition;
            contentAnchoredPos.y = 0f;
            _contentRectTransform.anchoredPosition = contentAnchoredPos;

            Vector2 svcmAnchorMin = _scrollBarCurrentMaskRectTransform.anchorMin;
            Vector2 svcmAnchorMax = _scrollBarCurrentMaskRectTransform.anchorMax;

            svcmAnchorMin.y = (1f - _contentAreaRatio);
            svcmAnchorMax.y = 1f;

            _currentScrollRatio = 0f;

            _scrollBarCurrentMaskRectTransform.anchorMin = svcmAnchorMin;
            _scrollBarCurrentMaskRectTransform.anchorMax = svcmAnchorMax;

            if (_contentAreaMask != null) _contentAreaMask.enabled = true;
            
            _maxContentScroll = _contentRectTransform.rect.height - _contentAreaMaskRectTransform.rect.height;

            _isScrolling = false;
        }

        private void Start()
        {
            Init();
        }

        public void SetScroll(float scrollRatio)
        {
            scrollRatio = Mathf.Clamp01(scrollRatio);
            _currentScrollRatio = scrollRatio;

            if (_contentAreaRatio >= 1f) return;

            Vector2 svcmAnchorMin = _scrollBarCurrentMaskRectTransform.anchorMin;
            Vector2 svcmAnchorMax = _scrollBarCurrentMaskRectTransform.anchorMax;

            svcmAnchorMin.y = Mathf.Lerp(_contentAreaRatio / 2f, 1f - _contentAreaRatio / 2f, 1f - scrollRatio) - _contentAreaRatio / 2f;
            svcmAnchorMax.y = Mathf.Lerp(_contentAreaRatio / 2f, 1f - _contentAreaRatio / 2f, 1f - scrollRatio) + _contentAreaRatio / 2f;

            _scrollBarCurrentMaskRectTransform.anchorMin = svcmAnchorMin;
            _scrollBarCurrentMaskRectTransform.anchorMax = svcmAnchorMax;

            Vector2 contentAnchoredPos = _contentRectTransform.anchoredPosition;
            contentAnchoredPos.y = _maxContentScroll * scrollRatio;
            _contentRectTransform.anchoredPosition = contentAnchoredPos;
        }

        public void ScrollAmount(float amount)
        {
            SetScroll(_currentScrollRatio + amount / _maxContentScroll);
        }

        private float ComputeRatio(float min, float max, float value)
        {
            return (value - min) / (max - min);
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            // スクロール処理

            if (_contentAreaRatio >= 1f) return;

            if (mouse.leftButton.wasPressedThisFrame && _scrollBarCurrent.IsOnPointer)
            {
                _isScrolling = true;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_scrollBarAreaRectTransform, mouse.position.ReadValue(), null, out Vector2 pointerPos);

                float scrollBarCurrentPosY =
                    Mathf.Lerp(_contentAreaRatio / 2f, 1f - _contentAreaRatio / 2f, 1f - _currentScrollRatio)
                    * _scrollBarAreaRectTransform.rect.height - _scrollBarAreaRectTransform.rect.height / 2f;

                _originalDiff = scrollBarCurrentPosY - pointerPos.y;
            }

            if (_isScrolling)
            {
                if (mouse.leftButton.wasReleasedThisFrame)
                {
                    _isScrolling = false;
                    return;
                }

                RectTransformUtility.ScreenPointToLocalPointInRectangle(_scrollBarAreaRectTransform, mouse.position.ReadValue(), null, out Vector2 pointerPos);

                float pointerRatio = (pointerPos.y + _originalDiff) / _scrollBarAreaRectTransform.rect.height + 0.5f;

                pointerRatio = Mathf.Clamp(pointerRatio, _contentAreaRatio / 2f, 1f - _contentAreaRatio / 2f);

                float scrollRatio = 1f - ComputeRatio(_contentAreaRatio / 2f, 1f - _contentAreaRatio / 2f, pointerRatio);

                SetScroll(scrollRatio);
            }

            if (mouse.scroll.ReadValue().y != 0f)
            {
                ScrollAmount(-1f * mouse.scroll.ReadValue().y * _mouseScrollSpeed * Time.deltaTime);
            }
            else if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                ScrollAmount(-1f * _mouseScrollSpeed * Time.deltaTime);
            }
            else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                ScrollAmount(_mouseScrollSpeed * Time.deltaTime);
            }
        }
    }
}
