using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity2DGameTemplate.UI
{
    public class CreditElement : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _nameText;

        [SerializeField]
        private Image _iconImage;

        [SerializeField]
        private Button _openUrlButton;

        [SerializeField]
        private string _name;

        [SerializeField]
        private Sprite _iconSprite;

        [SerializeField]
        private string _accountUrl;

        private void Start()
        {
            _nameText.text = _name;
            _iconImage.sprite = _iconSprite;
            _openUrlButton.onClick.AddListener(() => Application.OpenURL(_accountUrl));
        }
    }
}
