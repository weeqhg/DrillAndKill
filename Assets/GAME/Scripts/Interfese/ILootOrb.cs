using UnityEngine;

public interface ILootOrb
{
    void Initialize(int value);
    void Launch(Vector3 direction, float force);
}