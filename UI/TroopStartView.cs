using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MySRPGGame.UI
{
    public class TroopStartView : MonoBehaviour
    {
        [SerializeField]
        private Image troopImage;

        [SerializeField]
        private TextMeshProUGUI troopNameText;

        [SerializeField]
        private float viewAlpha;

        public void Init(Color color, string name)
        {
            color.a = viewAlpha;
            troopImage.color = color;
            troopNameText.text = name;
        }
    }
}
