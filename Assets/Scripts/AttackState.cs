using UnityEngine;
using System.Collections;

public class AttackState : State {
	
	GameObject attack_target;
	ShipStats stats;
	SteeringBehaviours sb;
	FlightManager fm;
	StateMachine sm;
	
	public AttackState(GameObject myGameObject, GameObject target):base (myGameObject)
	{
		attack_target = target;
		//grab needed scripts
		stats = myGameObject.GetComponent<ShipStats>();
		sb = myGameObject.GetComponent<SteeringBehaviours>();
		fm = myGameObject.GetComponent<FlightManager>();
		sm = myGameObject.GetComponent<StateMachine>();
		sb.target = target;
	}
	
	public override void EnterState()
	{
		if(attack_target != null) {
			sb.target = attack_target;
		}
		sb.pursueEnabled = true;
		sb.RandomTurn();
	}
	
	public override void Update(float dt)
	{
		if(sb.target != null && sb.target.activeSelf) {
			Vector3 toTarget = sb.target.transform.position - gameObject.transform.position;
			float dist = toTarget.magnitude;
			if(sb.pursueEnabled && dist < stats.laserRange/2) {
				//getting a bit too close for comfort
				sm.changeState(new AttackEvadeState(gameObject, attack_target));
			}
			if(fm.lasers.Length > 0) {
				if(dist < stats.laserRange && Vector3.Dot (toTarget, gameObject.transform.forward) > stats.firingCone){
					//pew pew pew
					fm.FireLasers(gameObject.transform.forward);
				}
			}
			if(fm.missles.Length > 0) {
				if(dist < stats.missleRange && Vector3.Dot (toTarget, gameObject.transform.forward) > stats.firingCone){
					//pew pew pew
					fm.FireMissles(gameObject.transform.forward);
				}
			}
		} else {
			fm.LookingForOrders(0);
		}
	}
	
	public override void ExitState()
	{
		gameObject.GetComponent<SteeringBehaviours>().TurnOffAll();
	}
}
