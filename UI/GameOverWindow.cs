using UnityEngine;
using UnityEngine.SceneManagement;

namespace MySRPGGame.UI.Main
{
    public class GameOverWindow : MonoBehaviour
    {
        [SerializeField] private SimpleSelectiveWindow selectiveWindow;

        private void Start()
        {
            selectiveWindow.InitWithNotInstantiate(id =>
            {
                switch (id)
                {
                    case 0:
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
