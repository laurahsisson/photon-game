using UnityEngine;
using System.Collections;

public class PhaserScript : Photon.MonoBehaviour {
	float velocity = 10f;
	private Vector3 correctPlayerPos;
    private Quaternion correctPlayerRot;
	float time = 1f;
	GameObject parent;
	bool hit;
	int team;
	bool started;
	Cooldown cooldown;
	// Use this for initialization
	void Start () {
		//Debug.Log("Hello");
	}
	
	// Update is called once per frame
	void Update () {
		if (photonView.isMine & started) {
			time -= Time.deltaTime;
			if (time<=0){
				PhotonNetwork.Destroy(this.gameObject);
			}
			
		}
		transform.Translate(new Vector3(0f,-1f * velocity * Time.deltaTime,0f));
	}
	public void setup(GameObject gameobject, Cooldown cooldown){
		parent=gameobject;
		team = parent.GetComponent<CosmoMove>().getTeam();
		collider.isTrigger=true;
		started=true;
		this.cooldown=cooldown;
	}
	[RPC]
	public void setup(int team){
		switch (team){
		case 0:
			renderer.material.color = new Color(.7f,0,0);
			break;
		case 1:
			renderer.material.color = new Color(0,0,.7f);
			break;
		default:
			renderer.material.color = new Color(0,.7f,0);
			break;
		}
	}
	[RPC]
	void doHit(int attackID){
		Movement mov =Master.Find(PhotonNetwork.player.ID).movement;
		object[] argu = new object[1];
		argu[0] = false;
		if (mov.doHit(attackID)){
			mov.getCondition().setHealth(-8f);
			argu[0] = true;
		}
		photonView.RPC("doDestroy",photonView.owner,argu);	
	}
	[RPC]
	void doTransparent(){
		renderer.material.color = new Color(0f,0f,0f,0f);
	}
	[RPC]
	void doDestroy(bool hasHit){
		if (hasHit){
			cooldown.changeCooldown(-1f);
		}
		PhotonNetwork.Destroy(gameObject);
	}
	void OnTriggerEnter(Collider co){ 
		if (photonView.isMine & started){ //On hit we go invisible, send a message, recieve the reply, destroy ourself
			Movement move = co.GetComponent<Movement>();
			if (move && move.getTeam()!=team & !hit){
				hit=true;
				PhotonView pv = co.GetComponent<PhotonView>();
				photonView.RPC("doTransparent",PhotonTargets.Others); //Because we have to recieve a message from the person we hit
				object[] argu = new object[1];
				argu[0] = parent.GetPhotonView().viewID;
				photonView.RPC("doHit",pv.owner,argu);
				renderer.material.color = new Color(0f,0f,0f,0f); //We go invisible and pretend that we were destroyed
				time=10f; //Make sure we dont time out before we get the relays
			}
		}
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
    }
}
