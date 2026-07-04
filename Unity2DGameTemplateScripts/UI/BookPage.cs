using UnityEngine;

namespace Unity2DGameTemplate.UI
{
    [CreateAssetMenu(menuName = "ScriptableObject/BookPage")]
    public class BookPage : ScriptableObject
    {
        [SerializeField]
        private string _pageTitle = "ページのタイトル";

        [SerializeField]
        private GameObject _pageContent;

        public string PageTitle => _pageTitle;

        public GameObject PageContent => _pageContent;
    }
}
