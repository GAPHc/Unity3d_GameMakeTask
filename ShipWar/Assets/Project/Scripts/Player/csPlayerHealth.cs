using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class csPlayerHealth : MonoBehaviour {

	[HideInInspector]
	public int m_Health = 100;
	[HideInInspector]
	public bool isDead = false;
	public ParticleSystem m_HitParticles;
	public ParticleSystem[] m_FireParticles;
	public ParticleSystem m_BigFireParticles;
	Rigidbody m_ObjectRigidBody;
	public Slider healthSlider;
	public Image healtImage;
	public GameObject Explosion;

	void Awake()
	{
		m_ObjectRigidBody = GetComponent<Rigidbody> ();
		for(int i = 0 ; i < m_FireParticles.Length ; i++)
			m_FireParticles[i].Stop();
		m_BigFireParticles.Stop ();
	}

	public void setInit()
	{
		isDead = false;
		if (csManagerClass.ManagerClass.Weapon_1_UpgradedState < 1)
			m_Health = 100;
		else
			m_Health = 100 * csManagerClass.ManagerClass.Health_1_UpgradedState;
		m_ObjectRigidBody.isKinematic = false;

		transform.position = new Vector3 (0, 0, 0);

		Quaternion OroginalRot = Quaternion.identity;
		OroginalRot.eulerAngles = new Vector3 (0, 0, 0);
		transform.rotation = OroginalRot;

		for(int i = 0 ; i < m_FireParticles.Length ; i++)
			m_FireParticles[i].Stop();
		m_BigFireParticles.Stop ();
	}

	void Update()
	{
		int HealthPoint = csManagerClass.ManagerClass.Health_1_UpgradedState;

		int RangeOfHealth = 10 - (m_Health / (10*HealthPoint));
		if (RangeOfHealth <= 1)
			RangeOfHealth = 0;

		for (int i = 0; i < RangeOfHealth; i++) 
		{
			if(m_FireParticles[i].isPlaying != true)
				m_FireParticles[i].Play();
		}

		if (healthSlider.value < 30)
			healtImage.color = Color.red;
		else if (healthSlider.value < 55)
			healtImage.color = Color.yellow;
		else if( healthSlider.value < 85)
		    healtImage.color = Color.white;
		else
			healtImage.color = Color.green;

		healthSlider.value = m_Health / HealthPoint;
	}

	public void TakeDamage(int Damage, Vector3 hitPoint)
	{
		if (isDead)
			return;
			
		//m_EnemyAudio.Play ();
		m_Health -= Damage;
		m_HitParticles.transform.position = hitPoint;
		m_HitParticles.Play ();

		if (m_Health <= 0) {
			isDead = true;
			m_BigFireParticles.Play();
			m_ObjectRigidBody.isKinematic = true;
			GameObject Exp = Instantiate(Explosion, transform.position, transform.rotation) as GameObject;
			Destroy(Exp,3);
			//m_EnemyAudio.clip = m_DeadClip;
			//m_EnemyAudio.Play ();
		}
	}
}
