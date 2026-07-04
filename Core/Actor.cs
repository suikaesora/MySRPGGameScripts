using System;
using System.Collections.Generic;
using UnityEngine;

namespace MySRPGGame.Core
{
    /// <summary>
    /// アクター。キャラクターの論理単位
    /// </summary>
    public class Actor
    {
        public string ActorName { get; }

        public int Hp { get; private set; }

        // 結果パラメータ

        /// <summary>
        /// 攻撃力
        /// </summary>
        public int Atk => Str + ((Weapons != null && Weapons.Count > 0) ? Weapons[0].Atk : 0);

        /// <summary>
        /// 魔法攻撃力
        /// </summary>
        public int MAtk => Mag;

        /// <summary>
        /// 防御力
        /// </summary>
        public int Def => SDef;

        /// <summary>
        /// 魔法防御力
        /// </summary>
        public int MDef => SMDef;

        /// <summary>
        /// 移動力
        /// </summary>
        public int Mov => SMov;

        // 元パラメータ

        public int MaxHp { get; set; }

        /// <summary>
        /// 力
        /// </summary>
        public int Str { get; set; }

        /// <summary>
        /// 魔力
        /// </summary>
        public int Mag { get; set; }

        /// <summary>
        /// 守備力
        /// </summary>
        public int SDef { get; set; }

        /// <summary>
        /// 魔法守備力
        /// </summary>
        public int SMDef { get; set; }

        /// <summary>
        /// 基本移動力
        /// </summary>
        public int SMov { get; set; }

        public MilitaryTypeData MilitaryTypeDataRef { get; private set; }

        public List<WeaponData> Weapons { get; private set; }

        public ActorData Data { get; private set; }

        public TroopData TroopDataRef { get; private set; }

        public Action<int> SetHpEvent { get; set; }

        public Actor(ActorInitData actorInitData)
        {
            ActorName = actorInitData.ActorName;

            MaxHp = actorInitData.MaxHp;
            Hp = MaxHp;

            Str = actorInitData.Str;
            Mag = actorInitData.Mag;
            SDef = actorInitData.SDef;
            SMDef = actorInitData.SMDef;
            SMov = actorInitData.SMov;

            MilitaryTypeDataRef = actorInitData.MilitaryTypeDataRef;
            Weapons = new List<WeaponData>();
            foreach (WeaponData weapon in actorInitData.WeaponDataList)
            {
                AddWeapon(weapon);
            }

            TroopDataRef = actorInitData.TroopDataRef;
        }

        public void Attack(Actor targetActor)
        {
            int damageValue = Mathf.Max(0, Atk - targetActor.Def);
            targetActor.Damage(damageValue);
        }

        public void Heal(int healValue)
        {
            SetHp(Hp + healValue);
        }

        public bool Damage(int damageValue)
        {
            SetHp(Hp - damageValue);

            return Hp == 0;
        }

        public void SetHp(int hp)
        {
            Hp = Mathf.Clamp(hp, 0, MaxHp);

            SetHpEvent?.Invoke(Hp);
        }

        public void AddWeapon(WeaponData weapon)
        {
            if (!MilitaryTypeDataRef.WeaponTypes.Contains(weapon.WeaponType))
            {
                Debug.LogError("装備できない武器です");
            }
            Weapons.Add(weapon);
        }

        public void RemoveWeapon(WeaponData weapon)
        {
            Weapons.Remove(weapon);
        }
    }
}
