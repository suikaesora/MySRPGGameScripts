using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace MySRPGGame.Core
{
    /// <summary>
    /// カメラの位置と回転を管理する
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraManager : MonoBehaviour
    {
        [SerializeField]
        private float minCameraRotation;

        [SerializeField]
        private float maxCameraRotation;

        [SerializeField]
        private float minCameraDistance;
        [SerializeField]
        private float maxCameraDistance;

        [SerializeField]
        private float initialCameraHeightRatio;

        [SerializeField]
        private float moveUpDownSpeedMouse;

        [SerializeField]
        private float moveUpDownSpeedKeyboard;

        [SerializeField]
        private float transformedTime;

        private float unitPerScreenPixel;

        public Camera GameCamera { get; private set; }

        private Vector3 originalTargetPos;
        private Vector2 originalMousePos;

        public float HeightRatio { get; private set; }

        public Vector3 TargetPosition { get; private set; }

        public float Rotation { get; private set; }
        public float Distance { get; private set; }

        private CancellationTokenSource transformedTaskCancellation = new CancellationTokenSource();

        public bool IsCameraTransforming { get; private set; } = false;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            GameCamera = GetComponent<Camera>();
            SetTargetPosition(Vector3.zero);
            SetHeight(initialCameraHeightRatio);
            Rotation = 0f;
            Distance = 0f;
            Activate();
        }

        /// <summary>
        /// 現在の状態に合わせてカメラの位置と角度を調整
        /// </summary>
        public void SetCameraTransform()
        {
            GameCamera.transform.position = new Vector3
            (
                TargetPosition.x,
                Mathf.Sin(Mathf.Deg2Rad * Rotation) * Distance + TargetPosition.y,
                -Mathf.Cos(Mathf.Deg2Rad * Rotation) * Distance + TargetPosition.z
            );
            GameCamera.transform.eulerAngles = new Vector3(Rotation, GameCamera.transform.eulerAngles.y, GameCamera.transform.eulerAngles.z);
        }

        /// <summary>
        /// 現在の状態に合わせてカメラの位置と角度を調整（時間をかけて）
        /// </summary>
        public async UniTask SetCameraTransformAsync(CancellationToken ct)
        {
            Vector3 targetPosition = new Vector3
            (
                TargetPosition.x,
                Mathf.Sin(Mathf.Deg2Rad * Rotation) * Distance + TargetPosition.y,
                -Mathf.Cos(Mathf.Deg2Rad * Rotation) * Distance + TargetPosition.z
            );
            Vector3 targetEulerAngles = new Vector3(Rotation, GameCamera.transform.eulerAngles.y, GameCamera.transform.eulerAngles.z);

            Vector3 originalPosition = transform.position;
            Vector3 originalEulerAngles = transform.eulerAngles;

            IsCameraTransforming = true;

            // 元の位置と目標位置を補完して動かす
            float timer = transformedTime;
            while (timer > 0f)
            {
                transform.position = Vector3.Lerp(originalPosition, targetPosition, 1f - timer / transformedTime);
                transform.eulerAngles = Vector3.Lerp(originalEulerAngles, targetEulerAngles, 1f - timer / transformedTime);

                await UniTask.Yield(ct);
                ct.ThrowIfCancellationRequested();

                timer -= Time.deltaTime;
            }

            transform.position = targetPosition;
            transform.eulerAngles = targetEulerAngles;

            IsCameraTransforming = false;
        }

        /// <summary>
        /// 高さに応じてパラメータを設定
        /// </summary>
        /// <param name="heightRatio">高さの割合0~1</param>
        public void SetHeight(float heightRatio)
        {
            if (IsCameraTransforming) return;
            //heightRatio = Mathf.Clamp(heightRatio, minHeightRatio, maxHeightRatio);
            HeightRatio = heightRatio;
            Rotation = Mathf.Lerp(minCameraRotation, maxCameraRotation, heightRatio);
            Distance = Mathf.Lerp(minCameraDistance, maxCameraDistance, heightRatio);
            SetCameraTransform();
        }

        /// <summary>
        /// 高さに応じてパラメータを設定し、動かす（時間をかけて）
        /// </summary>
        /// <param name="heightRatio">高さの割合0~1</param>
        public void SetHeightAsync(float heightRatio)
        {
            //heightRatio = Mathf.Clamp(heightRatio, minHeightRatio, maxHeightRatio);
            HeightRatio = heightRatio;
            Rotation = Mathf.Lerp(minCameraRotation, maxCameraRotation, heightRatio);
            Distance = Mathf.Lerp(minCameraDistance, maxCameraDistance, heightRatio);

            transformedTaskCancellation?.Cancel();
            IsCameraTransforming = false;
            transformedTaskCancellation = new CancellationTokenSource();
            _ = SetCameraTransformAsync(transformedTaskCancellation.Token);
        }

        /// <summary>
        /// 目標座標を設定し動かす
        /// </summary>
        /// <param name="targetPosition">目標座標</param>
        public void SetTargetPosition(Vector3 targetPosition)
        {
            if (IsCameraTransforming) return;
            SetHeight(HeightRatio);
            TargetPosition = targetPosition;
            SetCameraTransform();
        }

        /// <summary>
        /// 目標座標を設定し動かす（時間をかけて）
        /// </summary>
        /// <param name="targetPosition">目標座標</param>
        public void SetTargetPositionAsync(Vector3 targetPosition)
        {
            SetHeight(HeightRatio);
            TargetPosition = targetPosition;
            transformedTaskCancellation?.Cancel();
            IsCameraTransforming = false;
            transformedTaskCancellation = new CancellationTokenSource();
            _ = SetCameraTransformAsync(transformedTaskCancellation.Token);
        }

        private void CheckMouseInput()
        {
            Mouse current = Mouse.current;
            if (current == null) return;

            // 上下左右移動
            if (current.rightButton.wasPressedThisFrame)
            {
                originalTargetPos = TargetPosition;
                originalMousePos = Input.mousePosition;

                // スクリーンの1ピクセルがワールド座標上の約何mに当たるかを計算
                unitPerScreenPixel = (GameCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, Distance)) - GameCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f + 1f, Screen.height / 2f, Distance))).magnitude;
            }
            else if (current.rightButton.isPressed)
            {
                // マウス座標に応じて差分を計算しカメラを動かす
                Vector2 currentMousePos = Input.mousePosition;
                Vector2 mousePosDiff = currentMousePos - originalMousePos;

                Vector3 worldPosDiff = mousePosDiff * unitPerScreenPixel;
                worldPosDiff.z = worldPosDiff.y;
                worldPosDiff.y = 0f;
                SetTargetPosition(originalTargetPos - worldPosDiff);
            }
            else
            {
                // スクロール。高さ調整
                float scroll = current.scroll.y.value;

                if (scroll != 0f)
                {
                    SetHeight(HeightRatio - scroll * Time.deltaTime * moveUpDownSpeedMouse * Distance);
                }
            }
        }

        /// <summary>
        /// キーボードによる高さ調整
        /// </summary>
        private void CheckKeyboardInput()
        {
            Keyboard current = Keyboard.current;
            if (current == null) return;

            float moveUp = 0f;
            if (current.cKey.isPressed)
            {
                moveUp = -1f;
            }
            else if (current.vKey.isPressed)
            {
                moveUp = 1f;
            }

            if (moveUp == 0f) return;

            SetHeight(HeightRatio + moveUp * Time.deltaTime * moveUpDownSpeedKeyboard * Distance);
        }

        private void Update()
        {
            if (!IsActive) return;
            CheckMouseInput();
            CheckKeyboardInput();
        }

        private void OnDestroy()
        {
            transformedTaskCancellation?.Cancel();
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
