using UnityEngine;
using System.Collections;

public class FlipScript : Photon.MonoBehaviour {
	private Vector3 correctPlayerPos;
    private Quaternion correctPlayerRot;
	float time = 0;
	float totalTime;
	GameObject parent;
	bool started=false;
	bool hit=false;
	bool on =true;
	float width = .4f;
	int team;
	// Use this for initialization
	void Start () {
		totalTime=time;
	}
	
	// Update is called once per frame
	void Update () {
		if (parent==null & photonView.isMine){
			PhotonNetwork.Destroy(gameObject);
			on=false;
		}
		if (on){
			Debug.Log (transform.localScale.y);
			time += Time.deltaTime;
			if (time>=.5f & photonView.isMine){
				PhotonNetwork.Destroy(gameObject);
			}
			if (started){
				float distance = transform.localScale.y+4f*Time.deltaTime;
				transform.localScale=new Vector3(width,distance,width);
				transform.position=parent.transform.position;
				transform.Translate(new Vector3(0,-1*transform.lossyScale.y,0)); //Move it forward so it doesn't stick out as it grows
			}
		}
		
	}
	public void setup(GameObject parent){
		this.parent=parent;
		team = parent.GetComponent<BrawlerMove>().getTeam();
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
		collider.isTrigger=true;
		started=true;
	}
	[RPC]
	public void setup(int parentID){;
		parent=PhotonView.Find(parentID).gameObject;
		team = parent.GetComponent<BrawlerMove>().getTeam();
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
		collider.isTrigger=true;
		started=true;
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
    }
	[RPC]
	void doHit(int attackID){
		Movement mov =Master.Find(PhotonNetwork.player.ID).movement;
		if (mov.doHit(attackID)){
			Vector3 parentPos = parent.transform.position; //The brawler
			Vector3 targetPos = mov.transform.position; //Us
			mov.getCondition().setPull(.25f,"BrawlerFlip",20f,new Vector3(3f*(parentPos.x+(parentPos.x-targetPos.x)),3f,3f*(parentPos.z+(parentPos.z-targetPos.z))));
		}
	}
	void OnTriggerEnter(Collider co){
		if (started & photonView.isMine & !hit & co.gameObject!=parent){
			Movement move = co.GetComponent<Movement>();
			if (move && move.getTeam()!=team){
				PhotonView pv = co.GetComponent<PhotonView>();
				object[] argu = new object[1];
				argu[0] = parent.GetPhotonView().viewID;
				photonView.RPC("doHit",pv.owner,argu);
			}
		}
	}
}
