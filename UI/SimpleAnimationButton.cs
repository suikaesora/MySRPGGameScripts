using UnityEngine;
using UnityEngine.UI;

namespace MySRPGGame.UI
{
    [RequireComponent(typeof(SimpleSelectiveElement))]
    [RequireComponent(typeof(Image))]
    public class SimpleAnimationButton : MonoBehaviour
    {
        [SerializeField]
        private Image decideKeyImage;

        private SimpleSelectiveElement selectiveElement;
        private Image targetImage;

        private void Awake()
        {
            selectiveElement = GetComponent<SimpleSelectiveElement>();
            targetImage = GetComponent<Image>();

            selectiveElement.OnInitEvent += _ =>
            {
                selectiveElement.OnSelectedEvent += _ =>
                {
                    targetImage.color = new Color(0.8f, 0.8f, 1f);
                    if (!selectiveElement.Window.IsUseMouseInput)
                    {
                        decideKeyImage.gameObject.SetActive(true);
                    }
                };
                selectiveElement.OnDeselectedEvent += _ =>
                {
                    targetImage.color = new Color(1f, 1f, 1f);
                    if (!selectiveElement.Window.IsUseMouseInput)
                    {
                        decideKeyImage.gameObject.SetActive(false);
                    }
                };
            };
        }
    }
}
