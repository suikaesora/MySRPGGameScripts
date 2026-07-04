using MySRPGGame.UI;
using Unity2DGameTemplate.AudioSystem;
using Unity2DGameTemplate.FadeSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MySRPGGame
{
    public class TitleSceneManager : MonoBehaviour
    {
        [SerializeField]
        private SimpleSelectiveWindow selectiveWindow;

        [SerializeField]
        private LicenseWindow _licenseWindow;

        private bool _isLoading = false;

        private void LoadMainGame()
        {
            _isLoading = true;
            AudioManager.Instance.StopBgm();
            _ = FadeManager.Instance.ExecuteFade(FadeManager.EFadeElementId.Normal, fadeElement => 
            {
                fadeElement.SetFadeOutDuration(0.5f);
                fadeElement.SetFadeInDuration(0.5f);
            }, 
            async () => { await SceneManager.LoadSceneAsync("Main"); 
            });
        }

        private void Start()
        {
            AudioManager.Instance.PlayBgm(AudioIDList.Title);
            selectiveWindow.InitWithNotInstantiate(id =>
            {
                switch (id)
                {
                    case 0:
                        if (_licenseWindow.IsOpen || _isLoading) return;
                        LoadMainGame();
                        break;
                    case 1:
                        if (_licenseWindow.IsOpen || _isLoading) return;
                        _licenseWindow.Open();
                        break;
                    case 2:
                        if (_licenseWindow.IsOpen || _isLoading) return;
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                        break;
                    default:
                        Debug.Log("ìoò^Ç≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅB");
                        break;
                }
            });
        }
    }
}
