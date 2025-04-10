using UnityEngine.LowLevel;
using UnityEditor;
using UnityEngine;
using System;

namespace Tdk.PlayerLoopBootstrapper
{
    public static class PlayerLoopBootstrapper
    {
#if UNITY_EDITOR
        static PlayerLoopSystem systemToRemove;
        static Action manualDomainReload;
#endif

        public static void Initialize<T>(in PlayerLoopSystem systemToInsert, int index
#if UNITY_EDITOR
            , Action onManualDomainReload
#endif
            )
        {
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if (!PlayerLoopUtils.InsertSystem<T>(ref currentPlayerLoop, in systemToInsert, index))
            {
                Debug.LogWarning("Initialization failed, unable to register " + systemToInsert.ToString() + "into the " + typeof(T).ToString() + "loop");
                return;
            }
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;

            static void OnPlayModeState(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
                    PlayerLoopUtils.RemoveSystem<T>(ref currentPlayerLoop, in systemToRemove);
                    PlayerLoop.SetPlayerLoop(currentPlayerLoop);
                    manualDomainReload?.Invoke();
                }
            }

            manualDomainReload = onManualDomainReload;
            systemToRemove = systemToInsert;
#endif
        }
    }
}