
using UnityEngine;
using System.Collections;

public class BrawlerMove : Movement {
	float x=0f;
	float y=0f;
	bool isControllable = false;
	GameObject gme; 
	float rotation;
	Vector3 aims;
	string username;
	int type;
	int team;
	Cooldown ability1;
	Cooldown ability2;
	Cooldown ability3;
	Cooldown ability4;
	Condition condition;
	Master master;
	GameObject tether;
	public Material redMaterial;
	public Material blueMaterial;
	public Material greenMaterial;
	bool centered = true;
	float channelTime;
	int channelAbility;
	string channelName;
	bool channelCanMove=true;
	float channelMaxTime;
	float gravitySpeed; //How fast we are falling
	bool onTheFloor=true;
	float size=1.2f;
	float cameraHeight=10f;
	public Texture greenBar;
	bool speedUp = false;
	bool paused=false;
	Collider[] floors = new Collider[5];
	float abilityRotation; //The rotation we used the ability at
	FlagControl flag;
	bool stomping=false;
	float slowTime=0f; //This is the time in between the last onPhotonViewSerialize, used to set slow more accurately
	float lastTime=Time.time;
	float tranceTime=0;
	float tranceMaxTime=4;
	public GameObject particleHolder;
	// Use this for initialization
	void Start () {
		transform.localScale= new Vector3(size,size,size);
		particleHolder.particleSystem.emissionRate=0;
		if (isControllable){
			onTheFloor=false;
			gme = new GameObject();
			gme.AddComponent("Light");
			gme.light.type = LightType.Directional;
			gme.light.intensity = .3f;
			gme.light.transform.position = new Vector3(0,8,0);
			gme.name = "PlayerView"; 
			gme.transform.rotation = Quaternion.Euler(new Vector3(90f,0f,0f)); //Up until here this is creating the light to point towards the player.
			ability1 = gameObject.AddComponent<Cooldown>();
			ability1.setup(1,"Smash");
			ability2 = gameObject.AddComponent<Cooldown>();
			ability2.setup(2,"Flip");
			ability3 = gameObject.AddComponent<Cooldown>();
			ability3.setup(3,"Stomp");
			ability4 = gameObject.AddComponent<Cooldown>();
			ability4.setup(4,"Counter");
			condition=gameObject.GetComponent<Condition>(); //This sets up the four abilities and their names
			PhotonView conditionpv=condition.GetComponent<PhotonView>();
			object[] argu=new object[3]; //Setups the condition for the player
			argu[0] = 150f; //Health
			argu[1] = 2.2f; //Movement Speed
			argu[2] = team; //Team
			conditionpv.RPC("setup",PhotonTargets.All, argu); //Concludes the setup
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (isControllable & condition!=null){ //If we have been setup and this is ours
			if (!paused){
				aimAtMouse();
				if (Input.GetKey(KeyCode.Mouse0) && ability1.isReady() & condition.canCast() & channelName==null & tranceTime<=0){
					doAbility1();
				}
				if (Input.GetKey(KeyCode.Mouse1) && ability2.isReady() & condition.canCast() & channelName==null & tranceTime<=0){
					doAbility2();
    			}
				if (Input.GetKeyDown(KeyCode.Q) && ability3.isReady() & condition.canCast() & channelName==null & tranceTime<=0){
					doAbility3();
				}
				if (Input.GetKey(KeyCode.E) && ability4.isReady() & condition.canCast() & channelName==null & tranceTime<=0){
					doAbility4();
				}
				if (Input.GetKeyDown(KeyCode.F)){
					stopChannel();	
					stopTrance();
				}
				if (Input.GetKeyDown(KeyCode.Space) & flag){
					condition.setFlag(false);
					object[] argu = new object[1];
					argu[0]=null;
					flag.photonView.RPC("capture",PhotonTargets.All,argu);
					takeFlag(null);
				}
				if (stomping){
					condition.setMoveSpeed(Time.deltaTime*2f,"StompSpeed",75f,false);	
				}
				if (tranceTime>0){
					condition.setMoveSpeed(Time.deltaTime*2f,"CounterSpeed",140f,false);	
				}
				float cameraScroll = Input.GetAxis("Mouse ScrollWheel");
				if (cameraScroll!=0){
					if (cameraScroll < 0 & cameraHeight<12.5) {
						Camera.main.transform.position =new Vector3(Camera.main.transform.position.x,Camera.main.transform.position.y+15*Time.deltaTime,Camera.main.transform.position.z);
						cameraHeight+=15*Time.deltaTime;
					}
					if (cameraScroll > 0 & cameraHeight>7.5) {
						Camera.main.transform.position =new Vector3(Camera.main.transform.position.x,Camera.main.transform.position.y-15*Time.deltaTime,Camera.main.transform.position.z);
						cameraHeight-=15*Time.deltaTime;
					}
				}
				y=Input.GetAxisRaw("Vertical");
				x=Input.GetAxisRaw("Horizontal");
				if ((x!=0 | y!=0) & canMove()){
					float speed = condition.getMoveSpeed();
					transform.Translate(new Vector3(x*Time.deltaTime*speed,0,y*Time.deltaTime*speed));
					x=0;
					y=0;
				}
				if (Input.GetKeyDown(KeyCode.Mouse2)){
					centered = !centered;
				}
				Vector3 location = transform.position;
				gme.light.transform.position= new Vector3(location.x,8,location.z);
				if (centered){
					Camera.main.transform.position= new Vector3(location.x,cameraHeight,location.z);
				}
				else {
					doFreeCam();
				}
			}
			doGravity();
			doChannel();
			if (tranceTime>0){
				tranceTime-=Time.deltaTime;
			}
		}
	}
	void stopTrance(){
		tranceTime=0;
		condition.clearEffect("CounterSpeed");
	}
	void doGravity(){
		if (!onTheFloor & !condition.isMoving()){
			transform.Translate(new Vector3(0f,-.25f*gravitySpeed,0));
			gravitySpeed+=Time.deltaTime;
			if (transform.position.y<-15f){
				condition.die();
				if (flag){
					object[] argu = new object[1];
					argu[0]=flag.baseHolder.GetPhotonView().viewID;
					flag.photonView.RPC("capture",PhotonTargets.All,argu);
					condition.setFlag(false);
				}
			}
		}
	}
	void aimAtMouse(){
		float relativex = Camera.mainCamera.WorldToScreenPoint(transform.position).x; //We want to constantly look at the mouse
		float relativey = Camera.mainCamera.WorldToScreenPoint(transform.position).y;
		float anglex = Input.mousePosition.x - (relativex);
		float angley = Input.mousePosition.y - (relativey);
		rotation = Mathf.Atan(anglex/angley) * Mathf.Rad2Deg;
		if (angley<0 ){
			rotation = rotation-180f;
		}
		rotation = 180f-rotation;
		rotation = Mathf.Repeat(rotation,360f); //Aiming code finishes
	}
	void doChannel(){
		if (channelTime>0 & channelName!=null){
			if (condition.canCast()){ 
				channelTime-=Time.deltaTime;
				if (channelAbility==1 | channelAbility==2 | channelAbility==5){
					transform.rotation= Quaternion.Euler(90f,0f,abilityRotation);
					transform.Translate(new Vector3(0f,condition.getMoveSpeed()*Time.deltaTime*-2f,0f));
					transform.rotation= Quaternion.identity;
				}
			}
			else { //Our channel was interrupted
				stopChannel();
			}
		}
		if (channelTime<0 & channelName!=null){ //Our channel was completed, we now cast our ability
			if (channelAbility==1){
				stopChannel();
				activateAbility1();
			}
			if (channelAbility==2){
				stopChannel();
				activateAbility2();
			}
			if (channelAbility==4){
				stopChannel();
				activateAbility4();
			}
		}
	}
	void stopChannel(){
		channelName=null;
		channelTime=0;
		channelAbility=0;
		channelCanMove=true;
	}
	void setChannel(float time, bool canMove, int ability, string abilityName){
		channelTime=time;
		channelCanMove = canMove;
		channelAbility=ability;
		channelName=abilityName;
		channelMaxTime = time;
	}
	void doAbility1(){
		abilityRotation=rotation;
		setChannel(.25f * condition.getAttackSpeed(),false,1,ability1.getName());
	}
	void activateAbility1(){
		ability1.setCooldown(1f * condition.getAttackSpeed());
		GameObject knife = (GameObject) PhotonNetwork.Instantiate("Brawler/Smash",new Vector3(transform.position.x,transform.position.y,transform.position.z),Quaternion.Euler(90f,0f,rotation-50),0);
		SmashScript script = knife.GetComponent<SmashScript>();
		PhotonView pv = script.photonView;
		object[] argu = new object[1];
		argu[0]= photonView.viewID;
		pv.RPC("setup",PhotonTargets.All,argu);
	}
	void doAbility2(){
		abilityRotation=rotation;
		setChannel(.2f,false,2,ability2.getName());
	}
	void activateAbility2(){
		ability2.setCooldown(4f);
		GameObject knife = (GameObject) PhotonNetwork.Instantiate("Brawler/Flip",new Vector3(transform.position.x,transform.position.y,transform.position.z),Quaternion.Euler(90f,0f,rotation),0);
		FlipScript script = knife.GetComponent<FlipScript>();
		PhotonView pv = script.photonView;
		script.setup(gameObject);
		object[] argu = new object[1];
		argu[0]= photonView.viewID;
		pv.RPC("setup",PhotonTargets.Others,argu);
	}
	void doAbility3(){
		stomping=!stomping;
		photonView.RPC("changeStomp",PhotonTargets.All);
		if (stomping==false){
			ability3.setCooldown(2.5f);	
		}
		else {
			ability3.setCooldown(.5f);	
		}
	}
	[RPC]
	void changeStomp(){
		bool on = particleHolder.particleSystem.emissionRate==20;
		if (on){
			particleHolder.particleSystem.emissionRate=0;
		}
		else{
			particleHolder.particleSystem.emissionRate=20;
		}
	}
	GameObject findPlayer(bool onTeam, Vector3 findPosition){
		float checkDistance=2f; //How far we are willing to check
		Collider[] colliderTargets = Physics.OverlapSphere(findPosition,checkDistance); //Find all the colliders within checkDistance units of our mouse
		float distance = checkDistance; //The distance of the nearest object we currently have
		GameObject target = null; //The current closest gameObject
		for (int i=0; i<colliderTargets.Length; i++){
			GameObject newTarget = colliderTargets[i].gameObject;
			Movement move = newTarget.GetComponent<Movement>();
			if (move && ((move.getTeam()==team & onTeam) | (move.getTeam()!=team & !onTeam))){ //If they are a player and they fit the team requirements
				float newDistance = Vector3.Distance(findPosition,newTarget.transform.position); //We see if they are closer than our current target
				if (newDistance<distance){ //If they are we update our info
						distance=newDistance;
						target=newTarget;
				}
			}
		}
		return target;
	}
	void doAbility4(){
		setChannel(.25f,false,4,ability4.getName());
	}

	void activateAbility4(){
		tranceTime=2;
		ability4.setCooldown(4f);
	}
	void cancelSpeed(){ //Removes the two effects
		if (condition.hasEffect("speedArmor")){
			condition.clearEffect("speedArmor");
		}
		if (condition.hasEffect("speedMove")){
			condition.clearEffect("speedMove");
		}
	}
	void doFreeCam(){
		Vector3 playerLocation = Camera.main.WorldToScreenPoint(transform.position); 
		if (Input.mousePosition.x<Screen.width/2  & Input.mousePosition.x>0 & playerLocation.x < Screen.width-5){ //Moves controlled by the mouse
			float moveRate = (Screen.width/2 - Input.mousePosition.x)/(Screen.width/2);
			Camera.main.transform.Translate(new Vector3(moveRate*-6f*Time.deltaTime,0f,0f));
		}
		if (Input.mousePosition.x>Screen.width/2  & Input.mousePosition.x<Screen.width & playerLocation.x > 5){
			float moveRate = ((Input.mousePosition.x-Screen.width)/(Screen.width/2) + 1f);
			Camera.main.transform.Translate(new Vector3(moveRate*6f*Time.deltaTime,0f,0f));
		}
		if (Input.mousePosition.y<Screen.height/2  & Input.mousePosition.y>0 & playerLocation.y < Screen.height-5) {
			float moveRate = (Screen.height/2 - Input.mousePosition.y)/(Screen.height/2);
			Camera.main.transform.Translate(new Vector3(0f,moveRate*-6f*Time.deltaTime,0f));
		}
		if (Input.mousePosition.y>Screen.height/2  & Input.mousePosition.y<Screen.height & playerLocation.y > 5){
			float moveRate = ((Input.mousePosition.y-Screen.height)/(Screen.height/2) + 1f);
			Camera.main.transform.Translate(new Vector3(0f,moveRate*6f*Time.deltaTime,0f));
		}
		playerLocation = Camera.main.WorldToScreenPoint(transform.position); //Make sure we are on screen
		if (playerLocation.x < 0){ //Lerp is used because it is smoother. The flat move is the commented out line
			Vector3  camPosition = Camera.main.transform.position;
			Vector3 sidePosition = Camera.main.ScreenToWorldPoint(new Vector3(0f,0f,(cameraHeight-transform.localScale.y * .5f)));
			//Camera.main.transform.position = new Vector3(camPosition.x + (transform.position.x-sidePosition.x),camPosition.y,camPosition.z);
			Camera.main.transform.position = Vector3.Lerp(camPosition,new Vector3(camPosition.x + (transform.position.x-sidePosition.x),camPosition.y,camPosition.z) ,.5f);
			
		}
		if (playerLocation.x > (Screen.width)){
			Vector3  camPosition = Camera.main.transform.position;
			Vector3 sidePosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,(cameraHeight-transform.localScale.y * .5f)));
			//Camera.main.transform.position = new Vector3(camPosition.x + (transform.position.x-sidePosition.x),camPosition.y,camPosition.z);
			Camera.main.transform.position = Vector3.Lerp(camPosition,new Vector3(camPosition.x + (transform.position.x-sidePosition.x),camPosition.y,camPosition.z) ,.5f);
		}
		if (playerLocation.y < 0){
			Vector3  camPosition = Camera.main.transform.position;
			Vector3 sidePosition = Camera.main.ScreenToWorldPoint(new Vector3(0f,0f,(cameraHeight-transform.localScale.y * .5f)));
			//Camera.main.transform.position = new Vector3(camPosition.x,camPosition.y,camPosition.z + (transform.position.z-sidePosition.z));
			Camera.main.transform.position = Vector3.Lerp(camPosition,new Vector3(camPosition.x ,camPosition.y,camPosition.z + (transform.position.z-sidePosition.z)) ,.5f);
		}
		if (playerLocation.y > (Screen.height)) {
			Vector3  camPosition = Camera.main.transform.position;
			Vector3 sidePosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,(cameraHeight-transform.localScale.y * .5f)));
			//Camera.main.transform.position = new Vector3(camPosition.x ,camPosition.y ,camPosition.z + (transform.position.z-sidePosition.z));
			Camera.main.transform.position = Vector3.Lerp(camPosition,new Vector3(camPosition.x ,camPosition.y ,camPosition.z + (transform.position.z-sidePosition.z)),.5f);
		}
	}
	public override void setup(string username, int type, int team, Master master, Color c){
		isControllable=true;
		this.username=username;
		this.type=type;
		this.team=team;
		this.master=master;
		switch (team)
		{
		case 0:
			renderer.material=redMaterial;
			particleHolder.GetComponent<ParticleSystem>().startColor=Color.red;
			break;
		case 1:
			renderer.material=blueMaterial;
			particleHolder.GetComponent<ParticleSystem>().startColor=Color.blue;
			break;
		default:
			renderer.material=greenMaterial;
			particleHolder.GetComponent<ParticleSystem>().startColor=Color.green;
			break;
		}
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		slowTime=Time.time-lastTime;
		lastTime=Time.time;
		if (stomping){
			Collider[] colliderTargets = Physics.OverlapSphere(transform.position,2*transform.localScale.z); //Find all the colliders within checkDistance units of our mouse
			for (int i=0; i<colliderTargets.Length; i++){
				GameObject newTarget = colliderTargets[i].gameObject;
				Movement move = newTarget.GetComponent<Movement>();
				if (move && move.getTeam()!=team){ //If they are a player and they fit the team requirements
					object[] argu = new object[4];
					argu[0]=slowTime*2;
					argu[1]="StompSpeed";
					argu[2]=50f;
					argu[3]=false;
					PhotonView pv = move.photonView;
					pv.RPC("setMoveSpeed",pv.owner,argu);
				}
			}
		}
	}
	public override Master getMaster(){
		return master;	
	}
	void OnDestroy() {
        Destroy(gme);
    }
	public override int getTeam(){
		return team;	
	}
	[RPC]
	public override void setup(string username, int team, float r, float g, float b){
		this.username=username;
		this.team=team;
		switch (team)
		{
		case 0:
			renderer.material=redMaterial;
			particleHolder.GetComponent<ParticleSystem>().startColor=Color.red;
			break;
		case 1:
			renderer.material=blueMaterial;
			particleHolder.GetComponent<ParticleSystem>().startColor=Color.blue;
			break;
		default:
			renderer.material=greenMaterial;
			particleHolder.GetComponent<ParticleSystem>().startColor=Color.green;
			break;
		}
	}
	bool canMove(){
		if (condition.getMoveSpeed()==0 | !channelCanMove ){
			return false;
		}
		return true;
	}
	void OnTriggerExit(Collider co) {
        if (co.gameObject.name=="Floor" & photonView.isMine){
			bool hasLeft = true; //Check if we are on any floor
			for (int i=0;i<floors.Length;i++){
				if (floors[i]==co){ //If we just left the floor we remove it
					floors[i]=null;	
				}
				if (floors[i]){ //If we are still on a floor we are still on the floor
					hasLeft=false;
				}
			}
			if (hasLeft){ //If we have no more floors left we are in the air
				onTheFloor=false;
			}
		}
    }
    void OnTriggerEnter(Collider co){
		if (co.gameObject.name=="Floor" & photonView.isMine){ 
			transform.position = new Vector3(transform.position.x,co.transform.position.y + transform.localScale.y/2 -.1f,transform.position.z);
			onTheFloor=true;
			gravitySpeed=0;
			for (int i=0;i<floors.Length;i++){ //Add the floor we are on to the floor array
				if (!floors[i]){
					floors[i]=co;
					return;
				}
			}
			Debug.LogError("We are touching too many floors!");
		}
	}
	public override void pause(){	
		paused=!paused;	
		ability1.pause();
		ability2.pause();
		ability3.pause();
		ability4.pause();
		condition.pause();
		gme.light.enabled=!paused;
	}
	public override void takeFlag(FlagControl flag){
		this.flag=flag;
	}
	public override bool doHit(int attackID){
		if (tranceTime>0){
			stopTrance();
			object[] argu = new object[1];
			argu[0]=-10f;
			PhotonView pv = PhotonView.Find(attackID);
			pv.RPC("setHealth",pv.owner,argu);
			return false;
		}
		return true;	
	}
	
	public override Condition getCondition(){
		return condition;
	}
	void OnGUI(){
		if (photonView.isMine){
			if (channelName!=null & !paused){
				float drawChannel = channelTime/channelMaxTime;
				GUI.DrawTexture(new Rect(Screen.width/2-40*drawChannel,Screen.height-115,80*drawChannel,15), greenBar, ScaleMode.StretchToFill, true, 0f);
				GUI.Box(new Rect(Screen.width/2-40*drawChannel-2.5f,Screen.height-117.5f,80*drawChannel+5,20),"");
				//GUI.Box(new Rect(Screen.width/2-30,Screen.height-140,60,20),channelName);
			}
			if (tranceTime>0 & !paused){
				float drawTrance = tranceTime/tranceMaxTime;
				GUI.DrawTexture(new Rect(Screen.width/2-40*drawTrance,Screen.height-115,80*drawTrance,15), greenBar, ScaleMode.StretchToFill, true, 0f);
				GUI.Box(new Rect(Screen.width/2-40*drawTrance-2.5f,Screen.height-117.5f,80*drawTrance+5,20),"");
			}
		}
	}
}
