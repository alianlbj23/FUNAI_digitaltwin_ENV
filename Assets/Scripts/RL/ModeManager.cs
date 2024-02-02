using UnityEngine;

public class ModeManager : MonoBehaviour
{
    public string mode = "inference";

    public void SetMode(string newMode)
    {
        mode = newMode;
    }

    public string GetMode()
    {
        return mode;
    }
}
