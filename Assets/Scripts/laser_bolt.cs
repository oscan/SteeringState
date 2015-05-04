using UnityEngine;
using System.Collections;

public class laser_bolt : MonoBehaviour {

	public float range;
	public float speed;
	public float power;
	public GameObject owner;
	Vector3 start_pos;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate(transform.forward * Time.deltaTime * speed, Space.World);
		if(Vector3.Distance(start_pos, transform.position) > range) {
			gameObject.SetActive(false);
		}
	}
	
	public void Fire(Vector3 forward, Vector3 pos, float p, GameObject _owner) {
		owner = _owner;
		transform.forward = forward;
		transform.position = pos;
		start_pos = transform.position;
		gameObject.SetActive(true);
		power = p;
	}
	
	void OnTriggerEnter(Collider other) {
		int layer = other.transform.gameObject.layer;
		string tag = other.transform.gameObject.tag;
		if(layer == LayerMask.NameToLayer("Fighters") || layer == LayerMask.NameToLayer("Ships")){
			FlightManager fm = other.transform.gameObject.GetComponent<FlightManager>();
			if(fm != null){
				if(tag != gameObject.tag) {
					fm.TakeTamage(power, owner);
					gameObject.SetActive(false);
				}
			}
		}
		
	}
}
