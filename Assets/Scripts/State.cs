using UnityEngine;
using System.Collections;

public abstract class State {

	protected GameObject gameObject;
	public State(GameObject go){
		gameObject = go;
	}
	
	public abstract void EnterState();
	public abstract void Update(float dt);
	public abstract void ExitState();
}
