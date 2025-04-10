using System.Collections.Generic;
using UnityEngine.LowLevel;
using UnityEngine;

namespace Tdk.PlayerLoopBootstrapper.Samples
{
    public interface IPostLateUpdate
    {
        void OnPostLateUpdate();
    }

    public static class PostLateUpdate
    {
        static readonly HashSet<IPostLateUpdate> PostLateUpdates = new();

        public static void RegisterPostLateUpdate(IPostLateUpdate PostLateUpdate) => PostLateUpdates.Add(PostLateUpdate);
        public static void DeregisterPostLateUpdate(IPostLateUpdate PostLateUpdate) => PostLateUpdates.Remove(PostLateUpdate);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Bootstrap()
        {
            PlayerLoopSystem PostLateUpdate = new()
            {
                updateDelegate = OnPostLateUpdate,
                type = typeof(PostLateUpdate),
                subSystemList = null
            };

            PlayerLoopBootstrapper.Initialize<UnityEngine.PlayerLoop.PostLateUpdate>(in PostLateUpdate, 0
#if UNITY_EDITOR
                , Clear
#endif
                );
        }

        static void OnPostLateUpdate()
        {
            using var e = PostLateUpdates.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current?.OnPostLateUpdate();
            }
        }

        static void Clear() => PostLateUpdates.Clear();
    }
}