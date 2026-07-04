using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity2DGameTemplate.UI
{
    public class ScrollBarCurrent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform RectTransform { get; private set; }
        public bool IsOnPointer { get; private set; }

        private void Start()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsOnPointer = true;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            IsOnPointer = false;
        }
    }
}
