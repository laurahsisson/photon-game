using UnityEngine;
using System.Collections;

public class Master : Photon.MonoBehaviour {
	int[] players = new int[9]; //Holds the players's master object
	public int position;
	public int team=-1;
	public int count=0;
	public float reChooseTime=8f;
	// TODO: Add character description to the menu selection

	//Vector3 teamOneSpawn = new Vector3(20,.5f,21);
	//Vector3 teamTwoSpawn = new Vector3(20,.5f,-21);
	Vector3 teamOneSpawn = new Vector3(20,2,21);
	Vector3 teamTwoSpawn = new Vector3(20,2,-21);
	Vector3 teamThreeSpawn = new Vector3(-29,2,0);
	Vector3 spawnPosition;
	public Movement movement;
	public GameObject character;
	string username = "Username";
	bool hasSetName = true;
	float respawn;
	bool isReadyToSpawn=true;
	bool paused=false;
	public bool choosingTeam = true;
	public bool reChoosingCharacter=false;
	public bool reChoosingTeam=false;
	public GameControl gc;
	int nextCharacter=-1;

	public Camera CameraMain;

	public Camera CameraBall;
	public GameObject ModelBall;

	Rect[] rects = new Rect[5];

	public Texture2D[] skins = new Texture2D[18];

	Color color;
	float r; //The r g b components of our color
	float g;
	float b;
	GUIStyle leftStyle;
	//TODO:Fix where the red ninja and certain others spawn naked
	//TODO:Comment so I look like a decent programmer
	//TODO:Migrate the NinjaMove CosmoMove to the original move and then inherit
	// Use this for initialization
	void Start () {
		leftStyle = new GUIStyle();
		leftStyle.alignment= TextAnchor.UpperLeft;
		leftStyle.normal.textColor= new Color(1f,1f,1f);
		leftStyle.fontSize= 18;
		leftStyle.wordWrap=true;
		if (photonView.isMine){
			CameraBall.gameObject.SetActive(false);
		}
		if (PhotonNetwork.player.isMasterClient){
			setPosition(); //If we created the room we set ourselves up	
			PhotonNetwork.automaticallySyncScene=true;
		}
		
	}
	
