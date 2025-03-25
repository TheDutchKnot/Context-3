using UnityEngine;

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 获取当前 GameManager 实例
        GameManager gameManager = (GameManager)target;
        
        // 绘制默认 Inspector（可以显示其他属性）
        DrawDefaultInspector();

        // 绘制一个枚举下拉框，显示当前状态
        GameState newState = (GameState)EditorGUILayout.EnumPopup("Game State", gameManager.CurrentGameState);

        // 当选择的状态发生变化时，调用 SetGameState 方法
        if (newState != gameManager.CurrentGameState)
        {
            gameManager.SetGameState(newState);
            // 标记对象已修改，确保状态保存
            EditorUtility.SetDirty(gameManager);
        }
    }
}