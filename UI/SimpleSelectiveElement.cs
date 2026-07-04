using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MySRPGGame.UI
{
    public class SimpleSelectiveElement : MonoBehaviour
    {
        public SimpleSelectiveWindow Window { get; private set; }

        public int ElementId { get; private set; }

        public Action<int> OnInitEvent { get; set; }
        public Action<int> OnSelectedEvent { get; set; }
        public Action<int> OnDeselectedEvent { get; set; }
        public Action<int> OnDecidedEvent { get; set; }

        public bool IsInitialized { get; private set; }

        private RectTransform rectTransform;

        private bool isPointerOn;

        private bool isUseMouseInput;

        public void Init(SimpleSelectiveWindow window, int elementId, bool isUseMouseInput)
        {
            Window = window;
            ElementId = elementId;

            rectTransform = GetComponent<RectTransform>();

            OnSelectedEvent = default;
            OnDeselectedEvent = default;
            OnDecidedEvent = default;

            this.isUseMouseInput = isUseMouseInput;

            IsInitialized = true;
            OnInitEvent?.Invoke(elementId);
        }

        public void Select()
        {
            OnSelectedEvent?.Invoke(ElementId);
        }
        public void Deselect()
        {
            OnDeselectedEvent?.Invoke(ElementId);
        }
        public void Decide()
        {
            OnDecidedEvent?.Invoke(ElementId);
        }

        private void Update()
        {
            if (!IsInitialized || !isUseMouseInput) return;

            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mouse.position.ReadValue(), null, out Vector2 localPoint);

            if (rectTransform.rect.Contains(localPoint))
            {
                // ポインターが触れている

                if (!isPointerOn)
                {
                    isPointerOn = true;

                    Select();
                }

                if (mouse.leftButton.wasPressedThisFrame)
                {
                    // ボタンが押された

                    Decide();
                }
            }
            else
            {
                // ポインターが離れている

                if (isPointerOn)
                {
                    isPointerOn = false;

                    Deselect();
                }
            }
        }

        private void OnDisable()
        {
            IsInitialized = false;
        }
    }
}
