using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public static FPSCounter Instance;

    public float SmoothSpeed = 1f;
    public float fps, smoothFps;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        fps = 1f / Time.smoothDeltaTime;
        if (Time.timeSinceLevelLoad < 0.1f) smoothFps = fps;
        smoothFps += (fps - smoothFps) * Mathf.Clamp(Time.deltaTime * SmoothSpeed, 0, 1);
    }
}