using UnityEngine;
using System.Collections;

public class csEnemy : MonoBehaviour 
{
	Transform m_PlayerObject;
	csEnemyHealth m_EnemyHealth;
	NavMeshAgent m_Nma;

	void Awake()
	{
		m_PlayerObject = GameObject.FindGameObjectWithTag ("Player").transform;
		m_EnemyHealth = GetComponent<csEnemyHealth>();
		m_Nma = GetComponent<NavMeshAgent> ();
	}

	void Update()
	{
		if (m_EnemyHealth.isDead == false || csManagerClass.ManagerClass.isGameRunning == true) 
		{
			if(m_Nma.enabled == false)
				m_Nma.enabled = true;
			m_Nma.SetDestination (m_PlayerObject.position);
		}
		else
			m_Nma.enabled = false;
	}

	void Init()
	{
		m_EnemyHealth.SetInit();
	}
}