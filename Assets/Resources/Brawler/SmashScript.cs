using UnityEngine;
using System.Collections;

public class SmashScript : Photon.MonoBehaviour {
	private Vector3 correctPlayerPos;
    private Quaternion correctPlayerRot;
	float time = .25f;
	float totalTime;
	GameObject parent;
	bool started=false;
	bool hit=false;
	bool on =true;
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
			time -= Time.deltaTime;
			if (time<=0 & photonView.isMine){
				PhotonNetwork.Destroy(gameObject);
			}
			if (started){
				transform.position=parent.transform.position;
				transform.Translate(new Vector3(0f,-1 * (transform.localScale.y+parent.transform.localScale.y/3),0f)); //The sum of half our parents radii and our length is how far out we are
				transform.RotateAround(parent.transform.position, new Vector3(0,1,0), -100/totalTime * Time.deltaTime);
			}
		}
		
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
		transform.localScale= new Vector3(.4f,.8f,.4f);
		transform.Translate(new Vector3(0f,-1 * (transform.localScale.y+parent.transform.localScale.y/2),0f));
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
    }
	[RPC]
	void doHit(int attackID){
		Movement mov =Master.Find(PhotonNetwork.player.ID).movement;
		if (mov.doHit(attackID)){
			mov.getCondition().setHealth(-14f);
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
				hit=true;
			}
		}
	}
}
