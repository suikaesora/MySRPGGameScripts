using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MySRPGGame.Grid;
using MySRPGGame.Pathfinding;
using MySRPGGame.UI;
using MySRPGGame.UI.Main;
using Unity2DGameTemplate.AudioSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MySRPGGame.Core
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField]
        private Spot spotPrefab;

        [SerializeField]
        private GroundData[] groundDatas;

        [SerializeField]
        private StageData stageData;

        [SerializeField]
        private Camera mainCamera;

        [SerializeField]
        private WorldActor worldActorPrefab;

        // 決定とキャンセルのインプットアクション
        [SerializeField]
        private InputAction decideAction;
        [SerializeField]
        private InputAction cancelAction;

        // 選択肢を移動するインプットアクション
        [SerializeField]
        private InputAction moveItemForwardAction;
        [SerializeField]
        private InputAction moveItemBackAction;

        [SerializeField]
        private ActorInitData[] actorInitDataArray;

        [SerializeField]
        private TroopData[] troopDataArray;

        [SerializeField]
        private CursorManager cursorManager;

        [SerializeField]
        private SelectionWindow selectionWindowPrefab;

        [SerializeField]
        private Transform canvasTransform;

        [SerializeField]
        private TroopStartView troopStartView;

        [SerializeField]
        private float troopStartViewShowingDuration;

        [SerializeField]
        private StatusWindow statusWindow;

        [SerializeField]
        private GameObject gameClearWindow;

        [SerializeField]
        private GameObject gameOverWindow;

        [SerializeField]
        private float actorMoveDurationPerCell;

        // 攻撃アイコン
        [SerializeField]
        private GameObject arrowObject;

        [SerializeField]
        private Sprite arrowSprite;

        [SerializeField]
        private float beforeMovedWaitDuration = 0.5f;
        [SerializeField]
        private float afterMovedWaitDuration = 0.5f;

        // 軍の表示のイメージ（複数）
        [SerializeField]
        private Image[] troopTurnImages;

        [SerializeField]
        private MenuWindow menuWindow;

        [SerializeField]
        private float battleWaitInterval;

        [SerializeField]
        private float battleResultInterval;

        public CursorManager CursorManagerRef => cursorManager;

        public InputAction DecideAction => decideAction;
        public InputAction CancelAction => cancelAction;

        public InputAction MoveItemForwardAction => moveItemForwardAction;
        public InputAction MoveItemBackAction => moveItemBackAction;

        public GridWorldConverter<Spot> GwConverter { get; private set; }
        public PathGrid<Spot> Grid { get; private set; }

        // ワールドアクター管理
        public List<WorldActor> WorldActors { get; set; } = new List<WorldActor>();

        // 選択済みのアクター
        public WorldActor SelectedActor { get; set; }
        
        // 状態関連
        public WorldState CurrentState { get; private set; }

        public Dictionary<string, WorldState> worldStates = new Dictionary<string, WorldState>();

        // 移動開始予定座標
        public Vector2Int BeginPos { get; set; } = GridUtility.OutSide;

        // 移動目標座標
        public Vector2Int DestinationPos { get; set; } = GridUtility.OutSide;

        // 移動可能セル
        public List<Vector2Int> MovableCells { get; set; }

        // 攻撃可能セル
        public List<Vector2Int> AttackableCells { get; set; }

        public SelectionWindow SelectionWindowPrefab => selectionWindowPrefab;

        public Transform CanvasTransform => canvasTransform;

        // 戦闘時などアクター選択がある時の候補リスト
        public List<WorldActor> CandidateWorldActors { get; set; }

        // 行動の対象のアクター
        public WorldActor TargetActor { get; set; }

        public TroopData[] TroopDataArray => troopDataArray;
        public int CurrentTroopIndex { get; set; }

        public TroopStartView TroopStartViewRef => troopStartView;

        public float TroopStartViewShowingDuration => troopStartViewShowingDuration;

        public CameraManager CameraManagerRef { get; private set; }

        private HashSet<Action<InputAction.CallbackContext>> decideEvents = new HashSet<Action<InputAction.CallbackContext>>();
        private HashSet<Action<InputAction.CallbackContext>> cancelEvents = new HashSet<Action<InputAction.CallbackContext>>();

        // 敵と、敵が順番に行動するための番号
        public List<WorldActor> Enemies { get; set; }
        public int CurrentEnemyIndex { get; set; }

        public GameObject GameOverWindow => gameOverWindow;
        public GameObject GameClearWindow => gameClearWindow;

        public StatusWindow StatusWindowRef => statusWindow;

        public float ActorMoveDurationPerCell => actorMoveDurationPerCell;

        public GameObject ArrowObject => arrowObject;

        public Sprite ArrowSprite => arrowSprite;

        public float BeforeMovedWaitDuration => beforeMovedWaitDuration;
        public float AfterMovedWaitDuration => afterMovedWaitDuration;

        public Image[] TroopTurnImages => troopTurnImages;

        // バトル中の待ち
        public float BattleWaitInterval => battleWaitInterval;

        public float BattleResultInterval => battleResultInterval;

        // UniTaskの自動キャンセル用のキャンセレーショントークン
        public CancellationToken CancellationToken => this.GetCancellationTokenOnDestroy();

        // メニュー関連の設定
        public bool IsMenuActive { get; private set; } = false;
        private bool isPreviousCursorManagerActive;
        private bool isPreviousCameraManagerActive;
        private const float menuCloseWaitDuration = 0.05f;

        private string currentStateName;

        // ステージの配置
        private string[][] stageGroundIDs;
        private string[][] stageActorIDs;

        // イベントの登録/解除
        public void RegisterDecideEvent(Action<InputAction.CallbackContext> decideEvent)
        {
            decideEvents.Add(decideEvent);
            decideAction.performed += decideEvent;
        }

        public void RegisterCancelEvent(Action<InputAction.CallbackContext> cancelEvent)
        {
            cancelEvents.Add(cancelEvent);
            cancelAction.performed += cancelEvent;
        }

        public void UnregisterDecideEvent(Action<InputAction.CallbackContext> decideEvent)
        {
            if (!decideEvents.Contains(decideEvent))
            {
                Debug.LogError("対象の決定イベントが登録されておらず削除できません。");
                return;
            }
            decideEvents.Remove(decideEvent);
            decideAction.performed -= decideEvent;
        }

        public void UnregisterCancelEvent(Action<InputAction.CallbackContext> cancelEvent)
        {
            if (!cancelEvents.Contains(cancelEvent))
            {
                Debug.LogError("対象のキャンセルイベントが登録されておらず削除できません。");
                return;
            }
            cancelEvents.Remove(cancelEvent);
            cancelAction.performed -= cancelEvent;
        }

        public void RegisterWorldState(WorldState state)
        {
            worldStates.Add(state.StateName, state);
        }

        public void ChangeWorldState(string stateName)
        {
            if (CurrentState != null) CurrentState.OnExit();
            if (!worldStates.ContainsKey(stateName))
            {
                Debug.LogError($"WorldState {stateName} の状態はありません。");
            }
            currentStateName = stateName;
            CurrentState = worldStates[stateName];
            CurrentState.OnEnter();
        }

        /// <summary>
        /// ステージに沿ったCSVファイルを読み込む
        /// </summary>
        /// <param name="textAsset">CSVファイルのテキストアセット</param>
        /// <param name="actualWidth">ステージの実際の幅</param>
        /// <param name="actualHeight">ステージの実際の高さ</param>
        /// <param name="result">文字列の2次元配列データ</param>
        /// <returns></returns>
        private bool ImportStageCSVFile(TextAsset textAsset, int actualWidth, int actualHeight, out string[][] result)
        {
            result = null;

            List<string> lines = new List<string>();

            try
            {
                using (StringReader sr = new StringReader(textAsset.text))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }
            }
            catch (Exception)
            {
                Debug.LogError("ファイルの読み込みに失敗しました。");
                return false;
            }

            int width = -1;
            int height = lines.Count;

            if (height != actualHeight)
            {
                Debug.Log("データの行数がステージの幅と異なります。");
                return false;
            }

            result = new string[height][];
            for (int i = 0; i < height; ++i)
            {
                int reversedIndex = height - 1 - i; // 行の上下を逆転しておく
                result[reversedIndex] = lines[i].Split(',');
                if (width != -1 && result[reversedIndex].Length != width)
                {
                    Debug.LogError($"データの各行の列数が一致しません。行番号={i}");
                    return false;
                }
                width = result[reversedIndex].Length;
            }

            if (width != actualWidth)
            {
                Debug.Log("データの列数がステージの高さと異なります。");
                return false;
            }

            return true;
        }

        private void ImportStage(StageData stageData)
        {
            // ファイル読み込み
            if (!ImportStageCSVFile(stageData.GroundTextAsset, stageData.Width, stageData.Height, out stageGroundIDs))
            {
                return;
            }
            if (!ImportStageCSVFile(stageData.ActorTextAsset, stageData.Width, stageData.Height, out stageActorIDs))
            {
                return;
            }
        }

        private void Start()
        {
            // DOTweenの初期化
            DOTween.SetTweensCapacity(1000, 1000);

            DOTween.Init();

            AudioManager.Instance.PlayBgm(AudioIDList.Main);


            ImportStage(stageData);


            CameraManagerRef = mainCamera.GetComponent<CameraManager>();

            Grid = new PathGrid<Spot>(stageData.Width, stageData.Height);
            GwConverter = new GridWorldConverter<Spot>(Grid, 1f);


            Grid.ExecuteAllCells(pos =>
            {
                Spot spot = Instantiate(spotPrefab, transform);
                Vector2 worldPos = GwConverter.GetGridToWorldPosition(pos);
                spot.transform.position = new Vector3(worldPos.x, 0, worldPos.y);
                spot.DeactivateImage();
                spot.InitCamera(mainCamera);
                spot.SetWorldActor(null);

                string groundID = stageGroundIDs[pos.y][pos.x];
                GroundData gData = Array.Find(groundDatas, d => d.GroundId == groundID);
                if (gData != null)
                {
                    spot.SetGround(gData);
                }
                else
                {
                    Debug.LogError($"ワールドデータに地形情報が正しく記載されていません。場所={stageData.Height - 1 - pos.y}行 {pos.x}列");
                    return null;
                }

                return spot;
            });

            Grid.ExecuteAllCells(pos =>
            {

                // アクターの生成

                string actorID = stageActorIDs[pos.y][pos.x];

                if (actorID != "N")
                {
                    WorldActor wActor = Instantiate(worldActorPrefab);
                    ActorInitData initData = Array.Find(actorInitDataArray, x => x.ActorInitDataID == actorID);
                    if (initData == null)
                    {
                        Debug.LogError($"アクターデータにアクター情報が正しく記載されていません。場所={stageData.Height - 1 - pos.y}行 {pos.x}列");
                    }
                    wActor.Init(GwConverter, new Actor(initData));
                    wActor.SetPosition(pos);
                    WorldActors.Add(wActor);
                }

                return Grid.GetCell(pos);
            });

            cursorManager.Init(GwConverter, Grid);

            CurrentTroopIndex = -1;

            // ステートの生成（登録は自動）
            new WSMapWatching(this);
            new WSMovePreparation(this);
            new WSActorMove(this);
            new WSActionSelection(this);
            new WSActorSelection(this);
            new WSBattle(this);
            new WSTroopChange(this);
            new WSWorldMenu(this);
            new WSEnemyMove(this);
            new WSStatusWindow(this);
            new WSGameClear(this);
            new WSGameOver(this);

            ChangeWorldState("TroopChange");
        }

        /// <summary>
        /// 移動可能セル表示を消す
        /// </summary>
        public void EraceMovableCells()
        {
            if (BeginPos != -Vector2Int.one)
            {
                Spot beginSpot;
                beginSpot = Grid.GetCell(BeginPos);
                beginSpot.DeactivateImage();
            }

            if (MovableCells != null)
            {
                foreach (Vector2Int p in MovableCells)
                {
                    Spot spot = Grid.GetCell(p);
                    spot.DeactivateImage();
                }
            }
        }

        /// <summary>
        /// 攻撃可能セル表示を消す
        /// </summary>
        public void EraceAttackableCells()
        {
            if (AttackableCells != null)
            {
                foreach (Vector2Int p in AttackableCells)
                {
                    Spot spot = Grid.GetCell(p);
                    spot.DeactivateImage();
                }
            }
        }

        private async UniTask WaitMenuClose(CancellationToken ct)
        {
            // 操作の干渉を防ぐため待つ
            await UniTask.WaitForSeconds(menuCloseWaitDuration, cancellationToken: ct);


            IsMenuActive = false;

            if (isPreviousCursorManagerActive)
            {
                CursorManagerRef.Activate();
            }

            if (isPreviousCameraManagerActive)
            {
                CameraManagerRef.Activate();
            }
        }

        private void Update()
        {
            if (IsMenuActive) return;

            Keyboard keyboard = Keyboard.current;

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && currentStateName == "MapWatching")
            {
                // メニュー表示
                AudioManager.Instance.PlaySe(AudioIDList.Menu, false);

                menuWindow.gameObject.SetActive(true);
                menuWindow.Init();
                IsMenuActive = true;
                isPreviousCursorManagerActive = CursorManagerRef.IsActive;
                isPreviousCameraManagerActive = CameraManagerRef.IsActive;
                CursorManagerRef.Deactivate(true);
                CameraManagerRef.Deactivate();

                menuWindow.OnResumeEvent = () =>
                {
                    _ = WaitMenuClose(this.GetCancellationTokenOnDestroy());
                };
            }
            else
            {
                // ステート更新
                if (CurrentState != null)
                {
                    CurrentState.Update();
                }
            }
        }

        private void OnEnable()
        {
            // InputAction用の処理
            decideAction?.Enable();
            cancelAction?.Enable();

            moveItemForwardAction?.Enable();
            moveItemBackAction?.Enable();
        }

        private void OnDisable()
        {
            // InputAction用の処理

            foreach (var dEvent in decideEvents)
            {
                decideAction.performed -= dEvent;
            }
            foreach (var cEvent in cancelEvents)
            {
                cancelAction.performed -= cEvent;
            }

            decideAction?.Disable();
            cancelAction?.Disable();

            moveItemForwardAction?.Disable();
            moveItemBackAction?.Disable();
        }

        /// <summary>
        /// その位置のワールドアクター取得（死亡しているなら無視）
        /// </summary>
        /// <param name="gridPosition">チェックするグリッド座標</param>
        /// <returns></returns>
        public WorldActor GetWorldActor(Vector2Int gridPosition)
        {
            foreach (WorldActor wActor in WorldActors)
            {
                if (wActor.WorldActorState != WorldActor.WorldActorStateEnum.Dead && wActor.GridPosition == gridPosition)
                {
                    return wActor;
                }
            }
            return null;
        }

        /// <summary>
        /// 軍隊に所属する全てのワールドアクターを取得（死亡しているなら無視）
        /// </summary>
        /// <param name="troopData">軍隊データ</param>
        /// <returns></returns>
        public List<WorldActor> GetWorldActorsInTroop(TroopData troopData)
        {
            List<WorldActor> worldActors = new List<WorldActor>();
            foreach (WorldActor wActor in WorldActors)
            {
                if (wActor.WorldActorState != WorldActor.WorldActorStateEnum.Dead && wActor.ActorRef.TroopDataRef == troopData)
                {
                    worldActors.Add(wActor);
                }
            }
            return worldActors;
        }

        /// <summary>
        /// 現在の軍隊データを取得
        /// </summary>
        /// <returns></returns>
        public TroopData GetCurrentTroopData()
        {
            return TroopDataArray[CurrentTroopIndex];
        }

        /// <summary>
        /// 軍隊内の全てのワールドアクターが待機状態にあるかを取得
        /// </summary>
        /// <param name="troopData"></param>
        /// <returns></returns>
        public bool IsAllWorldActorsInTroopWaiting(TroopData troopData)
        {
            bool isAll = true;
            foreach (WorldActor actor in WorldActors)
            {
                if (actor.WorldActorState == WorldActor.WorldActorStateEnum.Dead) continue;

                if (actor.ActorRef.TroopDataRef == troopData && !actor.IsWaiting)
                {
                    isAll = false;
                }
            }
            return isAll;
        }

        /// <summary>
        /// 今の軍隊内の次の敵を選択
        /// </summary>
        /// <returns></returns>
        public bool SelectNextEnemy()
        {
            do
            {
                ++CurrentEnemyIndex;
                if (CurrentEnemyIndex == Enemies.Count) break;
            }
            while (Enemies[CurrentEnemyIndex].WorldActorState == WorldActor.WorldActorStateEnum.Dead);

            if (CurrentEnemyIndex != Enemies.Count) SelectedActor = Enemies[CurrentEnemyIndex];

            return CurrentEnemyIndex != Enemies.Count;
        }

        /// <summary>
        /// 死亡したワールドアクターを排除する
        /// </summary>
        public void CleanUpWorldActors()
        {
            List<WorldActor> aliveActors = new List<WorldActor>();
            foreach (WorldActor wActor in WorldActors)
            {
                if (wActor.WorldActorState == WorldActor.WorldActorStateEnum.Dead)
                {
                    Destroy(wActor.gameObject);
                }
                else
                {
                    aliveActors.Add(wActor);
                }
            }
            WorldActors = aliveActors;
        }

        /// <summary>
        /// 自軍隊以外のワールドアクターの座標を取得する
        /// </summary>
        /// <param name="troopData">自軍隊データ</param>
        /// <returns></returns>
        public HashSet<Vector2Int> GetEnemyWorldActorPositions(TroopData troopData)
        {
            HashSet<Vector2Int> enemyActorPositions = new HashSet<Vector2Int>();
            foreach (WorldActor actor in WorldActors)
            {
                if (actor.ActorRef.TroopDataRef != troopData)
                {
                    enemyActorPositions.Add(actor.GridPosition);
                }
            }
            return enemyActorPositions;
        }

        /// <summary>
        /// ゲームオーバーかどうかチェック
        /// </summary>
        /// <returns></returns>
        public bool CheckGameOver()
        {
            // 全員が死亡しているかチェック

            bool isGameOver = true;

            foreach (WorldActor wActor in GetWorldActorsInTroop(Array.Find(troopDataArray, x => x.IsPlayer)))
            {
                if (wActor.WorldActorState != WorldActor.WorldActorStateEnum.Dead)
                {
                    isGameOver = false;
                    break;
                }
            }

            if (isGameOver)
            {
                ChangeWorldState("GameOver");
            }

            return isGameOver;
        }

        /// <summary>
        /// ゲームクリアかどうかチェック
        /// </summary>
        /// <returns></returns>
        public bool CheckGameClear()
        {
            // 味方以外の軍隊が全て1人もいなくなったかチェック
            
            bool isGameClear = true;
            foreach (WorldActor wActor in GetWorldActorsInTroop(Array.Find(troopDataArray, x => !x.IsPlayer)))
            {
                if (wActor.WorldActorState != WorldActor.WorldActorStateEnum.Dead)
                {
                    isGameClear = false;
                    break;
                }
            }

            if (isGameClear)
            {
                ChangeWorldState("GameClear");
            }

            return isGameClear;
        }
    }
}
