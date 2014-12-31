using UnityEngine;
using System.Collections;

public class FlagControl : Photon.MonoBehaviour {
	public GameObject holder;
	public int team;
	public GameObject baseHolder; //The actual holder
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (holder){
			if (holder.name=="RedHolder" || holder.name=="BlueHolder" || holder.name=="GreenHolder"){
				if (team==0){
					transform.position=new Vector3(holder.transform.position.x-.7071f,(holder.transform.position.y-holder.transform.localScale.y/2f)+1f,holder.transform.position.z-.7071f);
				}
				if (team==1){
					transform.position=new Vector3(holder.transform.position.x-.7071f,(holder.transform.position.y-holder.transform.localScale.y/2f)+1f,holder.transform.position.z+.7071f);
				}
				if (team==2){
					transform.position=new Vector3(holder.transform.position.x+1f,(holder.transform.position.y-holder.transform.localScale.y/2f)+1f,holder.transform.position.z);
				}
			}
			else {
				transform.position=new Vector3(holder.transform.position.x,(holder.transform.position.y-holder.transform.localScale.y/2f)+1f,holder.transform.position.z);
			}
		}
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		
    }
	[RPC]
	void setup(Vector3 newPosition){
		transform.position=newPosition;
	}
	[RPC]
	void capture(int holder){
		if (PhotonView.Find(holder)==null){
			this.holder=null;
			return;
		}
		GameObject go = PhotonView.Find(holder).gameObject;
		HolderControl holderControl = go.GetComponent<HolderControl>();
		if (holderControl & PhotonNetwork.player.isMasterClient){ //Only the master can decide if the flag has been dropped off for a point
			holderControl.capture(team);
		}
		if (this.holder){
			holderControl = this.holder.GetComponent<HolderControl>();
			if (holderControl & PhotonNetwork.player.isMasterClient){ //Or if it has been stolen
				holderControl.release(team); 	
			}
		}
		this.holder=go;
	}
	void playerGrab(GameObject go){ //A player is attempting to grab us
		if (go.GetComponent<Condition>().getFlag()==false){
			if (holder==null){ //We can be picked up if we are not held
				object[] argu = new object[1];
				argu[0]=go.GetPhotonView().viewID;
				photonView.RPC("capture",PhotonTargets.All,argu);
				go.GetComponent<Condition>().setFlag(true); //The player picks up the flag
				holder.GetComponent<Movement>().takeFlag(this);
				return;
			}
			if (holder.GetComponent<HolderControl>() && holder.GetComponent<HolderControl>().team!=go.GetComponent<Condition>().team){
				object[] argu = new object[1]; //We can be grabbed from a base by a player if the player is not on the same team as the base
				argu[0]=go.GetPhotonView().viewID;
				photonView.RPC("capture",PhotonTargets.All,argu);
				go.GetComponent<Condition>().setFlag(true); //The player picks up the flag
				holder.GetComponent<Movement>().takeFlag(this);
				return;
			}
		}
	}
	void playerReturn(GameObject go){ //We are being dropped off at a base
		 //If the player grabbing us is not currently holding a flag
		if (holder==null){ //We can be picked up if we are not held
			object[] argu = new object[1];
			argu[0]=go.GetPhotonView().viewID;
			photonView.RPC("capture",PhotonTargets.All,argu);
			return;
		}
		if (holder.GetComponent<Condition>() && holder.GetComponent<Condition>().team==go.GetComponent<HolderControl>().team){
			/*object[] argu = new object[1];
			argu[0]=false;
			holder.GetComponent<Condition>().photonView.RPC("setFlag",PhotonTargets.All,argu); //The player has dropped the flag. We can be dropped off at a base by a player
			*/holder.GetComponent<Condition>().setFlag(false);
			holder.GetComponent<Movement>().takeFlag(null);
			object[] argu = new object[1]; //as long as the player is on the same team as the base
			argu[0]=go.GetPhotonView().viewID;
			photonView.RPC("capture",PhotonTargets.All,argu);
			return;
		}
	}
	void OnCollisionEnter(Collision co){
		if (co.gameObject.GetPhotonView() && co.gameObject.GetPhotonView().isMine){ //If we are the collider
			if (co.gameObject.GetComponent<Condition>()){
				playerGrab(co.gameObject);
				return;
			}
			if (co.gameObject.GetComponent<HolderControl>()){
				playerReturn(co.gameObject);
				return;
			}
		}
	}
	void OnPhotonPlayerConnected(PhotonPlayer newPlayer){ //If someone is holding the flag, we send the holder, else, we send the current location
		if (photonView.isMine){
			if (holder!=null){ //If there is a holder
				object[] argu = new object[1];
				argu[0]=holder.GetPhotonView().viewID;
				photonView.RPC("capture",newPlayer,argu);
			}
			else{ //No holder
				object[] argu = new object[1];
				argu[0]=transform.position;
				photonView.RPC("setup",newPlayer,argu);
			}
		}
	}
	public PhotonView getPhotonView(){
		return photonView;	
	}
}
