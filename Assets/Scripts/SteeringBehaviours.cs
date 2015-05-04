using UnityEngine;
using System.Collections.Generic;

//NOTE: comments denote changed code from original

public class SteeringBehaviours : MonoBehaviour {

	public Vector3 force; 
	public Vector3 velocity; 
	public float mass = 1f; 
	public float maxSpeed = 25f; 
	public GameObject target; 
	public Vector3 seekTargetPos;
	public bool seekEnabled, fleeEnabled, pursueEnabled, evadeEnabled, arriveEnabled, pathFollowEnabled, offsetPursuitEnabled, offsetPursuitPosition, seperationEnabled, cohesionEnabled, alignmentEnabled;
	public Vector3 offsetPursuitOffset;
	Path path = new Path();
	private List<GameObject> tagged  = new List<GameObject>();
	public float overlapRadius = 5f;
	public float maxForce = 100f;
	public float seperationWeight = 1f, cohesionWeight = 1f, alignmentWeight = 1f;
	
	public int turnDirection = 0;

	ShipStats stats;
	float myMaxSpeed = 100f;
	float myMass = 1f;
	float myMaxForce = 100f;
	
	List<GameObject> otherFighters;
	
	void Start()
	{
		stats = GetComponent<ShipStats>(); //Grab the force stats from the ship, otherwise use default
		if(stats != null){
			myMass = stats.mass/100f;
			myMaxSpeed = stats.maxSpeed;
			myMaxForce = stats.maxForce;
			
			if(gameObject.layer == LayerMask.NameToLayer("Fighters") ) {
				otherFighters = new List<GameObject>();
				GameObject[] imps = GameObject.FindGameObjectsWithTag("Empire");
				foreach(GameObject imp in imps){
					if(imp != gameObject && imp.layer == gameObject.layer){
						otherFighters.Add(imp);
					}
				}
				GameObject[] rebs = GameObject.FindGameObjectsWithTag("Rebels");
				foreach(GameObject reb in rebs){
					if(reb != gameObject && reb.layer == gameObject.layer){
						otherFighters.Add(reb);
					}
				}
			}
		} else {
			myMass = mass;
			myMaxForce = maxForce;
			myMaxSpeed = maxSpeed;
		}
		path.CreatePath(); 
		if(offsetPursuitPosition && target != null) {
			offsetPursuitOffset = transform.position - target.transform.position; 
		}
	}
	
	public void TurnOffAll()
	{
		seekEnabled = false;
		fleeEnabled = false;
		pursueEnabled = false;
		evadeEnabled = false;
		arriveEnabled = false;
		pathFollowEnabled = false;
		offsetPursuitEnabled = false;
		seperationEnabled = false;
		cohesionEnabled = false;
		alignmentEnabled = false;
	}
	
	void Update () 
	{
		ForceIntegrator();
	}

	void ForceIntegrator()
	{
		Vector3 accel = force / myMass;
		velocity += accel * Time.deltaTime;
		transform.position = transform.position + velocity * Time.deltaTime;
		force = Vector3.zero; 
		if(velocity.magnitude > float.Epsilon)
		{
			if(Vector3.Normalize(velocity) != Vector3.zero){
				transform.forward = Vector3.Normalize(velocity);
			}
		}
		velocity *= 0.99f; 
		force = CalculateWeightedPrioritised();
	}
	
