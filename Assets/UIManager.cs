using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	[InspectorLabel("Pause Menu")]
	[SerializeField] private GameObject pauseMenu;
	public bool isPauseMenuOpen { get; private set; }
	
	//Singleton
	public static UIManager Instance;

	//Initialize the singleton
	public void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		else
		{
			Instance = this;
		}
	}

	public void Start()
	{
		pauseMenu.SetActive(false);
	}
	
	public void PauseMenu(bool openState)
	{
		pauseMenu.SetActive(openState);
	}

}
