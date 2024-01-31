using UnityEngine;

public class BombManager : Singleton<BombManager>
{
    [SerializeField] private GameObject bombPrefab = default;

    [SerializeField] private float bombVelocityX = 20f;
    [SerializeField] private float bombVelocityY = 20f;

    private float nextBombTime;

    public void StartBomb(bool facingRight)
	{
        if (Time.time >= nextBombTime)
        {
            nextBombTime = Time.time + Bomb.MaxLifetime;

            Transform spawnPoint;
            float velocityX;

            if (facingRight)
            {
                spawnPoint = AttackManager.Instance.attackPointRight.transform;
                velocityX = bombVelocityX;
            }
            else
            {
                spawnPoint = AttackManager.Instance.attackPointLeft.transform;
                velocityX = -bombVelocityX;
            }

            GameObject spawnedBomb = Instantiate(bombPrefab, spawnPoint);
            spawnedBomb.GetComponent<Rigidbody2D>().velocity = new Vector2(velocityX, bombVelocityY);
        }
    }
}
