using Unity2DGameTemplate.AudioSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Unity2DGameTemplate.UI
{
    /// <summary>
    /// ボリューム調整スライダー
    /// </summary>
    public class AudioVolumeSlider : MonoBehaviour
    {
        [SerializeField]
        private Slider _volumeSlider;

        [SerializeField]
        private EAudioElementType _audioElementType;

        public enum EAudioElementType
        {
            Master,
            BGM,
            SE,
        }

        public EAudioElementType AudioElementType => _audioElementType;

        private void Start()
        {
            // タイプごとに、初期化とスライダー調整時のコールバックを設定
            switch (_audioElementType)
            {
                case EAudioElementType.Master:
                    _volumeSlider.value = AudioManager.Instance.MasterVolume;
                    _volumeSlider.onValueChanged.AddListener(value => AudioManager.Instance.SetDefaultMasterVolumeForced(value));
                    break;
                case EAudioElementType.BGM:
                    _volumeSlider.value = AudioManager.Instance.DefaultVolumeBgm;
                    _volumeSlider.onValueChanged.AddListener(value => AudioManager.Instance.SetDefaultBgmVolumeForced(value));
                    break;
                case EAudioElementType.SE:
                    _volumeSlider.value = AudioManager.Instance.DefaultVolumeSe;
                    _volumeSlider.onValueChanged.AddListener(value => AudioManager.Instance.SetDefaultSeVolume(value));
                    break;
            }
        }
    }
}
