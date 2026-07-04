using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MySRPGGame.UI.Main
{
    public class GameClearWindow : MonoBehaviour
    {
        [SerializeField] private SimpleSelectiveWindow selectiveWindow;

        private void Start()
        {
            selectiveWindow.InitWithNotInstantiate(id =>
            {
                switch (id)
                {
                    case 0:
                        SceneManager.LoadScene("Title");
                        break;
                    case 1:
                        SceneManager.LoadScene("Main");
                        break;
                    default:
                        Debug.Log("—\Šú‚µ‚È‚¢‘I‘ğ‚ªŒŸo‚³‚ê‚Ü‚µ‚½B");
                        break;
                }
            });
        }
    }

}