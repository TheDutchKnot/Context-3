using System.Collections.Generic;

public interface IRenderMeshIndirect
{
    void RenderMeshIndirect();
}

public static class IndirectMeshManager
{
    static readonly HashSet<IRenderMeshIndirect> instances = new();

    public static void RegisterInstance(IRenderMeshIndirect i) => instances.Add(i);
    public static void DeregisterInstance(IRenderMeshIndirect i) => instances.Remove(i);

    static void Bootstrap()
    {

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
