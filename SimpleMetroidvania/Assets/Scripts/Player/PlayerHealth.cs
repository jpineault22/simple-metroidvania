using UnityEngine;

public class PlayerHealth : Singleton<PlayerHealth>
{
    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }

    [SerializeField] private int baseHP = 3;

	protected override void Awake()
	{
		base.Awake();

		MaxHP = CurrentHP = baseHP;
		PlayerHealthUI.Instance.SetHUBActive(true);
		PlayerHealthUI.Instance.UpdateHearts(MaxHP, CurrentHP);
	}

	public void LoadHP(int pCurrentHP, int pMaxHP)
	{
		CurrentHP = pCurrentHP;
		MaxHP = pMaxHP;
		PlayerHealthUI.Instance.UpdateHearts(MaxHP, CurrentHP);
	}

	public void IncrementHearts()
	{
		MaxHP++;
		CurrentHP = MaxHP;
		PlayerHealthUI.Instance.UpdateHearts(MaxHP, CurrentHP);
	}

	public void TakeDamage(int pDamage)
	{
		CurrentHP -= pDamage;
		PlayerHealthUI.Instance.UpdateHearts(MaxHP, CurrentHP);

		if (CurrentHP <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		Debug.Log("PLAYER DIES.");
	}

	protected override void OnDestroy()
	{
		if (PlayerHealthUI.IsInitialized)
		{
			PlayerHealthUI.Instance.SetHUBActive(false);
		}

		base.OnDestroy();
	}
}