	Vector3 CalculateWeightedPrioritised() 
	{
		Vector3 steeringForce = Vector3.zero;
		if(seekEnabled && target != null)
		{
			force = Seek(seekTargetPos) * 0.2f;
			if(AccumulateForce(ref steeringForce, force) == false) 
			{
				return steeringForce; 
			}
		}
		if(fleeEnabled && target != null)
		{
			force = Flee(target.transform.position) * 0.6f;
			if(AccumulateForce(ref steeringForce, force) == false) 
			{
				return steeringForce; 
			}
		}
		if(arriveEnabled && target != null)
		{
			force = Arrive(target.transform.position) * 0.3f; 
			if(AccumulateForce(ref steeringForce, force) == false)
			{
				return steeringForce;
			}
		}
		if(pursueEnabled && target != null)
		{
			force = Pursue(target) * 0.3f;
			if(AccumulateForce(ref steeringForce, force) == false)
			{
				return steeringForce;
			}
		}
		if(evadeEnabled && target != null)
		{
			force = Evade(target) * 0.3f;
			if(AccumulateForce(ref steeringForce, force) == false)
			{
				return steeringForce;
			}
		}
		if(offsetPursuitEnabled && target != null)
		{
			force = OffsetPursuit(offsetPursuitOffset) * 0.6f;
			if(AccumulateForce(ref steeringForce, force) == false)
			{
				return steeringForce;
			}
		}
		if(pathFollowEnabled)
		{
			force = PathFollow() * 0.6f;
			if(AccumulateForce(ref steeringForce, force) == false)
			{
				return steeringForce;
			}
		}
		int taggedCount = 0;
		if(seperationEnabled || cohesionEnabled || alignmentEnabled)
		{
			taggedCount = TagNeighbours(50);
		}	
		if(seperationEnabled && taggedCount > 0) 
		{
			force = Seperation() * seperationWeight;
			if(AccumulateForce(ref steeringForce, force) == false)
			{
				return steeringForce;
			}
		}
		if(cohesionEnabled && taggedCount > 0)
		{
			force = Cohesion() * cohesionWeight;
			if(AccumulateForce(ref steeringForce, force) == false)
			{
				return steeringForce;
			}
		}
		if(alignmentEnabled && taggedCount > 0)
		{
			force = Alignment() * alignmentWeight;
			if(AccumulateForce(ref steeringForce, force) == false)
			{
				return steeringForce;
			}
		}
	 	return steeringForce;
	}
	
	bool AccumulateForce(ref Vector3 runningTotal, Vector3 force)
	{
		float soFar = runningTotal.magnitude;
		float remaining = myMaxForce - soFar; 
		if(remaining <= 0)
		{
			return false;
		}
		float toAdd = force.magnitude;
		if(toAdd < remaining) 
		{
			runningTotal += force;
		}
		else
		{
			runningTotal += force.normalized * remaining;
		}
		return true; 
	}
	
	public void RandomTurn(){
		turnDirection = Random.Range(0,4);
	}
	
	Vector3 getTurnDirection() { //turning direction. want a gradual arc, not a sudden about turn
		Vector3 t = transform.forward;
		switch(turnDirection) {
			case 0 : t = transform.right + transform.up*0.2f; break;
			case 1 : t = -transform.right + transform.up*0.2f; break;
			case 2 : t = transform.right - transform.up*0.2f; break;
			case 3 : t = -transform.right - transform.up*0.2f; break;
		}
		return t;
	}
	
	Vector3 Seek(Vector3 target) 
	{
		Vector3 desiredVel = (target - transform.position); 
		if(Vector3.Dot(desiredVel, transform.forward) < -0.3) {
			//the target is behind us, do a hard arc turn
			desiredVel = getTurnDirection();
		}
		desiredVel.Normalize();
		desiredVel *= myMaxSpeed;
		return (desiredVel - velocity);
	}
	
	Vector3 Flee(Vector3 target)
	{
		Vector3 desiredVel = (target - transform.position);
		if(Vector3.Dot(desiredVel, transform.forward) > 0.5) {
			//the target is ahead of us, do a hard arc turn
			desiredVel = getTurnDirection();
		}
		desiredVel.Normalize();
		desiredVel *= myMaxSpeed;
		return velocity - desiredVel;
	}
	
	Vector3 Pursue(GameObject target) 
	{
		Vector3 desiredVel = target.transform.position - transform.position; 
		float distance = desiredVel.magnitude;
		float lookAhead = distance / myMaxSpeed;
		Vector3 desPos = target.transform.position+(lookAhead * target.GetComponent<SteeringBehaviours>().velocity); 
		return Seek (desPos);
	}
	Vector3 Evade(GameObject target) //Evade is the fleeing version of Prusue
	{
		Vector3 desiredVel = target.transform.position - transform.position; 
		float distance = desiredVel.magnitude; 
		float lookAhead = distance / myMaxSpeed; 
		Vector3 desPos = target.transform.position+(lookAhead * target.GetComponent<SteeringBehaviours>().velocity);
		return Flee (desPos); 
	}
	
