using UnityEngine;
using System.Collections;

public class AsceticMove : Movement {
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
	float size= 1.1f;
	float cameraHeight=10f;
	int seedCount=0;
	bool useOnSelf=false; //Use our next heal on our self
	public Texture greenBar;
	bool paused=false;	
	Collider[] floors = new Collider[5];
	FlagControl flag;
	// Use this for initialization
	void Start () {
		transform.localScale= new Vector3(size,size,size);
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
			ability1.setup(1,"Sacrifice");
			ability2 = gameObject.AddComponent<Cooldown>();
			ability2.setup(2,"Condemn");
			ability3 = gameObject.AddComponent<Cooldown>();
			ability3.setup(3,"Numb");
			ability4 = gameObject.AddComponent<Cooldown>();
			ability4.setup(4,"Piety");
			condition=gameObject.GetComponent<Condition>(); //This sets up the four abilities and their names
			PhotonView conditionpv=condition.GetComponent<PhotonView>();
			object[] setup=new object[3]; //Setups the condition for the player
			setup[0] = 130f; //Health
			setup[1] = 2.05f; //Movement Speed
			setup[2] = team; //Team
			conditionpv.RPC("setup",PhotonTargets.All, setup); //Concludes the setup
			transform.Translate(new Vector3(0f,-.5f,0));
		}
	}
	// Update is called once per frame
	void Update () {
		if (isControllable & condition!=null){ //If we have been setup and this is ours
			if (!paused){
				aimAtMouse();
				if (Input.GetKeyDown(KeyCode.Mouse0) && ability1.isReady() & condition.canCast() & channelName==null){ //The four abilites are mapped to Left Mouse, Right Mouse, Q and E
					doAbility1();
					if (Input.GetKey(KeyCode.LeftShift)){
						useOnSelf=true;	
					}
				}
				if (Input.GetKeyDown(KeyCode.Mouse1) && ability2.isReady() & condition.canCast() & channelName==null){ 
					doAbility2();
					
   		 		}
				if (Input.GetKeyDown(KeyCode.Q) && ability3.isReady() & condition.canCast() & channelName==null){
					if (Input.GetKey(KeyCode.LeftShift)){
						useOnSelf=true;	
					}
					doAbility3();
				}
				if (Input.GetKeyDown(KeyCode.E) && ability4.isReady() & condition.canCast() & channelName==null){
					doAbility4();
				}
				if (Input.GetKeyDown(KeyCode.F)){
					stopChannel();	
				}
				if (Input.GetKeyDown(KeyCode.Space) & flag){
					condition.setFlag(false);
					object[] argu = new object[1];
					argu[0]=null;
					flag.photonView.RPC("capture",PhotonTargets.All,argu);
					takeFlag(null);
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
				if ((x!=0 | y!=0) & canMove()){ //If we want to move and we can, we do so
					float speed = condition.getMoveSpeed();
					transform.Translate(new Vector3(x*Time.deltaTime*speed,0,y*Time.deltaTime*speed));
					x=0;
					y=0;
				}
				if (Input.GetKeyDown(KeyCode.Mouse2)){ //Center and uncenter the camera
					centered = !centered;
				}
				Vector3 location = transform.position; //The light follows us directly
				gme.light.transform.position= new Vector3(location.x,8,location.z);
				if (centered){ //If we are centered the camera follows us
					Camera.main.transform.position= new Vector3(location.x,cameraHeight,location.z);
				}
				else { //If not, the camera moves to where we want it
					doFreeCam();
				}
			}
			doGravity();
			doChannel();
		}
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
			}
			else { //Our channel was interrupted
				stopChannel();
			}
		}
		if (channelTime<0 & channelName!=null){ //Our channel was completed, we now cast our ability
			 if (channelAbility==1){ //Put the stop channel inside the statement, but before so that it allows it to be cast,
				stopChannel(); //but does not interrupt further channels
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
		setChannel(.2f * condition.getAttackSpeed(),false,1,ability1.getName());
	}
	void activateAbility1(){
		ability1.setCooldown(1.5f * condition.getAttackSpeed());
		if (useOnSelf){
			condition.setDoT(3f,"Sacrifice",5f,true);
			condition.setHealth(-7f);
		}
		else {
			GameObject go = (GameObject) PhotonNetwork.Instantiate("Ascetic/Sacrifice",transform.position,Quaternion.Euler(new Vector3(90,0f,rotation)),0);
			SacrificeScript ss = go.GetComponent<SacrificeScript>();
			ss.setup(this.gameObject); //Send it our GameObject and our condition
			object[] argu = new object[1];
			argu[0]=team;
			ss.photonView.RPC("setup",PhotonTargets.All,argu);
		}
		useOnSelf=false;
	}
	void doAbility2(){
		setChannel(.25f * condition.getAttackSpeed(),false,2,ability2.getName());
	}
	void activateAbility2(){
		ability2.setCooldown(2.5f * condition.getAttackSpeed()); //Set the cooldown for the ability
		GameObject go = (GameObject) PhotonNetwork.Instantiate("Ascetic/Condemn",transform.position,Quaternion.Euler(new Vector3(90,0f,rotation)),0);
		CondemnScript cs= go.GetComponent<CondemnScript>();
		cs.setup(gameObject);
		object[] argu = new object[1];
		argu[0]=team;
		cs.photonView.RPC("setup",PhotonTargets.All,argu);
	}
	void doAbility3(){
		if (useOnSelf){
			condition.setArmor(5f,"NumbArmor",75f,false);	
			condition.setAttackSpeed(5f,"NumbAttack",135f,false);
			ability3.setCooldown(7f);
			useOnSelf=false;
			return;
		}
		else {
			Vector3 mousePosition = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x,Input.mousePosition.y,cameraHeight)); //Mouse position
			GameObject target = findPlayer(true, mousePosition);
			if (target){
				if (target==gameObject){ //If we used it on ourself we use this to be more efficient
					condition.setArmor(5f,"NumbArmor",75f,false);	
					condition.setAttackSpeed(5f,"NumbAttack",135f,false);
					ability3.setCooldown(7f);
					useOnSelf=false;
					return;
				}
				PhotonView pv = target.GetPhotonView();
				object[] argu  = new object[4];
				argu[0] = 5f;
				argu[1] = "NumbArmor";
				argu[2] = 75f;
				argu[3] = false;
				pv.RPC("setArmor",pv.owner,argu);
				argu[0] = 5f;
				argu[1] = "NumbAttack";
				argu[2] = 135f;
				argu[3] = false;
				pv.RPC("setAttackSpeed",pv.owner,argu);
				ability3.setCooldown(7f);
				return;
			}
		}
	}
	void doAbility4(){
		setChannel(.2f,false,4,ability4.getName());
	}
	void activateAbility4(){
		ability4.setCooldown(8f); //Set the cooldown for the ability
		GameObject go = (GameObject) PhotonNetwork.Instantiate("Ascetic/Piety",transform.position,Quaternion.Euler(new Vector3(90,0f,rotation)),0);
		PietyScript ps = go.GetComponent<PietyScript>();
		ps.setup(gameObject);
		object[] argu = new object[1];
		argu[0]=team;
		ps.photonView.RPC("setup",PhotonTargets.All,argu);
	}
	void tauntNearbyPlayers(){ //Leftover code from ability 4
		ability4.setCooldown(8f);
		Vector3 mousePosition = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x,Input.mousePosition.y,cameraHeight)); //Mouse position
		Collider[] colliderTargets =Physics.OverlapSphere(transform.position,5f);
		object[] argu = new object[4];
		argu[0] = 3f;
		argu[1] = "Piety";
		argu[2] = 2f;
		argu[3] = mousePosition;
		for (int i=0; i<colliderTargets.Length; i++){
			GameObject newTarget = colliderTargets[i].gameObject;
			Movement move = newTarget.GetComponent<Movement>();
			if (move && move.getTeam()!=team){ //If they are a player and they are not on our team
				PhotonView pv = newTarget.GetPhotonView();
				pv.RPC("setPull",pv.owner,argu);
			}
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
			break;
		case 1:
			renderer.material=blueMaterial;
			break;
		default:
			renderer.material=greenMaterial;
			break;
		}
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
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
			break;
		case 1:
			renderer.material=blueMaterial;
			break;
		default:
			renderer.material=greenMaterial;
			break;
		}
	}
	bool canMove(){
		if (condition.getMoveSpeed()==0 | !channelCanMove){
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
		}
	}
}
