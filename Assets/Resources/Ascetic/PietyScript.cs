using UnityEngine;
using System.Collections;

public class PietyScript : Photon.MonoBehaviour {
	float velocity = 12f;
	float time = .8f;
	GameObject parent;
	bool hit=false;
	int team;
	bool started=false;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (photonView.isMine & started) {
			time -= Time.deltaTime;
			if (time<=0 & ! hit){
				PhotonNetwork.Destroy(this.gameObject);
			}
			
		}
		transform.Translate(new Vector3(0f,-1f * velocity * Time.deltaTime,0f));
	}
	public void setup(GameObject gameobject){
		parent=gameobject;
		team = parent.GetComponent<AsceticMove>().getTeam();
		collider.isTrigger=true;
		started=true;
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
		if (mov.doHit(attackID)){
			mov.getCondition().setTaunt(2f,"PietyTaunt",attackID);
		}
	}
	void OnTriggerEnter(Collider co){
		if (photonView.isMine & started){ //Set up our parent
			Movement move = co.GetComponent<Movement>();
			if (move && move.getTeam()!=team){
				PhotonView pv = co.GetComponent<PhotonView>();
				object[] argu = new object[1];
				argu[0] = parent.GetPhotonView().viewID;
				photonView.RPC("doHit",pv.owner,argu);
				PhotonNetwork.Destroy(this.gameObject);
			}
		}
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
    }
}
