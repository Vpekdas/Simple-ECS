using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

[BurstCompile]
public partial class CarMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        BufferLookup<CircuitPoint> circuitPointLookup = GetBufferLookup<CircuitPoint>(true);

        // Since i'm running on multiple threads, we need to use a command buffer that fits with multithreading.
        EndSimulationEntityCommandBufferSystem ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        EntityCommandBuffer.ParallelWriter ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        JobHandle jobHandle = Entities
            .WithReadOnly(circuitPointLookup)
            .ForEach((Entity entity, int entityInQueryIndex, ref LocalTransform transform,
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
                        ecb.DestroyEntity(entityInQueryIndex, entity);
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

            }).ScheduleParallel(Dependency);

        // Register the job with the ECB system to ensure playback occurs after the job completes.
        ecbSystem.AddJobHandleForProducer(jobHandle);

        // Update the system's dependency to include our scheduled job.
        Dependency = jobHandle;
    }
}