using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using UnityEngine.LowLevel;
using UnityEngine;
using System.Linq;

namespace Tdk.PlayerLoopSystems.Timers
{
    public static class TimerManager
    {
        readonly static HashSet<Timer> timers = new();

        public static void RegisterTimer(Timer timer) => timers.Add(timer);
        public static void DeregisterTimer(Timer timer) => timers.Remove(timer);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Bootstrap()
        {
            PlayerLoopSystem timerUpdate = new()
            {
                updateDelegate = UpdateTimers,
                type = typeof(TimerManager),
                subSystemList = null
            };

            PlayerLoopBootstrapper.Initialize<Update>(in timerUpdate, 0
#if UNITY_EDITOR
                    , Clear
#endif
                    );
        }

        static void UpdateTimers()
        {
            if (timers.Count == 0) return;

            using var e = timers.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current?.Tick();
            }
        }

        static void Clear()
        {
            if (timers.Count == 0) return;

            foreach(var timer in timers.ToList())
            {
                timer.Dispose();
            }
            timers.Clear();
        }
    }
}