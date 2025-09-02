using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;

[BurstCompile]
public partial class CarSpawnerSystem : SystemBase
{
    private EntityQuery _spawnerQuery;
    private BeginSimulationEntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        RequireForUpdate<CarSpawner>();
        _ecbSystem = World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    partial struct CarSpawnerJob : IJobEntity
    {
        public float DeltaTime;
        public uint RandomSeed;
        public EntityCommandBuffer.ParallelWriter ECB;

        [ReadOnly] public BufferLookup<CircuitPoint> CircuitLookup;
        [ReadOnly] public BufferLookup<ColorBufferElement> ColorLookup;

        void Execute([EntityIndexInQuery] int entityInQueryIndex, Entity entity,
                     ref CarSpawner spawner)
        {
            spawner.SpawnTimer += DeltaTime;

            if (spawner.SpawnTimer < spawner.SpawnInterval)
            {
                return;
            }

            spawner.SpawnTimer = 0f;


            Entity car = ECB.Instantiate(entityInQueryIndex, spawner.Prefab);

            DynamicBuffer<ColorBufferElement> colors = ColorLookup[entity];
            Unity.Mathematics.Random random = new(RandomSeed + (uint)entityInQueryIndex);
            int colorIndex = random.NextInt(0, colors.Length);
            float4 color = colors[colorIndex].Value;

            if (!CircuitLookup.HasBuffer(entity))
            {
                return;
            }

            DynamicBuffer<CircuitPoint> circuit = CircuitLookup[entity];
            if (circuit.Length > 0)
            {
                ECB.SetComponent(entityInQueryIndex, car, new LocalTransform
                {
                    Position = circuit[0].Position,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
            }
            ECB.AddComponent(entityInQueryIndex, car, new MoveSpeed { Value = spawner.MoveSpeed });
            ECB.AddComponent(entityInQueryIndex, car, new URPMaterialPropertyBaseColor { Value = color });
            ECB.AddComponent(entityInQueryIndex, car, new CurrentTargetIndex { Value = 0 });
            ECB.AddComponent(entityInQueryIndex, car, new RouteReference { CircuitEntity = spawner.CircuitEntity });
        }
    }

    protected override void OnUpdate()
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        CarSpawnerJob job = new()
        {
            DeltaTime = deltaTime,
            RandomSeed = (uint)System.DateTime.Now.Ticks,
            ECB = _ecbSystem.CreateCommandBuffer().AsParallelWriter(),
            CircuitLookup = SystemAPI.GetBufferLookup<CircuitPoint>(isReadOnly: true),
            ColorLookup = SystemAPI.GetBufferLookup<ColorBufferElement>(),
        };

        Dependency = job.ScheduleParallel(Dependency);
        _ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
