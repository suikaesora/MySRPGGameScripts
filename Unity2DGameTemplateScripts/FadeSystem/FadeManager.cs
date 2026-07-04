using Cysharp.Threading.Tasks;
using System;
using Unity2DGameTemplate.FadeSystem;
using Unity2DGameTemplate.StateMachines;
using UnityEngine;

namespace Unity2DGameTemplate.FadeSystem
{
    /// <summary>
    /// フェードイン・フェードアウトを管理する
    /// </summary>
    public class FadeManager : MonoBehaviour
    {
        public static FadeManager Instance;

        /// <summary>
        /// フェードの種類
        /// </summary>
        public enum EFadeElementId
        {
            Normal,
            Special,
        }

        // フェード要素のリスト
        
        [System.Serializable]
        public class FadeElementItem
        {
            public EFadeElementId ElementId;
            public FadeElement ElementPrefab;
        }

        [SerializeField]
        private FadeElementItem[] fadeElementItems;

        // フェード中かどうか
        public bool IsFading { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// フェードアウト・フェードインを実行する
        /// </summary>
        /// <param name="fadeElementId">フェード要素の種類</param>
        /// <param name="settingEvent">フェード要素の初期化イベント</param>
        /// <param name="intervalEvent">フェードアウト・フェードインの間のイベント</param>
        /// <returns></returns>
        public async UniTask ExecuteFade(EFadeElementId fadeElementId, Action<FadeElement> settingEvent, Func<UniTask> intervalEvent)
        {
            IsFading = true;
            var fadeElement = Instantiate(Array.Find(fadeElementItems, x => x.ElementId == fadeElementId).ElementPrefab, transform);
            fadeElement.Init();
            settingEvent?.Invoke(fadeElement);
            fadeElement.ChangeState(EFadeElementStateName.FadeOut);

            await UniTask.WaitUntil(() => fadeElement.CurrentStateName == EFadeElementStateName.Waiting);

            await intervalEvent();

            fadeElement.ChangeState(EFadeElementStateName.FadeIn);

            await UniTask.WaitUntil(() => fadeElement.IsFadeEnd);

            Destroy(fadeElement.gameObject);
            IsFading = false;
        }
    }
}
