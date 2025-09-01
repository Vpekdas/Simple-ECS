using Unity.Entities;
public struct CarSpawner : IComponentData
{
    public Entity Prefab;
    public Entity CircuitEntity;
    public float MoveSpeed;
    public float RotationSpeed;

}
