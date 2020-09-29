using UnityEngine;

// Split this class into enums when possible (maybe tags, animation parameters)
public static class Constants
{
	public const int StartingMapNumber = 1;

	// Game object/Scene names and prefixes
	public const string NamePrefixSceneMap = "Map_";
	public const string NamePrefixMapExit = "MapExit_";
	public const string NameSceneStartMenu = "StartMenu";
	public const string NameGameObjectLevelLoader = "LevelLoader";
	public const string NameGameObjectMapBoundaries = "MapBoundaries";

	// Tags
	public const string TagPlayer = "Player";
	public const string TagMapBoundaries = "MapBoundaries";
	public const string TagFunctionalMap = "FunctionalMap";
	public const string TagSpawnPoints = "SpawnPoints";
	public const string TagFirstPlayerSpawnPoint = "FirstPlayerSpawnPoint";
	public const string TagDashPowerupSpawnPoint = "DashPowerupSpawnPoint";
	public const string TagWallJumpPowerupSpawnPoint = "WallJumpPowerupSpawnPoint";
	public const string TagBombPowerupSpawnPoint = "BombPowerupSpawnPoint";
	public const string TagHeartSpawnPoint = "HeartSpawnPoint";
	public const string TagEnemyPlaceholderSpawnPoint = "Enemy-placeholderSpawnPoint";
	public const string TagDashPowerup = "DashPowerup";
	public const string TagWallJumpPowerup = "WallJumpPowerup";
	public const string TagBombPowerup = "BombPowerup";
	public const string TagHeart = "Heart";
	public const string TagEnemy = "Enemy";     // Consider using a layer instead

	// Layers
	public const string LayerPlayer = "Player";
	public const string LayerGround = "Ground";
	public const string LayerInteractibleObject = "InteractibleObject";
	public const string LayerFunctionalEnemy = "FunctionalEnemy";

	// Animator parameters
	public const string AnimatorPlayerAttack = "Attack";
	public const string AnimatorPlayerAttackUp = "AttackUp";
	public const string AnimatorPlayerAttackDown = "AttackDown";
	public const string AnimatorCrossfadeStart = "CrossfadeStart";
	public const string AnimatorCrossfadeEnd = "CrossfadeEnd";
	public const string AnimatorDialogueIsOpen = "IsOpen";

	// Audio mixer parameters
	public const string AudioMasterVolume = "MasterVolume";

	// These variables determine the angle thresholds (in radians) for the dash direction to change (2/16 of a full circle for each direction). They correspond to:
	public const float AngleConstantFirst = Mathf.PI / 8;           // A quarter of the first quadrant
	public const float AngleConstantSecond = 3 * Mathf.PI / 8;      // 3 quarters of the first quadrant
	public const float AngleConstantThird = 5 * Mathf.PI / 8;       // The first quadrant plus a quarter of the second
	public const float AngleConstantFourth = 7 * Mathf.PI / 8;      // The first quadrant plus 3 quarters of the second
	// Same thing for attack direction
	public const float AngleConstant45 = Mathf.PI / 4;              // 45 degrees
	public const float AngleConstant135 = 3 * Mathf.PI / 4;         // 135 degrees
}
