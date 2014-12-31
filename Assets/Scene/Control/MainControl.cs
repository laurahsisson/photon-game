using UnityEngine;
 
public class MainControl : Photon.MonoBehaviour  {
	public Object musicPrefab;
	GameObject music;
	public Texture2D background;
    // Use this for initialization //Holds the viewId of the master object's script 
    void Start(){
    }
    void OnGUI(){
		GUI.DrawTexture(new Rect(0f,0f,Screen.width,Screen.height),background);
		if (GUI.Button(new Rect(Screen.width/2-50,Screen.height/2-50f,100f,100f),"Play")){
			Application.LoadLevel("FlagCapture");
			
		}
		if (Input.GetKeyDown(KeyCode.M)){
			if (music){
				Destroy(music);	
			}
			else {
				music = (GameObject) Instantiate(musicPrefab);
				DontDestroyOnLoad(music);
			}
		}
    }
}