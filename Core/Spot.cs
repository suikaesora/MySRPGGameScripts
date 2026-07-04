using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MySRPGGame.Core
{
    /// <summary>
    /// SRPGのグリッド内の1セル
    /// </summary>
    public class Spot : MonoBehaviour, IPathItem
    {
        [SerializeField]
        private TextMeshProUGUI ValueText;

        [SerializeField]
        private Image pathImage;

        [SerializeField]
        private Image actorSignImage;

        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private float defaultSpotImageAlpha = 0.5f;

        private Sequence imageAnimation;

        /// <summary>
        /// 通行可能かどうか
        /// </summary>
        public bool CanPass
        {
            get
            {
                if (SpotGround != null)
                {
                    return SpotGround.CanPass;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 地形に紐づいた重み
        /// </summary>
        public int Weight
        {
            get
            {
                if (SpotGround != null)
                {
                    return SpotGround.Weight;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 地形
        /// </summary>
        public Ground SpotGround { get; private set; }

        /// <summary>
        /// 地形に紐づいたワールドアクター
        /// </summary>
        public WorldActor SpotWorldActor { get; private set; }

        /// <summary>
        /// 地形をセット
        /// </summary>
        /// <param name="groundData">地形データ</param>
        public void SetGround(GroundData groundData)
        {
            SpotGround = Instantiate(groundData.GroundPrefab, transform);
            SpotGround.Init(groundData);
        }

        /// <summary>
        /// ワールドアクターをセット
        /// </summary>
        public void SetWorldActor(WorldActor worldActor)
        {
            SpotWorldActor = worldActor;
            if (worldActor != null)
            {
                actorSignImage.gameObject.SetActive(true);
                actorSignImage.color = worldActor.ActorRef.TroopDataRef.TroopColor;
            }
            else
            {
                actorSignImage.gameObject.SetActive(false);
            }
        }

        public void InitCamera(Camera mainCamera)
        {
            canvas.worldCamera = mainCamera;
        }

        public void SetValue(string value)
        {
            ValueText.text = value;
        }

        /// <summary>
        /// 板画像を表示
        /// </summary>
        /// <param name="color">画像の色</param>
        public void ActivateImage(Color color)
        {
            StopAnimateImage();
            pathImage.gameObject.SetActive(true);
            color.a = defaultSpotImageAlpha;
            pathImage.color = color;
        }

        /// <summary>
        /// 板画像を非表示
        /// </summary>
        public void DeactivateImage()
        {
            StopAnimateImage();
            pathImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// 画像を一定間隔でアニメーションさせる
        /// </summary>
        /// <param name="color">画像の色</param>
        public void AnimateImage(Color color)
        {
            imageAnimation?.Kill();
            color.a = defaultSpotImageAlpha;
            pathImage.color = color * 0.75f;
            imageAnimation = DOTween.Sequence()
                .Append(pathImage.DOColor(color * 1.25f, 0.5f).SetEase(Ease.InOutSine))
                .Append(pathImage.DOColor(color * 0.75f, 0.5f).SetEase(Ease.InOutSine))
                .SetLoops(-1, LoopType.Restart);
        }

        /// <summary>
        /// アニメーションを停止
        /// </summary>
        public void StopAnimateImage()
        {
            imageAnimation?.Kill();
        }

        private void OnDisable()
        {
            imageAnimation?.Kill();
        }
    }
}
