using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;
using UnityEngine;

[BurstCompile]
public partial class CarSpawnerSystem : SystemBase
{

    // Run only if there is a CarSpawner.
    protected override void OnCreate()
    {
        RequireForUpdate<CarSpawner>();
    }

    protected override void OnStartRunning()
    {
        EntityCommandBuffer ecb = new(Allocator.Temp);

        uint seed = (uint)System.DateTime.Now.Ticks;
        Unity.Mathematics.Random random = new(seed);

        foreach ((CarSpawner spawner, DynamicBuffer<ColorBufferElement> colors, Entity entity)
                   in SystemAPI.Query<CarSpawner, DynamicBuffer<ColorBufferElement>>().WithEntityAccess())
        {
            Entity car = ecb.Instantiate(spawner.Prefab);

            int colorIndex = random.NextInt(0, colors.Length);
            float4 color = colors[colorIndex].Value;

            ecb.SetComponent(car, new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            });

            ecb.AddComponent(car, new MoveSpeed { Value = spawner.MoveSpeed });
            ecb.AddComponent(car, new RotationSpeed { Value = spawner.RotationSpeed });
            ecb.AddComponent(car, new URPMaterialPropertyBaseColor { Value = color });

            ecb.RemoveComponent<CarSpawner>(entity);
        }

        // Execute all queued entity commands.
        ecb.Playback(EntityManager);

        // Free the memory.
        ecb.Dispose();
    }

    [BurstDiscard]
    private void DebugLogColor(float4 color)
    {
        Debug.Log(color);
    }
    protected override void OnUpdate() { }
}
