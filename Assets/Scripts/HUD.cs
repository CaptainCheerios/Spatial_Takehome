using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private GameObject crosshair;

    public Canvas canvas;

    void Start()
    {
        GameManager.Instance.OnTimerTick += UpdateTimer;
        GameManager.Instance.OnStateChanged += UIStateChanged;
        CreatureAndSpawnerCounter.Instance.OnCreatureCaptured += UpdateCounter;
        UpdateTimer(); 
        UpdateCounter();
        UIStateChanged(GameManager.Instance.State);
    }

    void UIStateChanged(GameManager.GameState state)
    {
        if (state != GameManager.GameState.Playing)
        {
            canvas.enabled = false;
        }
        else
        {
            canvas.enabled = true;
        }
    }

    void UpdateTimer()
    {
        float t = Mathf.Max(0, GameManager.Instance.TimeRemaining);
        timerText.text = $"Time {(int)(t / 60):00}:{(int)(t % 60):00}";
    }

    void UpdateCounter()
    {
        var cc = CreatureAndSpawnerCounter.Instance;
        counterText.text = $"Captured {cc.captured} / {cc.spawned}";
    }
}
