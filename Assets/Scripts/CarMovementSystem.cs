using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public partial class CarMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        BufferLookup<CircuitPoint> circuitPointLookup = GetBufferLookup<CircuitPoint>(true);
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        Entities
            .WithReadOnly(circuitPointLookup)
            .ForEach((Entity entity, ref LocalTransform transform,
                      ref CurrentTargetIndex targetIndex,
                      in MoveSpeed moveSpeed,
                      in RotationSpeed rotationSpeed,
                      in RouteReference routeRef) =>
            {
                if (!circuitPointLookup.HasBuffer(routeRef.CircuitEntity))
                {
                    return;
                }

                DynamicBuffer<CircuitPoint> route = circuitPointLookup[routeRef.CircuitEntity];

                if (route.Length == 0)
                {
                    return;
                }

                int currentIndex = targetIndex.Value;
                float3 currentPos = transform.Position;
                float3 targetPos = route[currentIndex].Position;
                float3 direction = targetPos - currentPos;
                float distance = math.length(direction);

                if (distance < 0.1f)
                {
                    if (currentIndex == route.Length - 1)
                    {
                        ecb.DestroyEntity(entity);
                        return;
                    }

                    targetIndex.Value = currentIndex + 1;
                    targetPos = route[targetIndex.Value].Position;
                    direction = targetPos - currentPos;
                }

                direction = math.normalize(new float3(direction.x, 0, direction.z));
                transform.Rotation = quaternion.LookRotationSafe(direction, math.up());
                float3 forward = math.forward(transform.Rotation);
                transform.Position += deltaTime * moveSpeed.Value * forward;

            }).Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}
