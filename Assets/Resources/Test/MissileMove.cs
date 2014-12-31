using UnityEngine;
using System.Collections;

public class MissileMove : Photon.MonoBehaviour {
	float velocity = 0;
	public bool started = false;
	private Vector3 correctPlayerPos;
    private Quaternion correctPlayerRot;
	float time = 3f;
	GameObject parent;
	// Use this for initialization
	void Start () {
		//Debug.Log("Hello");
	}
	
	// Update is called once per frame
	void Update () {
		if (!photonView.isMine)
        {
            transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, this.correctPlayerRot, Time.deltaTime * 5);
        }
		else {
			time -= Time.deltaTime;
			if (time<=0){
				PhotonNetwork.Destroy(this.gameObject);
			}
			transform.Translate(new Vector3(0f,-1f * velocity * Time.deltaTime,0f));
		}
	}
	public void setup(float velocity){
		started=true;
		this.velocity=velocity;
		collider.isTrigger = true;
	}
	void OnTriggerEnter(Collider co){
		if (started & photonView.isMine){
			var cond = co.GetComponent<Condition>();
			if (cond!=null){
				var pv = co.GetComponent<PhotonView>();
				object[] arg = new object[3];
				arg[0]=5f;
				arg[1]="Missile";
				arg[2]=50f;
				pv.RPC("setMoveSpeed",PhotonTargets.Others,arg);
			}
		}
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Network player, receive data
            this.correctPlayerPos = (Vector3)stream.ReceiveNext();
            this.correctPlayerRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
