using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    
    public float timerLength = 120f;


    public enum GameState
    {
        StartMenu,
        Playing,
        Paused,
        Ended
    }

    [SerializeField] private float levelDuration = 120f;
    [SerializeField] private PlayerInput playerInput;

    public GameState State { get; private set; } = GameState.StartMenu;
    public float TimeRemaining { get; private set; }
    public bool GameWon{get; private set;}

    public event Action<GameState> OnStateChanged;
    public event Action OnTimerTick;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        
        TimeRemaining = levelDuration;
    }

    void Start()
    {
        SetState(GameState.StartMenu);
    }
    
    

    public void StartGame()
    {
        TimeRemaining = levelDuration;
        SetState(GameState.Playing);
    }

    public void TogglePauseInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
    }

    public void TogglePause()
    {
        if(State == GameState.Playing)
            SetState(GameState.Paused);
        else if (State == GameState.Paused)
            SetState(GameState.Playing);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void EndLevel(bool won)
    {
        GameWon = won;
        SetState(GameState.Ended);
    }
    
    void SetState(GameState nextState)
    {
        State = nextState;
        Time.timeScale = (nextState == GameState.Playing) ? 1f : 0f;

        // Cursor
        if (nextState == GameState.Playing)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Action map switch
        if (playerInput)
        {
            if (nextState == GameState.Playing) 
                playerInput.SwitchCurrentActionMap("Gameplay");
            else 
                playerInput.SwitchCurrentActionMap("UI");
        }
        OnStateChanged?.Invoke(nextState);
    }
    
    void Update()
    {
        if (State != GameState.Playing)
            return;
        TimeRemaining -= Time.deltaTime;
        OnTimerTick?.Invoke();
        if (TimeRemaining <= 0f)
            EndLevel(won: false);
    }
}