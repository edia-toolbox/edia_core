using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;
using eDIA;
using UnityEngine.Events;


namespace eDIA {

	/// <summary>
	/// Main menu can be openen in the app with the action 'Menu'
	/// It generates a list of scenes (which are tasks) and the default 'continue' and 'quit' option
	/// </summary>
	public class MainMenu : MonoBehaviour {

	#region Definitions

		/// <summary> A scene menuitem container</summary>
		[System.Serializable]
		public class aScene {	 // class to hold the scene info from the build list
			public string name;
			public string path;
			public Button button;

			public aScene (string _name, string _path) {
				name = _name;
				path = _path;
			}
		}

	#endregion // -------------------------------------------------------------------------------------------------------------------------------
	#region Variables

		List<aScene> scenes = new List<aScene>();
		int currentSceneIndex = 0;

		[Header("Settings")]
		public bool startOpen = false;
		public float menuDistance = 2f;
		[Tooltip("")]
		public int overLayer = 10;
		public bool isDebug = false;

		[Header("Menu Generation")]
		public Transform menuHolder;
		public RectTransform buttonHolder;
		public GameObject buttonPrefab;

		[Header("Events")]
		public UnityEvent OnLoadBegin = new UnityEvent();
		public UnityEvent OnLoadEnd = new UnityEvent();
		
		ScreenFader screenFader = null;
		
		private bool isLoading = false;
		private bool isOpen = false;


	#endregion // -------------------------------------------------------------------------------------------------------------------------------
	#region Starters

		void Awake() {
			EventManager.StartListening("EvMenuPerformed", OnEvMenuPerformed);
		}

		void OnDestroy() {
			EventManager.StopListening("EvMenuPerformed", OnEvMenuPerformed);
		}

		void OnEnable() {
		}

		void Start() {
			screenFader = eDIA.XRrigUtilities.GetXRcam().GetComponent<ScreenFader>();
			GetSceneList();
			GenerateMenu ();
			OpenMenu(startOpen);
		}

	#endregion // -------------------------------------------------------------------------------------------------------------------------------
	#region MENU GENERATION

		void GetSceneList () {
			int sceneCount = SceneManager.sceneCountInBuildSettings;
			
			for( int i = 0; i < sceneCount; i++ )
			{
				string aPath = SceneUtility.GetScenePathByBuildIndex(i);
				string aName = aPath.Split('.')[0].Split('/').Last();
				scenes.Add(new aScene(aName,aPath));

				// Init the correct currentscene index
				if (aName == SceneManager.GetActiveScene().name)
					currentSceneIndex = i;
			}
		}

		GameObject GenerateButtonUI () {
			GameObject newButton = Instantiate(buttonPrefab);
			newButton.layer = overLayer;
			newButton.GetComponent<RectTransform>().SetParent(buttonHolder);
			newButton.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
			newButton.GetComponent<RectTransform>().localPosition = new Vector3(0,0,0);
			newButton.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0,0,0);

			return newButton;
		}

		void GenerateMenu () {
			for( int i = 0; i < scenes.Count; i++ )
			{
				GameObject newButton = GenerateButtonUI();
				newButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = scenes[i].name;
				scenes[i].button = newButton.GetComponent<Button>();
				var newIndex = i;
				newButton.GetComponent<Button>().onClick.AddListener( () => { LoadNewScene(newIndex); });
			}

			GameObject closeButton = GenerateButtonUI();
			closeButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Close Menu";
			closeButton.GetComponent<Button>().onClick.AddListener( () => { OpenMenu(false); });

			if (!isDebug)
				return;

			GameObject startSessionButton = GenerateButtonUI();
			startSessionButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Start Session";
			startSessionButton.GetComponent<Button>().onClick.AddListener( () => { EventManager.TriggerEvent("EvStartExperiment", null); OpenMenu(false); });


		}

	#endregion // -------------------------------------------------------------------------------------------------------------------------------
	#region MENU HANDLERS

		void OnEvMenuPerformed (eParam e) {
			OpenMenu(!isOpen);
		}

		public void OpenMenu(bool onOff) {

			isOpen = onOff;
			menuHolder.gameObject.SetActive(onOff);

			if (!onOff)
				return;

			// Disable current scene button
			for (int s=0; s<scenes.Count; s++) {
				scenes[s].button.interactable = scenes[s].name == scenes[currentSceneIndex].name ? false : true;
			}
			
			SetMenuPosition();
		}


	#endregion // -------------------------------------------------------------------------------------------------------------------------------
	#region SCENE LOADING

		private void LoadNewScene(int sceneIndex) {
			if (!isLoading)
				StartCoroutine(LoadScene(sceneIndex));
		}

		private IEnumerator LoadScene(int sceneIndex) {
			SystemManager.instance.AddToLog("Loading scene: " + scenes[sceneIndex].name);
			OpenMenu(false);

			isLoading = true;

			OnLoadBegin?.Invoke();
			yield return screenFader.StartFadeIn();

			yield return new WaitForSeconds(0.5f);

			yield return StartCoroutine(LoadNew(sceneIndex));
			yield return screenFader.StartFadeOut();

			OnLoadEnd?.Invoke();

			isLoading = false;
			currentSceneIndex = sceneIndex;

			yield return null;
		}

		private IEnumerator LoadNew(int sceneIndex) {

			AsyncOperation loadOperation = SceneManager.LoadSceneAsync(scenes[sceneIndex].name, LoadSceneMode.Single);

			while (!loadOperation.isDone)
				yield return null;
		}

	#endregion // -------------------------------------------------------------------------------------------------------------------------------
	#region helpers

		void SetMenuPosition () {
			// TODO how to put it in front off the user always, part of the global XR rig maybe?

			menuHolder.transform.localPosition = new Vector3(0, eDIA.XRrigUtilities.GetXRcam().position.y, menuDistance);
		}

	#endregion // -------------------------------------------------------------------------------------------------------------------------------
	}
}