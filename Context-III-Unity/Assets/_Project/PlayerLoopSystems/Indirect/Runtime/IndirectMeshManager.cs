using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using UnityEngine.LowLevel;
using UnityEngine;

namespace Tdk.PlayerLoopSystems.Indirect
{
    public interface IRenderMeshIndirect
    {
        void RenderMeshIndirect();
    }

    public static class IndirectMeshManager
    {
        static readonly HashSet<IRenderMeshIndirect> instances = new();

        public static void RegisterInstance(IRenderMeshIndirect i) => instances.Add(i);
        public static void DeregisterInstance(IRenderMeshIndirect i) => instances.Remove(i);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Bootstrap()
        {
            PlayerLoopSystem renderIndirect = new()
            {
                updateDelegate = RenderInstancedIndirect,
                type = typeof(IndirectMeshManager),
                subSystemList = null
            };

            PlayerLoopBootstrapper.Initialize<PostLateUpdate>(in renderIndirect, 0
#if UNITY_EDITOR
            , Clear
#endif
            );
        }

        public static void RenderInstancedIndirect()
        {
            using var e = instances.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current?.RenderMeshIndirect();
            }
        }

        static void Clear() => instances.Clear();
    }
}