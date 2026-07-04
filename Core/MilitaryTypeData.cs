using System.Collections.Generic;
using UnityEngine;

namespace MySRPGGame.Core
{
    /// <summary>
    /// ï∫éÌÉfÅ[É^
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObject/MilitaryType")]
    public class MilitaryTypeData : ScriptableObject
    {
        [SerializeField]
        private string typeName;

        [SerializeField]
        private GameObject defaultActorObject;

        [SerializeField]
        private List<WeaponData.WeaponTypeEnum> weaponTypes;

        public string TypeName => typeName;

        public GameObject DefaultActorObject => defaultActorObject;

        public List<WeaponData.WeaponTypeEnum> WeaponTypes => weaponTypes;
    }
}
