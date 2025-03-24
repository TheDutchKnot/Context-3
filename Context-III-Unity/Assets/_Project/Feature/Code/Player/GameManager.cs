using UnityEngine;
using System;


/// <summary>
/// Game states
/// </summary>
public enum GameState
{
    MainMenu,     
    Playing,      
    ZeroGravity,  
    Paused        
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private GameState currentGameState;
    public GameState CurrentGameState => currentGameState;
    
    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetGameState(GameState.ZeroGravity);
    }

    /// <summary>
    /// switch game state
    /// </summary>
    public void SetGameState(GameState newGameState)
    {
        if (newGameState == currentGameState)
            return;

        currentGameState = newGameState;
        OnGameStateChanged?.Invoke(currentGameState);
        
        switch (currentGameState)
        {
            case GameState.MainMenu:
                Debug.Log("GameState -> MainMenu");
                break;

            case GameState.Playing:
                Debug.Log("GameState -> Playing");
                break;

            case GameState.ZeroGravity:
                Debug.Log("GameState -> ZeroGravity");
                break;

            case GameState.Paused:
                Debug.Log("GameState -> Paused");
                break;
        }
    }
}
