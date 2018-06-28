using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Vuforia;
using UnityEngine.UI;
using System.IO;


public class FlowController : MonoBehaviour {
	public GameObject instructionScreen;
	public GameObject mainMenu;
	public GameObject infoMenu;
	public GameObject settingsMenu;
	public GameObject aboutMenu;
	public GameObject collectionMenu;
	public GameObject photoMenu;
	public GameObject logoMain;
	public Button fixButton;
	public Button flashButton;
	public GameObject flashImage;
	public UnityEngine.UI.Image settingsExtended;
	public UnityEngine.UI.Image settingsFlash;
	public UnityEngine.UI.Image settingsAutofocus;
	bool takeScreenshot = false;
	int screenshotCount = 0;
	public Sprite settingsON;
	public Sprite settingsOFF;
	private static string persistentDataPath = null;
	ImageTarget trackedTarget;
	bool extendedTracking = false;
	bool flashLight = false;
	bool autoFocus = false;
	bool extendedStatus = false;
	public Sprite fixON;
	public Sprite fixOFF;
	public Sprite flashON;
	public Sprite flashOFF;
	private float holdTime = 0.1f; //or whatever
	private float acumTime = 0;
    int counter = 0;
	public AudioClip impact;
	public AudioClip zatvor;
	AudioSource audioSource;

	public void PlaySound()
	{
		audioSource.PlayOneShot(impact, 1F);
	}

	public void PlayZatvor(){
		audioSource.PlayOneShot(zatvor, 1F);
	}

	void Update()
	{
		if (!autoFocus) {
            if (Input.touchCount == 1) {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    Debug.Log("AutoFocus" + counter++.ToString());

                    CameraDevice.Instance.SetFocusMode(
                        CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
                }
			}
		}
	}

	//FLASH

	 
	// Use this for initialization
	void Start () {
        VuforiaARController vuforia = VuforiaARController.Instance;
        vuforia.RegisterVuforiaStartedCallback(OnVuforiaStarted);
		audioSource = GetComponent<AudioSource>();
		if (persistentDataPath == null)
			persistentDataPath = Application.persistentDataPath;        
		Debug.Log("Data Path =  " + persistentDataPath); // If you want to easily see where that is.

//		var vuforia = VuforiaARController.Instance;
//		vuforia.RegisterVuforiaStartedCallback(OnVuforiaStarted);
//		vuforia.RegisterOnPauseCallback(OnPaused);

		if (!PlayerPrefs.HasKey ("firstLoad")) {
			instructionScreen.SetActive (true);
			PlayerPrefs.SetInt ("firstLoad", 1);
		} else {
			mainMenu.SetActive (true);

		}
	}

    private void OnVuforiaStarted()
    {
        CameraDevice.Instance.SetFocusMode(
            CameraDevice.FocusMode.FOCUS_MODE_NORMAL);
    }

	//Instruction Screen
	public void InstructionClicked() {
		instructionScreen.SetActive (false);
		mainMenu.SetActive (true);
	}


	//Main Screen
	public void MainScanClicked() { 
		mainMenu.SetActive (false);
		photoMenu.SetActive (true);
		logoMain.SetActive (true);
	}

	public void MainInfoClicked() {
		infoMenu.SetActive (true);
	}

	//Info Screen
	public void InfoInstructionClicked() {
		instructionScreen.SetActive (true);
	}

	public void InfoCollectionClicked() {
		collectionMenu.SetActive (true);
	}

	public void InfoAboutClicked() {
		aboutMenu.SetActive (true);
	}

	public void InfoCloseClicked() {
		infoMenu.SetActive (false);
	}

	//Collection
	public void CollectionCloseClicked() {
		collectionMenu.SetActive (false);
	}


	//About
	public void AboutCloseClicked() {
		aboutMenu.SetActive (false);
	}

	public void AboutFacebookLinkClicked() {
		Application.OpenURL("http://fb.me/MagicMagnet3D");
	}


	//Photo

	public void PhotoScreenshotClicked() {
		//MakeScreenShot

		StartCoroutine(ScreenshotEncode());
	}
		


