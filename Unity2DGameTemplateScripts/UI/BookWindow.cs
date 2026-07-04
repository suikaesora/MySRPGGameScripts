using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Unity2DGameTemplate.UI
{
    public class BookWindow : MonoBehaviour
    {
        [SerializeField]
        private BookPage[] _bookPages;

        [SerializeField]
        private BookContentsElement _bookContentsElementPrefab;

        [SerializeField]
        private Transform _bookContentsElementParent;

        [SerializeField]
        private Transform _bookPageContentParent;

        [SerializeField]
        private int _startPageIndex = 0;

        private List<BookContentsElement> _bookContentsElements = new List<BookContentsElement>();

        private GameObject _currentPageContent;

        public void SelectPage(int selectedElementIndex)
        {
            _currentPageContent = Instantiate(_bookPages[selectedElementIndex].PageContent, _bookPageContentParent);
        }
        public void DeselectPage()
        {
            if (_currentPageContent != null)
            {
                Destroy(_currentPageContent);
            }
        }

        private void CleanUp()
        {
            foreach (BookContentsElement bookContentsElement in _bookContentsElements)
            {
                Destroy(bookContentsElement.gameObject);
            }
            _bookContentsElements = new List<BookContentsElement>();
        }

        private void OnClick(int selectedElementIndex)
        {
            DeselectPage();
            SelectPage(selectedElementIndex);
        }

        private void OnPointerEnter(int selectedElementIndex)
        {

        }

        private void OnPointeExit(int selectedElementIndex)
        {

        }

        private void OnEnable()
        {
            for (int index = 0; index < _bookPages.Length; ++index)
            {
                BookContentsElement element = Instantiate(_bookContentsElementPrefab, _bookContentsElementParent);
                element.Init(index, _bookPages[index].PageTitle, OnClick, OnPointerEnter, OnPointeExit);
                _bookContentsElements.Add(element);
            }

            if (_startPageIndex >= 0)
            {
                SelectPage(_startPageIndex);
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            DeselectPage();
            CleanUp();
        }
    }
}
