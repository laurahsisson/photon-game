using UnityEngine;
using System.Collections;

public class HolderControl : Photon.MonoBehaviour {
	public int team;
	bool[] flags = new bool[3];
	public GameObject flag;
	string messageToDisplay=null;
	// Use this for initialization
	void Start () {
		
	}
	public void resetFlag(){
		
		object[] argu = new object[1];
		argu[0] = photonView.viewID;
		flag.GetComponent<FlagControl>().getPhotonView().RPC("capture",PhotonTargets.All,argu);
		flags[team]=true;
	}
	// Update is called once per frame
	void Update () {
	}
	
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		
    }
	void OnCreatedRoom(){
		resetFlag();
	}
	
	public void capture(int flagTeam){
		flags[flagTeam]=true;
		if (!PhotonView.Find(PhotonNetwork.player.ID * 1000 + 1)){ //If we have not started yet, we do nothing
			return;	
		}
		if (flags[0] & flags[1] &  flags[2]){
			restartGame();
		}
	}
	public void restartGame(){
		HolderControl[] holders = (HolderControl[]) FindObjectsOfType(typeof(HolderControl));
       		for (int i=0; i<holders.Length; i++){
				if (!holders[i].hasFlag()){
					holders[i].resetFlag();	
					holders[i].Invoke("resetFlag",.01f);
					object[] argu = new object[1];
					argu[0] = "Team " + team + " won the game!";
					photonView.RPC("displayMessage",PhotonTargets.All,argu);
				}
			}
		Master[] masters = (Master[]) FindObjectsOfType(typeof(Master));
		for (int i=0; i<masters.Length; i++){
			PhotonView pv = masters[i].photonView;
			pv.RPC("sendToSpawn",pv.owner);
		}
	}
	bool hasFlag(){
		if (flag.GetComponent<FlagControl>().holder==gameObject){
			return true;	
		}
		return false;
	}
	public void release(int flagTeam){
		flags[flagTeam]=false;
	}
	[RPC]
	void displayMessage(string message){
		messageToDisplay=message;
		Invoke("cancelMessage",2f);
	}
	void cancelMessage(){
		messageToDisplay=null;	
	}
	void OnGUI(){
		if (messageToDisplay!=null){
			GUI.Box(new Rect(0f,0f,Screen.width,Screen.height),messageToDisplay);
		}
	}
}
