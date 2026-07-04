using System;
using Unity2DGameTemplate.FadeSystem;
using Unity2DGameTemplate.StateMachines;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Unity2DGameTemplate.UI
{
    public enum EEnhancedButtonState
    {
        Idle,
        PointerOn,
        Clicked,
    }

    public class EnhancedButtonMessage
    {
        public RectTransform ButtonBgImageRectTransform;
        public Image ButtonBgImage;
        public bool IsPointerOn;
    }

    public abstract class EnhancedButton : StateMachineBase<EEnhancedButtonState, EnhancedButton, EnhancedButtonMessage>
    {
        protected override EnhancedButton StateMachine => this;

        [SerializeField]
        protected bool IsClickOverride = false;

        [SerializeField]
        protected RectTransform ButtonBgImageRectTransform;

        [SerializeField]
        protected Image ButtonBgImage;

        public Action OnStartIdleEvent { get; set; }
        public Action OnStartPointerOnEvent { get; set; }
        public Action OnStartClickedEvent { get; set; }

        protected bool IsPointerOn = false;

        protected RectTransform RectTransformRef;

        public virtual void Init()
        {
            StateMachineMessage = new EnhancedButtonMessage();
            StateMachineMessage.ButtonBgImageRectTransform = ButtonBgImageRectTransform;
            StateMachineMessage.ButtonBgImage = ButtonBgImage;
            RectTransformRef = GetComponent<RectTransform>();
        }

        public void OnPointerEnter()
        {
            IsPointerOn = true;
            StateMachineMessage.IsPointerOn = IsPointerOn;

            if (CurrentStateName != EEnhancedButtonState.Clicked)
            {
                ChangeState(EEnhancedButtonState.PointerOn);
            }
        }

        public void OnPointerExit()
        {
            IsPointerOn = false;
            StateMachineMessage.IsPointerOn = IsPointerOn;

            if (CurrentStateName != EEnhancedButtonState.Clicked)
            {
                ChangeState(EEnhancedButtonState.Idle);
            }
        }

        public void OnClick()
        {
            if (!IsClickOverride && CurrentStateName == EEnhancedButtonState.Clicked) return;

            ChangeState(EEnhancedButtonState.Clicked);
        }

        protected override void Update()
        {
            base.Update();

            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransformRef, mouse.position.ReadValue(), null, out Vector2 localMousePos);

            if (RectTransformRef.rect.Contains(localMousePos))
            {
                if (!IsPointerOn)
                {
                    OnPointerEnter();
                }
                else if (mouse.leftButton.wasPressedThisFrame)
                {
                    OnClick();
                }
            }
            else
            {
                if (IsPointerOn)
                {
                    OnPointerExit();
                }
            }
        }
    }

    public abstract class EnhancedButtonStateIdle : StateBase<EEnhancedButtonState, EnhancedButton, EnhancedButtonMessage>
    {
        public override EEnhancedButtonState StateName => EEnhancedButtonState.Idle;

        public abstract override void OnEnter(EnhancedButton sm, EnhancedButtonMessage message);

        public abstract override void Update(EnhancedButton sm, EnhancedButtonMessage message);

        public abstract override void OnExit(EnhancedButton sm, EnhancedButtonMessage message);
    }

    public abstract class EnhancedButtonStatePointerOn : StateBase<EEnhancedButtonState, EnhancedButton, EnhancedButtonMessage>
    {
        public override EEnhancedButtonState StateName => EEnhancedButtonState.PointerOn;

        public abstract override void OnEnter(EnhancedButton sm, EnhancedButtonMessage message);

        public abstract override void Update(EnhancedButton sm, EnhancedButtonMessage message);

        public abstract override void OnExit(EnhancedButton sm, EnhancedButtonMessage message);
    }

    public abstract class EnhancedButtonStateClicked : StateBase<EEnhancedButtonState, EnhancedButton, EnhancedButtonMessage>
    {
        public override EEnhancedButtonState StateName => EEnhancedButtonState.Clicked;

        public abstract override void OnEnter(EnhancedButton sm, EnhancedButtonMessage message);

        public abstract override void Update(EnhancedButton sm, EnhancedButtonMessage message);

        public abstract override void OnExit(EnhancedButton sm, EnhancedButtonMessage message);
    }
}
