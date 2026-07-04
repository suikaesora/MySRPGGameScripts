using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Unity2DGameTemplate.UI
{
    public class PopupWindow : MonoBehaviour
    {
        [SerializeField]
        protected RectTransform WindowAreaRectTransform;

        [SerializeField]
        protected RectTransform CanvasRectTransform;

        [SerializeField]
        protected Button closeButton;

        public virtual void InitOpened()
        {
            closeButton?.onClick.RemoveAllListeners();
            closeButton?.onClick.AddListener(Close);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            InitOpened();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private bool IsCursorPosInArea(Vector2 cursorPos, RectTransform areaRT)
        {
            if (areaRT == null) return true;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                CanvasRectTransform, cursorPos, null, out Vector2 localPoint
            );

            return areaRT.rect.Contains(localPoint);
        }

        protected virtual void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame && !IsCursorPosInArea(mouse.position.ReadValue(), WindowAreaRectTransform))
            {
                Close();
            }
        }
    }
}
