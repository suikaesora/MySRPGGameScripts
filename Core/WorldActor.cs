using Cysharp.Threading.Tasks;
using DG.Tweening;
using MySRPGGame.Grid;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity2DGameTemplate.AudioSystem;
using UnityEngine;
using UnityEngine.UI;

namespace MySRPGGame.Core
{
    /// <summary>
    /// シーン上に置かれるキャラの実体。アクターを所持する。
    /// </summary>
    public class WorldActor : MonoBehaviour
    {
        [SerializeField]
        private GameObject hpBar;

        [SerializeField]
        private Image hpValueImage;

        [SerializeField]
        private Image troopSignImage;

        [SerializeField]
        private Material normalActorMaterial;

        // 待機中のワールドアクターのマテリアル
        [SerializeField]
        private Material waitingActorMaterial;

        [SerializeField]
        private float troopSignWaitignValue = 0.75f;

        [SerializeField]
        private float hpValueAnimationDuration = 0.25f;

        [SerializeField]
        private float defaultTroopSignAlpha = 0.75f;

        public Vector2Int GridPosition { get; private set; }

        private GridWorldConverter<Spot> gwConverter;

        public bool IsMoving { get; private set; } = false;

        public Actor ActorRef { get; private set; }

        public bool IsWaiting { get; private set; }

        // キャラの見た目のオブジェクト
        public GameObject VisualObject { get; private set; }

        public enum RotationTypeEnum
        {
            Default,
            Up,
            Down,
            Right,
            Left,
        }

        public RotationTypeEnum RotationType { get; private set; }

        public enum WorldActorStateEnum
        {
            Normal,
            Dead,
        }

        public WorldActorStateEnum WorldActorState { get; private set; }

        private Tween hpValueAnim;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Init(GridWorldConverter<Spot> gwConverter, Actor actor)
        {
            this.gwConverter = gwConverter;
            ActorRef = actor;
            SetWaiting(false);

            troopSignImage.gameObject.SetActive(true);
            Color troopSignImageColor = ActorRef.TroopDataRef.TroopColor;
            troopSignImageColor.a = defaultTroopSignAlpha;
            troopSignImage.color = troopSignImageColor;

            VisualObject = Instantiate(ActorRef.MilitaryTypeDataRef.DefaultActorObject, transform);
            SetRotation(RotationTypeEnum.Default);

            SetMaterial(true);

            actor.SetHpEvent += hp =>
            {
                hpValueAnim?.Kill();

                hpValueAnim = hpValueImage.DOFillAmount((float)hp / actor.MaxHp, hpValueAnimationDuration);

                if (hp == 0) _ = Die();
            };

            WorldActorState = WorldActorStateEnum.Normal;
        }

        public void SetPosition(Vector2Int position)
        {
            //gwConverter.GridRef.GetCell(GridPosition).SetWorldActor(null);

            GridPosition = position;
            Vector3 worldPos = gwConverter.GetGridToWorldPosition(GridPosition);
            worldPos.z = worldPos.y;
            worldPos.y = 0f;
            transform.position = worldPos;

            //gwConverter.GridRef.GetCell(position).SetWorldActor(this);
        }

        /// <summary>
        /// 回転の列挙型を元に回転する
        /// </summary>
        public void SetRotation(RotationTypeEnum rotationType)
        {
            float rotY;
            switch (rotationType)
            {
                case RotationTypeEnum.Up:
                    rotY = 0f;
                    break;
                case RotationTypeEnum.Down:
                    rotY = 180f;
                    break;
                case RotationTypeEnum.Right:
                    rotY = 90f;
                    break;
                case RotationTypeEnum.Left:
                    rotY = 270f;
                    break;
                default:
                    rotY = 180f;
                    break;
            }

            Vector3 eulerAngles = VisualObject.transform.localEulerAngles;
            eulerAngles.y = rotY;
            VisualObject.transform.localEulerAngles = eulerAngles;
        }

