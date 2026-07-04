using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MySRPGGame.UI
{
    public class StatusItem : MonoBehaviour
    {
        [SerializeField]
        private Image headerImage;

        [SerializeField]
        private TextMeshProUGUI headerText;

        [SerializeField]
        private TextMeshProUGUI valueText;

        public void Set(Color headerColor, Color valueTextColor, string valueStr)
        {
            headerImage.color = headerColor;
            valueText.color = valueTextColor;
            valueText.text = valueStr;
        }
    }
}
