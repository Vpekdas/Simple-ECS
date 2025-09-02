# Approach

Once I read the documentation and understood the architecture of ECS and DOTS, I decided to organize myself as follows:
  - Since the idea is to use Entities instead of GameObjects, the first step was to convert GameObjects into Entities. One way to do this is by baking with a SubScene. It took me some time to realize that my baker scripts won’t be called unless the GameObject is placed inside a SubScene.
  - After that realization, I focused on organizing my scripts. I followed a similar philosophy to OOP: each script should do one thing.

I ended up with three kinds of scripts:
### Data Components
These hold data, such as MoveSpeed.cs or ColorBufferElement.cs.
### Baker Scripts
These convert GameObjects to Entities and add components during baking, such as CarSpawnerBaker.cs.
### Systems
These run the logic, like CarMovementSystem.cs or CarSpawnerSystem.cs. They handle the actual work via ECS Jobs.

Example of ScheduleParallel Jobs:

```cs
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
```

# Thoughts

I had a lot of fun working on this project and trying to follow the ECS and DOTS architecture rules. I’ve never learned so much from a single project.
Now, when starting a new project, I can ask myself whether OOP or ECS architecture is the better fit for the use case.
