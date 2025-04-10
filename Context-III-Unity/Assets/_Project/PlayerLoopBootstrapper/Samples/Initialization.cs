using System.Collections.Generic;
using UnityEngine.LowLevel;
using UnityEngine;

namespace Tdk.PlayerLoopBootstrapper.Samples
{
    public interface IInitialization
    {
        void OnInitialize();
    }

    public static class Initialization
    {
        static readonly HashSet<IInitialization> Initializations = new();

        public static void RegisterInitialization(IInitialization Initialization) => Initializations.Add(Initialization);
        public static void DeregisterInitialization(IInitialization Initialization) => Initializations.Remove(Initialization);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Bootstrap()
        {
            PlayerLoopSystem Initialization = new()
            {
                updateDelegate = OnInitialization,
                type = typeof(Initialization),
                subSystemList = null
            };

            PlayerLoopBootstrapper.Initialize<UnityEngine.PlayerLoop.Initialization>(in Initialization, 0
#if UNITY_EDITOR
                , Clear
#endif
                );
        }

        static void OnInitialization()
        {
            using var e = Initializations.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current?.OnInitialize();
            }
        }

        static void Clear() => Initializations.Clear();
    }
}