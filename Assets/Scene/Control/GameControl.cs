using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {
	public Object musicPrefab;
	GameObject music;
	public Texture2D background;
	bool connected=false;
	public Camera cameraMain;
	public Camera cameraBall;
	Master master;
	public bool paused=false;
	bool changingColor=false;
	float a,b,c=0f;
	public GameObject colorBall;
	int teamNumber;
	float primaryLimit = .5f;
	float secondaryLimit=.6f;
	// Use this for initialization
	void Start () {
		cameraBall.gameObject.SetActive(false);
		PhotonNetwork.ConnectUsingSettings("Alpha 1.2");
	}
	void OnJoinedLobby(){
    	PhotonNetwork.JoinRandomRoom();
	}
	// Update is called once per frame
	public void pause(){
		master.pause();
		paused=!paused;
		if (changingColor){
			changingColor=false;
			cameraBall.gameObject.SetActive(false);
			cameraMain.gameObject.SetActive(true);		
		}
	}
	//The error is
	void Update () {
		if (music){
			music.transform.position=transform.position;
		}
		if (Input.GetKeyDown(KeyCode.Escape)&&master.character){
			pause();
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
	void OnGUI(){
		if (!connected){
			GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
		}
		if (paused){
			if (!master.reChoosingTeam & !master.reChoosingCharacter){
				if (GUI.Button(new Rect(Screen.width/2-75f,50f,150f,100f),"Change Character")){

					master.reChoosingCharacter=true;
				}
				if (GUI.Button(new Rect(Screen.width/2-75f,350f,150f,100f),"Change Team")){
					master.reChoosingTeam=true;
				}
			}		
		}
		if (changingColor){
			if (master.team!=-1){
				doColor();
			}
			else{
				GUI.Box(new Rect(Screen.width/2-75f,50f,150f,100f),"Choose a team first");
			}
		}
	}
	void doColor(){
		a = GUI.HorizontalSlider (new Rect (25, 300, 100, 30), a, 0.0f, 1f);
		b = GUI.HorizontalSlider (new Rect (25, 340, 100, 30), b, 0.0f, 1f);
		c = GUI.HorizontalSlider (new Rect (25, 380, 100, 30), c, 0.0f, 1f);
		if (teamNumber==0){
			if (a<primaryLimit){
				a = primaryLimit;
			}
			if (b>a*secondaryLimit){
				b = a * secondaryLimit;
			}
			if (c>a*secondaryLimit){
				c = a *secondaryLimit;
			}
		}
		if (teamNumber==1){
			if (c<primaryLimit){
				c = primaryLimit;
			}
			if (b>c*secondaryLimit){
				b = c * secondaryLimit;
			}
			if (a>c*secondaryLimit){
				a = c *secondaryLimit;
			}
		}
		if (teamNumber==2){
			if (b<primaryLimit){
				b = primaryLimit;
			}
			if (a>b*secondaryLimit){
				a = b * secondaryLimit;
			}
			if (c>b*secondaryLimit){
				c = b *secondaryLimit;
			}
		}
		
		Color color = new Color(a,b,c,1f);
		colorBall.renderer.material.color = color;
		if (GUI.Button(new Rect(25f,420f,100f,25f),"Apply Color")){
			master.setColor(a,b,c);
			changingColor=false;
			cameraBall.gameObject.SetActive(false);
			cameraMain.gameObject.SetActive(true);	
		}
	}
	void OnPhotonRandomJoinFailed(){
    	PhotonNetwork.CreateRoom(null);
	}
	void OnJoinedRoom(){ //Once I have joined the room, I connect to the correct level
		master = PhotonNetwork.Instantiate("MasterObject",Vector3.zero,Quaternion.identity,0).GetComponent<Master>();
		master.gc=this;
		connected=true;
		master.CameraBall=cameraBall;
		master.CameraMain=cameraMain;
		master.ModelBall=colorBall;
	}
}
