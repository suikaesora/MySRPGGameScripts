using DG.Tweening;
using Unity2DGameTemplate.StateMachines;
using UnityEngine;

namespace Unity2DGameTemplate.UI.Title
{
    public class SampleEnhancedButton : EnhancedButton
    {
        [SerializeField]
        private float _pointerOnSizeRatio = 1.1f;

        [SerializeField]
        private float _imageAnimationDuration = 0.25f;

        [SerializeField]
        private float _clickedSizeRatio = 0.85f;

        [SerializeField]
        private float _clickedAnimationDuration = 0.25f;

        [SerializeField]
        private float stayPointerOnShakePositionStrength = 1.0f;

        public Vector2 OriginalImageSizeDelta { get; private set; }

        public float PointerOnSizeRatio => _pointerOnSizeRatio;
        public float ImageAnimationDuration => _imageAnimationDuration;
        public float ClickedSizeRatio => _clickedSizeRatio;
        public float ClickedAnimationDuration => _clickedAnimationDuration;

        public float StayPointerOnShakePositionStrength => stayPointerOnShakePositionStrength;

        public override void Init()
        {
            base.Init();

            RegisterState(new SampleEnhancedButtonStateIdle(this));
            RegisterState(new SampleEnhancedButtonStatePointerOn(this));
            RegisterState(new SampleEnhancedButtonStateClicked(this));

            OriginalImageSizeDelta = ButtonBgImageRectTransform.sizeDelta;

            InitState(EEnhancedButtonState.Idle);
        }

        private void Awake()
        {
            Init();
        }
    }

    public class SampleEnhancedButtonStateIdle : EnhancedButtonStateIdle
    {
        private SampleEnhancedButton _sampleEnhancedButton;

        private Tween _startIdleAnim;

        public SampleEnhancedButtonStateIdle(SampleEnhancedButton sampleEnhancedButton)
        {
            _sampleEnhancedButton = sampleEnhancedButton;
        }

        public override void OnEnter(EnhancedButton sm, EnhancedButtonMessage message)
        {
            sm.OnStartIdleEvent?.Invoke();

            message.ButtonBgImageRectTransform.DOSizeDelta(
                _sampleEnhancedButton.OriginalImageSizeDelta,
                _sampleEnhancedButton.ImageAnimationDuration);
        }

        public override void Update(EnhancedButton sm, EnhancedButtonMessage message)
        {
        }

        public override void OnExit(EnhancedButton sm, EnhancedButtonMessage message)
        {
            _startIdleAnim?.Kill();
        }
    }

    public class SampleEnhancedButtonStatePointerOn : EnhancedButtonStatePointerOn
    {
        private SampleEnhancedButton _sampleEnhancedButton;

        private Tween _startPointerOnAnim;

        private Tween _stayPointerOnAnim;

        private bool _stayAnimFlag;

        public SampleEnhancedButtonStatePointerOn(SampleEnhancedButton sampleEnhancedButton)
        {
            _sampleEnhancedButton = sampleEnhancedButton;
        }

        public override void OnEnter(EnhancedButton sm, EnhancedButtonMessage message)
        {
            sm.OnStartPointerOnEvent?.Invoke();

            _stayAnimFlag = false;
            _startPointerOnAnim = message.ButtonBgImageRectTransform.DOSizeDelta(
                _sampleEnhancedButton.OriginalImageSizeDelta * _sampleEnhancedButton.PointerOnSizeRatio,
                _sampleEnhancedButton.ImageAnimationDuration)
                .OnComplete(() => _stayAnimFlag = true);
        }

        public override void Update(EnhancedButton sm, EnhancedButtonMessage message)
        {
            if (_stayAnimFlag)
            {
                _stayAnimFlag = false;

                _stayPointerOnAnim =
                    message.ButtonBgImageRectTransform.DOShakePosition(1f, _sampleEnhancedButton.StayPointerOnShakePositionStrength, snapping: true)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1);
                _stayPointerOnAnim.Play();
            }
        }

        public override void OnExit(EnhancedButton sm, EnhancedButtonMessage message)
        {
            _startPointerOnAnim?.Kill();
            _stayPointerOnAnim?.Kill();
        }
    }

    public class SampleEnhancedButtonStateClicked : EnhancedButtonStateClicked
    {
        private SampleEnhancedButton _sampleEnhancedButton;

        private Tween _startClickedAnim;

        public SampleEnhancedButtonStateClicked(SampleEnhancedButton sampleEnhancedButton)
        {
            _sampleEnhancedButton = sampleEnhancedButton;
        }

        public override void OnEnter(EnhancedButton sm, EnhancedButtonMessage message)
        {
            sm.OnStartClickedEvent?.Invoke();

            _startClickedAnim = DOTween.Sequence()
                .Append(message.ButtonBgImageRectTransform.DOSizeDelta(
                    _sampleEnhancedButton.OriginalImageSizeDelta * _sampleEnhancedButton.ClickedSizeRatio,
                    _sampleEnhancedButton.ClickedAnimationDuration / 2f).SetEase(Ease.OutSine)
                )
                .Append(message.ButtonBgImageRectTransform.DOSizeDelta(
                    _sampleEnhancedButton.OriginalImageSizeDelta,
                    _sampleEnhancedButton.ClickedAnimationDuration / 2f).SetEase(Ease.InSine)
                )
                .OnComplete(() => 
                {
                    if (message.IsPointerOn)
                    {
                        sm.ChangeState(EEnhancedButtonState.PointerOn);
                    }
                    else
                    {
                        sm.ChangeState(EEnhancedButtonState.Idle);
                    }
                });
            _startClickedAnim.Play();

        }

        public override void Update(EnhancedButton sm, EnhancedButtonMessage message)
        {
        }

        public override void OnExit(EnhancedButton sm, EnhancedButtonMessage message)
        {
            _startClickedAnim?.Kill();
        }
    }
}
