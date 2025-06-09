using UnityEngine;
using UnityEngine.SceneManagement;  // لاستدعاء المشهد مباشرة إذا أردت

public class UIButtonHandler : MonoBehaviour
{
    // تستدعى من زر Reset
    public void OnResetButtonPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResetGame();
        else
            Debug.LogWarning("GameManager.Instance is null!");
    }

    // تستدعى من زر Exit
    public void OnExitButtonPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
        else
            Debug.LogWarning("GameManager.Instance is null!");
    }
}
