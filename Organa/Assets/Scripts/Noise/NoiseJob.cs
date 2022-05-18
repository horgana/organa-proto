using Unity.Jobs;
using Unity.Mathematics;


struct NoiseJob<T> : IJobParallelFor where T : struct, Noise.INoiseSource<float2, float>
{
    public void Execute(int index)
    {
        throw new System.NotImplementedException();
    }
}