using System.Collections.Generic;
using UnityEngine.LowLevel;
using UnityEngine;

namespace Tdk.PlayerLoopBootstrapper.Samples
{
    public interface IPreUpdate
    {
        void OnPreUpdate();
    }

    public static class PreUpdate
    {
        static readonly HashSet<IPreUpdate> preUpdates = new();

        public static void RegisterPreUpdate(IPreUpdate preUpdate) => preUpdates.Add(preUpdate);
        public static void DeregisterPreUpdate(IPreUpdate preUpdate) => preUpdates.Remove(preUpdate);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Bootstrap()
        {
            PlayerLoopSystem preUpdate = new()
            {
                updateDelegate = OnPreUpdate,
                type = typeof(PreUpdate),
                subSystemList = null
            };

            PlayerLoopBootstrapper.Initialize<UnityEngine.PlayerLoop.PreUpdate>(in preUpdate, 0
#if UNITY_EDITOR
                , Clear
#endif
                );
        }

        static void OnPreUpdate()
        {
            using var e = preUpdates.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current?.OnPreUpdate();
            }
        }

        static void Clear() => preUpdates.Clear();
    }
}