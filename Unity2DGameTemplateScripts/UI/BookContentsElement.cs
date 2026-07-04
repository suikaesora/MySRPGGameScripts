using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity2DGameTemplate.UI
{
    public class BookContentsElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private TextMeshProUGUI _titleText;

        private Action<int> _clickEvent;
        private Action<int> _pointerEnterEvent;
        private Action<int> _pointerExitEvent;

        public int Index { get; private set; }

        public void Init(int index, string title, Action<int> clickEvent, Action<int> pointerEnterEvent, Action<int> pointerExitEvent)
        {
            Index = index;

            _titleText.text = title;

            this._clickEvent = clickEvent;
            this._pointerEnterEvent = pointerEnterEvent;
            this._pointerExitEvent = pointerExitEvent;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _titleText.color = Color.red;
            _pointerEnterEvent?.Invoke(Index);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _titleText.color = Color.white;
            _pointerExitEvent?.Invoke(Index);
        }

        public void OnButtonClicked()
        {
            _clickEvent?.Invoke(Index);
        }
    }
}
