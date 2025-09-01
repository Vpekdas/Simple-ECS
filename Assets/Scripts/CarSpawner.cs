using Unity.Entities;
public struct CarSpawner : IComponentData
{
    public Entity Prefab;
    public float MoveSpeed;
    public float RotationSpeed;
}
