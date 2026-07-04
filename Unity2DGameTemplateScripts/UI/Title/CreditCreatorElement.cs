using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity2DGameTemplate.UI.Title
{
    public class CreditCreatorElement : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _nameText;

        [SerializeField]
        private TextMeshProUGUI _roleText;

        [SerializeField]
        private Button[] _openUrlButtons;

        private CreatorData _creatorData;

        private void OnUrlButtonClickEvent()
        {
            Application.OpenURL(_creatorData.Url);
        }

        public void Init(CreatorData creatorData)
        {
            _nameText.text = creatorData.Name;
            
            switch (creatorData.RoleType)
            {
                case ECreatorRoleType.Engineer:
                    _roleText.text = "エンジニア";
                    break;
                case ECreatorRoleType.Art:
                    _roleText.text = "アート";
                    break;
                case ECreatorRoleType.Sound:
                    _roleText.text = "サウンド";
                    break;
            }

            foreach (Button button in _openUrlButtons)
            {
                button.onClick.RemoveListener(OnUrlButtonClickEvent);
                button.onClick.AddListener(OnUrlButtonClickEvent);
            }

            _creatorData = creatorData;
        }
    }
}
