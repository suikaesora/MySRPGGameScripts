using UnityEngine;
using UnityEngine.UI;

namespace Unity2DGameTemplate.UI
{
    public class XPostButton : MonoBehaviour
    {
        [SerializeField]
        private Button _postButton;

        [SerializeField]
        private string _content;

        [SerializeField]
        private string[] _hashTags;

        private void Start()
        {
            _postButton.onClick.AddListener(() => XPostSystem.PostWithGameURL(_content, _hashTags));
        }
    }
}
