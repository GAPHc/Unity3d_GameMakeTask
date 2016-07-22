using UnityEngine;
using System.Collections;

public class csEnemyWeapon : MonoBehaviour {
	
	public int Damage = 20;
	public string PlayerTag;
	public GameObject Target;
	public float range = 100f;

	ParticleSystem GunParticle;
	LineRenderer GunLine;
	AudioSource GunAudio;
	int shootableMask;

	float FireRate;
	float TimeRate;
	Ray shootRay;
	RaycastHit shootHit;
	csPlayerHealth m_PlayerHealth;

	void Awake()
	{
		shootableMask = LayerMask.GetMask ("Shootable");
		GunParticle = GetComponent<ParticleSystem> ();
		GunLine = GetComponent<LineRenderer> ();
		GunAudio = GetComponent<AudioSource> ();
		FireRate = Random.Range (0, 5) / 10;
		Target = GameObject.FindGameObjectWithTag(PlayerTag);
		m_PlayerHealth = Target.GetComponent<csPlayerHealth>();
	}

	void Update () 
	{
		if (m_PlayerHealth.isDead) 
		{
			GunLine.enabled = false;
			return;
		}

		FireRate += Time.deltaTime;
		TimeRate += Time.deltaTime;

		transform.LookAt(Target.transform);

		if (FireRate > 3 - 0.2 * csManagerClass.ManagerClass.RealLevel) 
		{
			FireRate = 0;
			FireBullts();
		}

		if(FireRate > 0.05f)
			GunLine.enabled = false;
	}


	void FireBullts()
	{	
		shootRay.origin = transform.position;
		Vector3 RandomDirection = new Vector3 (Random.Range (-1, 1), Random.Range (-1, 1), Random.Range (-1, 1))/20;
		shootRay.direction = transform.forward + RandomDirection;

		GunAudio.Play ();
		GunParticle.Stop ();
		GunParticle.Play ();
		
		GunLine.enabled = true;
		GunLine.SetPosition (0, transform.position);

		if(Physics.Raycast (shootRay, out shootHit, range, shootableMask))
		{
			m_PlayerHealth.TakeDamage (Damage, shootHit.point);
			GunLine.SetPosition (1, shootHit.point);
		}
		else
			GunLine.SetPosition (1, shootRay.origin + shootRay.direction * range);
	}
}
