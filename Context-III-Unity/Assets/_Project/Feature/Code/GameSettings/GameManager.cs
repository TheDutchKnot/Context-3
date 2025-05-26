using UnityEngine;
using System;


/// <summary>
/// Game states
/// </summary>
public enum GameState
{
    ANIMATION,     
    PLAYING,      
    ZERO_GRAVITY,
    BOSS_FIGHT,
    GAMEOVER      
}


public class GameManager : MonoBehaviour
{
    public delegate void SwitchStateInfo(GameState newState);
    public event SwitchStateInfo StateChange;
        
    public static GameManager Instance { get; private set; }
    
    public GameState currentGameState;
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
        // TODO need change to ANIMATION later
        SetGameState(GameState.PLAYING);
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
        StateChange?.Invoke(newGameState);
        switch (currentGameState)
        {
            case GameState.ANIMATION:
                Debug.Log("GameState -> Animation");
                break;

            case GameState.PLAYING:
                Debug.Log("GameState -> Playing");
                break;
            
            case GameState.BOSS_FIGHT:
                Debug.Log("GameState -> BossFight");
                break;
            
            case GameState.ZERO_GRAVITY:
                Debug.Log("GameState -> ZeroGravity");
                break;

            case GameState.GAMEOVER:
                Debug.Log("GameState -> Game over");
                break;
        }
    }
}
