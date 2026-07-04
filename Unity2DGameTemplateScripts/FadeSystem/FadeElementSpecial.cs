using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Unity2DGameTemplate.FadeSystem
{
    /// <summary>
    /// ちょっと特殊なフェード要素
    /// </summary>
    public class FadeElementSpecial : FadeElement
    {
        [SerializeField]
        private Image fadeImage;

        public Image FadeImage => fadeImage;

        public override void Init()
        {
            base.Init();

            RegisterState(new FESpecialStateFadeOut(this));
            RegisterState(new FESpecialStateWaiting(this));
            RegisterState(new FESpecialStateFadeIn(this));

            IsFadeEnd = false;
        }
    }

    public class FESpecialStateFadeOut : FadeElementStateFadeOut
    {
        private FadeElementSpecial feSpecial;
        public FESpecialStateFadeOut(FadeElementSpecial fadeElementSpecial)
        {
            feSpecial = fadeElementSpecial;
        }

        public override void OnEnter(FadeElement sm, FadeElementMessage message)
        {
            feSpecial.FadeImage.gameObject.SetActive(true);

            RectTransform rt = feSpecial.FadeImage.GetComponent<RectTransform>();

            Sequence sequence = DOTween.Sequence()
                .AppendCallback(() =>
                {
                    Color color = message.FadeColor;
                    color.a = 0f;
                    feSpecial.FadeImage.color = color;

                    rt.anchoredPosition = new Vector2(-1920f, 0f);
                })
                .Append(feSpecial.FadeImage.DOFade(1f, message.FadeOutDuration))
                .Join(rt.DOAnchorPos(new Vector2(0f, 0f), message.FadeOutDuration))
                .OnComplete(() => sm.ChangeState(EFadeElementStateName.Waiting));
        }

        public override void Update(FadeElement sm, FadeElementMessage message)
        {

        }

        public override void OnExit(FadeElement sm, FadeElementMessage message)
        {

        }
    }

    public class FESpecialStateWaiting : FadeElementStateWaiting
    {
        private FadeElementSpecial feSpecial;
        public FESpecialStateWaiting(FadeElementSpecial fadeElementSpecial)
        {
            feSpecial = fadeElementSpecial;
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

    public class FESpecialStateFadeIn : FadeElementStateFadeIn
    {
        private FadeElementSpecial feSpecial;
        public FESpecialStateFadeIn(FadeElementSpecial fadeElementSpecial)
        {
            feSpecial = fadeElementSpecial;
        }

        public override void OnEnter(FadeElement sm, FadeElementMessage message)
        {
            RectTransform rt = feSpecial.FadeImage.GetComponent<RectTransform>();

            Sequence sequence = DOTween.Sequence()
                .Append(feSpecial.FadeImage.DOFade(0f, message.FadeInDuration))
                .Join(rt.DOAnchorPos(new Vector2(1920f, 0f), message.FadeInDuration))
                .OnComplete(() =>
                {
                    sm.IsFadeEnd = true;
                    feSpecial.FadeImage.gameObject.SetActive(false);
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