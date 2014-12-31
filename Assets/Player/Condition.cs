using UnityEngine;
using System.Collections;

public class Condition : Photon.MonoBehaviour { //The first three are set arbitrarily, then corrected when spawned
	float maxHP; //The max hp the character has: overheal allows currentHP to go up to 1.5 times the maxHP
	float currentHP; //The current amount of HP we have, at 0 or below we die
	float movementSpeed; //The base speed we move at
	
	public int team; //The team we are on
	public bool hasFlag; //If we have the flag
	
	public Texture redBar;
	public Texture whiteBar;
	
	int size = 16; //How many disables we can hold
	string[] word = new string[16]; //This one holds the name of the disable
	string[] type = new string[16]; //This one holds the type of disable
	float[] duration = new float[16]; //This one holds the duration of the disable
	float[] strength = new float[16]; //This one holds the power of the disable, values depend on the type of disable
	Vector3 location; //Because only one push or pull or taunt is used
	GameObject taunter; //we just have to have one variable
	bool paused;
	// Use this for initialization
	void Start () { //Here we intialize the array
		for (int i=0; i<size; i++){ //Default value for power is 100
			strength[i]=100f;	
		}
	}
	public void pause(){
		paused=!paused;
	}
	public string arrayToString(){ //Prints out the players array
		string s = "";
		for (int i=0; i<size; i++){
			s = s + type[i] + " , ";
		}
		return s;
	}
	[RPC]
	public void setup(float health, float speed, int team){
		maxHP=health;
		currentHP=health;
		movementSpeed=speed;
		this.team=team;
		
	}
	// Update is called once per frame
	void Update () {
		if (currentHP>maxHP){ //Overheal fades over time
			currentHP-=5*Time.deltaTime;	
		}
		for (int i=0; i<size; i++){ //If we are currently affected by a condition, we tick down its duration
			if (duration[i]>0){
				duration[i]-=Time.deltaTime;
				if (duration[i]<=0){ //If the duration has ended
					if (type[i]=="Invis"){
						type[i]=null;
						checkInvis();
					}
					if (location!=Vector3.zero){
						location=Vector3.zero;
					}
					if (!taunter){
						taunter=null;
					}
					duration[i]=0f;
					strength[i]=100f; 
					type[i]=null;
					word[i]=null;
				}
				if (type[i]=="Push"){
					doPush(i);
				}
				if (type[i]=="Pull"){
					doPull(i);
				}
				if (type[i]=="Taunt"){
					doTaunt(i);
				}
				if (type[i]=="DoT"){ //Damage over time
					setHealth(strength[i]*Time.deltaTime);
				}
			}	
		}
		if (hasFlag){
			//setMoveSpeed(Time.deltaTime*2,"flagSlow",75f,false);	
		}
	}
	void doPush(int index){
		Vector3 toMove = Vector3.Normalize(transform.position-location) * strength[index] * Time.deltaTime;
		transform.Translate(toMove);
	}
	void doPull(int index){
		Vector3 toMove = Vector3.Normalize(location-transform.position) * strength[index] * Time.deltaTime;
		transform.Translate(toMove);
		if (transform.position.AlmostEquals(location,.1f)){ //If we are close enough to our target, we stop moving towards it
			location=Vector3.zero;
			duration[index]=0f;
			strength[index]=100f; 
			type[index]=null;
			word[index]=null;
				
		}
	}
	void doTaunt(int index){
		if (!taunter){ //If our taunter has been destroyed
			location=Vector3.zero;
			duration[index]=0f;
			strength[index]=100f; 
			type[index]=null;
			word[index]=null;
		}
		Vector3 toMove = Vector3.Normalize(taunter.transform.position-transform.position) * getMoveSpeed() * Time.deltaTime;
		transform.Translate(toMove); //We dont want to remove the effect if we get too close because the target is always moving
	}
	[RPC]
	public void setFlag(bool setting){
		hasFlag=setting;
	}
	public bool getFlag(){
		return hasFlag;	
	}
	void checkInvis(){ 
		for (int i=0; i<size; i++){
			if (type[i]=="Invis"){
				return;	//Stop this method if we are still invisible
			}
		}
		photonView.RPC("unInvis",PhotonTargets.All);
	}
	[RPC]
	void unInvis(){
		Color C = gameObject.renderer.material.color;
		C.a=1f;
		gameObject.renderer.material.color = C;
	}
	[RPC]
	void doInvis(){
		Master ourMaster = PhotonView.Find(PhotonNetwork.player.ID*1000 +1).GetComponent<Master>();
		if (ourMaster.team==team){ //If we are on the same team as the player going invis
			Color C = gameObject.renderer.material.color;
			C.a=.5f;
			gameObject.renderer.material.color = C;
		}
		else {
			Color C = gameObject.renderer.material.color;
			C.a=0f;
			gameObject.renderer.material.color = C;
		}
	}
	public void clearEffect(string effectName){
		for (int i=0; i<size; i++){
			if (word[i]==effectName){
				if (type[i]=="Invis"){
						type[i]=null;
						checkInvis();
					}
				duration[i]=0f;
				type[i]=null;
				word[i]=null;
				strength[i]=100f; 
				location=Vector3.zero;
				taunter=null;
			}
		}
	}
	[RPC]
	public bool hasEffect(string effectName){
		for (int i=0; i<size; i++){
			if (word[i]==effectName){
				return true;	
			}
		}
		return false;
	}
	public bool isInvis(){
		for (int i=0; i<size; i++){
			if (type[i]=="Invis"){
				return true;	
			}
		}
		return false;
	}
	public bool isStunned(){
		for (int i=0; i<size; i++){
			if (type[i]=="Stun"){
				return true;	
			}
		}
		return false;
	}
	public bool isSleeped(){
		for (int i=0; i<size; i++){
			if (type[i]=="Sleep"){
				return true;	
			}
		}
		return false;
	}
	public bool isSilenced(){
		for (int i=0; i<size; i++){
			if (type[i]=="Silence"){
				return true;	
			}
		}
		return false;
	}
	public bool isMoving(){
		for (int i=0; i<size; i++){
			if (type[i]=="Push" | type[i]=="Pull"){
				return true;	
			}
		}
		return false;
	}
	public bool canCast(){
		if (isStunned() | isSilenced() | isSleeped() | isMoving()){ //If we are stunned, silenced, or asleep we cannot cast
			return false;	
		}
		else {
			return true;	
		}
	}
	public float getMoveSpeed(){
		if (isStunned()  | isSleeped() | isMoving()){ //If we are asleep or stunned we cannot move
			return 0f;
		}
		else {
			float modifier = 1;
			for (int i=0; i<size; i++){
				if (type[i]=="MoveSpeed"){ //Multiply out every change to our movespeed
					modifier*=(strength[i]/100);
				}
			}
			return movementSpeed*modifier;
			
		}
	}
	public float getAttackSpeed(){
		float modifier = 1;
		for (int i=0; i<size; i++){
			if (type[i]=="AttackSpeed"){ //Multiply out every change to our attackspeed
				modifier*=Mathf.Pow((strength[i]/100),-1);
			}
		}
		return modifier;
	}
	public float getArmor(){
		float modifier = 1;
		for (int i=0; i<size; i++){
			if (type[i]=="Armor"){ //Multiply out every change to our armor
				modifier*=strength[i]/100;
			}
		}
		return modifier;	
	}
	[RPC]
	public void setPush(float duration, string disableName, float speed, Vector3 location){ //We are knocked back from a point, for a duration and a speed
		int loc=-1;
		for (int i=0; i<size; i++){ //Because we dont care about duplicates we find the first slot and put it in
			if (type[i] == "Push" | type[i] == "Pull" | type[i] == "Taunt"){
				this.duration[i] = duration;
				word[i] = disableName;
				type[i] = "Push";
				strength[i] = speed;
				this.location = location; 
				return;
			}
			if (this.duration[i] == 0){
				loc = i;	
			}
		}
		this.duration[loc] = duration;
		word[loc] = disableName;
		type[loc] = "Push";
		strength[loc] = speed;
		this.location = location;
		
	}
	[RPC]
	public void setPull(float duration, string disableName, float speed, Vector3 location){ //We are knocked back from a point, for a duration and a speed
		int loc=-1;
		for (int i=0; i<size; i++){ //Because we dont care about duplicates we find the first slot and put it in
			if (type[i] == "Push" | type[i] == "Pull" | type[i] == "Taunt"){
				this.duration[i] = duration;
				word[i] = disableName;
				type[i] = "Pull";
				strength[i] = speed;
				this.location = location;
				return;
			}
			if (this.duration[i] == 0){
				loc = i;	
			}
		}
		this.duration[loc] = duration;
		word[loc] = disableName;
		type[loc] = "Pull";
		strength[loc] = speed;
		this.location = location;
	}
	[RPC]
	public void setTaunt(float duration, string disableName, int taunterID){
		int loc=-1;
		for (int i=0; i<size; i++){ //Because we dont care about duplicates we find the first slot and put it in
			if (type[i] == "Push" | type[i] == "Pull" | type[i] == "Taunt"){
				this.duration[i] = duration;
				word[i] = disableName;
				type[i] = "Taunt";
				taunter=PhotonView.Find(taunterID).gameObject;
				return;
			}
			if (this.duration[i] == 0){
				loc = i;	
			}
		}
		this.duration[loc] = duration;
		word[loc] = disableName;
		type[loc] = "Taunt";
		taunter=PhotonView.Find(taunterID).gameObject;
	}
	[RPC]
	public void setInvis(float duration, string disableName){ //The way invisibility works:
		if (photonView.isMine){ 
			int loc=-1;
			for (int i=0; i<size; i++){
				if (word[i] == disableName){
					this.duration[i] = duration; //The invisibility can be refreshed by another ability
					return;
				}
				if (this.duration[i] == 0){
					loc = i;	
				}
			} 
			word[loc]=disableName; //But if we are not currently invisible
			this.duration[loc]=duration; 
			type[loc]="Invis";
			photonView.RPC("doInvis",PhotonTargets.All); //We become invisible
		}
	}	
	[RPC]
	public void setStun(float duration, string disableName){ // If we find the disable we have we simply refresh the duration, else, we add it in
		if (photonView.isMine){ //Fills up from the top to the bottom, not faster, but easier to use
			int loc=-1;
			for (int i=0; i<size; i++){
				if (word[i] == disableName){
					this.duration[i] = duration;
					return;
				}
				if (this.duration[i] == 0){
					loc = i;	
				}
			} 
			word[loc]=disableName;
			this.duration[loc]=duration;
			type[loc]="Stun";
		}
	}
	[RPC]
	public void setSleep(float duration, string disableName){ // If we find the disable we have we simply refresh the duration, else, we add it in
		if (photonView.isMine){
			int loc=-1;
			for (int i=0; i<size; i++){
				if (word[i] == disableName){
					this.duration[i] = duration;
					return;
				}
				if (this.duration[i] == 0){
					loc = i;	
				}
			} 
			word[loc]=disableName;
			this.duration[loc]=duration;
			type[loc]="Sleep";
		}
	}
	[RPC]
	public void setSilence(float duration, string disableName){ // If we find the disable we have we simply refresh the duration, else, we add it in
		if (photonView.isMine){
			int loc=-1;
			for (int i=0; i<size; i++){
				if (word[i] == disableName){
					this.duration[i] = duration;
					return;
				}
				if (this.duration[i] == 0){
					loc = i;	
				}
			}	
			word[loc]=disableName;
			this.duration[loc]=duration;
			type[loc]="Silence";
		}
	}
	[RPC]
	public void setEffect(float duration,string disableName, bool isStackable){
		int loc=-1;
		for (int i=0; i<size; i++){
			if (word[i] == disableName & !isStackable){
				this.duration[i] = duration;
				return;
			}
			if (this.duration[i] == 0){
				loc = i;
			}
		} 
		word[loc]=disableName;
		this.duration[loc]=duration;
		type[loc]="Effect";
	}
	[RPC]
	public void setDoT(float duration, string disableName, float strength, bool isStackable){
		int loc=-1;
		for (int i=0; i<size; i++){
			if (word[i] == disableName & !isStackable){
				this.duration[i] = duration;
				return;
			}
			if (this.duration[i] == 0){
				loc = i;
			}
		} 
		word[loc]=disableName;
		this.duration[loc]=duration;
		type[loc]="DoT";
		this.strength[loc]=strength;
	}
	[RPC]
	public void setArmor(float duration, string disableName, float strength, bool isStackable){
		int loc=-1;
		for (int i=0; i<size; i++){
			if (word[i] == disableName & !isStackable){
				this.duration[i] = duration;
				return;
			}
			if (this.duration[i] == 0){
				loc = i;
			}
		} 
		word[loc]=disableName;
		this.duration[loc]=duration;
		type[loc]="Armor";
		this.strength[loc]=strength;
	}
	[RPC]
	public void setMoveSpeed(float duration, string disableName, float strength, bool isStackable){ //Strength is 0 to infinity, 100 being normal strength, 0 being a snare, above 100 being a speed boost
		if (photonView.isMine){
			int loc=-1;
			for (int i=0; i<size; i++){
				if (word[i] == disableName & !isStackable){
					this.duration[i] = duration;
					return;
				}
				if (this.duration[i] == 0){
					loc = i;
				}
			} 
			word[loc]=disableName;
			this.duration[loc]=duration;
			this.strength[loc]=strength;
			type[loc]="MoveSpeed";
		}
	}
	[RPC]
	public void setAttackSpeed(float duration, string disableName, float strength, bool isStackable){ //Strength is 0 to infinity, 100 being normal strength, 0 being a snare, above 100 being a speed boost
		if (photonView.isMine){
			int loc=-1;
			for (int i=0; i<size; i++){
				if (word[i] == disableName & !isStackable){
					this.duration[i] = duration;
					return;
				}
				if (this.duration[i] == 0){
					loc = i;
				}
			} 
			word[loc]=disableName;
			this.duration[loc]=duration;
			this.strength[loc]=strength;
			type[loc]="AttackSpeed";
		}
	}
	[RPC]
	public void setHealth(float health){ //Health is a relative value
		if (photonView.isMine){	
			if (health<0f){ //If we took damage we wake up our sleep and take into account our armor
				for (int i=0; i<size; i++){
					if (type[i]=="Sleep"){
						word[i]=null;
						type[i]=null;
						duration[i]=0;
						strength[i]=100f;
					}	
				}
				currentHP+=health*getArmor();
			}
			else { //If we were healed we just change our hp
				currentHP+=health;
			}
			if (currentHP<=0){
				die (); //If we no health left, we die
			}
			if (currentHP>maxHP*1.5f){
				currentHP=maxHP*1.5f; //We cannot be healed over 1.5 times our maxHP	
			}
		}
	}
	public void die(){
		Movement mov = (Movement) gameObject.GetComponent<Movement>();
		mov.getMaster().setSpawn(mov.getMaster().reChooseTime);
		PhotonNetwork.Destroy(gameObject);	
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		//Do nothing
	}
	int[] getNegative(){ //Returns the index of all the negative effects currently on us.
		int num=1;
		int[] array= new int[size];
		for (int i=size-1;i>0;i--){
			if (type[i]==null){
				continue;	
			}
			if (type[i]=="Stun" | type[i]=="Sleep" | type[i]=="Silence"){ //Absolute values
				array[num]=i;
				num++;
			}
			if ((type[i]=="MoveSpeed" | type[i] == "AttackSpeed") & strength[i]<100f){  //Less than 100 means it is a negative affect, these are all relative values
				array[num]=i;
				num++;
			}
		}
		array[0]=num-1;
		return array;
	}
	int[] getPositive(){ //Returns the index of all the negative effects currently on us.
		int num=1;//Save the first index
		int[] array= new int[size];
		for (int i=size-1;i>0;i--){
			if (type[i]==null){
				continue;	
			}
			if (type[i]=="Invis"){
				array[num]=i;
				num++;
			}
			if ((type[i]=="MoveSpeed" | type[i] == "AttackSpeed") & strength[i]>100f){  //More than 100 means it is a positive affect, these are all relative values
				array[num]=i;
				num++;
			}
		}
		array[0]=num-1;
		return array;
	}
	
