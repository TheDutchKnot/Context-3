using System.Collections.Generic;

public static class TimerManager
{
    readonly static HashSet<Timer> timers = new();
    readonly static HashSet<Timer> sweep = new();

    public static void RegisterTimer(Timer timer) => timers.Add(timer);
    public static void DeregisterTimer(Timer timer) => timers.Remove(timer);

    public static void UpdateTimers()
    {
        if (timers.Count == 0) return;

        sweep.UnionWith(timers);
        using var e = sweep.GetEnumerator();
        while (e.MoveNext())
        {
            e.Current?.Tick();
        }
        sweep.Clear();
    }

    public static void Clear()
    {
        if (timers.Count == 0) return;

        sweep.UnionWith(timers);
        using var e = timers.GetEnumerator();
        while (e.MoveNext())
        {
            e.Current?.Dispose();
        }
        sweep.Clear();

        timers.Clear();
    }
}