	IEnumerator ScreenshotEncode()
	{
		photoMenu.SetActive (false);
		// wait for graphics to render
		yield return new WaitForEndOfFrame();

		// create a texture to pass to encoding
		Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

		// put buffer into texture
		texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		texture.Apply();

		// split the process up--ReadPixels() and the GetPixels() call inside of the encoder are both pretty heavy
		yield return 0;

		byte[] bytes = texture.EncodeToPNG();
		string date = System.DateTime.Now.ToString();
		date = date.Replace("/","-");
		date = date.Replace(" ","_");
		date = date.Replace(":","-");

		// save our test image (could also upload to WWW)

		string filePath = "testscreen-" + date + ".png";
		NativeGallery.SaveImageToGallery( bytes, "Screenshots", filePath );
		yield return new WaitForSeconds(.5f);
		flashImage.SetActive (true);
		yield return new WaitForSeconds(.1f);
		flashImage.SetActive(false);
		photoMenu.SetActive(true);


		// Added by Karl. - Tell unity to delete the texture, by default it seems to keep hold of it and memory crashes will occur after too many screenshots.
		DestroyObject( texture );

		//Debug.Log( Application.dataPath + "/../testscreen-" + count + ".png" );
	}



	public void PhotoSettingsClicked() {
		settingsMenu.SetActive (true);
	}

	public void PhotoFlashClicked() {
		//turn on/off flash
		if (flashLight == false) {
			settingsFlash.sprite = settingsON;
			flashButton.image.sprite = flashON;
			flashLight = true;
			CameraDevice.Instance.SetFlashTorchMode (true);
		} else {
			settingsFlash.sprite = settingsOFF;
			flashButton.image.sprite = flashOFF;
			flashLight = false;
			CameraDevice.Instance.SetFlashTorchMode (false);
		}
	}

	public void PhotoFixClicked() {
		//turn on/off extended
		
		if (!extendedTracking) {
			extendedTracking = true;
			settingsExtended.sprite = settingsON;
			fixButton.image.sprite = fixON;
		
		} else {
			extendedTracking = false;
			settingsExtended.sprite = settingsOFF;
			fixButton.image.sprite = fixOFF;
		}
		FindAllTargets();
//		Another approach
// 		ImageTargetBehaviour mTrackableBehaviour = GetComponent<ImageTargetBehaviour>();
//
//		mTrackableBehaviour.ImageTarget.StartExtendedTracking();
	}

	//Settings 

	public void SettingsCloseClicked() {
		settingsMenu.SetActive (false);
	}

	public void SettingsHomeClicked() {
		settingsMenu.SetActive (false);
		photoMenu.SetActive (false);
		logoMain.SetActive (false);
		mainMenu.SetActive (true);
	}
	public void SettingsFixClicked() {

		if (!extendedTracking) {
			extendedStatus = true;
		} else {
			extendedStatus = false;
		}

		if (!extendedTracking) {
			settingsExtended.sprite = settingsON;
			fixButton.image.sprite = fixON;
			extendedTracking = true;
		} else {
			fixButton.image.sprite = fixOFF;
			settingsExtended.sprite = settingsOFF;
			extendedTracking = false;
		}
	}

	public void SettingsFlashClicked() {
		if (!flashLight) {
			settingsFlash.sprite = settingsON;
			flashButton.image.sprite = flashON;
			CameraDevice.Instance.SetFlashTorchMode (true);
			flashLight = true;
		} else {
			settingsFlash.sprite = settingsOFF;
			flashButton.image.sprite = flashOFF;
			CameraDevice.Instance.SetFlashTorchMode (false);
			flashLight = false;
		}
	}

	public void SettingsAutofocusClicked() {
		if (!autoFocus) {
			CameraDevice.Instance.SetFocusMode(
				CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
			settingsAutofocus.sprite = settingsON;
			autoFocus = true;
		} else {
			CameraDevice.Instance.SetFocusMode(
				CameraDevice.FocusMode.FOCUS_MODE_NORMAL);
			settingsAutofocus.sprite = settingsOFF;
			autoFocus = false;
		} 
	}

	void FindAllTargets() {
		List<ImageTargetBehaviour> es = new List<ImageTargetBehaviour> ();
		es.AddRange(GameObject.FindObjectsOfType<ImageTargetBehaviour> ());
		Debug.Log ("LOL "+es.Count);
		foreach (ImageTargetBehaviour ez in es) {
			if (extendedTracking) {
				ez.ImageTarget.StartExtendedTracking ();
			} else {
				ez.ImageTarget.StopExtendedTracking ();
			}
		}
	}

}
