using UnityEngine;

public enum ShipClass { Capitol, SmallCapitol, Fighter, Bomber, Interceptor };

public class ShipStats : MonoBehaviour {
		
		//vital statatics
		public ShipClass shipClass;

		public float maxSpeed = 100f;
		public float mass = 100f;
		public float maxForce = 100f;
		
		public float laserRange = 10f;
		public float laserPower = 10f;
		public float missleRange = 50f;
		public float misslePower = 100f;
		
		public float maxShields = 0f;
		public float maxHull = 100f;
		
		public float firingCone = 0.9f;
}
