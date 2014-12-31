using UnityEngine;
using System.Collections;

public class SplashControl: MonoBehaviour {
	public Texture2D unityLogo;
	public Texture2D photonLogo;
	public Texture2D ourLogo;
	public Texture2D background;
	float unityWidth=577f;
	float unityHeight=240f;
	float photonWidth=1466f;
	float photonHeight=471f;
	
	// Use this for initialization
	void Start () {
		Invoke("nextScene",3f);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown){
			nextScene();	
		}
	}
	void nextScene(){
		Application.LoadLevel("StartScreen");	
	}
	void OnGUI(){
		GUI.DrawTexture(new Rect(0f,0f,Screen.width,Screen.height),background);
		GUI.DrawTexture(new Rect(Screen.width/2-unityWidth/3,50f,unityWidth*2/3,unityHeight*2/3),unityLogo);
		GUI.DrawTexture(new Rect(Screen.width/2-photonWidth/6,100f+unityHeight*2/3,photonWidth*1/3,photonHeight*1/3),photonLogo);
	}
}
