using System.Collections.Generic;
using UnityEngine.LowLevel;
using UnityEngine;

namespace Tdk.PlayerLoopBootstrapper.Samples
{
    public interface IEarlyUpdate
    {
        void OnEarlyUpdate();
    }

    public static class EarlyUpdate
    {
        static readonly HashSet<IEarlyUpdate> earlyUpdates = new();

        public static void RegisterEarlyUpdate(IEarlyUpdate earlyUpdate) => earlyUpdates.Add(earlyUpdate);
        public static void DeregisterEarlyUpdate(IEarlyUpdate earlyUpdate) => earlyUpdates.Remove(earlyUpdate);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Bootstrap()
        {
            PlayerLoopSystem earlyUpdate = new()
            {
                updateDelegate = OnEarlyUpdate,
                type = typeof(EarlyUpdate),
                subSystemList = null
            };

            PlayerLoopBootstrapper.Initialize<UnityEngine.PlayerLoop.EarlyUpdate>(in earlyUpdate, 0
#if UNITY_EDITOR
                , Clear
#endif
                );
        }

        static void OnEarlyUpdate()
        {
            using var e = earlyUpdates.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current?.OnEarlyUpdate();
            }
        }

        static void Clear() => earlyUpdates.Clear();
    }
}