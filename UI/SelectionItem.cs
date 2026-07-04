using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private TextMeshProUGUI itemNameText;

    [SerializeField]
    private Image itemImage;

    [SerializeField]
    private Color selectedColor = new Color(1f, 0.8f, 0.75f);
    [SerializeField]
    private Color notSelectedColor = new Color(1f, 1f, 1f);

    private Action<int, bool> pointerEvent;

    public string ItemName { get; private set; }
    public int Index { get; private set; }

    public void Init(string itemName, int index, Action<int, bool> pointerEvent)
    {
        ItemName = itemName;
        Index = index;

        itemNameText.text = itemName;

        itemImage.color = Color.white;

        this.pointerEvent = pointerEvent;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerEvent?.Invoke(Index, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerEvent?.Invoke(Index, false);
    }

    public void Activate()
    {
        itemImage.color = selectedColor;
    }

    public void Deactivate()
    {
        itemImage.color = notSelectedColor;
    }
}
