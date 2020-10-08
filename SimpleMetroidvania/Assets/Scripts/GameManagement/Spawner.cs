using UnityEngine;

public class Spawner : Singleton<Spawner>
{
    #region Prefabs

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab = default;

    [Header("Powerups and heart")]
    [SerializeField] private GameObject dashPowerupPrefab = default;
    [SerializeField] private GameObject wallJumpPowerupPrefab = default;
    [SerializeField] private GameObject bombPowerupPrefab = default;
    [SerializeField] private GameObject heartPrefab = default;

    [Header("Enemies")]
    [SerializeField] private GameObject enemyPlaceholderPrefab = default;

	#endregion

	public GameObject[] SpawnPoints { get; private set; }
    public GameObject FirstPlayerSpawnPoint { get; private set; }
    public Vector2? PlayerSavedPosition { get; set; }

    public GameObject InstantiatePlayer()
	{
        return Instantiate(playerPrefab);
	}

    public void DestroyPlayer(GameObject pPlayer)
	{
        Destroy(pPlayer);
	}

    public Vector3 GetPlayerInitialPosition()
	{
        return FirstPlayerSpawnPoint.transform.position;
	}

    public Vector2 GetPlayerPosition()
	{
        if (PlayerSavedPosition.HasValue)
        {
            Vector2 playerSavedPosition = new Vector2(PlayerSavedPosition.Value.x, PlayerSavedPosition.Value.y);
            PlayerSavedPosition = null;
            return playerSavedPosition;
        }
        else
        {
            Vector2 firstPlayerSpawnPointPosition = FirstPlayerSpawnPoint.transform.position;
            return new Vector2(firstPlayerSpawnPointPosition.x, firstPlayerSpawnPointPosition.y);
        }
    }

    public void FindSpawnPoints(GameObject[] pGameObjects)
	{
        foreach (GameObject obj in pGameObjects)
		{
            if (obj.CompareTag(Constants.TagSpawnPoints))
			{
                SpawnPoints = GameObjectUtils.GetChildren(obj);
			}
		}

        if (SpawnPoints.Length > 0)
		{
            InstantiatePowerupsAndEnemies();
		}
	}

    private void InstantiatePowerupsAndEnemies()
	{
		foreach (GameObject spawnPoint in SpawnPoints)
		{
            // Retrieve initial player spawn point if in first map
            if (LevelLoader.Instance.CurrentMapName == Constants.NamePrefixSceneMap + Constants.StartingMapNumber
            && spawnPoint.CompareTag(Constants.TagFirstPlayerSpawnPoint))
            {
                FirstPlayerSpawnPoint = spawnPoint;
            }
            // Spawn powerups/hearts
            else if (spawnPoint.CompareTag(Constants.TagDashPowerupSpawnPoint))
            {
                if (!PlayerController.Instance.HasDash)
                {
                    Instantiate(dashPowerupPrefab, spawnPoint.transform);
                }
            }
            else if (spawnPoint.CompareTag(Constants.TagWallJumpPowerupSpawnPoint))
            {
                if (!PlayerController.Instance.HasWallJump)
                {
                    Instantiate(wallJumpPowerupPrefab, spawnPoint.transform);
                }
            }
            else if (spawnPoint.CompareTag(Constants.TagBombPowerupSpawnPoint))
            {
                if (!PlayerController.Instance.HasBomb)
                {
                    Instantiate(bombPowerupPrefab, spawnPoint.transform);
                }
            }
            else if (spawnPoint.CompareTag(Constants.TagHeartSpawnPoint))
            {
                Instantiate(heartPrefab, spawnPoint.transform);
            }
            // Spawn enemies
            else if (spawnPoint.CompareTag(Constants.TagEnemyPlaceholderSpawnPoint))
            {
                Instantiate(enemyPlaceholderPrefab, spawnPoint.transform);
            }
        }
	}
}
