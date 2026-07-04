using MySRPGGame.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionWindow : MonoBehaviour
{
    [SerializeField]
    private Vector2 initialPosition;

    [SerializeField]
    private SelectionItem selectionItemPrefab;

    [SerializeField]
    private InputAction moveItemForwardAction;
    [SerializeField]
    private InputAction moveItemBackAction;

    [SerializeField]
    private InputAction decideAction;
    [SerializeField]
    private InputAction cancelAction;

    private SelectionItem[] items;

    private Action<int> selectedEvent;
    private Action<int> canceledEvent;


    private List<Action<InputAction.CallbackContext>> decideEvents = new List<Action<InputAction.CallbackContext>>();
    private List<Action<InputAction.CallbackContext>> cancelEvents = new List<Action<InputAction.CallbackContext>>();
    public void RegisterDecideEvent(Action<InputAction.CallbackContext> decideEvent)
    {
        decideEvents.Add(decideEvent);
        decideAction.performed += decideEvent;
    }

    public void RegisterCancelEvent(Action<InputAction.CallbackContext> cancelEvent)
    {
        cancelEvents.Add(cancelEvent);
        cancelAction.performed += cancelEvent;
    }

    public int CurrentItemIndex { get; private set; }

    public void Init(string[] selectionItemNames, Action<int> selectedEvent, Action<int> canceledEvent, Vector2 position)
    {
        RegisterDecideEvent(_ =>
        {
            if (CurrentItemIndex == -1) return;

            selectedEvent?.Invoke(CurrentItemIndex);
            CloseWindow();
        });
        RegisterCancelEvent(_ =>
        {
            canceledEvent?.Invoke(CurrentItemIndex);
            CloseWindow();
        });

        

        GetComponent<RectTransform>().anchoredPosition = position;

        items = new SelectionItem[selectionItemNames.Length];

        for (int i = 0; i < selectionItemNames.Length; ++i)
        {
            SelectionItem item = Instantiate(selectionItemPrefab, transform);
            item.Init(selectionItemNames[i], i, OnPointerItem);
            items[i] = item;
        }

        SetItemIndex(0);
    }

    private void SetItemIndex(int index)
    {
        items[CurrentItemIndex]?.Deactivate();
        if (index == -1) return;
        CurrentItemIndex = index;
        items[index].Activate();
    }

    public void OnPointerItem(int index, bool isEnter)
    {
        SetItemIndex(isEnter ? index : -1);
    }

    public void CloseWindow()
    {
        Destroy(gameObject);
    }

    private void MoveItem(bool isForward)
    {
        int index = CurrentItemIndex + (isForward ? 1 : -1);
        SetItemIndex((int)Mathf.Repeat(index, items.Length));
    }

    private void MoveItemForward(InputAction.CallbackContext context)
    {
        MoveItem(true);
    }

    private void MoveItemBack(InputAction.CallbackContext context)
    {
        MoveItem(false);
    }

    private void OnEnable()
    {
        moveItemForwardAction.performed += MoveItemForward;
        moveItemBackAction.performed += MoveItemBack;

        moveItemForwardAction?.Enable();
        moveItemBackAction?.Enable();

        decideAction?.Enable();
        cancelAction?.Enable();
    }

    private void OnDisable()
    {
        moveItemForwardAction.performed -= MoveItemForward;
        moveItemBackAction.performed -= MoveItemBack;

        moveItemForwardAction?.Disable();
        moveItemBackAction?.Disable();

        foreach (var dEvent in decideEvents)
        {
            decideAction.performed -= dEvent;
        }
        foreach (var cEvent in cancelEvents)
        {
            cancelAction.performed -= cEvent;
        }

        decideAction?.Disable();
        cancelAction?.Disable();
    }
}
