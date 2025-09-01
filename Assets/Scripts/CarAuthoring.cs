using UnityEngine;
using Unity.Entities;

public class CarAuthoring : MonoBehaviour
{
    public float MoveSpeed;
    public float RotationSpeed;

    class Baker : Baker<CarAuthoring>
    {
        public override void Bake(CarAuthoring authoring)
        {
            // This removes the obsolete warning and ensures transform components are added during baking.
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MoveSpeed { Value = authoring.MoveSpeed });
            AddComponent(entity, new RotationSpeed { Value = authoring.RotationSpeed });
        }
    }
}
