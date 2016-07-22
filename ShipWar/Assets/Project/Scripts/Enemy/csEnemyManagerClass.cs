using UnityEngine;
using System.Collections;

public class csEnemyManagerClass : MonoBehaviour {
	private static csEnemyManagerClass m_EnemyManagerClass;
	public static csEnemyManagerClass EnemyManagerClass
	{
		get
		{
			if(!m_EnemyManagerClass)
				m_EnemyManagerClass = FindObjectOfType(typeof(csEnemyManagerClass)) as csEnemyManagerClass;
			return m_EnemyManagerClass;
		}
	}

	public csEnemyManager[] m_EnemyManager;

	public void InitAllEnemyManager()
	{
		for(int i = 0 ; i < m_EnemyManager.Length; i++)
			m_EnemyManager[i].InitManager();
	}
	
}
