using UnityEngine;
using System.Collections;

public class Cooldown : MonoBehaviour {
	float cooldown=0f;
	float lockedTime=0f;
	bool boolMoving=false;
	bool boolReady=true;
	bool boolLocked=false;
	string abilityName;
	Rect position;
	string toPrint;
	bool paused=false;
	bool visibile=true; //Are we the cooldown for an actual ability or just for test purposes?
	// Use this for initialization
	void Start () {
	}
	public void pause(){
		paused=!paused;
	}
	// Update is called once per frame
	void Update () {
		if (boolLocked){ //First we check if we are locked, if we are, we move to being unlocked
			lockedTime-=Time.deltaTime;	
		}
		if (boolLocked & lockedTime<=0){ //Tells us we are ready to move
			boolLocked=false;
			boolMoving = true;
			lockedTime=0;
		}
		if (boolMoving & !boolLocked){ //Next we check if we are on cooldown, if we are, we move to being ready
			cooldown-=Time.deltaTime;
			toPrint = abilityName + "\r\n" + (Mathf.Ceil(cooldown * 10f) / 10f);
		}
		if (boolMoving & !boolLocked & cooldown<=0){ //Tells us we are ready to use
			boolReady=true;
			boolMoving=false;
			cooldown=0f;
			toPrint = abilityName + "\r\n" + "Ready";
		}
	}
	public void setCooldown(float cooldown){ //Cooldown time is absolute
		this.cooldown=cooldown;
		boolMoving=true;
		boolReady=false;
	}
	public void changeCooldown(float cooldown){ //But it can be changed relatively
		this.cooldown+=cooldown;
	}
	public void setLock(float time){ //Locked time is relative
		lockedTime+=time;
		boolLocked=true;
		boolMoving=false;
	}
	public bool isReady(){ //Returns the status of ability
		if (!boolLocked & boolReady){
			return true;
		}
		else {
			return false;	
		}
	}
	public void setup(int number, string abilityName){ //Recieve the position of the ability as well as its name
		position = new Rect(Screen.width/2 + 5 + 60 * (number-3), Screen.height-90, 50,50);
		this.abilityName=abilityName;
		toPrint = abilityName + "\r\n" + "Ready";
	}
	public void setup(int number){ //Setup for a test ability
		visibile=false;
		name = "Test " + number;
	}
	void OnGUI () { //Draws the box
		if (visibile && !abilityName.Equals(null) & !paused) {
			GUI.Box (position, toPrint);
		}
	}
	public string getName(){
		return abilityName;	
	}
}
