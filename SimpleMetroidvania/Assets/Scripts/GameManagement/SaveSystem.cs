using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
	private static string savePath = Application.persistentDataPath + "/player-data.dza";

	public static void SavePlayerData(PlayerData pData)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(savePath, FileMode.Create);

		formatter.Serialize(stream, pData);
		stream.Close();
	}

	public static PlayerData LoadPlayerData()
	{
		if (File.Exists(savePath))
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(savePath, FileMode.Open);

			PlayerData data = (PlayerData) formatter.Deserialize(stream);
			stream.Close();

			return data;
		}
		else
		{
			Debug.Log("[SaveSystem] Save file not found in " + savePath + ". Starting new game." );
			return null;
		}
	}
}
