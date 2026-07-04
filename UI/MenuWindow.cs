using System;
using Unity2DGameTemplate.AudioSystem;
using Unity2DGameTemplate.FadeSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MySRPGGame.UI.Main
{
    public class MenuWindow : MonoBehaviour
    {
        [SerializeField] private SimpleSelectiveWindow selectiveWindow;

        public Action OnResumeEvent { get; set; }

        private bool _isLoading = false;

        public void Init()
        {
            selectiveWindow.InitWithNotInstantiate(id =>
            {
                switch (id)
                {
                    case 0:
                        if (_isLoading) return;
                        gameObject.SetActive(false);
                        OnResumeEvent?.Invoke();
                        break;
                    case 1:
                        if (_isLoading) return;
                        _isLoading = true;
                        AudioManager.Instance.StopBgm();
                        _ = FadeManager.Instance.ExecuteFade(FadeManager.EFadeElementId.Normal, fadeElement =>
                        {
                            fadeElement.SetFadeOutDuration(0.5f);
                            fadeElement.SetFadeInDuration(0.5f);
                        },
                        async () => {
                            await SceneManager.LoadSceneAsync("Title");
                        });
                        break;
                    default:
                        if (_isLoading) return;
                        //ゲームプレイ終了
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                        break;
                }
            });
        }
    }
}
