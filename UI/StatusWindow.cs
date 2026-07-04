using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace MySRPGGame.UI
{
    public class StatusWindow : MonoBehaviour
    {
        public enum StatusItemId
        {
            Hp,
            MilitaryType,
            Troop,
            Weapon,

            Str,
            Mag,
            SDef,
            SMDef,
            SMov,

            Atk,
            MAtk,
            Def,
            MDef,
            Mov,
        }

        [SerializeField]
        private TextMeshProUGUI titleText;

        [System.Serializable]
        public class StatusItemIdAndItem
        {
            public StatusItemId ItemId;
            public StatusItem Item;
        }

        [SerializeField]
        private List<StatusItemIdAndItem> statusItemList;

        public void SetTitle(string title)
        {
            titleText.text = title;
        }

        public StatusItem GetStatusItem(StatusItemId itemId)
        {
            return statusItemList.First(x => x.ItemId == itemId).Item;
        }
    }
}