	Vector3 Arrive(Vector3 targetPos)
	{
		Vector3 toTarget = targetPos - transform.position;
		float distance = toTarget.magnitude;
		if(distance <= 1f)
		{
			return Vector3.zero;
		}
		float slowingDistance = 8.0f;
		float decelerateTweaker = myMaxSpeed / 10f;
		float rampedSpeed = myMaxSpeed * (distance / slowingDistance * decelerateTweaker); 
		float newSpeed = Mathf.Min (rampedSpeed, myMaxSpeed); 
		Vector3 desiredVel = newSpeed * toTarget.normalized; 
		return desiredVel - velocity; 
	}
	
	Vector3 PathFollow() 
	{
		float distance = (transform.position - path.NextWaypoint()).magnitude;
		if(distance < 0.5f)
		{
			path.AdvanceWaypoint();
		}
		if(!path.looped && path.IsLastCheckpoint())
		{
			return Arrive (path.NextWaypoint());
		}
		else
		{
			return Seek (path.NextWaypoint());
		}
	}
	
	Vector3 OffsetPursuit(Vector3 offset)
	{
		Vector3 desiredVel = Vector3.zero;
		if(target != null && target.activeSelf) {
			desiredVel = target.transform.TransformPoint(offset);
			float distance = (desiredVel - transform.position).magnitude;
			float lookAhead = distance / myMaxSpeed;
			SteeringBehaviours osb = target.GetComponent<SteeringBehaviours>();
			if(osb != null){
				desiredVel = desiredVel +(lookAhead * osb.velocity);
			}
		}
		return Arrive (desiredVel);
		
	}
	
	Vector3 Seperation() 
	{
		Vector3 steeringForce = Vector3.zero;
		for(int i = 0; i < tagged.Count; i++)
		{
			GameObject entity = tagged[i];
			if(entity != null)
			{
				Vector3 toEntity = this.transform.position - entity.transform.position;
				toEntity.Normalize();
				float distance = toEntity.magnitude;
				steeringForce += toEntity / distance;
			}
		}
		return steeringForce;
	}
	 
	Vector3 Cohesion()
	{
		Vector3 steeringForce = Vector3.zero;
		Vector3 centerOfMass = Vector3.zero;
		int taggedCount = 0;
		foreach(GameObject boid in tagged)
		{
			centerOfMass += boid.transform.position;
			taggedCount ++;
		}
		if(taggedCount > 0)
		{
			centerOfMass /= taggedCount;
			if(centerOfMass.magnitude == 0)
			{
				steeringForce = Vector3.zero;
			}
			else
			{
				steeringForce = Seek (centerOfMass);
				steeringForce.Normalize();
			}
		}
		return steeringForce;
	}
	
	Vector3 Alignment() 
	{
		Vector3 steeringForce = Vector3.zero;
		int taggedCount = 0;
		foreach(GameObject boid in tagged)
		{
			steeringForce += boid.transform.forward;
			taggedCount++;
		}
		if(taggedCount > 0)
		{
			steeringForce /= taggedCount;
		}
		return steeringForce;
	}
	
	int TagNeighbours(float radius)
	{
		tagged.Clear ();
		if(gameObject.layer == LayerMask.NameToLayer("Fighters") ) {
			foreach(GameObject boid in otherFighters)
			{
				if(boid != this.gameObject)
				{
					if((this.transform.position - boid.transform.position).magnitude < radius)
					{
						tagged.Add (boid);
					}
				}
			}
		}
		return tagged.Count;
	}
	
	void NoOverlap()
	{
		foreach(GameObject boid in tagged)
		{
			Vector3 toOther = boid.transform.position - transform.position;
			float distance = toOther.magnitude;
			toOther.Normalize();
			float overlap = overlapRadius + boid.GetComponent<SteeringBehaviours>().overlapRadius - distance;
			if(overlap >= 0)
			{
				boid.transform.position = boid.transform.position + toOther * overlap;
			}
		}
	}
}
