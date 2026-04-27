using UnityEngine;
using TMPro;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text endHeader;

    void Start()
    {
        GameManager.Instance.OnStateChanged += OnState;
        OnState(GameManager.Instance.State);
    }

    void OnState(GameManager.GameState s)
    {
        startPanel.SetActive(s == GameManager.GameState.StartMenu);
        pausePanel.SetActive(s == GameManager.GameState.Paused);
        endPanel.SetActive(s == GameManager.GameState.Ended);
        if (s == GameManager.GameState.Ended)
        {
            float timeRemaining = GameManager.Instance.TimeRemaining;
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            endHeader.text = $"Time Remaining: {minutes:00}:{seconds:00}" + 
                             "\nTotal Creatures Caught:{CreatureAndSpawnerCounter.Instance.captured}" +
                             "/{CreatureAndSpawnerCounter.Instance.totalCreatures}";
        }
    }

    public void OnStart()   => GameManager.Instance.StartGame();
    public void OnResume()  => GameManager.Instance.TogglePause(default);
    public void OnRestart() => GameManager.Instance.Restart();
    public void OnQuit()    => GameManager.Instance.Quit();
}