using System;
using System.Collections.Generic;
using Unity2DGameTemplate.AudioSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MySRPGGame.UI
{
    public class SimpleSelectiveWindow : MonoBehaviour
    {
        [SerializeField]
        private bool isUseMouseInput = false;

        [SerializeField]
        private InputAction moveForwardEvent;

        [SerializeField]
        private InputAction moveBackEvent;

        [SerializeField]
        private InputAction decideEvent;

        [SerializeField]
        private bool isUseWithInstantiate = true;

        [Header("IsUseWithInstantiate=true")]
        [SerializeField]
        private SimpleSelectiveElement elementPrefab;

        [Header("IsUseWithInstantiate=false")]
        [SerializeField]
        private List<SimpleSelectiveElement> elements;

        public int CurrentElementId { get; private set; }

        public bool IsInitialized { get; private set; }

        public bool IsUseMouseInput => isUseMouseInput;

        public void InitWithInstantiate(List<string> elementContents, Action<int> onDecideEvent)
        {
            if (!isUseWithInstantiate) return;

            elements = new List<SimpleSelectiveElement>();
            for (int index = 0; index < elementContents.Count; ++index)
            {
                SimpleSelectiveElement element = Instantiate(elementPrefab, transform);
                element.Init(this, index, isUseMouseInput);
                element.OnDecidedEvent += onDecideEvent;
            }

            if (!isUseMouseInput)
            {
                CurrentElementId = 0;
                elements[CurrentElementId].Select();
            }

            IsInitialized = true;
        }

        public void InitWithNotInstantiate(Action<int> onDecideEvent)
        {
            if (isUseWithInstantiate) return;

            for (int index = 0; index < elements.Count; ++index)
            {
                SimpleSelectiveElement element = elements[index];
                element.Init(this, index, isUseMouseInput);
                element.OnDecidedEvent += onDecideEvent;
            }

            if (!isUseMouseInput)
            {
                CurrentElementId = 0;
                elements[CurrentElementId].Select();
            }

            IsInitialized = true;
        }

        public SimpleSelectiveElement GetElement(int id)
        {
            if (elements == null) return null;

            return elements[id];
        }

        private void OnMoveForward(InputAction.CallbackContext context)
        {
            if (!IsInitialized) return;

            elements[CurrentElementId].Deselect();
            ++CurrentElementId;
            if (CurrentElementId == elements.Count) CurrentElementId = 0;
            elements[CurrentElementId].Select();

            AudioManager.Instance.PlaySe(AudioIDList.Cursor, false);
        }
        private void OnMoveBack(InputAction.CallbackContext context)
        {
            if (!IsInitialized) return;

            elements[CurrentElementId].Deselect();
            --CurrentElementId;
            if (CurrentElementId < 0) CurrentElementId = elements.Count - 1;
            elements[CurrentElementId].Select();

            AudioManager.Instance.PlaySe(AudioIDList.Cursor, false);
        }
        private void OnDecide(InputAction.CallbackContext context)
        {
            if (!IsInitialized) return;

            elements[CurrentElementId].Decide();

            AudioManager.Instance.PlaySe(AudioIDList.Decide, false);
        }

        private void OnEnable()
        {
            if (!isUseMouseInput)
            {
                moveForwardEvent.performed += OnMoveForward;
                moveBackEvent.performed += OnMoveBack;
                decideEvent.performed += OnDecide;

                moveForwardEvent?.Enable();
                moveBackEvent?.Enable();
                decideEvent?.Enable();
            }
        }
        private void OnDisable()
        {
            if (!isUseMouseInput)
            {
                moveForwardEvent.performed -= OnMoveForward;
                moveBackEvent.performed -= OnMoveBack;
                decideEvent.performed -= OnDecide;

                moveForwardEvent?.Disable();
                moveBackEvent?.Disable();
                decideEvent?.Disable();
            }

            IsInitialized = false;
        }
    }
}
