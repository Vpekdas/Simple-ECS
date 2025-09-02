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
    protected override void OnCreate()
    {
        RequireForUpdate<CarSpawner>();
    }

    protected override void OnUpdate()
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        EntityCommandBuffer ecb = new(Allocator.Temp);

        uint seed = (uint)System.DateTime.Now.Ticks;
        Unity.Mathematics.Random random = new(seed);

        foreach (
            (RefRW<CarSpawner> spawner, DynamicBuffer<ColorBufferElement> colors, Entity entity)
            in SystemAPI.Query<RefRW<CarSpawner>, DynamicBuffer<ColorBufferElement>>().WithEntityAccess()
        )
        {
            spawner.ValueRW.SpawnTimer += deltaTime;

            if (spawner.ValueRW.SpawnTimer >= spawner.ValueRO.SpawnInterval)
            {
                spawner.ValueRW.SpawnTimer = 0f; 

                Entity car = ecb.Instantiate(spawner.ValueRO.Prefab);
                int colorIndex = random.NextInt(0, colors.Length);
                float4 color = colors[colorIndex].Value;

                DynamicBuffer<CircuitPoint> circuit = SystemAPI.GetBuffer<CircuitPoint>(entity);
                if (circuit.Length > 0)
                {
                    ecb.SetComponent(car, new LocalTransform
                    {
                        Position = circuit[0].Position,
                        Rotation = quaternion.identity,
                        Scale = 1f
                    });
                }
                ecb.AddComponent(car, new MoveSpeed { Value = spawner.ValueRO.MoveSpeed });
                ecb.AddComponent(car, new URPMaterialPropertyBaseColor { Value = color });
                ecb.AddComponent(car, new CurrentTargetIndex { Value = 0 });
                ecb.AddComponent(car, new RouteReference { CircuitEntity = spawner.ValueRO.CircuitEntity });
            }
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }

    [BurstDiscard]
    private void DebugLogColor(float4 color)
    {
        Debug.Log(color);
    }
}