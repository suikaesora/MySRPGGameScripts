using UnityEngine;

namespace Unity2DGameTemplate.UI.Title
{
    public enum ECreatorRoleType
    {
        Engineer,
        Art,
        Sound,
    }

    [CreateAssetMenu(menuName = "ScriptableObject/Creator")]
    public class CreatorData : ScriptableObject
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        private ECreatorRoleType _roleType;

        [SerializeField]
        private string _url;

        public string Name => _name;

        public ECreatorRoleType RoleType => _roleType;

        public string Url => _url;
    }
}