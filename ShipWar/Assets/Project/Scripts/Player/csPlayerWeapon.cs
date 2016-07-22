using UnityEngine;
using System.Collections;

public class csPlayerWeapon : MonoBehaviour {
	
	public int Damage = 20;
	public string EnemyTag;
	public GameObject Target;
	public float range = 100f;

	ParticleSystem GunParticle;
	LineRenderer GunLine;
	AudioSource GunAudio;
	int shootableMask;

	float FireRate;
	float SearchTimeRate;
	Ray shootRay;
	RaycastHit shootHit;

	void Awake()
	{
		shootableMask = LayerMask.GetMask ("Shootable");
		GunParticle = GetComponent<ParticleSystem> ();
		GunLine = GetComponent<LineRenderer> ();
		GunAudio = GetComponent<AudioSource> ();
		FireRate = Random.Range (0, 5) / 10;
	}

	void Update () 
	{
		if (csPlayerClass.PlayerClass.m_PlayerHealth.isDead == true)
			return;

		FireRate += Time.deltaTime;
		SearchTimeRate += Time.deltaTime;

		if (!Target)
			FindTargets ();
		else 
		{
			transform.LookAt(Target.transform);

			csEnemyHealth EnemyHealth = Target.GetComponent<csEnemyHealth>();
			if(EnemyHealth.isDead)
				Target = null;

			if (FireRate > 1 - 0.06 * csManagerClass.ManagerClass.Weapon_1_UpgradedState) {
				FireRate = 0;
				FireBullts();
			}

			if(SearchTimeRate > 1.5){
				SearchTimeRate = 0;
				FindTargets();
			}
		}
		if(FireRate > 0.05f)
			GunLine.enabled = false;

	}
	

	void FindTargets()
	{
		GameObject[] TargetObjects = GameObject.FindGameObjectsWithTag (EnemyTag);
		float MinDistance = 1000000;
		for (int i = 0; i < TargetObjects.Length; i++) 
		{
			if(TargetObjects[i])
			{
				csEnemyHealth EnemyHealth = TargetObjects[i].GetComponent<csEnemyHealth>();
				if(EnemyHealth.isDead == false)
				{
					float Distance = (TargetObjects[i].transform.position - transform.position).magnitude;
					if(Distance < MinDistance)
					{
						MinDistance = Distance;
						Target = TargetObjects[i];
					}
				}
			}
		}
	}

	void FireBullts()
	{	
		shootRay.origin = transform.position;
		Vector3 RandomDirection = new Vector3 (Random.Range (-1, 1), Random.Range (-1, 1), Random.Range (-1, 1))/(20+3 * csManagerClass.ManagerClass.Weapon_1_UpgradedState);
		shootRay.direction = transform.forward+RandomDirection;

		GunAudio.Play ();
		GunParticle.Stop ();
		GunParticle.Play ();
		
		GunLine.enabled = true;
		GunLine.SetPosition (0, transform.position);

		if(Physics.Raycast (shootRay, out shootHit, range, shootableMask))
		{
			csEnemyHealth enemyHealth = shootHit.collider.GetComponent <csEnemyHealth> ();
			if(enemyHealth != null)
			{
				int SubmittedDamage = Damage + 2 * csManagerClass.ManagerClass.Weapon_1_UpgradedState;
				enemyHealth.TakeDamage (SubmittedDamage, shootHit.point);
			}
			GunLine.SetPosition (1, shootHit.point);
		}
		else
			GunLine.SetPosition (1, shootRay.origin + shootRay.direction * range);
	}
}
