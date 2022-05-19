using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;


public struct JobProgress : IComponentData
{
    public int MaxFrameLength;
    public int FrameCount;
    public JobHandle Dependency;
}

public partial class ProgressManagerSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem _beginInitializationECB;

    protected override void OnCreate()
    {
        base.OnCreate();

        _beginInitializationECB = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = _beginInitializationECB.CreateCommandBuffer();

        Entities.ForEach((Entity entity, ref JobProgress jobProgress) =>
        {
            jobProgress.FrameCount++;
            var dependency = jobProgress.Dependency;
            if (dependency.IsCompleted || jobProgress.FrameCount == jobProgress.MaxFrameLength)
            {
                dependency.Complete();
                ecb.RemoveComponent<JobProgress>(entity);
            }
        }).Run();
    }
}