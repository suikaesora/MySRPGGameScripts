using MySRPGGame.Grid;
using Unity2DGameTemplate.AudioSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MySRPGGame.Core
{
    /// <summary>
    /// カーソルを管理するクラス
    /// </summary>
    public class CursorManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject cursorObject;

        [SerializeField]
        private CameraManager cameraManager;

        [SerializeField]
        private LayerMask cursorTouchLayer;

        [SerializeField]
        private InputAction cursorMoveAction;

        [SerializeField]
        private float cursorAutoMoveInterval;

        [SerializeField]
        private float holdToAutoMoveDuration;

        public Vector2Int GridPosition { get; private set; }

        private GridWorldConverter<Spot> gwConverter;
        private Grid<Spot> grid;

        private Vector2 previousMousePos;
        private bool isGameStarting;

        private Vector2 previousMoveInput = Vector2.zero;

        private bool isHoldingToAutoMove = false;
        private bool isHolding = false;
        private float holdTimer;
        private float autoMoveTimer;

        private Vector2Int _previousGridPos;
        private int _skipCount = 0;
        private const int _startCount = 2;

        public bool IsActive { get; private set; }

        /// <summary>
        /// 初期化。必ず呼んでください
        /// </summary>
        public void Init(GridWorldConverter<Spot> gwConverter, Grid<Spot> grid)
        {
            this.gwConverter = gwConverter;
            this.grid = grid;
            isGameStarting = true;

            // カーソルを中心に移動（カメラも移動）
            cursorObject.gameObject.SetActive(true);
            Vector2 centerPos = gwConverter.GetWorldCenterPosition();
            SetGridPosition(Vector2Int.FloorToInt(centerPos));
            MoveCameraByTargetPos(false);

            Activate();
        }

        /// <summary>
        /// カーソル位置を移動
        /// </summary>
        /// <param name="gridPosition">グリッド座標</param>
        public void SetGridPosition(Vector2Int gridPosition)
        {
            GridPosition = gridPosition;
            Vector2 worldPos = gwConverter.GetGridToWorldPosition(gridPosition);
            cursorObject.transform.position = new Vector3(worldPos.x, 0f, worldPos.y);

            if (_skipCount < _startCount)
            {
                _previousGridPos = gridPosition;
                ++_skipCount;
            }

            _previousGridPos = gridPosition;
        }

        /// <summary>
        /// カメラをカーソルに移動する
        /// </summary>
        /// <param name="isAsync"></param>
        public void MoveCameraByTargetPos(bool isAsync)
        {
            if (isAsync)
            {
                cameraManager.SetTargetPositionAsync(cursorObject.transform.position);
            }
            else
            {
                cameraManager.SetTargetPosition(cursorObject.transform.position);
            }
        }

        public void Activate(bool isActivateObject = true)
        {
            IsActive = true;
            if (isActivateObject) cursorObject?.SetActive(true);
        }

        public void Deactivate(bool isDeactivateObject = true)
        {
            IsActive = false;
            if (isDeactivateObject) cursorObject?.SetActive(false);
        }

        /// <summary>
        /// マウス入力によりカーソルをコントロール
        /// </summary>
        private void CheckMouseControl()
        {
            Mouse current = Mouse.current;
            if (current == null) return;

            Vector2 mousePos = current.position.ReadValue();

            bool isMouseMoved = false;
            if (mousePos != previousMousePos)
            {
                if (isGameStarting)
                {
                    // ゲーム開始時は無視
                    isGameStarting = false;
                    previousMousePos = mousePos;
                }
                else
                {
                    // マウスが動いた
                    isMouseMoved = true;
                    previousMousePos = mousePos;
                }
            }


            if (isMouseMoved)
            {
                // マウスからレイを飛ばした地点のワールド座標にカーソルを移動
                Ray ray = cameraManager.GameCamera.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, cursorTouchLayer))
                {
                    Vector2 mouseWorldPos = new Vector2(hitInfo.point.x, hitInfo.point.z);
                    if (gwConverter.IsInRange(mouseWorldPos))
                    {
                        Vector2Int gridPosMouse = gwConverter.GetWorldToGridPosition(mouseWorldPos);

                        SetGridPosition(gridPosMouse);
                    }
                }
            }
        }

        /// <summary>
        /// キーボード入力によりカーソルをコントロール
        /// </summary>
        private void CheckKeyboardControl()
        {
            Vector2 moveInput = cursorMoveAction.ReadValue<Vector2>();

            if (moveInput != previousMoveInput)
            {
                if (moveInput != Vector2.zero)
                {
                    // 入力が開始した
                    previousMoveInput = moveInput;

                    isHolding = true;
                    isHoldingToAutoMove = true;
                    holdTimer = 0f;

                    Vector2Int target = GridPosition + Vector2Int.RoundToInt(moveInput);
                    if (!grid.IsInRange(target)) return;
                    AudioManager.Instance.PlaySe(AudioIDList.Cursor, true, 0.5f);
                    SetGridPosition(target);
                    MoveCameraByTargetPos(true);
                }
                else if (previousMoveInput != Vector2.zero && moveInput == Vector2.zero)
                {
                    // 入力が終了した
                    isHolding = false;
                    previousMoveInput = moveInput;
                }
            }

            // キーをホールドしている場合の処理
            if (isHolding)
            {
                holdTimer += Time.deltaTime;
                if (isHoldingToAutoMove)
                {
                    // 一定時間経ったら自動移動状態にする
                    if (holdTimer > holdToAutoMoveDuration)
                    {
                        isHoldingToAutoMove = false;
                        autoMoveTimer = cursorAutoMoveInterval;

                        Vector2Int target = GridPosition + Vector2Int.RoundToInt(moveInput);
                        if (!grid.IsInRange(target)) return;
                        AudioManager.Instance.PlaySe(AudioIDList.Cursor, true, 0.5f);
                        SetGridPosition(target);
                    }
                }
                else
                {
                    // 一定間隔でカーソルを移動する
                    autoMoveTimer -= Time.deltaTime;
                    if (autoMoveTimer < 0f)
                    {
                        autoMoveTimer = cursorAutoMoveInterval;

                        Vector2Int target = GridPosition + Vector2Int.RoundToInt(moveInput);
                        if (!grid.IsInRange(target)) return;
                        AudioManager.Instance.PlaySe(AudioIDList.Cursor, true, 0.5f);
                        SetGridPosition(target);
                        cameraManager.SetTargetPositionAsync(cursorObject.transform.position);
                    }
                }
            }
        }

        private void Update()
        {
            if (!IsActive) return;
            CheckMouseControl();
            CheckKeyboardControl();
        }

        private void OnEnable()
        {
            cursorMoveAction?.Enable();
        }

        private void OnDisable()
        {
            cursorMoveAction?.Disable();
        }
    }
}
