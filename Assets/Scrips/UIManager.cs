using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("UIManager ¡ﬂ∫π! ªË¡¶µ ");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
