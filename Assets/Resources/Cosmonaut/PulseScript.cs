using UnityEngine;
using System.Collections;

public class PulseScript : Photon.MonoBehaviour {
	GameObject parent;
	float time=1f;
	int team;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (photonView.isMine) {
			time -= Time.deltaTime;
			if (time<=0){
				PhotonNetwork.Destroy(this.gameObject);
			}
		}
		if (parent){ //If we have a parent, we follow them
			transform.position=parent.transform.position;	
		}
		if (time>.5f){
			float scale = transform.localScale.x;
			transform.localScale = new Vector3(scale+3f*Time.deltaTime,.4f,scale+3f*Time.deltaTime);
		}
		if (time<.5f){
			float scale = transform.localScale.x;
			transform.localScale = new Vector3(scale-3f*Time.deltaTime,.4f,scale-3f*Time.deltaTime);
		}
	}
	[RPC]
	void setup(int parentId){
		parent = PhotonView.Find(parentId).gameObject;
		team = parent.GetComponent<CosmoMove>().getTeam();
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
	void OnTriggerEnter(Collider co){
		if (photonView.isMine & parent){ //Make sure we do not hit our parent
			if (co.gameObject==parent){
				return;	
			}
			Movement move = co.GetComponent<Movement>();
			if (move && move.getTeam()!=team){
				object[] argu = new object[4];
				argu[0]=.5f;
				argu[1]="Pulse";
				argu[2]=5f;
				argu[3]=transform.position;
				PhotonView pv = co.GetComponent<PhotonView>();
				pv.RPC("setPush",pv.owner,argu);
				Debug.Log(pv.ownerId);
			}
		}
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
    }
}
