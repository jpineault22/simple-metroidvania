using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : Singleton<PlayerHealth>
{
    // Consider moving the HP fields closer to the Player (ex: script component in Player game object)
	public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }

    [SerializeField] private int baseHP = 3;

	[SerializeField] private GameObject healthPanel = default;
    [SerializeField] private Sprite fullHeartSprite = default;
    [SerializeField] private Sprite emptyHeartSprite = default;

    private Image[] hearts;

	protected override void Awake()
	{
		base.Awake();

		MaxHP = CurrentHP = baseHP;
		hearts = healthPanel.GetComponentsInChildren<Image>();
	}

	public void SetHUBActive(bool pActive)
	{
		healthPanel.SetActive(pActive);
	}

	public void LoadHP(int pCurrentHP, int pMaxHP)
	{
		CurrentHP = pCurrentHP;
		MaxHP = pMaxHP;
		UpdateHearts();
	}

	public void IncrementHearts()
	{
		MaxHP++;
		CurrentHP = MaxHP;
		UpdateHearts();
	}

	public void TakeDamage(int pDamage)
	{
		CurrentHP -= pDamage;
		UpdateHearts();

		if (CurrentHP <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		//Debug.Log("PLAYER DIES.");
	}

    private void UpdateHearts()
	{
		for (int i = 0; i < hearts.Length; i++)
		{
			hearts[i].sprite = fullHeartSprite;
			hearts[i].enabled = true;

			if (i >= MaxHP)
			{
				hearts[i].enabled = false;
			}
			else if (i >= CurrentHP)
			{
				hearts[i].sprite = emptyHeartSprite;
			}
		}
	}
}
