using UnityEngine;
using UnityEngine.UI;

namespace Unity2DGameTemplate.UI.Title
{
    public class CreditWindow : PopupWindow
    {
        [SerializeField]
        private CreatorData[] _creatorDatas;

        [SerializeField]
        private CreditCreatorElement _creatorElementPrefab;

        [SerializeField]
        private Transform _creatorElementParent;

        public override void InitOpened()
        {
            base.InitOpened();

            foreach (Transform child in _creatorElementParent) Destroy(child.gameObject);

            foreach (CreatorData creatorData in _creatorDatas)
            {
                CreditCreatorElement element = Instantiate(_creatorElementPrefab, _creatorElementParent);
                element.Init(creatorData);
            }
        }
    }
}
