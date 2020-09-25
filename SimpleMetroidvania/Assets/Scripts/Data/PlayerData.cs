[System.Serializable]
public class PlayerData
{
    public float SaveSpotPositionX { get; set; }
    public float SaveSpotPositionY { get; set; }
    public int SaveSpotMapNumber { get; set; }
    public bool HasDash { get; set; }
    public bool HasWallJump { get; set; }
    public bool HasBomb { get; set; }
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }

    public PlayerData(float pSaveSpotPositionX, float pSaveSpotPositionY, int pSaveSpotMapNumber, bool pHasDash, bool pHasWallJump, bool pHasBomb, int pCurrentHP, int pMaxHP)
	{
        SaveSpotPositionX = pSaveSpotPositionX;
        SaveSpotPositionY = pSaveSpotPositionY;
        SaveSpotMapNumber = pSaveSpotMapNumber;
        HasDash = pHasDash;
        HasWallJump = pHasWallJump;
        HasBomb = pHasBomb;
        CurrentHP = pCurrentHP;
        MaxHP = pMaxHP;
	}
}
