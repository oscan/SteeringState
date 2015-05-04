using UnityEngine;
using System.Collections.Generic;

public enum Orders { None, Patrol, AttackBombers, DefendBombers, AttackCapitol, DefendCapitol, AttackFighters, DefentFighters, SlefDefence }

public class FlightManager : MonoBehaviour {
	
	public Orders[] orders;
	public Orders currentOrders;
	StateMachine sm;
	SteeringBehaviours sb;
	public ShipStats stats;
	public Laser[] lasers;
	public Missle[] missles;
	public float shields = 0f;
	public float hull = 0f;
	public List<GameObject> attackers = new List<GameObject>();

	float last_orders;
	
	void Awake () {
		sm = GetComponent<StateMachine>();
		sb = GetComponent<SteeringBehaviours>();
		stats = GetComponent<ShipStats>();
		sm.changeState(new AttackState(gameObject, null));
		
		shields = stats.maxShields;
		hull = stats.maxHull;
	}
	void Start () {
		//get initial orders
		LookingForOrders(0);
	}
	
	void Update () {
		if(stats.maxShields > 0) {
			shields += Time.deltaTime * 0.3f;
			shields = Mathf.Min(shields,stats.maxShields);
		}
		if(Time.time - last_orders > 10f){ //re-evaluate orders every 10 seconds. (i.e. don't get too sidetracked from your bombing run because you were attacked)
			LookingForOrders(0);
		}
	}
	
	public void LookingForOrders(int i){
		if(i >= orders.Length){
			return;
		}
		
		//grab target based on Orders hierarchy. (e.g bombers for after Capitol ships first, see inspector)
		Orders order = orders[i];
		currentOrders = Orders.None;
		sb.target = null;
		
		string otherTag = gameObject.tag == "Rebels" ? "Empire" : "Rebels";
		GameObject[] myTeam;
		GameObject[] otherTeam;
		List<GameObject> attacked = new List<GameObject>();
		List<GameObject> undefended = new List<GameObject>();
		List<GameObject> unattacked = new List<GameObject>();
		List<GameObject> targets = new List<GameObject>();
		GameObject target = null;
		float farthest = Mathf.Infinity;
		switch(order) {
			case Orders.AttackBombers :
				otherTeam = GameObject.FindGameObjectsWithTag(otherTag);
				FilterTeam(otherTeam, ShipClass.Bomber, true, ref targets);
				foreach(GameObject tar in targets) {
					float dist = Vector3.Distance(tar.transform.position, transform.position);
					if(dist < farthest){
						target = tar;
						farthest = dist;
					}
				}
			break;
			case Orders.AttackCapitol :
				otherTeam = GameObject.FindGameObjectsWithTag(otherTag);
				FilterTeam(otherTeam, ShipClass.Capitol, false, ref targets);
				foreach(GameObject tar in targets) {
					float dist = Vector3.Distance(tar.transform.position, transform.position);
					if(dist < farthest){
						target = tar;
						farthest = dist;
					}
				}
			break;
			case Orders.AttackFighters :
				otherTeam = GameObject.FindGameObjectsWithTag(otherTag);
				FilterTeam(otherTeam, ShipClass.Fighter, true, ref targets); //find all fighters on other team
				foreach(GameObject tar in targets) {
					if(tar.activeSelf) {
						float dist = Vector3.Distance(tar.transform.position, transform.position);
						if(dist < farthest){ //attack the nearest
							target = tar;
							farthest = dist;
						}
					}
				}
			break;
			case Orders.DefendBombers :
				//find all bombers on my team.
				myTeam = GameObject.FindGameObjectsWithTag(gameObject.tag);
				List<GameObject> bombers = new List<GameObject>();
				FilterTeam(myTeam, ShipClass.Bomber, true, ref bombers); //find all the bombers on the team
				Undefended(bombers, ref undefended); //see if any are being attacked unchecked.
				if(undefended.Count > 0){
					target = Defend(undefended); //go after an unopposed attacked
				} else {
					target = Defend(bombers); //otherwise pick a random.
				}
			break;
			case Orders.DefendCapitol :
				myTeam = GameObject.FindGameObjectsWithTag(gameObject.tag);		
				FilterTeam(myTeam, ShipClass.Capitol, false, ref attacked);
				Undefended(attacked, ref undefended);
				if(undefended.Count > 0){
					target = Defend(undefended);
				} else {
					target = Defend(attacked);
				}
			break;
			case Orders.DefentFighters :
				myTeam = GameObject.FindGameObjectsWithTag(gameObject.tag);		
				FilterTeam(myTeam, ShipClass.Fighter, true, ref attacked);
				Undefended(attacked, ref undefended);
				if(undefended.Count > 0){
					target = Defend(undefended);
				} else {
					target = Defend(attacked);
				}
			break;
			case Orders.Patrol:
				sm.changeState(new PatrolState(gameObject));
				currentOrders = Orders.Patrol;
			break;
		}
		if(target != null && target.activeSelf){
			FlightManager tfm = target.GetComponent<FlightManager>();
			if(tfm != null){
				currentOrders = order;
				sm.changeState(new AttackState(gameObject, target));
				tfm.attackers.Add(gameObject);
			} else {
				target = null;
			}
		}
		if(currentOrders == Orders.None){
			LookingForOrders(++i);
		}
		last_orders = Time.time;
	}
	
