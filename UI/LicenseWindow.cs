using Unity2DGameTemplate.AudioSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class LicenseWindow : MonoBehaviour
{
    public bool IsOpen { get; private set; }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (IsOpen && keyboard.xKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    public void Open()
    {
        IsOpen = true;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        IsOpen = false;
        gameObject.SetActive(false);
        AudioManager.Instance.PlaySe(AudioIDList.Cancel, false);
    }

    public void OnCloseButton()
    {
        Close();
    }
}
