using System.Collections.Generic;
using UnityEngine;

namespace MySRPGGame.Core
{
    /// <summary>
    /// アクターを初期化するためのデータ
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObject/ActorInitData")]
    public class ActorInitData : ScriptableObject
    {
        [SerializeField]
        private string actorInitDataID;

        [SerializeField]
        private string actorName;

        [SerializeField, Header("最大HP")]
        private int maxHp = 10;

        [SerializeField, Header("力/攻撃力に影響")]
        private int str = 2;

        [SerializeField, Header("魔力/魔法攻撃力に影響")]
        private int mag = 2;

        [SerializeField, Header("守備力/防御力に影響")]
        private int sDef = 1;

        [SerializeField, Header("魔法守備力/魔法防御力に影響")]
        private int sMDef = 1;

        [SerializeField, Header("基本移動力/移動力に影響")]
        private int sMov = 3;

        [SerializeField]
        private MilitaryTypeData militaryTypeData;

        [SerializeField]
        private List<WeaponData> weaponDataList;

        [SerializeField]
        private TroopData troopData;

        public string ActorInitDataID => actorInitDataID;

        public string ActorName => actorName;

        public int MaxHp => maxHp;

        /// <summary>
        /// 力
        /// </summary>
        public int Str => str;

        /// <summary>
        /// 知力
        /// </summary>
        public int Mag => mag;

        /// <summary>
        /// 守備力
        /// </summary>
        public int SDef => sDef;

        /// <summary>
        /// 魔法守備力
        /// </summary>
        public int SMDef => sMDef;

        /// <summary>
        /// 基本移動力
        /// </summary>
        public int SMov => sMov;

        public MilitaryTypeData MilitaryTypeDataRef => militaryTypeData;

        public List<WeaponData> WeaponDataList => weaponDataList;

        public TroopData TroopDataRef => troopData;
    }
}
