using UnityEngine;

namespace Unity2DGameTemplate.UI
{
    public class VerticalScrollAreaInitializer : MonoBehaviour
    {
        [SerializeField]
        private VerticalScrollArea _verticalScrollArea;

        private void OnEnable()
        {
            _verticalScrollArea.SetScroll(0f);
        }
    }
}

