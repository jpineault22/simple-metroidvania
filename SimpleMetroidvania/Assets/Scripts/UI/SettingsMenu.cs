using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
	[SerializeField] private TMP_Dropdown resolutionDropDown;
	[SerializeField] private AudioMixer mainMixer;

	private Resolution[] resolutions;

	private void Start()
	{
		// Initialize resolutions dropdown
		resolutions = Screen.resolutions;
		resolutionDropDown.ClearOptions();

		List<string> resolutionOptions = new List<string>();
		int currentResolutionIndex = 0;

		for (int i = 0; i < resolutions.Length; i++)
		{
			string option = resolutions[i].width + " x " + resolutions[i].height;
			resolutionOptions.Add(option);

			if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
			{
				currentResolutionIndex = i;
			}
		}

		resolutionDropDown.AddOptions(resolutionOptions);
		resolutionDropDown.value = currentResolutionIndex;
		resolutionDropDown.RefreshShownValue();
	}

	public void SetResolution(int pResolutionIndex)
	{
		Resolution resolution = resolutions[pResolutionIndex];
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
	}

	public void SetFullScreen(bool pIsFullScreen)
	{
		Screen.fullScreen = pIsFullScreen;
	}

    public void SetQuality(int pQualityIndex)
	{
		QualitySettings.SetQualityLevel(pQualityIndex);
	}
	
	public void SetVolume(float pVolume)
	{
		mainMixer.SetFloat(Constants.AudioMasterVolume, pVolume);
	}
}