        /// <summary>
        /// 回転方向ベクトルを回転の列挙型に変換する
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public RotationTypeEnum ConvertDirectionToRotationType(Vector2Int direction)
        {
            if (direction == Vector2Int.up) { return RotationTypeEnum.Up; }
            else if (direction == Vector2Int.down) { return RotationTypeEnum.Down; }
            else if (direction == Vector2Int.right) { return RotationTypeEnum.Right; }
            else if (direction == Vector2Int.left) { return RotationTypeEnum.Left; }
            return RotationTypeEnum.Default;
        }

        /// <summary>
        /// 死亡処理。自らをDestroyはしない
        /// </summary>
        public async UniTask Die()
        {
            await UniTask.WaitUntil(() => hpValueAnim == null || !hpValueAnim.IsActive());
            VisualObject.SetActive(false);
            troopSignImage.gameObject.SetActive(false);
            SetHpBarActive(false);
            gwConverter.GridRef.GetCell(GridPosition).SetWorldActor(null);

            WorldActorState = WorldActorStateEnum.Dead;
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="path">移動経路</param>
        /// <param name="moveDurationPerCell">移動時間（1セルごと）</param>
        /// <returns></returns>
        public async UniTask Move(List<Vector2Int> path, float moveDurationPerCell, CancellationToken ct)
        {
            ct = CancellationTokenSource.CreateLinkedTokenSource(ct, this.GetCancellationTokenOnDestroy()).Token;

            if (path.Count == 0) return;

            Vector2Int previousPos;

            IsMoving = true;
            foreach (Vector2Int pos in path)
            {
                AudioManager.Instance.PlaySe(AudioIDList.Walk, true);
                previousPos = GridPosition;
                SetPosition(pos);
                SetRotation(ConvertDirectionToRotationType(pos - previousPos));
                await UniTask.WaitForSeconds(moveDurationPerCell, cancellationToken: ct);
                ct.ThrowIfCancellationRequested();
            }
            IsMoving = false;
        }

        /// <summary>
        /// メイン武器の攻撃範囲を取得（今のところ武器を複数装備するアクターはいないのであまり意味はないが）
        /// </summary>
        public List<Vector2Int> GetAttackRange()
        {
            List<WeaponData> weapons = ActorRef.Weapons;
            if (weapons.Count == 0) return new List<Vector2Int>();

            return weapons[0].GetAttackRange();
        }

        /// <summary>
        /// 待機かどうかの設定
        /// </summary>
        public void SetWaiting(bool isWaiting)
        {
            IsWaiting = isWaiting;

            SetMaterial(!isWaiting);
            Color troopSignColor = ActorRef.TroopDataRef.TroopColor;
            troopSignColor.a = defaultTroopSignAlpha;
            if (isWaiting)
            {
                troopSignColor = troopSignImage.color;
                troopSignColor *= troopSignWaitignValue;
                troopSignColor.a = 1f;
            }
            troopSignImage.color = troopSignColor;

            if (IsWaiting)
            {
                SetRotation(RotationTypeEnum.Default);
            }
        }

        public void SetHpBarActive(bool isActive)
        {
            hpBar.SetActive(isActive);
            if (!isActive) hpValueAnim?.Kill();
        }

        /// <summary>
        /// ターゲットを攻撃可能か
        /// </summary>
        /// <param name="targetWorldActor">ターゲット</param>
        /// <returns></returns>
        public bool CanAttack(WorldActor targetWorldActor)
        {
            foreach (Vector2Int dir in GetAttackRange())
            {
                Vector2Int targetPos = GridPosition + dir;
                if (!gwConverter.GridRef.IsInRange(targetPos)) continue;

                if (targetWorldActor.GridPosition == targetPos)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetMaterial(bool isNormal)
        {
            if (VisualObject == null) return;

            Renderer[] renderers = VisualObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material = isNormal ? normalActorMaterial : waitingActorMaterial;
            }
        }
    }
}
