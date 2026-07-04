using UnityEngine;

namespace MySRPGGame.OperationView
{
    public enum EOperationType
    {
        MouseLClick,
        MouseRClick,
        MouseRDrag,
        ArrowKeys,
    }

    public class OperationView : MonoBehaviour
    {
        public static OperationView Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        // OperationItemí«â¡
        public void AddOperationItem(EOperationType operation, string operationEffect)
        {

        }

        // OperationItemçÌèú
        public void RemoveOperationItem(EOperationType operation)
        {

        }
    }
}
