using UnityEngine;
using System.Collections;

public class csEnemyHealth : MonoBehaviour {

	[HideInInspector]
	public int m_Health = 100;
	public int m_ScoreValue = 20;
	public float m_SinkSpeed = 2.5f;
	public ParticleSystem WaterEffect;
	public ParticleSystem HitEffect;
	public ParticleSystem FireEffect;
	public ParticleSystem FireEffect2;
	public GameObject Explosion;
	
	Vector3 OriginalPos;
	Quaternion OriginalRotation;
	[HideInInspector]
	public bool isDead = true;

	void Awake()
	{
		OriginalPos = transform.position;
		OriginalRotation = transform.rotation;
	}

	void Update()
	{
		//Check running state
		if (csManagerClass.ManagerClass.isGameRunning == false)
			return;

		//Check object state
		if (isDead) {
			gameObject.SetActive (false);
		}
	}

	//Reset object via new position, new rotation info
	public void SetInit(Vector3 newPos, Quaternion newRot)
	{
		transform.position = newPos;
		transform.rotation = newRot;
		SameInitProperties ();
	}

	//Rest object via saved position, rotation info
	public void SetInit()
	{
		transform.position = OriginalPos;
		transform.rotation = OriginalRotation;
		SameInitProperties ();
	}

	void SameInitProperties()
	{
		//Set health via level;
		m_Health = 100 + 20 * csManagerClass.ManagerClass.RealLevel;

		//Set external component to work
		GetComponent <NavMeshAgent> ().enabled = true;
		GetComponent <Rigidbody> ().isKinematic = false;

		//Effect play
		WaterEffect.Play ();
	}

	public void TakeDamage(int Damage, Vector3 hitPoint)
	{
		if (isDead)
			return;

		m_Health -= Damage;
		HitEffect.transform.position = hitPoint;
		HitEffect.Play ();

		//---- FireAnimation ----
		if (m_Health <= 50) {
			if(FireEffect.isPlaying == false)
				FireEffect.Play();
		}
		if (m_Health <= 25) {
			if(FireEffect2.isPlaying == false)
				FireEffect2.Play();
		}
		//-----------------------

		if (m_Health <= 0) {
			//Set state to dead
			isDead = true;

			//Set external component to not work
			GetComponent <NavMeshAgent> ().enabled = false;
			GetComponent <Rigidbody> ().isKinematic = true;

			//Add Score & Credit
			csManagerClass.ManagerClass.Score += m_ScoreValue;
			csManagerClass.ManagerClass.Credit += 20;

			//Make Explosion Effect
			GameObject Exp = Instantiate(Explosion, transform.position, transform.rotation) as GameObject;
			Destroy(Exp,3);

			//Stop Effect Running
			WaterEffect.Stop();
			FireEffect.Stop();
			FireEffect2.Stop();
		}
	}
}