	void OnGUI(){
		if (photonView.isMine & ! paused){
			float drawHP=currentHP/maxHP; //How far along the HP bar should be
			float overHP = (currentHP-maxHP)/maxHP;; //How far along the over bar should be
			if (currentHP>maxHP){
				drawHP = 1f;				
			}
			if (overHP<0){
				overHP=0;	
			}			
			GUI.Box(new Rect(Screen.width/2-105,Screen.height-35,210,30),""); //The box around the bar
			GUI.DrawTexture(new Rect(Screen.width/2-100,Screen.height-30,200*drawHP,20), redBar, ScaleMode.StretchToFill, true, 0f); //The health bar
			GUI.DrawTexture(new Rect(Screen.width/2-100,Screen.height-30,200*overHP,20), whiteBar, ScaleMode.StretchToFill, true, 0f); //The overheal bar		
			GUI.BeginGroup(new Rect(Screen.width/2+115f,Screen.height-110f,105f,110f));{ //Positive effects
				int [] array = getPositive();
				int row = 1;
				int col = 1;
				int length = array[0];
				for (int i=1;i<length+1;i++){
					GUI.Box(new Rect(5*row + (row-1)*50,5*col + (col-1)*50,50f,50f),word[array[i]].ToString());
					row++;
					if (row>2){
						col++;
						row=1;
					}			
				}
			}GUI.EndGroup();
			GUI.BeginGroup(new Rect(Screen.width/2-115f-105f,Screen.height-110f,105f,110f));{ //Negative affect
				int [] array = getNegative();
				int row = 1;
				int col = 1;
				int length = array[0];
				for (int i=1;i<length+1;i++){
					GUI.Box(new Rect(105-5*(row) - (row)*50,5*col + (col-1)*50,50f,50f),word[array[i]].ToString());
					row++;
					if (row>2){
						col++;
						row=1;
					}
				}
			}GUI.EndGroup();	
		}
	}
}