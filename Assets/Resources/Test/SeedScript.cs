using UnityEngine;
using System.Collections;

public class SeedScript : Photon.MonoBehaviour {
	float velocity=5f;
	GameObject parent;
	float time=5f;
	// Use this for initialization
	void Start () {
		
		
	}
	
	// Update is called once per frame
	void Update () {
		if (photonView.isMine & parent) {
			time -= Time.deltaTime;
			if (time<=0){
				PhotonNetwork.Destroy(this.gameObject);
			}
			transform.Translate(new Vector3(0,0,velocity * Time.deltaTime));
			transform.LookAt(parent.transform);
		}
	}
	[RPC]
	void setup(int parentID){
		parent=PhotonView.Find(parentID).gameObject;
	}
	void OnTriggerEnter(Collider co){
		if (co.gameObject==parent & photonView.isMine){
			parent.GetComponent<Condition>().setHealth(5);
			PhotonNetwork.Destroy(this.gameObject);
		}
	}
}
