using UnityEngine;
using System.Collections;

public abstract class Movement : Photon.MonoBehaviour {
	public abstract void setup(string username, int type, int team, Master master, Color c);
	
	public abstract Master getMaster();
	
	public abstract int getTeam();
	
	public abstract void setup(string username, int team, float r, float g, float b);
	
	public abstract void pause();
	[RPC]
	public void setColor(float r, float g, float b){
		renderer.material.color= new Color(r,g,b,renderer.material.color.a);
	}
	public abstract void takeFlag(FlagControl fc);
	public abstract bool doHit(int attackID); //Aggressor's id, returns true if the target can be hit
	public abstract Condition getCondition();
}
