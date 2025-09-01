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

        Entities.ForEach((ref LocalTransform transform, in MoveSpeed moveSpeed, in RotationSpeed rotationSpeed) =>
        {
            float3 forward = math.forward(transform.Rotation);
            transform.Position += deltaTime * moveSpeed.Value * forward;

            quaternion rotationChange = quaternion.AxisAngle(math.up(), math.radians(rotationSpeed.Value) * deltaTime);
            transform.Rotation = math.mul(transform.Rotation, rotationChange);

        // Schedule the job to run in parallel across multiple threads for better performance.
        }).ScheduleParallel();
    }

}

