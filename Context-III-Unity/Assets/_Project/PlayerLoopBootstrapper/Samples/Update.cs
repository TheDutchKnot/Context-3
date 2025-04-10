using System.Collections.Generic;
using UnityEngine.LowLevel;
using UnityEngine;

namespace Tdk.PlayerLoopBootstrapper.Samples
{
    public interface IUpdate
    {
        void OnUpdate();
    }

    public static class Update
    {
        static readonly HashSet<IUpdate> Updates = new();

        public static void RegisterUpdate(IUpdate Update) => Updates.Add(Update);
        public static void DeregisterUpdate(IUpdate Update) => Updates.Remove(Update);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Bootstrap()
        {
            PlayerLoopSystem Update = new()
            {
                updateDelegate = OnUpdate,
                type = typeof(Update),
                subSystemList = null
            };

            PlayerLoopBootstrapper.Initialize<UnityEngine.PlayerLoop.Update>(in Update, 0
#if UNITY_EDITOR
                , Clear
#endif
                );
        }

        static void OnUpdate()
        {
            using var e = Updates.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current?.OnUpdate();
            }
        }

        static void Clear() => Updates.Clear();
    }
}