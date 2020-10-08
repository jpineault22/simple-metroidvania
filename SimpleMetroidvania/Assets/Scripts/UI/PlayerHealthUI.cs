using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : Singleton<PlayerHealthUI>
{
    [SerializeField] private GameObject healthPanel = default;
    [SerializeField] private Sprite fullHeartSprite = default;
    [SerializeField] private Sprite emptyHeartSprite = default;

    private Image[] hearts;

	protected override void Awake()
	{
		base.Awake();

		hearts = healthPanel.GetComponentsInChildren<Image>();
	}

	public void SetHUBActive(bool pActive)
	{
		healthPanel.SetActive(pActive);
	}

	public void UpdateHearts(int pMaxHP, int pCurrentHP)
	{
		for (int i = 0; i < hearts.Length; i++)
		{
			hearts[i].sprite = fullHeartSprite;
			hearts[i].enabled = true;

			if (i >= pMaxHP)
			{
				hearts[i].enabled = false;
			}
			else if (i >= pCurrentHP)
			{
				hearts[i].sprite = emptyHeartSprite;
			}
		}
	}
}