	void FilterTeam(GameObject[] team, ShipClass _class, bool isFighter, ref List<GameObject> bucket) {
		foreach(GameObject teammem in team){
			if(teammem != gameObject){
				if(teammem.layer == LayerMask.NameToLayer(isFighter?"Fighters":"Ships")){
					FlightManager ofm = teammem.GetComponent<FlightManager>();
					if(ofm != null) {
						if(ofm.stats.shipClass == _class){
							bucket.Add(teammem);
						}
					}
				}
			}
		}
	}
	
	void Undefended(List<GameObject> bucket, ref List<GameObject> undef){
		foreach(GameObject o in bucket){
			FlightManager ofm = o.GetComponent<FlightManager>();
			foreach(GameObject a in ofm.attackers){
				FlightManager afm = a.GetComponent<FlightManager>();
				if(afm.attackers.Count == 0){
					undef.Add(o);
				}
			}
		}
	}
	
	void Unattacked(List<GameObject> bucket, ref List<GameObject> unatt){
		foreach(GameObject o in bucket){
			FlightManager ofm = o.GetComponent<FlightManager>();
			if(ofm.attackers.Count == 0) {
				unatt.Add(o);
			}
		}
	}
	
	GameObject RandomTarget(List<GameObject> bucket) {
		if(bucket.Count > 0) {
			if(bucket.Count == 1) {
				return bucket[0];
			} else {
				int r = Random.Range(0,bucket.Count);
				return bucket[r];
			}
		}
		return null;
	}
	
	GameObject Defend(List<GameObject> prey) {
		if(prey.Count > 0){
			GameObject p = prey[Random.Range(0, prey.Count)];
			FlightManager pfm = p.GetComponent<FlightManager>();
			if(pfm != null){
				if(pfm.attackers.Count > 0){
					return RandomTarget(pfm.attackers);
				}
			}
		}
		return null;
	}
	
	//FIRE!
	public void FireLasers(Vector3 forward) {
		foreach(Laser laser in lasers){
			laser.Fire(forward, stats.laserPower, gameObject);
		}
	}
	public void FireMissles(Vector3 forward) {
		foreach(Missle mis in missles){
			mis.Fire(forward, stats.misslePower, gameObject);
		}
	}
	
	//got hit by lasers or missles. Fighters take evasive action and engage attacker.
	public void TakeTamage(float p, GameObject owner) {
		if(stats.shipClass != ShipClass.Capitol) {
			sm.changeState(new AttackEvadeState(gameObject, owner));
		}
		float rem = 0f;
		if(shields > p){
			shields -= p;
		} else {
			p -= shields;
			shields = 0;
			hull -= p;
		}
		if(hull <= 0){
			Explode();
		}
	}
	public void Explode(){
		//Invoke("CleanUp", 2f);
		//was going to make the section of the ships fly off. alas no time.
		gameObject.SetActive(false);
	}
	public void CleanUp(){
		gameObject.SetActive(false);
	}
}
