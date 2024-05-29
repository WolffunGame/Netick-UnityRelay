using FusionExamples.UIHelpers;
using TMPro;
using UnityEngine;

namespace Examples.Tank
{
	/// <summary>
	/// App entry point and main UI flow management.
	/// </summary>
	public class App : MonoBehaviour
	{
		[SerializeField] private TMP_InputField _room;
		[SerializeField] private TextMeshProUGUI _progress;
		[SerializeField] private Panel _uiCurtain;
		[SerializeField] private Panel _uiStart;
		[SerializeField] private Panel _uiProgress;
		[SerializeField] private Panel _uiRoom;
		[SerializeField] private Panel _joinCode;
		[SerializeField] private GameObject _uiGame;
		private int _nextPlayerIndex;

		private void Awake() => DontDestroyOnLoad(this);

		private void Start()
		{
			_uiStart.SetVisible(true);
			_uiCurtain.SetVisible(true);
		}

		private void Update()
		{
			if (!Input.GetKeyDown(KeyCode.Escape)) return;
			_uiStart.SetVisible(true);
			_uiRoom.SetVisible(false);
			// if (_uiProgress.isShowing)
			// {
			// 	// if (Input.GetKeyUp(KeyCode.Escape))
			// 	// {
			// 	// 	NetworkRunner runner = FindObjectOfType<NetworkRunner>();
			// 	// 	if (runner != null && !runner.IsShutdown)
			// 	// 	{
			// 	// 		// Calling with destroyGameObject false because we do this in the OnShutdown callback on FusionLauncher
			// 	// 		runner.Shutdown(false);
			// 	// 	}
			// 	// }
			// 	UpdateUI();
			// }
		}

		// What mode to play - Called from the start menu
		public void OnHostOptions()
		{
			if (GateUI(_uiStart))
				_joinCode.SetVisible(true);
		}
		public void OnJoinOptions() => SetGameMode();

		private void SetGameMode()
		{
			if (GateUI(_uiStart))
				_uiRoom.SetVisible(true);
		}

		public void OnEnterRoom()
		{
			// if (GateUI(_uiRoom))
			// {
			// 	FusionLauncher.Launch(_gameMode, _room.text, _gameManagerPrefab, _levelManager, OnConnectionStatusUpdate);
			// }
		}

		/// <summary>
		/// Call this method from button events to close the current UI panel and check the return value to decide
		/// if it's ok to proceed with handling the button events. Prevents double-actions and makes sure UI panels are closed. 
		/// </summary>
		/// <param name="ui">Currently visible UI that should be closed</param>
		/// <returns>True if UI is in fact visible and action should proceed</returns>
		private static bool GateUI(Panel ui)
		{
			if (!ui.isShowing)
				return false;
			ui.SetVisible(false);
			return true;
		}

		// private void OnConnectionStatusUpdate(NetworkRunner runner, FusionLauncher.ConnectionStatus status, string reason)
		// {
		// 	if (!this)
		// 		return;
		//
		// 	Debug.Log(status);
		//
		// 	if (status != _status)
		// 	{
		// 		switch (status)
		// 		{
		// 			case FusionLauncher.ConnectionStatus.Disconnected:
		// 				ErrorBox.Show("Disconnected!", reason, () => { });
		// 				break;
		// 			case FusionLauncher.ConnectionStatus.Failed:
		// 				ErrorBox.Show("Error!", reason, () => { });
		// 				break;
		// 		}
		// 	}
		//
		// 	_status = status;
		// 	UpdateUI();
		// }

		private void UpdateUI()
		{
			bool intro = false;
			bool progress = true;
			bool running = false;
			intro = true;
			// switch (_status)
			// {
			// 	case FusionLauncher.ConnectionStatus.Disconnected:
			// 		_progress.text = "Disconnected!";
			// 		intro = true;
			// 		break;
			// 	case FusionLauncher.ConnectionStatus.Failed:
			// 		_progress.text = "Failed!";
			// 		intro = true;
			// 		break;
			// 	case FusionLauncher.ConnectionStatus.Connecting:
			// 		_progress.text = "Connecting";
			// 		progress = true;
			// 		break;
			// 	case FusionLauncher.ConnectionStatus.Connected:
			// 		_progress.text = "Connected";
			// 		progress = true;
			// 		break;
			// 	case FusionLauncher.ConnectionStatus.Loading:
			// 		_progress.text = "Loading";
			// 		progress = true;
			// 		break;
			// 	case FusionLauncher.ConnectionStatus.Loaded:
			// 		running = true;
			// 		break;
			// }

			_uiCurtain.SetVisible(!running);
			_uiStart.SetVisible(intro);
			_uiProgress.SetVisible(progress);
			_uiGame.SetActive(running);
			
			if(intro)
				MusicPlayer.instance.SetLowPassTranstionDirection( -1f);
		}
	}
}