	void Update () {
		if (photonView.isMine){
			//Debug.Log(character);
			if (respawn>0f){
				respawn-=Time.deltaTime;	
			}
			if (respawn<=0f && !character && !reChoosingTeam && !reChoosingCharacter){ //If we have finished waiting to respawn and we have no character
				isReadyToSpawn=true; //We are ready to spawn
				if (nextCharacter!=-1){
					spawn(nextCharacter);	
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.L)){
			CameraBall.gameObject.SetActive(true);
			CameraMain.gameObject.SetActive(false);
		}
	}
	public void pause(){
		paused=!paused;
		if(!paused){
			reChoosingTeam=false;
			reChoosingCharacter=false;
		}
		if (character){
			character.GetComponent<Movement>().pause();
		}

	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		
	}
	int getNextSlot(int team){
		if (team==0){
			if (players[0]==0){
				return 0;	
			}
			if (players[1]==0){
				return 1;	
			}
			if (players[2]==0){
				return 2;	
			}
			return -1;
		}
		if (team==1){
			if (players[3]==0){
				return 3;	
			}
			if (players[4]==0){
				return 4;	
			}
			if (players[5]==0){
				return 5;	
			}
			return -1;
		}
		if (team==2){
			if (players[6]==0){
				return 6;	
			}
			if (players[7]==0){
				return 7;	
			}
			if (players[8]==0){
				return 8;	
			}
			return -1;
		}
		return -1;
	}
	void OnGUI () {
		if (photonView.isMine){
			if (!hasSetName){
				username = GUI.TextField(new Rect (Screen.width/2-100,Screen.height/2-10,200,20), username, 25);	//Keep prompting until we have decided
			}
			if (Input.GetKeyDown(KeyCode.Return)){
				hasSetName=true;
				photonView.owner.name=username;
			}
			
			if (hasSetName & !character & !isReadyToSpawn && !reChoosingTeam && !reChoosingCharacter){
				GUI.Box(new Rect(Screen.width/4,Screen.height/4,Screen.width/2,Screen.height/2),Mathf.Round(respawn) + " more seconds till respawn.");	
			}
			if (hasSetName & choosingTeam){
				if (getNextSlot(0)!=-1 && GUI.Button (new Rect (0,Screen.height/5,Screen.width/3,Screen.height*3/5), "Team 1")) {
					setTeam(getNextSlot(0),0);
					choosingTeam=false;
				}
				if (getNextSlot(1)!=-1 && GUI.Button (new Rect (Screen.width/3,Screen.height/5,Screen.width/3,Screen.height*3/5), "Team 2")) {
					setTeam(getNextSlot(1),1);
					choosingTeam=false;
				}
				if (getNextSlot(2)!=-1 && GUI.Button (new Rect (Screen.width*2/3,Screen.height/5,Screen.width/3,Screen.height*3/5), "Team 3")) {
					setTeam(getNextSlot(2),2);
					choosingTeam=false;
				}
				
			}
			if ((hasSetName & !choosingTeam & isReadyToSpawn)||reChoosingCharacter){
				CameraMain.gameObject.SetActive(false);
				CameraBall.gameObject.SetActive(true);
				GUI.Box(new Rect(0,0,Screen.width,25),"Choose Your Unit!");
				rects[0]=new Rect (0,Screen.height/2-75,Screen.width/5,150);
				rects[1]=new Rect (Screen.width/5,Screen.height/2-75,Screen.width/5,150);
				rects[2]=new Rect (Screen.width*2/5,Screen.height/2-75,Screen.width/5,150);
				rects[3]=new Rect (Screen.width*3/5,Screen.height/2-75,Screen.width/5,150);
				rects[4]=new Rect (Screen.width*4/5,Screen.height/2-75,Screen.width/5,150);
				if (GUI.Button (rects[0], "Cosmonaut")) {
					if (character){
						PhotonNetwork.Destroy(character);
						setSpawn(reChooseTime);
					}
					if (paused){
						gc.pause();
						nextCharacter=0;
						return;
					}
					spawn(0);
				}
				if (GUI.Button (rects[1], "Ninja")) {
					if (character){
						PhotonNetwork.Destroy(character);
						setSpawn(reChooseTime);
					}
					if (paused){
						gc.pause();
						nextCharacter=1;
						return;
					}
					spawn(1);
				}
				if (GUI.Button (rects[2], "Ascetic")) {
					if (character){
						PhotonNetwork.Destroy(character);
						setSpawn(reChooseTime);
					}
					if (paused){
						gc.pause();
						nextCharacter=2;
						return;
					}
					spawn(2);
				}
				if (GUI.Button (rects[3], "Medic")) {
					if (character){
						PhotonNetwork.Destroy(character);
						setSpawn(reChooseTime);
					}
					if (paused){
						gc.pause();
						nextCharacter=3;
						return;
					}
					spawn(3);
				}
				if (GUI.Button (rects[4], "Brawler")) {
					if (character){
						PhotonNetwork.Destroy(character);
						setSpawn(reChooseTime);
					}
					if (paused){
						gc.pause();
						nextCharacter=4;
						return;
					}
					spawn(4);
				}
				if (rects[0].Contains(Input.mousePosition)){ 
					GUI.Box(new Rect(0,25,Screen.width/3,Screen.height/2-100),"The Cosmonaut deals damage from afar and specializes in staying away from danger. The Cosmonaut kites away would be foes using blinks, knockback and a powerful moving channel. However, slow movement speed and low health prevent this unit from carrying the flag or fighting in the thick of battle.",leftStyle);
					GUI.Box(new Rect(Screen.width*2/3, 25,Screen.width/3,Screen.height/2-100),"Health:\t 110 \nSpeed:\t 2.05 \nSize:\t\t 0.95", leftStyle);
					ModelBall.renderer.material.SetTexture("_MainTex",skins[9+team]);
					ModelBall.renderer.material.mainTextureScale= new Vector2(2,1);
					ModelBall.transform.localScale= new Vector3(0.9f,0.9f,0.9f);
					GUI.Box(new Rect(0,Screen.height/2+75,Screen.width,Screen.height-75),	"Ability 1: Low damage shot. On hit it reduces the cooldown of ability 3. Affected by attack speed. Short stationary channel.\n\nAbility 2: Low to high damage shot. The longer the ability key is held down the faster the projectile and the more damage dealt. Short to long stationary channel.\n\nAbility 3: Medium range blink. Very short stationary channel.\n\nAbility 4: Knocks back enemies within a short range for a short period of time.",leftStyle);
					//"Ability 1: Low damage shot. On hit it reduces the cooldown of ability 3. Affected by attack speed.\n\tShort stationary channel.\n\nAbility 2: Low to high damage shot. The longer the ability key is held down the faster the projectile\n\tand the more damage dealt. Short to long stationary channel.\n\nAbility 3: Medium range blink. Very short stationary channel.\n\nAbility 4: Knocks back enemies within a short range for a short period of time."
				}
				else if (rects[1].Contains(Input.mousePosition)){ 
					GUI.Box(new Rect(0,25,Screen.width/3,Screen.height/2-100),"The Ninja moves fast and strikes quickly in melee range. Although low in health, this unit’s high speed and short term stealth allow it to thrive right in the middle of battle. The Ninja’s short invincibility and tethered slow bring utility to this dual flag carrier and chaser.",leftStyle);
					GUI.Box(new Rect(Screen.width*2/3, 25,Screen.width/3,Screen.height/2-100),"Health:\t 120 \nSpeed:\t 2.5 \nSize:\t\t 1.05", leftStyle);
					//GUI.DrawTexture(new Rect(Screen.width/2-100,25,200,200),renders[6+team]);
					ModelBall.renderer.material.SetTexture("_MainTex",skins[15+team]);
					ModelBall.renderer.material.mainTextureScale= new Vector2(2,1);
					ModelBall.transform.localScale= new Vector3(1f,1f,1f);
					GUI.Box(new Rect(0,Screen.height/2+75,Screen.width,Screen.height-75),	"Ability 1: Medium damage melee attack. Cancels ability 4. Affected by attack speed. Medium charging channel. \n\nAbility 2: Tethers to an enemy and slows the target. Breaks when the tether exceeds its short range.\n\nAbility 3: Short range dash. Invisible while dashing.\n\nAbility 4: Medium duration haste. Any attack that hits deals no damage, but the Ninja can still be affected by movement impairing debuffs.",leftStyle);
				}
				else if (rects[2].Contains(Input.mousePosition)){ 
					GUI.Box(new Rect(0,25,Screen.width/3,Screen.height/2-100),"The Ascetic helps out friends and debuffs foes equally. The Ascetic’s primary ability functions as both a damage dealing shot and an aimed heal. With surprisingly high health and additional temporary armor, this range support does not shy away from taking damage.", leftStyle);
					GUI.Box(new Rect(Screen.width*2/3, 25,Screen.width/3,Screen.height/2-100),"Health:\t 135 \nSpeed:\t 2.05 \nSize:\t\t 1.10", leftStyle);
					//GUI.DrawTexture(new Rect(Screen.width/2-100,25,200,200),renders[6+team]);
					ModelBall.renderer.material.SetTexture("_MainTex",skins[3+team]);
					ModelBall.renderer.material.mainTextureScale= new Vector2(1,2);
					ModelBall.transform.localScale= new Vector3(1.1f,1.1f,1.1f);
					GUI.Box(new Rect(0,Screen.height/2+75,Screen.width,Screen.height-75),	"Ability 1: Heal over time on a friendly target. Hold shift to use on self. Affected by attack speed. Short stationary channel.\n\nAbility 2: Low damage shot. Increases the amount of damage an enemy takes for an amount of time. This effect can stack on itself. Affected by attack speed. Short stationary channel.\n\nAbility 3: Increases attack speed and decreases damage taken by a friendly unit. Hold shift to use on self.\n\nAbility 4: Shot that taunts an enemy and forces them to walk towards you for a medium duration.Short stationary channel.",leftStyle);
				}
				else if (rects[3].Contains(Input.mousePosition)){ 
					GUI.Box(new Rect(0,25,Screen.width/3,Screen.height/2-100),"The Medic heals and buffs allies over time while harassing enemies. Fast and ranged, this unit skirts around the battle looking for allies in need of healing or an escape. Although capable of pushing attackers out of range, once an attacker moves into range, the Medic needs outside help.",leftStyle);
					GUI.Box(new Rect(Screen.width*2/3, 25,Screen.width/3,Screen.height/2-100),"Health:\t 100 \nSpeed:\t 2.35 \nSize:\t\t 0.95", leftStyle);
					//GUI.DrawTexture(new Rect(Screen.width/2-100,25,200,200),renders[6+team]);
					ModelBall.renderer.material.SetTexture("_MainTex",skins[12+team]);
					ModelBall.renderer.material.mainTextureScale= new Vector2(2,2);
					ModelBall.transform.localScale= new Vector3(.9f,.9f,.9f);
					GUI.Box(new Rect(0,Screen.height/2+75,Screen.width,Screen.height-75),	"Ability 1: Hold key to fire a barrage of low damage shots. After a short moving channel the medic begins to fire. Letting go cancels the ability. Attack speed increases the amount of shots fired.\n\nAbility 2: Tethers to a friendly target and heals the ally. Breaks when the tether exceeds its long range. \n\nAbility 3: Haste on a friendly unit. Hold shift to use on self. If the medic is using tether and the target is the tethered ally or the medic both units receive the haste. Short moving channel. \n\nAbility 4: Three shots that knock back the enemy they hit. Consecutive hits stack knockback. The first shot has a long moving channel while the next two have a medium­-short moving channels.",leftStyle);
				}
				else if (rects[4].Contains(Input.mousePosition)){ 
					GUI.Box(new Rect(0,25,Screen.width/3,Screen.height/2-100),"Large, tanky and aggressive, the Brawler puts other units in their places. Able to soak damage thanks to a reflect and very high health, the Brawler sits in the middle of the battle, slowing himself and others. Although slow and limited in range, once the Brawler moves in, the Brawler stays there.", leftStyle);
					GUI.Box(new Rect(Screen.width*2/3, 25,Screen.width/3,Screen.height/2-100),"Health:\t 150 \nSpeed:\t 2.2 \nSize:\t\t 1.20", leftStyle);
					//GUI.DrawTexture(new Rect(Screen.width/2-100,25,200,200),renders[6+team]);
					ModelBall.renderer.material.SetTexture("_MainTex",skins[6+team]);
					ModelBall.renderer.material.mainTextureScale= new Vector2(2,2);
					ModelBall.transform.localScale= new Vector3(1.2f,1.2f,1.2f);
					GUI.Box(new Rect(0,Screen.height/2+75,Screen.width,Screen.height-75),	"Ability 1: Medium­-high damage melee attack. Affected by attack speed. Medium charging channel. \n\nAbility 2: Melee attack that flips the enemy over the brawler. Medium charging channel. \n\nAbility 3: Toggle-­able effect that slows both the brawler and any enemies in a short radius around the brawler. \n\nAbility 4: Blocks the first offensive ability to hit the brawler and deals medium damage to the caster. Additionally, the brawler is hasted until an ability is blocked or the effect ends. Short stationary channel.",leftStyle);
				}
				else {
					ModelBall.renderer.material.SetTexture("_MainTex",skins[team]);
					ModelBall.transform.localScale= new Vector3(1f,1f,1f);
				}
			}
			if (reChoosingTeam){
				if ((getNextSlot(0)!=-1 || team==0)&& GUI.Button (new Rect (0,Screen.height/5,Screen.width/3,Screen.height*3/5), "Team 1")) {
					Debug.Log(character);
					if (character){
						PhotonNetwork.Destroy(character);
						Debug.Log("Test");
					}
					setTeam(getNextSlot(0),0);
					reChoosingTeam=false;
					reChoosingCharacter=true;
				}
				if ((getNextSlot(1)!=-1 || team==1) && GUI.Button (new Rect (Screen.width/3,Screen.height/5,Screen.width/3,Screen.height*3/5), "Team 2")) {
					Debug.Log(character);
					if (character){
						PhotonNetwork.Destroy(character);
						Debug.Log("Test");
					}
					setTeam(getNextSlot(1),1);
					reChoosingTeam=false;
					reChoosingCharacter=true;
				}
				if ((getNextSlot(2)!=-1 || team==2) && GUI.Button (new Rect (Screen.width*2/3,Screen.height/5,Screen.width/3,Screen.height*3/5), "Team 3")) {
					Debug.Log(character);
					if (character){
						PhotonNetwork.Destroy(character);
						Debug.Log("Test");
					}
					setTeam(getNextSlot(2),2);
					reChoosingTeam=false;
					reChoosingCharacter=true;
				}
			}
		}
	}
	public void setColor(float r, float g, float b){
		color = new Color(r, g, b, 1f);
		this.r=r;
		this.g=g;
		this.b=b;
		if (character){
			object[] argu = new object[3];
			argu[0]=r;
			argu[1]=g;
			argu[2]=b;
			character.GetPhotonView().RPC("setColor",PhotonTargets.All,argu);
		}
	}
	public void setPosition(){
		bool hasFound=false;
		if (!photonView.isMine){
			return;	
		}
		for (int i=1; i<photonView.ownerId; i++){
			PhotonView pv = PhotonView.Find(i*1000 + 1);
			if (pv){
				players[pv.GetComponent<Master>().team]=(i*1000 +1);	
			}
		}
		choosingTeam=true;
	}
	void setTeam(int position, int team){
		object[] argu = new object[1];
		argu[0]=this.position;
		Debug.Log(position + " , " + team);
		photonView.RPC("clearSlot",PhotonTargets.All,argu);
		this.position=position;
		this.team=team;
		players[position]=photonView.viewID;
		argu = new object[4];
		argu[0]=position;
		argu[1]=team;
		argu[2]=photonView.ownerId;
		argu[3]=false;
		photonView.RPC("setup",PhotonTargets.Others,argu);
		if (team==0){
			spawnPosition=teamOneSpawn;
			color=Color.red;
			r=1;
		}
		if (team==1){
			spawnPosition=teamTwoSpawn;
			color=Color.blue;
			b=1;
		}
		if (team==2){
			spawnPosition=teamThreeSpawn;
			color=Color.green;
			g=1;
		}
	}
	[RPC]
	void clearSlot(int position){
		players[position]=0;
	}
	[RPC]
	void setup(int newPosition, int newTeam, int target, bool hasReceiver){
		this.position=newPosition;
		this.team=newTeam;
		if (hasReceiver){
			Master master = Find(target);
			master.count++;
			if (master.count==PhotonNetwork.room.playerCount - 1){
				Find(target).setPosition();
			}
		}
		else {
			Find(PhotonNetwork.player.ID ).setPlayers(newPosition,target);
		}
	}
	
	// Update is called once per frame
	
	
	void OnPhotonPlayerConnected(PhotonPlayer player){
		if (photonView.isMine){
			object[] argu = new object[4];
			argu[0]=position;
			argu[1]=team;
			argu[2]=player.ID;
			argu[3]=true;
			photonView.RPC("setup",player,argu);
			Hashtable ht = photonView.owner.customProperties;
			photonView.owner.SetCustomProperties(ht);
			if (character){
				Movement mov = character.GetComponent<Movement>();
			argu = new object[5];
			argu[0]=username;
			argu[1]=team;
			argu[2]=r;
			argu[3]=g;
			argu[4]=b;
			mov.photonView.RPC("setup",PhotonTargets.Others,argu); //Setup for others
			}
		}
	}
	
	void OnPhotonPlayerDisconnected(PhotonPlayer player){
		if (photonView.isMine){
			for (int i=0; i<players.Length; i++){
				if (players[i]==(player.ID* 1000 +1)){
					players[i]=0;
				}
			}
		}
	}
	public void setPlayers(int newPosition, int id){
		players[newPosition]=(id*1000 +1);
	}

	void spawn(int choice){
		CameraBall.gameObject.SetActive(false);
		CameraMain.gameObject.SetActive(true);
		if (choice==0){
			character=(GameObject) PhotonNetwork.Instantiate("Cosmonaut/CosmoPlayer",spawnPosition,Quaternion.identity,0);
		}
		if (choice==1){
			character=(GameObject) PhotonNetwork.Instantiate("Ninja/NinjaPlayer",spawnPosition,Quaternion.identity,0);	
		}
		if (choice==2){
			character=(GameObject) PhotonNetwork.Instantiate("Ascetic/AsceticPlayer",spawnPosition,Quaternion.identity,0);	
		}
		if (choice==3){
			character=(GameObject) PhotonNetwork.Instantiate("Medic/MedicPlayer",spawnPosition,Quaternion.identity,0);	
		}
		if (choice==4){
			character=(GameObject) PhotonNetwork.Instantiate("Brawler/BrawlerPlayer",spawnPosition,Quaternion.identity,0);	
		}
		isReadyToSpawn=false;
		Movement mov = character.GetComponent<Movement>();
		movement = mov;
		mov.setup(username,0,team,this,color); //Personal setup
		object[] argu = new object[5];
		argu[0]=username;
		argu[1]=team;
		argu[2]=r;
		argu[3]=g;
		argu[4]=b;
		mov.photonView.RPC("setup",PhotonTargets.Others,argu); //Setup for others
	}
	public void setSpawn(float time){
		respawn=time;
	}

	public string arrayToString(){ //Prints out the players array
		string s = "";
		for (int i=0; i<players.Length; i++){
			s = s + players[i] + " , ";
		}
		return s;
	}
	[RPC]
	void sendToSpawn(){ //Sends the character back to their spawn
		if (photonView.isMine){
			character.transform.position=spawnPosition;
		}
	}
	public static Master Find(int playerID){
		PhotonView pv = PhotonView.Find(playerID*1000 + 1);
		if (!pv){
			Debug.LogError("No PhotonView exists by that player");
			return null;
		}
		Master master = pv.GetComponent<Master>();
		if (!master){
			Debug.LogError("The player has no Master");
			return null;
		}
		return master;
	}
}
