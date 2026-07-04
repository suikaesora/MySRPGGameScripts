using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;

namespace Unity2DGameTemplate.AudioSystem
{
    /// <summary>
    /// サウンドを管理するシングルトンクラスです。
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Serializable]
        public class AudioClipItem
        {
            public string Id;
            public AudioClip Clip;
        }

        [Header("BGMのフェード時間のデフォルト値")]
        [SerializeField]
        private float _defaultFadeBgmDuration;

        [Header("マスターのボリュームのデフォルト値")]
        [SerializeField]
        private float _defaultVolumeMaster;

        [Header("BGMのボリュームのデフォルト値")]
        [SerializeField]
        private float _defaultVolumeBgm;

        [Header("SEのボリュームのデフォルト値")]
        [SerializeField]
        private float _defaultVolumeSe;

        [Header("BGMのオーディオソースを生成するゲームオブジェクト")]
        [SerializeField]
        private GameObject _sourceBgmsObject;

        [Header("SEのオーディオソースを持ったゲームオブジェクト")]
        [SerializeField]
        private GameObject _sourceSesObject;

        [Header("オーディオクリップ")]
        [SerializeField]
        private AudioClipItem[] _seItems;
        [SerializeField]
        private AudioClipItem[] _bgmItems;

        [Header("BGMソースオブジェクトのプレハブ")]
        [SerializeField]
        private BgmSourceObject _bgmSourceObjectPrefab;

        private List<BgmSourceObject> _sourceBgms = new List<BgmSourceObject>();

        private AudioSource[] _sourceSes;

        public static AudioManager Instance;

        public float DefaultVolumeBgm => _defaultVolumeBgm;

        public float DefaultVolumeSe => _defaultVolumeSe;

        public float DefaultFadeBgmDuration => _defaultFadeBgmDuration;

        public float MasterVolume => _defaultVolumeMaster;

        private BgmSourceObject _currentBgmSource;

        private Dictionary<string, AudioClip> seDictionary = new Dictionary<string, AudioClip>();

        private const string MasterVolumeKey = "MASTER_VOLUME";
        private const string BgmVolumeKey = "BGM_VOLUME";
        private const string SeVolumeKey = "SE_VOLUME";

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            _sourceSes = _sourceSesObject.GetComponents<AudioSource>();

            // Awake内でデフォルトボリュームにセーブデータを反映
            SetDefaultMasterVolumeForced(PlayerPrefs.GetFloat(MasterVolumeKey, 0.5f));
            SetDefaultBgmVolumeForced(PlayerPrefs.GetFloat(BgmVolumeKey, 0.5f));
            SetDefaultSeVolume(PlayerPrefs.GetFloat(SeVolumeKey, 0.5f));
        }

        private AudioClip FindSeClip(string clipId)
        {
            if (seDictionary.ContainsKey(clipId)) return seDictionary[clipId];

            seDictionary.Add(clipId, Array.Find(_seItems, item => item.Id == clipId).Clip);
            return seDictionary[clipId];
        }
        private AudioClip FindBgmClip(string clipId)
        {
            return Array.Find(_bgmItems, item => item.Id == clipId).Clip;
        }

        /// <summary>
        /// BGMを再生
        /// </summary>
        /// <param name="clipId">クリップのID</param>
        public void PlayBgm(string clipId)
        {
            PlayBgm(clipId, _defaultFadeBgmDuration, 1f);
        }

        /// <summary>
        /// BGMを再生
        /// </summary>
        /// <param name="clipId">クリップのID</param>
        /// <param name="fadeBgmDuration">BGMのクロスフェード時間</param>
        /// <param name="volumeBgmRatio">新BGMのボリューム（デフォルトのボリュームに対する比率）</param>
        public void PlayBgm(string clipId, float fadeBgmDuration, float volumeBgmRatio)
        {
            if (_currentBgmSource != null)
            {
                // 再生中だったBGMをフェードアウト
                _currentBgmSource.SetTarget(0f, _defaultVolumeBgm, fadeBgmDuration);
                _currentBgmSource.ChangeState(EBgmSourceName.FadeOut);
            }

            _currentBgmSource = Instantiate(_bgmSourceObjectPrefab, _sourceBgmsObject.transform);
            _sourceBgms.Add(_currentBgmSource);

            _currentBgmSource.SetClip(FindBgmClip(clipId));

            // フェードイン
            _currentBgmSource.SetTarget(volumeBgmRatio, _defaultVolumeBgm, fadeBgmDuration);
            _currentBgmSource.ChangeState(EBgmSourceName.FadeIn);
        }

        /// <summary>
        /// BGMを止める
        /// </summary>
        public void StopBgm()
        {
            StopBgm(_defaultFadeBgmDuration);
        }

        /// <summary>
        /// BGMを止める
        /// </summary>
        /// <param name="fadeBgmDuration">消えるまでの時間</param>
        public void StopBgm(float fadeBgmDuration)
        {
            if (_currentBgmSource == null) return;

            // フェードアウト
            _currentBgmSource.SetTarget(0f, _defaultVolumeBgm, fadeBgmDuration);
            _currentBgmSource.ChangeState(EBgmSourceName.FadeOut);
            _currentBgmSource = null;
        }

        /// <summary>
        /// SEを再生
        /// </summary>
        /// <param name="clipId">クリップのID</param>
        /// <param name="isOverride">同じクリップが再生中なら上書き再生するか</param>
        public void PlaySe(string clipId, bool isOverride)
        {
            PlaySe(clipId, isOverride, 1f);
        }

        /// <summary>
        /// SEを再生
        /// </summary>
        /// <param name="clipId">クリップのID</param>
        /// <param name="isOverride">同じクリップが再生中なら上書き再生するか</param>
        /// <param name="volumeSeRatio">>SEのボリューム（デフォルトのボリュームに対する比率）</param>
        public void PlaySe(string clipId, bool isOverride, float volumeSeRatio)
        {
            bool isFull = true;

            // 一番長い時間再生されているSEのインデックス
            int maxTimeIndex = -1;

            for (int i = 0; i < _sourceSes.Length; ++i)
            {
                bool isAudioEmpty = _sourceSes[i].clip == null || !_sourceSes[i].isPlaying;

                bool canOverride = false;
                if (!isAudioEmpty)
                {
                    canOverride = isOverride && _sourceSes[i].clip == FindSeClip(clipId);
                }

                if (isAudioEmpty || canOverride)
                {
                    // 再生されていないまたは上書き可能なら、そのオーディオソースで鳴らす
                    isFull = false;
                    _sourceSes[i].Stop();
                    _sourceSes[i].clip = FindSeClip(clipId);
                    _sourceSes[i].volume = volumeSeRatio * _defaultVolumeSe;
                    _sourceSes[i].Play();
                    break;
                }
                else
                {
                    if (maxTimeIndex == -1 || _sourceSes[i].time > _sourceSes[maxTimeIndex].time)
                    {
                        maxTimeIndex = i;
                    }
                }
            }

            if (isFull)
            {
                _sourceSes[maxTimeIndex].Stop();
                _sourceSes[maxTimeIndex].clip = FindSeClip(clipId);
                _sourceSes[maxTimeIndex].volume = volumeSeRatio;
                _sourceSes[maxTimeIndex].Play();
            }
        }

        /// <summary>
        /// デフォルトマスターボリュームを強制設定
        /// </summary>
        /// <param name="volume"></param>
        public void SetDefaultMasterVolumeForced(float volume)
        {
            _defaultVolumeMaster = volume;
            AudioListener.volume = volume;

            PlayerPrefs.SetFloat(MasterVolumeKey, volume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// デフォルトBGMボリュームを強制設定
        /// </summary>
        /// <param name="volume"></param>
        public void SetDefaultBgmVolumeForced(float volume)
        {
            _defaultVolumeBgm = volume;
            foreach (BgmSourceObject source in _sourceBgms)
            {
                source.SetDefaultVolume(volume);
            }

            PlayerPrefs.SetFloat(BgmVolumeKey, volume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// デフォルトBGMボリュームを設定
        /// </summary>
        /// <param name="volume"></param>
        public void SetDefaultBgmVolume(float volume)
        {
            _defaultVolumeBgm = volume;
            PlayerPrefs.SetFloat(BgmVolumeKey, volume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// デフォルトSEボリュームを設定
        /// </summary>
        /// <param name="volume"></param>
        public void SetDefaultSeVolume(float volume)
        {
            _defaultVolumeSe = volume;
            PlayerPrefs.SetFloat(SeVolumeKey, volume);
            PlayerPrefs.Save();
        }
    }

}
