using UnityEngine;

public static class SystemGet
{
    private static readonly LayerMask groundLayer = LayerMask.GetMask("Ground");
    public static Vector3 GetGroundPosition(Vector3 basePos, float radius = 1f)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * radius;
            Vector3 randomPos = basePos + new Vector3(randomOffset.x, 200f, randomOffset.y);

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 500f, groundLayer))
            {
                Vector3 spawnPos = hit.point;

                if (Physics.CheckCapsule(spawnPos, spawnPos + Vector3.up * 100f, 0.5f, ~groundLayer))
                    continue;

                return spawnPos;
            }
        }
        return basePos;
    }
}
