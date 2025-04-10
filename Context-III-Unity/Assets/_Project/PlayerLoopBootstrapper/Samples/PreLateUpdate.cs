using System.Collections.Generic;
using UnityEngine.LowLevel;
using UnityEngine;

namespace Tdk.PlayerLoopBootstrapper.Samples
{
    public interface IPreLateUpdate
    {
        void OnPreLateUpdate();
    }

    public static class PreLateUpdate
    {
        static readonly HashSet<IPreLateUpdate> preLateUpdates = new();

        public static void RegisterPreLateUpdate(IPreLateUpdate preLateUpdate) => preLateUpdates.Add(preLateUpdate);
        public static void DeregisterPreLateUpdate(IPreLateUpdate preLateUpdate) => preLateUpdates.Remove(preLateUpdate);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Bootstrap()
        {
            PlayerLoopSystem preLateUpdate = new()
            {
                updateDelegate = OnPreLateUpdate,
                type = typeof(PreLateUpdate),
                subSystemList = null
            };

            PlayerLoopBootstrapper.Initialize<UnityEngine.PlayerLoop.PreLateUpdate>(in preLateUpdate, 0
#if UNITY_EDITOR
                , Clear
#endif
                );
        }

        static void OnPreLateUpdate()
        {
            using var e = preLateUpdates.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current?.OnPreLateUpdate();
            }
        }

        static void Clear() => preLateUpdates.Clear();
    }
}