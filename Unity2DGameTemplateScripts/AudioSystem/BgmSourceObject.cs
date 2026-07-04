using Unity2DGameTemplate.StateMachines;
using UnityEngine;

namespace Unity2DGameTemplate.AudioSystem
{
    public enum EBgmSourceName
    {
        FadeIn,
        FadeOut,
        Normal,
        Dead,
    }

    public class BgmSourceSMMessage
    {
        public BgmSourceObject BgmSourceObjectRef;
        public AudioSource AudioSourceRef;
    }

    public class BgmSourceObject : StateMachineBase<EBgmSourceName, BgmSourceObject, BgmSourceSMMessage>
    {
        [SerializeField]
        private AudioSource _audioSource;

        public float TargetVolumeRatio { get; private set; }
        public float TargetDuration { get; private set; }

        public float DefaultVolumeBgm { get; private set; }

        protected override BgmSourceObject StateMachine => this;

        private void Awake()
        {
            StateMachineMessage = new BgmSourceSMMessage();
            StateMachineMessage.BgmSourceObjectRef = this;
            StateMachineMessage.AudioSourceRef = _audioSource;

            RegisterState(new BgmSourceStateFadeIn());
            RegisterState(new BgmSourceStateFadeOut());
            RegisterState(new BgmSourceStateNormal());
            RegisterState(new BgmSourceStateDead());

            _audioSource.volume = 0f;
        }

        public void SetTarget(float volumeRatio, float defaultVolume, float duration)
        {
            TargetVolumeRatio = volumeRatio;
            DefaultVolumeBgm = defaultVolume;
            TargetDuration = duration;
        }

        public void SetClip(AudioClip clip)
        {
            _audioSource.clip = clip;
        }

        public void SetDefaultVolume(float volume)
        {
            if (CurrentStateName == EBgmSourceName.Normal)
            {
                DefaultVolumeBgm = volume;
            }
        }

        /// <summary>
        /// InOutSineのイージング
        /// https://easings.net/ja#easeInOutSine
        /// </summary>
        /// <param name="progress">進行度</param>
        /// <returns></returns>
        public float GetEasingRatio(float progress)
        {
            return -(Mathf.Cos(Mathf.PI * progress) - 1) / 2f;
        }
    }

    public class BgmSourceStateFadeIn : StateBase<EBgmSourceName, BgmSourceObject, BgmSourceSMMessage>
    {
        public override EBgmSourceName StateName => EBgmSourceName.FadeIn;

        private float _originalVolume;
        private float _volumeChangeTimer;

        public override void OnEnter(BgmSourceObject sm, BgmSourceSMMessage message)
        {
            _originalVolume = message.AudioSourceRef.volume;
            message.AudioSourceRef.Play();
        }

        public override void Update(BgmSourceObject sm, BgmSourceSMMessage message)
        {
            _volumeChangeTimer += Time.deltaTime;

            float currentRatio = sm.GetEasingRatio(_volumeChangeTimer / sm.TargetDuration);
            message.AudioSourceRef.volume = Mathf.Lerp(_originalVolume, sm.TargetVolumeRatio * sm.DefaultVolumeBgm, currentRatio);

            if (_volumeChangeTimer > sm.TargetDuration)
            {
                message.AudioSourceRef.volume = sm.TargetVolumeRatio * sm.DefaultVolumeBgm;
                sm.ChangeState(EBgmSourceName.Normal);
            }
        }

        public override void OnExit(BgmSourceObject sm, BgmSourceSMMessage info)
        {
        }
    }

    public class BgmSourceStateFadeOut : StateBase<EBgmSourceName, BgmSourceObject, BgmSourceSMMessage>
    {
        public override EBgmSourceName StateName => EBgmSourceName.FadeOut;

        private float _originalVolume;
        private float _volumeChangeTimer;

        public override void OnEnter(BgmSourceObject sm, BgmSourceSMMessage message)
        {
            _originalVolume = message.AudioSourceRef.volume;
        }

        public override void Update(BgmSourceObject sm, BgmSourceSMMessage message)
        {
            _volumeChangeTimer += Time.deltaTime;

            float currentRatio = sm.GetEasingRatio(_volumeChangeTimer / sm.TargetDuration);
            message.AudioSourceRef.volume = Mathf.Lerp(_originalVolume, sm.TargetVolumeRatio * sm.DefaultVolumeBgm, currentRatio);

            if (_volumeChangeTimer > sm.TargetDuration)
            {
                message.AudioSourceRef.volume = sm.TargetVolumeRatio * sm.DefaultVolumeBgm;
                sm.ChangeState(EBgmSourceName.Dead);
            }
        }

        public override void OnExit(BgmSourceObject sm, BgmSourceSMMessage message)
        {
        }
    }

    public class BgmSourceStateNormal : StateBase<EBgmSourceName, BgmSourceObject, BgmSourceSMMessage>
    {
        public override EBgmSourceName StateName => EBgmSourceName.Normal;

        public override void OnEnter(BgmSourceObject sm, BgmSourceSMMessage message)
        {
        }

        public override void Update(BgmSourceObject sm, BgmSourceSMMessage message)
        {
            message.AudioSourceRef.volume = sm.TargetVolumeRatio * sm.DefaultVolumeBgm;
        }

        public override void OnExit(BgmSourceObject sm, BgmSourceSMMessage message)
        {
        }
    }

    public class BgmSourceStateDead : StateBase<EBgmSourceName, BgmSourceObject, BgmSourceSMMessage>
    {
        public override EBgmSourceName StateName => EBgmSourceName.Dead;

        public override void OnEnter(BgmSourceObject sm, BgmSourceSMMessage message)
        {
            Object.Destroy(sm.gameObject);
        }

        public override void Update(BgmSourceObject sm, BgmSourceSMMessage message)
        {
        }

        public override void OnExit(BgmSourceObject sm, BgmSourceSMMessage message)
        {
        }
    }
}
