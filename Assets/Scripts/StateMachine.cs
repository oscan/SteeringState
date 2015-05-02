using UnityEngine;
using System.Collections;

public class StateMachine : MonoBehaviour {

	public State currentState;
	
	void Update(){
		if( currentState != null){
			currentState.Update(Time.deltaTime);
		}
	}
	
	public void changeState(State newState){
		if(currentState != null){
			currentState.ExitState();
		}
		currentState = newState;
		if(currentState != null){
			currentState.EnterState();
		}
	}
}
