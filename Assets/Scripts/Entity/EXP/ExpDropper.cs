using UnityEngine;

public class ExpDropper : MonoBehaviour
{
    [SerializeField] private int minExp = 5;
    [SerializeField] private int maxExp = 15;
    [SerializeField] private int orbCount = 3;
    [SerializeField] private Transform spawnPoint;

    public void DropExp()
    {
        int totalExp = Random.Range(minExp, maxExp + 1);
        int expPerOrb = Mathf.Max(1, totalExp / orbCount);

        for (int i = 0; i < orbCount; i++)
        {
            Vector3 spawnPos = spawnPoint.position + Vector3.up * 3f;

            GameObject orb = PoolManager.Instance.Get(PoolId.ExpOrb, spawnPos);

            if (orb != null)
            {
                var exp = orb.GetComponent<ExpOrb>();
                if (exp != null)
                {
                    exp.Initialize(expPerOrb);

                    Vector3 horizontal = Random.insideUnitCircle.normalized;

                    Vector3 dir = new Vector3(
                        horizontal.x,
                        Random.Range(0.7f, 1f),
                        horizontal.y
                    );

                    float force = Random.Range(1f, 3f);

                    exp.Launch(dir, force);
                }
            }
        }
    }
}