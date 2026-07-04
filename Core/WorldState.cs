using Cysharp.Threading.Tasks;
using DG.Tweening;
using MySRPGGame.Grid;
using System.Collections.Generic;
using System.Threading;
using Unity2DGameTemplate.AudioSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MySRPGGame.Core
{
    /// <summary>
    /// ワールドステートの基底クラス
    /// </summary>
    public class WorldState
    {
        public virtual string StateName { get; }

        protected WorldManager worldManager;

        public WorldState(WorldManager worldManager)
        {
            this.worldManager = worldManager;
            worldManager.RegisterWorldState(this);
        }

        /// <summary>
        /// ステート開始時に呼ばれる
        /// </summary>
        public virtual void OnEnter()
        {

        }

        /// <summary>
        /// ステート更新時に呼ばれる
        /// </summary>
        public virtual void Update()
        {

        }

        /// <summary>
        /// ステート終了時に呼ばれる
        /// </summary>
        public virtual void OnExit()
        {

        }
    }

    /// <summary>
    /// マップ全体を操作するステート
    /// </summary>
    public class WSMapWatching : WorldState
    {
        public override string StateName => "MapWatching";

        private Vector2Int cursorGridPosition;

        public WSMapWatching(WorldManager worldManager) : base(worldManager)
        {
        }

        /// <summary>
        /// 移動可能セルを取得する
        /// </summary>
        /// <param name="worldActor">選択されたワールドアクター</param>
        /// <param name="enemyActorPositions">自軍以外のワールドアクターの座標</param>
        private void ShowMovableCells(WorldActor worldActor, HashSet<Vector2Int> enemyActorPositions)
        {
            worldManager.MovableCells = worldManager.Grid.FindMovableCells(worldActor.GridPosition, worldActor.ActorRef.Mov, true, enemyActorPositions);
            foreach (Vector2Int p in worldManager.MovableCells)
            {
                Spot spot = worldManager.Grid.GetCell(p);
                spot.ActivateImage(worldActor.ActorRef.TroopDataRef.TroopColor);
            }
        }

        /// <summary>
        /// 攻撃可能セルを取得する
        /// </summary>
        /// <param name="worldActor">選択されたワールドアクター</param>
        /// <param name="enemyActorPositions">自軍以外のワールドアクターの座標</param>
        private void ShowAttackableCells(WorldActor worldActor, HashSet<Vector2Int> enemyActorPositions)
        {
            worldManager.AttackableCells = worldManager.Grid.GetAttackableCells(new HashSet<Vector2Int>(worldManager.MovableCells), worldActor.GetAttackRange(), null);
            foreach (Vector2Int p in worldManager.AttackableCells)
            {
                Spot spot = worldManager.Grid.GetCell(p);
                spot.ActivateImage(Color.gray);
            }
        }

        /// <summary>
        /// 画面上に選択されたワールドアクターについて表示をする
        /// </summary>
        /// <param name="worldActor">選択されたワールドアクター</param>
        private void ShowSignCells(WorldActor worldActor)
        {
            Vector2Int gridPos = worldActor.GridPosition;

            worldManager.BeginPos = gridPos;

            worldManager.Grid.GetCell(gridPos).ActivateImage(Color.yellow);

            HashSet<Vector2Int> enemyActorPositions = worldManager.GetEnemyWorldActorPositions(worldActor.ActorRef.TroopDataRef);

            ShowMovableCells(worldActor, enemyActorPositions);
            ShowAttackableCells(worldActor, enemyActorPositions);
        }

        private void OnDecide(InputAction.CallbackContext context)
        {
            if (worldManager.IsMenuActive) return;

            if (worldManager.SelectedActor != null &&
                !worldManager.SelectedActor.IsWaiting &&
                worldManager.SelectedActor.ActorRef.TroopDataRef == worldManager.GetCurrentTroopData())
            {
                // 表示をアニメーションさせる
                foreach (Vector2Int p in worldManager.MovableCells)
                {
                    Spot spot = worldManager.Grid.GetCell(p);
                    spot.AnimateImage(worldManager.SelectedActor.ActorRef.TroopDataRef.TroopColor);
                }
                foreach (Vector2Int p in worldManager.AttackableCells)
                {
                    Spot spot = worldManager.Grid.GetCell(p);
                    spot.AnimateImage(Color.gray);
                }

                AudioManager.Instance.PlaySe(AudioIDList.Decide, false);

                worldManager.ChangeWorldState("MovePreparation");
            }
            else if (worldManager.GetWorldActor(cursorGridPosition) == null)
            {
                AudioManager.Instance.PlaySe(AudioIDList.Decide, false);

                worldManager.ChangeWorldState("WorldMenu");
            }
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            if (!worldManager.IsMenuActive && worldManager.SelectedActor != null)
            {
                AudioManager.Instance.PlaySe(AudioIDList.Menu, false);

                worldManager.ChangeWorldState("StatusWindow");
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            worldManager.SelectedActor = null;
            cursorGridPosition = GridUtility.OutSide;

            worldManager.RegisterDecideEvent(OnDecide);
            worldManager.RegisterCancelEvent(OnCancel);
        }

        public override void Update()
        {
            base.Update();

            Vector2Int gridPos = worldManager.CursorManagerRef.GridPosition;

            if (gridPos != cursorGridPosition)
            {
                // カーソルが移動したとき必要なら移動可能範囲を計算
                worldManager.EraceMovableCells();
                worldManager.EraceAttackableCells();

                worldManager.SelectedActor = null;
                WorldActor wActor = worldManager.GetWorldActor(gridPos);
                if (wActor != null)
                {
                    if (!wActor.IsWaiting)
                    {
                        ShowSignCells(wActor);
                    }

                    worldManager.SelectedActor = wActor;
                }
                cursorGridPosition = gridPos;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            worldManager.UnregisterDecideEvent(OnDecide);
            worldManager.UnregisterCancelEvent(OnCancel);
        }
    }

    /// <summary>
    /// 移動地点の選択をするステート
    /// </summary>
    public class WSMovePreparation : WorldState
    {
        public override string StateName => "MovePreparation";

        public WSMovePreparation(WorldManager worldManager) : base(worldManager)
        {
        }

        /// <summary>
        /// 移動目的地を選択し、ActorMove状態に遷移する
        /// </summary>
        /// <param name="gridPos"></param>
        private void SelectDistination(Vector2Int gridPos)
        {
            worldManager.EraceMovableCells();
            worldManager.EraceAttackableCells();

            worldManager.DestinationPos = gridPos;

            worldManager.ChangeWorldState("ActorMove");
        }

        private void OnDecide(InputAction.CallbackContext context)
        {
            Vector2Int gridPos = worldManager.CursorManagerRef.GridPosition;
            if (!worldManager.MovableCells.Contains(gridPos)) return;

            WorldActor wActor = worldManager.GetWorldActor(gridPos);
            if (wActor != null && wActor != worldManager.SelectedActor) return;

            AudioManager.Instance.PlaySe(AudioIDList.Decide, false);

            SelectDistination(gridPos);
        }
        private void OnCancel(InputAction.CallbackContext context)
        {
            AudioManager.Instance.PlaySe(AudioIDList.Cancel, false);

            worldManager.CursorManagerRef.SetGridPosition(worldManager.BeginPos);

            Vector2 worldPos = worldManager.GwConverter.GetGridToWorldPosition(worldManager.BeginPos);
            worldManager.CameraManagerRef.SetTargetPositionAsync(new Vector3(worldPos.x, 0f, worldPos.y));

            worldManager.ChangeWorldState("MapWatching");
        }

        public override void OnEnter()
        {
            base.OnEnter();

            worldManager.RegisterDecideEvent(OnDecide);
            worldManager.RegisterCancelEvent(OnCancel);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnExit()
        {
            base.OnExit();

            worldManager.UnregisterDecideEvent(OnDecide);
            worldManager.UnregisterCancelEvent(OnCancel);
        }
    }

    /// <summary>
    /// アクターが移動するステート
    /// </summary>
    public class WSActorMove : WorldState
    {
        public override string StateName => "ActorMove";

        public WSActorMove(WorldManager worldManager) : base(worldManager)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (worldManager.DestinationPos == GridUtility.OutSide) return;

            HashSet<Vector2Int> enemyActorPositions = worldManager.GetEnemyWorldActorPositions(worldManager.SelectedActor.ActorRef.TroopDataRef);

            // 選択済みのワールドアクターを移動する
            _ = worldManager.SelectedActor.Move
            (
                worldManager.Grid.FindPath(worldManager.BeginPos, worldManager.DestinationPos, false, enemyActorPositions),
                worldManager.ActorMoveDurationPerCell,
                new CancellationTokenSource().Token
            );
        }

        public override void Update()
        {
            // 移動完了で遷移
            if (!worldManager.SelectedActor.IsMoving)
            {
                worldManager.ChangeWorldState("ActionSelection");
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }

    /// <summary>
    /// 行動選択をするステート
    /// </summary>
    public class WSActionSelection : WorldState
    {
        public override string StateName => "ActionSelection";

        public WSActionSelection(WorldManager worldManager) : base(worldManager)
        {
        }

        private bool CheckAroundEnemyWorldActors()
        {
            bool isFindEnemy = false;
            worldManager.CandidateWorldActors = new List<WorldActor>();
            foreach (Vector2Int dir in worldManager.SelectedActor.GetAttackRange())
            {
                WorldActor wActor = worldManager.GetWorldActor(worldManager.SelectedActor.GridPosition + dir);
                if (wActor == null) continue;

                if (wActor.ActorRef.TroopDataRef != worldManager.SelectedActor.ActorRef.TroopDataRef)
                {
                    isFindEnemy = true;
                    if (!worldManager.CandidateWorldActors.Contains(wActor))
                    {
                        worldManager.CandidateWorldActors.Add(wActor);
                    }
                }
            }

            return isFindEnemy;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            worldManager.CursorManagerRef.Deactivate();

            SelectionWindow sw = Object.Instantiate(worldManager.SelectionWindowPrefab, worldManager.CanvasTransform);
            List<string> selectionItemTitles = new List<string>();

            // 周りのアクターを検索し、敵なら「戦う」ボタンを表示
            bool isFindEnemy = CheckAroundEnemyWorldActors();

            if (isFindEnemy) selectionItemTitles.Add("戦う");
            selectionItemTitles.Add("待機");

            // 選択された位置にUIを表示するため、座標を計算
            Vector3 worldPos = worldManager.GwConverter.GetGridTo3DWorldPosition(worldManager.SelectedActor.GridPosition);
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            Vector2 canvasSize = worldManager.CanvasTransform.GetComponent<RectTransform>().sizeDelta;
            Vector2 uiPos = (screenPos * canvasSize.x / Screen.width) - canvasSize / 2f;

            sw.Init(selectionItemTitles.ToArray(), selectedIndex =>
            {
                if (selectionItemTitles[selectedIndex] == "戦う")
                {
                    AudioManager.Instance.PlaySe(AudioIDList.ActDecide, false);
                    worldManager.ChangeWorldState("ActorSelection");
                }
                else if (selectionItemTitles[selectedIndex] == "待機")
                {
                    AudioManager.Instance.PlaySe(AudioIDList.ActDecide, false);
                    worldManager.SelectedActor.SetWaiting(true);
                    
                    if (worldManager.IsAllWorldActorsInTroopWaiting(worldManager.GetCurrentTroopData()))
                    {
                        // 全員が待ち状態なら強制的に軍隊変更ステートに
                        worldManager.ChangeWorldState("TroopChange");
                    }
                    else
                    {
                        worldManager.ChangeWorldState("MapWatching");
                    }
                }
            },
            _ =>
            {
                // キャンセル時
                AudioManager.Instance.PlaySe(AudioIDList.Cancel, false);

                worldManager.SelectedActor.SetPosition(worldManager.BeginPos);
                worldManager.CursorManagerRef.SetGridPosition(worldManager.BeginPos);

                Vector2 worldPos = worldManager.GwConverter.GetGridToWorldPosition(worldManager.BeginPos);
                worldManager.CameraManagerRef.SetTargetPositionAsync(new Vector3(worldPos.x, 0f, worldPos.y));

                worldManager.ChangeWorldState("MapWatching");
            },
            uiPos);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnExit()
        {
            base.OnExit();

            worldManager.CursorManagerRef.Activate();
        }
    }

    /// <summary>
    /// ワールドアクターの選択をするステート
    /// </summary>
    public class WSActorSelection : WorldState
    {
        public override string StateName => "ActorSelection";

        private int selectedIndex;

        public WSActorSelection(WorldManager worldManager) : base(worldManager)
        {
        }

        /// <summary>
        /// アクターの選択をする
        /// </summary>
        /// <param name="index">選択番号</param>
        public void SelectWorldActor(int index)
        {
            worldManager.CursorManagerRef.SetGridPosition(worldManager.CandidateWorldActors[index].GridPosition);
        }

        public void MoveItemForward(InputAction.CallbackContext context)
        {
            // 選択を1進める。最後の次は最初
            selectedIndex = Mathf.RoundToInt(Mathf.Repeat(selectedIndex + 1, worldManager.CandidateWorldActors.Count));
            Debug.Log(worldManager.CandidateWorldActors.Count);
            SelectWorldActor(selectedIndex);
        }
        public void MoveItemBack(InputAction.CallbackContext context)
        {
            // 選択を1戻す。最初の前は最後
            selectedIndex = Mathf.RoundToInt(Mathf.Repeat(selectedIndex - 1, worldManager.CandidateWorldActors.Count));
            Debug.Log(worldManager.CandidateWorldActors.Count);
            SelectWorldActor(selectedIndex);
        }

        public void OnDecide(InputAction.CallbackContext context)
        {
            worldManager.TargetActor = worldManager.CandidateWorldActors[selectedIndex];
            worldManager.ChangeWorldState("Battle");
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            worldManager.SelectedActor.SetPosition(worldManager.BeginPos);
            worldManager.CursorManagerRef.SetGridPosition(worldManager.BeginPos);

            Vector2 worldPos = worldManager.GwConverter.GetGridTo3DWorldPosition(worldManager.BeginPos);
            worldManager.CameraManagerRef.SetTargetPositionAsync(worldPos);

            worldManager.ChangeWorldState("MapWatching");
        }

        public override void OnEnter()
        {
            base.OnEnter();

            worldManager.CursorManagerRef.Deactivate(false);

            worldManager.MoveItemForwardAction.performed += MoveItemForward;
            worldManager.MoveItemBackAction.performed += MoveItemBack;

            worldManager.DecideAction.performed += OnDecide;
            worldManager.CancelAction.performed += OnCancel;

            // 最初の選択
            selectedIndex = 0;

            SelectWorldActor(selectedIndex);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnExit()
        {
            base.OnExit();

            worldManager.MoveItemForwardAction.performed -= MoveItemForward;
            worldManager.MoveItemBackAction.performed -= MoveItemBack;

            worldManager.DecideAction.performed -= OnDecide;
            worldManager.CancelAction.performed -= OnCancel;

            worldManager.CursorManagerRef.Activate(false);
        }
    }

    /// <summary>
    /// 自動でバトル処理を行うステート
    /// </summary>
    public class WSBattle : WorldState
    {
        public override string StateName => "Battle";

        public WSBattle(WorldManager worldManager) : base(worldManager)
        {
        }

        /// <summary>
        /// バトル用の初期化
        /// </summary>
        private void BattleInitialize()
        {
            Vector3 worldPos = worldManager.GwConverter.GetGridTo3DWorldPosition(worldManager.SelectedActor.GridPosition);
            worldManager.CameraManagerRef.SetTargetPositionAsync(worldPos);
            worldManager.CursorManagerRef.Deactivate();

            // 向きをデフォルトに
            worldManager.SelectedActor.SetRotation(WorldActor.RotationTypeEnum.Default);
            worldManager.TargetActor.SetRotation(WorldActor.RotationTypeEnum.Default);

            worldManager.SelectedActor.SetHpBarActive(true);
            worldManager.TargetActor.SetHpBarActive(true);
        }

        /// <summary>
        /// 攻撃のシーケンスを実行
        /// </summary>
        /// <param name="origin">攻撃するワールドアクター</param>
        /// <param name="target">攻撃されるワールドアクター</param>
        /// <param name="ct"></param>
        private async UniTask Attack(WorldActor origin, WorldActor target, CancellationToken ct)
        {
            AudioManager.Instance.PlaySe(AudioIDList.AttackStart, false);

            // 武器を表示
            worldManager.ArrowObject.SetActive(true);

            if (origin.ActorRef.Weapons.Count > 0)
            {
                worldManager.ArrowObject.GetComponentInChildren<Image>().sprite = origin.ActorRef.Weapons[0].IconSprite;
            }
            else
            {
                worldManager.ArrowObject.GetComponentInChildren<Image>().sprite = worldManager.ArrowSprite;
            }

            worldManager.ArrowObject.transform.position = origin.transform.position;

            // アニメーション
            Sequence moveSequence = DOTween.Sequence()
                .Append(worldManager.ArrowObject.transform.DOMoveX(target.transform.position.x, 1f))
                .Join(worldManager.ArrowObject.transform.DOMoveZ(target.transform.position.z, 1f));

            Sequence jumpSequence = DOTween.Sequence()
                .Append(worldManager.ArrowObject.transform.DOMoveY(1.5f, 0.5f).SetEase(Ease.OutSine))
                .Append(worldManager.ArrowObject.transform.DOMoveY(0f, 0.5f).SetEase(Ease.InSine));

            Sequence mainSequence = DOTween.Sequence().Append(moveSequence).Join(jumpSequence);

            await mainSequence.Play().WithCancellation(ct);

            worldManager.ArrowObject.SetActive(false);

            // 数値的な攻撃処理
            origin.ActorRef.Attack(target.ActorRef);

            AudioManager.Instance.PlaySe(AudioIDList.Damage, false);
        }

        /// <summary>
        /// バトルの終了処理
        /// </summary>
        private void BattleFinish()
        {
            if (worldManager.SelectedActor.WorldActorState != WorldActor.WorldActorStateEnum.Dead)
            {
                worldManager.SelectedActor.SetHpBarActive(false);
            }
            if (worldManager.TargetActor.WorldActorState != WorldActor.WorldActorStateEnum.Dead)
            {
                worldManager.TargetActor.SetHpBarActive(false);
            }

            worldManager.CleanUpWorldActors();

            worldManager.CursorManagerRef.Activate();

            worldManager.SelectedActor.SetWaiting(true);
        }

        /// <summary>
        /// バトル全体の処理
        /// </summary>
        private async UniTask Battle(CancellationToken ct)
        {
            BattleInitialize();

            await UniTask.WaitForSeconds(worldManager.BattleWaitInterval);

            // 双方の攻撃

            // こちらからの攻撃
            await Attack(worldManager.SelectedActor, worldManager.TargetActor, ct);

            if (worldManager.TargetActor.ActorRef.Hp == 0)
            {
                await UniTask.WaitUntil(() => worldManager.TargetActor.WorldActorState == WorldActor.WorldActorStateEnum.Dead);
                
                AudioManager.Instance.PlaySe(AudioIDList.Death, false);
            }

            // 相手側が攻撃可なら、相手からの攻撃
            if (worldManager.TargetActor.WorldActorState != WorldActor.WorldActorStateEnum.Dead &&
                worldManager.TargetActor.CanAttack(worldManager.SelectedActor))
            {
                await UniTask.WaitForSeconds(worldManager.BattleWaitInterval);

                await Attack(worldManager.TargetActor, worldManager.SelectedActor, ct);

                if (worldManager.SelectedActor.ActorRef.Hp == 0)
                {
                    await UniTask.WaitUntil(() => worldManager.SelectedActor.WorldActorState == WorldActor.WorldActorStateEnum.Dead);

                    AudioManager.Instance.PlaySe(AudioIDList.Death, false);
                }
            }

            await UniTask.WaitForSeconds(worldManager.BattleResultInterval, cancellationToken: ct);

            BattleFinish();

            // ゲーム終了をチェック
            if (worldManager.CheckGameClear()) return;
            if (worldManager.CheckGameOver()) return;

            // 結果に応じてステート遷移
            if (worldManager.IsAllWorldActorsInTroopWaiting(worldManager.GetCurrentTroopData()))
            {
                worldManager.ChangeWorldState("TroopChange");
            }
            else
            {
                if (worldManager.SelectedActor.ActorRef.TroopDataRef.IsPlayer)
                {
                    worldManager.ChangeWorldState("MapWatching");
                }
                else
                {
                    bool isFind = worldManager.SelectNextEnemy();

                    if (isFind)
                    {
                        worldManager.ChangeWorldState("EnemyMove");
                    }
                    else
                    {
                        worldManager.ChangeWorldState("TroopChange");
                    }
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (worldManager.TargetActor == null) Debug.LogError("攻撃ターゲットが選択されていません。");

            _ = Battle(worldManager.GetCancellationTokenOnDestroy());
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }

    public class WSTroopChange : WorldState
    {
        public override string StateName => "TroopChange";

        public WSTroopChange(WorldManager worldManager) : base(worldManager)
        {
        }

        /// <summary>
        /// 軍隊をわかりやすく表示
        /// </summary>
        /// <param name="troopData">軍隊データ</param>
        /// <returns></returns>
        private async UniTask ShowTroopData(TroopData troopData, CancellationToken ct)
        {
            if (troopData == null) return;

            AudioManager.Instance.PlaySe(AudioIDList.TroopChange, false, 2f);

            worldManager.TroopStartViewRef.gameObject.SetActive(true);
            worldManager.TroopStartViewRef.Init(troopData.TroopColor, troopData.TroopName);

            // 軍隊の最初のキャラクターにカメラを寄せる
            Vector2Int targetGridPos = worldManager.GetWorldActorsInTroop(troopData)[0].GridPosition;
            Vector2 targetWorldPos = worldManager.GwConverter.GetGridToWorldPosition(targetGridPos);

            worldManager.CameraManagerRef.SetTargetPositionAsync(new Vector3(targetWorldPos.x, 0f, targetWorldPos.y));
            worldManager.CursorManagerRef.SetGridPosition(targetGridPos);

            await UniTask.WaitForSeconds(worldManager.TroopStartViewShowingDuration, cancellationToken: ct);
            worldManager.TroopStartViewRef.gameObject.SetActive(false);

            if (!troopData.IsPlayer)
            {
                foreach (Image turnImage in worldManager.TroopTurnImages)
                {
                    Color turnImageColor = troopData.TroopColor;
                    turnImageColor.a = 0.5f;
                    turnImage.color = turnImageColor;
                }
            }

            if (troopData.IsPlayer)
            {
                worldManager.ChangeWorldState("MapWatching");
            }
            else
            {
                // 敵の選択順序を初期化
                worldManager.Enemies = worldManager.GetWorldActorsInTroop(troopData);
                worldManager.CurrentEnemyIndex = 0;
                worldManager.SelectedActor = worldManager.Enemies[worldManager.CurrentEnemyIndex];
                worldManager.ChangeWorldState("EnemyMove");
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            worldManager.CleanUpWorldActors();

            worldManager.CursorManagerRef.Deactivate();

            // 強制的に待機状態にさせる
            if (worldManager.CurrentTroopIndex >= 0)
            {
                foreach (WorldActor wActor in worldManager.GetWorldActorsInTroop(worldManager.GetCurrentTroopData()))
                {
                    if (!wActor.IsWaiting) wActor.SetWaiting(true);
                }
                foreach (WorldActor wActor in worldManager.GetWorldActorsInTroop(worldManager.GetCurrentTroopData()))
                {
                    if (wActor.IsWaiting) wActor.SetWaiting(false);
                }
            }

            foreach (Image turnImage in worldManager.TroopTurnImages)
            {
                turnImage.color = new Color(0f, 0f, 0f, 0f);
            }

            // 次の軍隊にする
            worldManager.CurrentTroopIndex = (worldManager.CurrentTroopIndex + 1) % worldManager.TroopDataArray.Length;
            TroopData troopData = worldManager.TroopDataArray[worldManager.CurrentTroopIndex];

            if (worldManager.GetWorldActorsInTroop(troopData).Count == 0)
            {
                worldManager.ChangeWorldState("TroopChange");
                return;
            }

            _ = ShowTroopData(troopData, worldManager.CancellationToken);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnExit()
        {
            base.OnExit();

            worldManager.CursorManagerRef.Activate();
        }
    }

    public class WSWorldMenu : WorldState
    {
        public override string StateName => "WorldMenu";

        public WSWorldMenu(WorldManager worldManager) : base(worldManager)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            SelectionWindow sw = Object.Instantiate(worldManager.SelectionWindowPrefab, worldManager.CanvasTransform);
            string[] worldMenuTitles = new string[]
            {
                "ターン終了",
            };

            Vector3 worldPos = worldManager.GwConverter.GetGridToWorldPosition(worldManager.CursorManagerRef.GridPosition);
            Vector2 screenPos = Camera.main.WorldToScreenPoint(new Vector3(worldPos.x, 0f, worldPos.y));
            Vector2 canvasSize = worldManager.CanvasTransform.GetComponent<RectTransform>().sizeDelta;
            Vector2 uiPos = (screenPos * canvasSize.x / Screen.width) - canvasSize / 2f;

            sw.Init(worldMenuTitles, selectedIndex =>
            {
                if (worldMenuTitles[selectedIndex] == worldMenuTitles[0])
                {
                    worldManager.ChangeWorldState("TroopChange");
                }
            },
            _ => worldManager.ChangeWorldState("MapWatching"),
            uiPos);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }

    public class WSEnemyMove : WorldState
    {
        public override string StateName => "EnemyMove";

        private List<Vector2Int> path;

        private bool isAttackable;

        private bool isBeforeMovedWait;
        private float beforeMovedTimer;

        private bool isAfterMovedWait;
        private float afterMovedWaitTimer;

        public WSEnemyMove(WorldManager worldManager) : base(worldManager)
        {
        }

        private List<Vector2Int> ConsiderPath()
        {
            List<Vector2Int> path = new List<Vector2Int>();

            WorldActor selectedWActor = worldManager.SelectedActor;

            // 敵の座標を取得
            HashSet<Vector2Int> enemyActorPositions = new HashSet<Vector2Int>();
            foreach (WorldActor actor in worldManager.WorldActors)
            {
                if (actor.ActorRef.TroopDataRef != selectedWActor.ActorRef.TroopDataRef)
                {
                    enemyActorPositions.Add(actor.GridPosition);
                }
            }

            // 行動可能範囲を計算
            List<Vector2Int> movableCells = worldManager.Grid.FindMovableCells(selectedWActor.GridPosition, selectedWActor.ActorRef.Mov, true, enemyActorPositions);

            Vector2Int destination = -Vector2Int.one;
            WorldActor targetWActor;

            // 行動可能範囲の中で攻撃可能な地点（のうち最短）に移動する
            int minDestinationDistance = int.MaxValue;

            foreach (Vector2Int pos in movableCells)
            {
                if (worldManager.GetWorldActor(pos) != null && pos != selectedWActor.GridPosition) continue;

                foreach (Vector2Int dir in selectedWActor.GetAttackRange())
                {
                    Vector2Int targetPos = pos + dir;
                    targetWActor = worldManager.GetWorldActor(targetPos);
                    if (targetWActor != null && targetWActor.ActorRef.TroopDataRef != selectedWActor.ActorRef.TroopDataRef)
                    {
                        int destinationDistance = worldManager.Grid.FindPath(selectedWActor.GridPosition, pos, true, enemyActorPositions).Count;
                        if (destinationDistance < minDestinationDistance)
                        {
                            destination = pos;
                            minDestinationDistance = destinationDistance;

                            isAttackable = true;
                            worldManager.TargetActor = targetWActor;

                            break;
                        }
                    }
                }
            }

            if (destination == -Vector2Int.one)
            {
                // 目的地が見当たらない場合

                // 一番近い敵を探す
                targetWActor = null;
                int minDistance = int.MaxValue;

                List<WorldActor> enemyWActors = new List<WorldActor>();
                foreach (TroopData troop in worldManager.TroopDataArray)
                {
                    if (troop == selectedWActor.ActorRef.TroopDataRef) continue;
                    enemyWActors.AddRange(worldManager.GetWorldActorsInTroop(troop));
                }

                foreach (WorldActor wActor in enemyWActors)
                {
                    int distance = Mathf.Abs(wActor.GridPosition.x - selectedWActor.GridPosition.x) + Mathf.Abs(wActor.GridPosition.y - selectedWActor.GridPosition.y);
                    if (distance < minDistance)
                    {
                        targetWActor = wActor;
                        minDistance = distance;
                    }
                }

                if (targetWActor == null) return path;

                // 行動可能範囲の中でターゲットの敵に最も近い地点に移動する

                minDistance = int.MaxValue;
                foreach (Vector2Int pos in movableCells)
                {
                    if (worldManager.GetWorldActor(pos) != null && pos != selectedWActor.GridPosition) continue;

                    int distance = Mathf.Abs(pos.x - targetWActor.GridPosition.x) + Mathf.Abs(pos.y - targetWActor.GridPosition.y);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        destination = pos;
                    }
                }
            }

            // 移動経路計算
            path = worldManager.Grid.FindPath(selectedWActor.GridPosition, destination, true, enemyActorPositions);

            return path;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            isAttackable = false;

            worldManager.CursorManagerRef.SetGridPosition(worldManager.SelectedActor.GridPosition);

            Vector2 targetWorldPos = worldManager.GwConverter.GetGridToWorldPosition(worldManager.SelectedActor.GridPosition);
            worldManager.CameraManagerRef.SetTargetPositionAsync(new Vector3(targetWorldPos.x, 0f, targetWorldPos.y));

            isAfterMovedWait = false;

            isBeforeMovedWait = true;
            beforeMovedTimer = worldManager.BeforeMovedWaitDuration;
        }

        public override void Update()
        {
            base.Update();

            if (isBeforeMovedWait)
            {
                beforeMovedTimer -= Time.deltaTime;
                if (beforeMovedTimer < 0f)
                {
                    isBeforeMovedWait = false;

                    _ = worldManager.SelectedActor.Move
                    (
                        ConsiderPath(),
                        worldManager.ActorMoveDurationPerCell,
                        new CancellationTokenSource().Token
                    );
                }
            }
            else if (isAfterMovedWait)
            {
                afterMovedWaitTimer -= Time.deltaTime;
                if (afterMovedWaitTimer < 0f)
                {
                    isAfterMovedWait = false;

                    if (isAttackable)
                    {
                        worldManager.ChangeWorldState("Battle");
                    }
                    else
                    {
                        worldManager.SelectedActor.SetWaiting(true);
                        if (worldManager.CurrentEnemyIndex + 1 >= worldManager.Enemies.Count)
                        {
                            worldManager.ChangeWorldState("TroopChange");
                        }
                        else
                        {
                            bool isFind = worldManager.SelectNextEnemy();

                            if (isFind)
                            {
                                worldManager.ChangeWorldState("EnemyMove");
                            }
                            else
                            {
                                worldManager.ChangeWorldState("TroopChange");
                            }
                        }
                    }
                }
            }
            else if (!worldManager.SelectedActor.IsMoving)
            {
                isAfterMovedWait = true;
                afterMovedWaitTimer = worldManager.AfterMovedWaitDuration;

                Vector2 targetWorldPos = worldManager.GwConverter.GetGridToWorldPosition(worldManager.SelectedActor.GridPosition);
                worldManager.CameraManagerRef.SetTargetPositionAsync(new Vector3(targetWorldPos.x, 0f, targetWorldPos.y));
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }

    public class WSStatusWindow : WorldState
    {
        public override string StateName => "StatusWindow";

        public WSStatusWindow(WorldManager worldManager) : base(worldManager)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            worldManager.CursorManagerRef.Deactivate();

            Actor actor = worldManager.SelectedActor.ActorRef;

            // ステータスウィンドウをセット

            worldManager.StatusWindowRef.gameObject.SetActive(true);

            worldManager.StatusWindowRef.SetTitle(actor.ActorName);

            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.Hp).Set(actor.TroopDataRef.TroopColor, Color.white, $"{actor.Hp}/{actor.MaxHp}");
            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.MilitaryType).Set(actor.TroopDataRef.TroopColor, Color.white, actor.MilitaryTypeDataRef.TypeName);
            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.Troop).Set(actor.TroopDataRef.TroopColor, Color.white, actor.TroopDataRef.TroopName);
            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.Weapon).Set(actor.TroopDataRef.TroopColor, Color.white, (actor.Weapons != null && actor.Weapons.Count > 0) ? actor.Weapons[0].WeaponName : "武器未装備");

            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.Str).Set(actor.TroopDataRef.TroopColor, Color.white, actor.Str.ToString());
            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.Mag).Set(actor.TroopDataRef.TroopColor, Color.white, actor.Mag.ToString());
            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.SDef).Set(actor.TroopDataRef.TroopColor, Color.white, actor.SDef.ToString());
            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.SMDef).Set(actor.TroopDataRef.TroopColor, Color.white, actor.SMDef.ToString());
            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.SMov).Set(actor.TroopDataRef.TroopColor, Color.white, actor.SMov.ToString());

            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.Atk).Set(actor.TroopDataRef.TroopColor, Color.white, actor.Atk.ToString());
            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.MAtk).Set(actor.TroopDataRef.TroopColor, Color.white, actor.MAtk.ToString());
            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.Def).Set(actor.TroopDataRef.TroopColor, Color.white, actor.Def.ToString());
            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.MDef).Set(actor.TroopDataRef.TroopColor, Color.white, actor.MDef.ToString());
            worldManager.StatusWindowRef.GetStatusItem(UI.StatusWindow.StatusItemId.Mov).Set(actor.TroopDataRef.TroopColor, Color.white, actor.Mov.ToString());
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            AudioManager.Instance.PlaySe(AudioIDList.Cancel, false);

            worldManager.ChangeWorldState("MapWatching");
        }

        public override void Update()
        {
            base.Update();

            worldManager.RegisterCancelEvent(OnCancel);
        }

        public override void OnExit()
        {
            base.OnExit();

            worldManager.CursorManagerRef.Activate();

            worldManager.StatusWindowRef.gameObject.SetActive(false);

            worldManager.UnregisterCancelEvent(OnCancel);
        }
    }

    public class WSGameOver : WorldState
    {
        public override string StateName => "GameOver";

        public WSGameOver(WorldManager worldManager) : base(worldManager)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            AudioManager.Instance.StopBgm(0.2f);
            AudioManager.Instance.PlaySe(AudioIDList.Gameover, false);

            worldManager.GameOverWindow.SetActive(true);
            worldManager.CursorManagerRef.Deactivate();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }

    public class WSGameClear : WorldState
    {
        public override string StateName => "GameClear";

        public WSGameClear(WorldManager worldManager) : base(worldManager)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            AudioManager.Instance.PlayBgm(AudioIDList.Gameclear, 0.2f, 1f);

            worldManager.GameClearWindow.SetActive(true);
            worldManager.CursorManagerRef.Deactivate();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}