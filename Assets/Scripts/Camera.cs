using UnityEngine;
using System.Collections;

public class Camera : MonoBehaviour {
	
	SteeringBehaviours sb;
	// Use this for initialization
	void Start () {
		sb = GetComponent<SteeringBehaviours>();
		JumpToRandom();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)){
			JumpToRandom();
		}
	}
	
	void JumpToRandom(){
		string nexttag = "Rebels";
		if(sb.target){
			if(sb.target.tag == "Rebels"){
				nexttag = "Empire";
			}
		}
		
		GameObject[] objs = GameObject.FindGameObjectsWithTag(nexttag);
		
		if(objs.Length > 0){
			GameObject target = objs[Random.Range(0,objs.Length-1)];
			if(target && target.activeSelf){
				sb.target = target;
				transform.position = target.transform.position + sb.offsetPursuitOffset;
				
			}
		}		
	}
}
