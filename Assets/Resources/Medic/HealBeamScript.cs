using UnityEngine;
using System.Collections;

public class HealBeamScript : Photon.MonoBehaviour {
	private Vector3 correctPlayerPos;
    private Quaternion correctPlayerRot;
	private Vector3 correctPlayerScale;
	float time = 0f;
	float totalTime;
	GameObject parent;
	bool started=false;
	int type;
	bool hit=false;
	float maxLength=2f;
	float width = .5f;
	float slowTime=0f; //This is the time in between the last onPhotonViewSerialize, used to set slow more accurately
	float lastTime=Time.time;
	GameObject target;
	PhotonView pv; //The PhotonView of the person we are have hit and are affecting
	Cooldown cooldown; //No PhotonView is necessary because it is setting its owners cooldown, same player
	int team;
	
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
			if (photonView.isMine & hit & !target){ //If the target no longer exists we destroy ourselves.
				PhotonNetwork.Destroy(gameObject);	
			}
			if (photonView.isMine && !parent){
				PhotonNetwork.Destroy(gameObject);	
			}
			if (hit & target){ //This portion is the animation only
				float deltaX = target.transform.position.x - parent.transform.position.x;
				float deltaY = target.transform.position.z - parent.transform.position.z;
				float angle = Mathf.Atan(deltaY / deltaX) * Mathf.Rad2Deg;
				transform.position=Vector3.Lerp(parent.transform.position,target.transform.position,.5f); //We want to stay in between the two
				transform.rotation = Quaternion.Euler(new Vector3(90f,0,angle-90f));
				float distance = Vector3.Distance(parent.transform.position,target.transform.position)/2f;
				transform.localScale= new Vector3(width,distance,width); //Stretch out
				if (photonView.isMine & pv.owner!=null){ //Heal the person regularly, on low intervals
					object[] argu = new object[1];
					argu[0]=2*slowTime; //2 HP is healed per second, roughly
					pv.RPC("setHealth",pv.owner,argu);	
				}
			}
		}
	}
	[RPC]
	public void setup(int parentID){
		parent=PhotonView.Find(parentID).gameObject;
		started=true;
		team = parent.GetComponent<MedicMove>().getTeam();
		switch (team){
		case 0:
			renderer.material.color = new Color(1f,.4f,.4f);
			break;
		case 1:
			renderer.material.color = new Color(.4f,.4f,1f);
			break;
		default:
			renderer.material.color = new Color(.4f,1f,.4f);
			break;
		}
	}
	public void setup(GameObject gameobject, Cooldown cooldown){
		parent=gameobject;
		team = parent.GetComponent<MedicMove>().getTeam();
		switch (team){
		case 0:
			renderer.material.color = new Color(1f,.4f,.4f);
			break;
		case 1:
			renderer.material.color = new Color(.4f,.4f,1f);
			break;
		default:
			renderer.material.color = new Color(.4f,1f,.4f);
			break;
		}
		collider.isTrigger=true;
		started=true;
		this.cooldown=cooldown;
		cooldown.setCooldown(5f);
	}
	
	[RPC]
	public void sendTarget(int targetID){
		target=PhotonView.Find(targetID).gameObject;
		hit=true;
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){ //Only used to find how often we send messages
		slowTime=Time.time-lastTime;
		lastTime=Time.time;
		
    }
	public GameObject getTarget(){
		return target;
	}
	void OnTriggerEnter(Collider co){
		if (started & photonView.isMine & !hit & co.gameObject!=parent){
			Movement move = co.GetComponent<Movement>();
			if (move && move.getTeam()==team){ //If they are a hittable object
				pv = co.gameObject.GetPhotonView();
				object[] argu = new object[1];
				argu[0]=pv.viewID;
				photonView.RPC("sendTarget", PhotonTargets.All,argu);
			}
		}
	}
}
