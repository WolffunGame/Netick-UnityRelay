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
			_uiProgress.SetVisible(false);
		}

		// What mode to play - Called from the start menu
		public void OnHostOptions()
		{
			if (GateUI(_uiStart))
				_joinCode.SetVisible(true);
		}
		public void OnJoinOptions() => HostRoom();

		private void HostRoom()
		{
			if (GateUI(_uiStart))
				_uiProgress.SetVisible(true);
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
	}
}