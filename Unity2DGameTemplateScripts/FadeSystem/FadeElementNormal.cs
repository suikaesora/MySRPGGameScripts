using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Unity2DGameTemplate.FadeSystem
{
    /// <summary>
    /// 普通のフェード要素
    /// </summary>
    public class FadeElementNormal : FadeElement
    {
        [SerializeField]
        private Image fadeImage;

        public Image FadeImage => fadeImage;

        public Tween FadeTween { get; set; }

        public override void Init()
        {
            base.Init();

            RegisterState(new FENormalStateFadeOut(this));
            RegisterState(new FENormalStateWaiting(this));
            RegisterState(new FENormalStateFadeIn(this));

            IsFadeEnd = false;
        }
    }

    public class FENormalStateFadeOut : FadeElementStateFadeOut
    {
        private FadeElementNormal feNormal;
        public FENormalStateFadeOut(FadeElementNormal fadeElementNormal)
        {
            feNormal = fadeElementNormal;
        }

        public override void OnEnter(FadeElement sm, FadeElementMessage message)
        {
            feNormal.FadeImage.gameObject.SetActive(true);

            Color color = message.FadeColor;
            color.a = 0f;
            feNormal.FadeImage.color = color;

            feNormal.FadeTween = feNormal.FadeImage.DOFade(1f, message.FadeOutDuration)
                .OnComplete(() => sm.ChangeState(EFadeElementStateName.Waiting));
        }

        public override void Update(FadeElement sm, FadeElementMessage message)
        {

        }

        public override void OnExit(FadeElement sm, FadeElementMessage message)
        {

        }
    }

    public class FENormalStateWaiting : FadeElementStateWaiting
    {
        private FadeElementNormal feNormal;
        public FENormalStateWaiting(FadeElementNormal fadeElementNormal)
        {
            feNormal = fadeElementNormal;
        }

        public override void OnEnter(FadeElement sm, FadeElementMessage message)
        {
        }

        public override void Update(FadeElement sm, FadeElementMessage message)
        {
        }

        public override void OnExit(FadeElement sm, FadeElementMessage message)
        {
        }
    }

    public class FENormalStateFadeIn : FadeElementStateFadeIn
    {
        private FadeElementNormal feNormal;
        public FENormalStateFadeIn(FadeElementNormal fadeElementNormal)
        {
            feNormal = fadeElementNormal;
        }

        public override void OnEnter(FadeElement sm, FadeElementMessage message)
        {
            feNormal.FadeImage.DOFade(0f, message.FadeOutDuration)
                .OnComplete(() =>
                {
                    sm.IsFadeEnd = true;
                    feNormal.FadeImage.gameObject.SetActive(false);
                });
        }

        public override void Update(FadeElement sm, FadeElementMessage message)
        {
        }

        public override void OnExit(FadeElement sm, FadeElementMessage message)
        {
        }
    }
}