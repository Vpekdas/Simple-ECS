using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public class CarSpawnerBaker : Baker<CarSpawnerAuthoring>
{
    public override void Bake(CarSpawnerAuthoring authoring)
    {
        // The spawner wont be rendered, and no need for his transform, so flag is none.
        Entity entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new CarSpawner
        {
            Prefab = GetEntity(authoring.CarPrefab, TransformUsageFlags.Dynamic),
            CircuitEntity = entity,
            MoveSpeed = authoring.MoveSpeed,
            RotationSpeed = authoring.RotationSpeed
        });

        // UnityEngine.Color cannot be stored in ECS data. We convert each color to float4 (RGBA).
        DynamicBuffer<ColorBufferElement> colorsBuffer = AddBuffer<ColorBufferElement>(entity);
        foreach (Color color in authoring.ColorPicker)
        {
            colorsBuffer.Add(new ColorBufferElement
            {
                Value = new float4(color.r, color.g, color.b, color.a)
            });
        }

        DynamicBuffer<CircuitPoint> buffer = AddBuffer<CircuitPoint>(entity);
        if (authoring.CircuitParent != null)
        {
            int childCount = authoring.CircuitParent.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = authoring.CircuitParent.transform.GetChild(i);
                buffer.Add(new CircuitPoint { Position = child.position });
            }
        }
    }
}
