using UnityEngine.LowLevel;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System;
#endif

namespace Tdk.PlayerLoopBootstrapper
{
    public static class PlayerLoopBootstrapper
    {
#if UNITY_EDITOR
        static readonly List<(PlayerLoopSystem, Action)> systemsToDomainReload = new();
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
            systemsToDomainReload.Add((systemToInsert, onManualDomainReload));

            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;

            static void OnPlayModeState(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
                    PlayerLoopUtils.RemoveSystem<T>(ref currentPlayerLoop, systemsToDomainReload[0].Item1);
                    PlayerLoop.SetPlayerLoop(currentPlayerLoop);
                    systemsToDomainReload[0].Item2?.Invoke();
                    systemsToDomainReload.RemoveAt(0);
                }
            }
#endif
        }
    }
}