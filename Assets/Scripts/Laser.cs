using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {
	
	
	public GameObject bolt;
	public float last_fire;
	float cooldown = 1f;
	GameObject[] pool = new GameObject[5];

	// Use this for initialization
	void Start () {
		for (int i = 0; i < 5; i++){
			pool[i] = Instantiate(bolt,Vector3.zero, Quaternion.identity) as GameObject;
			pool[i].SetActive(false);
		}
	}
	
	public void Fire(Vector3 forward, float power, GameObject owner){
		if(Time.time - last_fire > 0.5f){
			foreach(GameObject b in pool){
				if(!b.activeSelf) {
					b.GetComponent<laser_bolt>().Fire(forward, transform.position, power, owner);
					last_fire = Time.time;
					break;
				}
			}
		}
	}
}
