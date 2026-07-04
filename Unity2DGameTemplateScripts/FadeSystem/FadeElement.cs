using DG.Tweening;
using Unity2DGameTemplate.StateMachines;
using UnityEngine;

namespace Unity2DGameTemplate.FadeSystem
{
    public enum EFadeElementStateName
    {
        FadeOut,
        Waiting,
        FadeIn,
    }

    public class FadeElementMessage
    {
        public float FadeOutDuration;
        public float FadeInDuration;
        public Color FadeColor;
    }

    /// <summary>
    /// フェード要素（フェードアウト・フェードインが実行される）
    /// </summary>
    public abstract class FadeElement : StateMachineBase<EFadeElementStateName, FadeElement, FadeElementMessage>
    {
        protected override FadeElement StateMachine => this;

        public bool IsFadeEnd { get; set; }

        protected float FadeOutDuration;
        protected float FadeInDuration;
        protected Color FadeColor;

        [SerializeField]
        protected float defaultFadeOutDuration = 2f;

        [SerializeField]
        protected float defaultFadeInDuration = 2f;

        [SerializeField]
        protected Color defaultColor = Color.black;

        public virtual void Init()
        {
            StateMachineMessage = new FadeElementMessage();

            SetFadeOutDuration(defaultFadeOutDuration);
            SetFadeInDuration(defaultFadeInDuration);
            SetFadeColor(defaultColor);

            IsFadeEnd = false;
        }

        // フェード要素の初期化メソッド

        public void SetFadeOutDuration(float duration)
        {
            FadeOutDuration = duration;
            StateMachineMessage.FadeOutDuration = FadeOutDuration;
        }
        public void SetFadeInDuration(float duration)
        {
            FadeInDuration = duration;
            StateMachineMessage.FadeInDuration = FadeInDuration;
        }
        public void SetFadeColor(Color color)
        {
            FadeColor = color;
            StateMachineMessage.FadeColor = FadeColor;
        }
    }

    public abstract class FadeElementStateFadeOut : StateBase<EFadeElementStateName, FadeElement, FadeElementMessage>
    {
        public override EFadeElementStateName StateName => EFadeElementStateName.FadeOut;

        public abstract override void OnEnter(FadeElement sm, FadeElementMessage message);

        public abstract override void Update(FadeElement sm, FadeElementMessage message);

        public abstract override void OnExit(FadeElement sm, FadeElementMessage message);
    }

    public abstract class FadeElementStateWaiting : StateBase<EFadeElementStateName, FadeElement, FadeElementMessage>
    {
        public override EFadeElementStateName StateName => EFadeElementStateName.Waiting;

        public abstract override void OnEnter(FadeElement sm, FadeElementMessage message);

        public abstract override void Update(FadeElement sm, FadeElementMessage message);

        public abstract override void OnExit(FadeElement sm, FadeElementMessage message);
    }

    public abstract class FadeElementStateFadeIn : StateBase<EFadeElementStateName, FadeElement, FadeElementMessage>
    {
        public override EFadeElementStateName StateName => EFadeElementStateName.FadeIn;

        public abstract override void OnEnter(FadeElement sm, FadeElementMessage message);

        public abstract override void Update(FadeElement sm, FadeElementMessage message);

        public abstract override void OnExit(FadeElement sm, FadeElementMessage message);
    }
}
