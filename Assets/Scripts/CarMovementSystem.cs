using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public partial class CarMovementSystem : SystemBase
{
    [BurstCompile]
    // Entities.ForEach will be deprecated, so docs advise to use IJobEntity.
    partial struct CarMovementJob : IJobEntity
    {
        public float DeltaTime;
        [ReadOnly] public BufferLookup<CircuitPoint> CircuitPointLookup;
        public EntityCommandBuffer.ParallelWriter ECB;

        void Execute(Entity entity, [EntityIndexInQuery] int entityInQueryIndex,
            ref LocalTransform transform,
            ref CurrentTargetIndex targetIndex,
            in MoveSpeed moveSpeed,
            in RouteReference routeRef)
        {
            if (!CircuitPointLookup.HasBuffer(routeRef.CircuitEntity))
            {
                return;
            }

            DynamicBuffer<CircuitPoint> route = CircuitPointLookup[routeRef.CircuitEntity];
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
                if (currentIndex >= route.Length - 1)
                {
                    ECB.DestroyEntity(entityInQueryIndex, entity);
                    return;
                }

                targetIndex.Value = currentIndex + 1;
                targetPos = route[targetIndex.Value].Position;
                direction = targetPos - currentPos;
            }

            direction = math.normalize(new float3(direction.x, 0, direction.z));
            transform.Rotation = quaternion.LookRotationSafe(direction, math.up());
            float3 forward = math.forward(transform.Rotation);
            transform.Position += DeltaTime * moveSpeed.Value * forward;
        }
    }

    protected override void OnUpdate()
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        BufferLookup<CircuitPoint> circuitPointLookup = SystemAPI.GetBufferLookup<CircuitPoint>(true);

        EndSimulationEntityCommandBufferSystem ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        EntityCommandBuffer.ParallelWriter ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        CarMovementJob job = new()
        {
            DeltaTime = deltaTime,
            CircuitPointLookup = circuitPointLookup,
            ECB = ecb
        };

        Dependency = job.ScheduleParallel(Dependency);
        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}