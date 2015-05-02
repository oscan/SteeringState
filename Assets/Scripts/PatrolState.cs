using UnityEngine;
using System.Collections;

public class PatrolState : State {
	
	public float changeStateTime, changeStateTimer = 5f;

	public PatrolState(GameObject myGameObject):base (myGameObject)
	{
		
	}
	
	public override void EnterState()
	{
		//This is where you toggle the steering behaviours ON!
		gameObject.GetComponent<SteeringBehaviours>().pathFollowEnabled = true;
		gameObject.GetComponent<SteeringBehaviours>().seperationEnabled = true;
		gameObject.GetComponent<SteeringBehaviours>().cohesionEnabled = true;
		gameObject.GetComponent<SteeringBehaviours>().alignmentEnabled = true;
		Debug.Log("Boids are patrolling!");
	}
	
	public override void Update(float dt)
	{
		changeStateTime += dt;
		if(changeStateTime >= changeStateTimer)
		{
			//gameObject.GetComponent<StateMachine>().changeState (new TrackState(gameObject));
		}	
	}
	
	public override void ExitState()
	{
		gameObject.GetComponent<SteeringBehaviours>().TurnOffAll();
	}
}
