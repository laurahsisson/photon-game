using UnityEngine;
using System.Collections;

public class SacrificeScript : Photon.MonoBehaviour {
	float velocity = 12f;
	float time = .8f;
	GameObject parent;
	bool hit;
	int team;
	bool started;
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
			renderer.material.color = new Color(1f,0,0);
			break;
		case 1:
			renderer.material.color = new Color(0,0,1f);
			break;
		default:
			renderer.material.color = new Color(0,1f,0);
			break;
		}
	}
	void OnTriggerEnter(Collider co){
		if (photonView.isMine & started){ //Set up our parent
			Movement move = co.GetComponent<Movement>();
			if (move && co.gameObject!=parent){
				PhotonView pv = co.GetComponent<PhotonView>();
				object[] argu = new object[4];
				argu[0]=3f;
				argu[1]="SacrificeDoT";
				argu[2]=6f;
				argu[3]=true;
				pv.RPC("setDoT",pv.owner,argu);
				argu= new object[1];
				argu[0]=-8;
				pv.RPC("setHealth",pv.owner,argu);
				PhotonNetwork.Destroy(this.gameObject);
			}
		}
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
    }
}
