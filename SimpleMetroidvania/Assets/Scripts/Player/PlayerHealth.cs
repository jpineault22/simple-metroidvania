using UnityEngine;

// This class has not been fully implemented yet
public class PlayerHealth : Singleton<PlayerHealth>
{
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }

    [SerializeField] private int baseHP = 3;
}
