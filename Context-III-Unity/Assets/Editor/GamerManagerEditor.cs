using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameManager gameManager = (GameManager)target;
        
        DrawDefaultInspector();

  
        GameState newState = (GameState)EditorGUILayout.EnumPopup("Game State", gameManager.CurrentGameState);

      
        if (newState != gameManager.CurrentGameState)
        {
            gameManager.SetGameState(newState);
    
            EditorUtility.SetDirty(gameManager);
        }
    }
}