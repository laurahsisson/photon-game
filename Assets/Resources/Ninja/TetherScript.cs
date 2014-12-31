using UnityEngine;
using System.Collections;

public class TetherScript : Photon.MonoBehaviour {
	private Vector3 correctPlayerPos;
    private Quaternion correctPlayerRot;
	private Vector3 correctPlayerScale;
	float time = 0f;
	float totalTime;
	GameObject parent;
	bool started=false;
	int type;
	bool hit=false;
	float maxLength=1.5f;
	float width = .5f;
	float slowTime=0f; //This is the time in between the last onPhotonViewSerialize, used to set slow more accurately
	float lastTime=Time.time;
	GameObject target;
	PhotonView pv; //The PhotonView of the person we are have hit and are affecting
	Cooldown cooldown; //No PhotonView is necessary because it is setting its owners cooldown, same player
	int team;
	bool result;
	
	// Use this for initialization
	void Start () {
		totalTime=time;
	}
	// Update is called once per frame
	void Update () {
		if (photonView.isMine){ //Keep up the cooldown
			cooldown.setLock(Time.deltaTime);			
		}
		if (started){
			if (transform.localScale.y>maxLength &  photonView.isMine){ //Snap is we are too thin
				PhotonNetwork.Destroy(gameObject);	
			}
			if (!hit) {
				time += Time.deltaTime;
				if (started & !hit){
					float distance = transform.localScale.y+3f*Time.deltaTime;
					transform.localScale=new Vector3(width,distance,width);
					transform.position=parent.transform.position;
					transform.Translate(new Vector3(0,-1*transform.lossyScale.y,0)); //Move it forward so it doesn't stick out as it grows
				}
			}
			if (photonView.isMine && hit && !target){ //If the target no longer exists we destroy ourselves.
				PhotonNetwork.Destroy(gameObject);	
			}
			if (photonView.isMine && hit && !parent){
				PhotonNetwork.Destroy(gameObject);	
			}
			if (hit & target){ //This portion is the animation only
				transform.position=Vector3.Lerp(parent.transform.position,target.transform.position,.5f); //We want to stay in between the two
				transform.rotation = Quaternion.FromToRotation(Vector3.up, target.transform.position-parent.transform.position);
				float distance = Vector3.Distance(parent.transform.position,target.transform.position)/2f;
				transform.localScale= new Vector3(width,distance,width); //Stretch out
				if (photonView.isMine && pv.owner!=null & result){ //Slow the person regularly, on low intervals
					object[] argu = new object[4];
					argu[0]=slowTime*2;
					argu[1]="Tether";
					argu[2]=50f;
					argu[3]=false;
					pv.RPC("setMoveSpeed",pv.owner,argu);
				}
			}
		}
	}
	[RPC]
	public void setup(int parentID){
		parent=PhotonView.Find(parentID).gameObject;
		team = parent.GetComponent<NinjaMove>().getTeam();
		Debug.Log(team);
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
	}
	public void setup(GameObject gameobject, Cooldown cooldown){
		parent=gameobject;
		team = parent.GetComponent<NinjaMove>().getTeam();
		collider.isTrigger=true;
		started=true;
		this.cooldown=cooldown;
		cooldown.setCooldown(4f);
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
	public void sendTarget(int targetID){
		pv = PhotonView.Find(targetID);
		target=pv.gameObject;
		hit=true;
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){ //Only used to find how often we send messages
		slowTime=Time.time-lastTime;
		lastTime=Time.time;
    }
	[RPC]
	void doHit(int attackID){
		Movement mov = Master.Find(PhotonNetwork.player.ID).movement;
		if (!mov.doHit(attackID)){ //If we were blocked we break
			renderer.material.color = new Color(1,1,1,0); //Go invisible so it looks broken
			object[] argu = new object[1];
			argu[0] = false;
			photonView.RPC("sendResult",photonView.owner,argu);
		}
		else {
			object[] argu = new object[1];
			argu[0] = true;
			photonView.RPC("sendResult",photonView.owner,argu);
		}
	}
	[RPC]
	void sendResult(bool result){
		if (!result){
			PhotonNetwork.Destroy(gameObject);
		}
		if (result){
			this.result=true;
		}
	}
	void OnTriggerEnter(Collider co){
		if (started & photonView.isMine & !hit & co.gameObject!=parent){
			Condition cond = co.GetComponent<Condition>();
			Movement move = co.GetComponent<Movement>();
			if (move && move.getTeam()!=team){ //If they are a hittable object
				object[] argu = new object[1];
				argu[0]=parent.GetPhotonView().viewID;
				photonView.RPC("doHit",co.gameObject.GetPhotonView().owner,argu);
				argu[0]=move.photonView.viewID;
				photonView.RPC("sendTarget", PhotonTargets.All,argu); 
			}
		}
	}
}
