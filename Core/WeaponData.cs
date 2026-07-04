using System.Collections.Generic;
using UnityEngine;

namespace MySRPGGame.Core
{
    /// <summary>
    /// ïêäÌÉfÅ[É^
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObject/Weapon")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField]
        private string weaponName;

        [SerializeField]
        protected int atk;

        [SerializeField]
        protected WeaponTypeEnum weaponType;

        [SerializeField]
        private Sprite iconSprite;

        public string WeaponName => weaponName;

        public int Atk => atk;

        public enum WeaponTypeEnum
        {
            Sword,
            Bow,
        }

        public WeaponTypeEnum WeaponType => weaponType;

        public Sprite IconSprite => iconSprite;

        public List<Vector2Int> GetAttackRange()
        {
            switch (weaponType)
            {
                case WeaponTypeEnum.Sword:

                    return new List<Vector2Int>()
                    {
                        Vector2Int.up,
                        Vector2Int.down,
                        Vector2Int.right,
                        Vector2Int.left,
                    };

                case WeaponTypeEnum.Bow:

                    return new List<Vector2Int>()
                    {
                        Vector2Int.up * 2,
                        Vector2Int.down * 2,
                        Vector2Int.right * 2,
                        Vector2Int.left * 2,
                        new Vector2Int(1, 1),
                        new Vector2Int(1, -1),
                        new Vector2Int(-1, -1),
                        new Vector2Int(-1, 1),
                    };

                default:
                    return new List<Vector2Int>();
            }
        }
    }
}