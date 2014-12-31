using UnityEngine;
using System.Collections;

public class BlastScript : Photon.MonoBehaviour {
	float velocity = 0;
	public bool started = false;
	private Vector3 correctPlayerPos;
    private Quaternion correctPlayerRot;
	float time = .8f;
	GameObject parent;
	bool hit;
	float charge;
	int team;
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
		transform.Translate(new Vector3(0f,-2f * velocity * Time.deltaTime,0f));
	}
	[RPC]
	public void setup(float charge, int team){
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
		started=true;
		this.charge=charge;
		velocity = 5  + 2.5f*charge/2;
		collider.isTrigger = true;
		transform.localScale= new Vector3(.35f + charge/2*.55f,.35f + charge/2*.55f,.35f + charge/2*.55f);
	}
	[RPC]
	void doHit(int attackID){
		Movement mov =Master.Find(PhotonNetwork.player.ID).movement;
		if (mov.doHit(attackID)){
			mov.getCondition().setHealth(-15f*charge/2f);
		}
	}
	public void setup(GameObject parent){
		this.parent=parent;	
		team=parent.GetComponent<CosmoMove>().getTeam();
	}
	void OnTriggerEnter(Collider co){
		if (started & photonView.isMine){
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
