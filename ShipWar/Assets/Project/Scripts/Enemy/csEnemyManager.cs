using UnityEngine;
using System.Collections;

public class csEnemyManager : MonoBehaviour {

	private static csEnemyManager m_EnemyManager;
	public static csEnemyManager EnemyManager
	{
		get
		{
			if(!m_EnemyManager)
				m_EnemyManager = FindObjectOfType(typeof(csEnemyManager)) as csEnemyManager;
			return m_EnemyManager;
		}
	}

	public Transform m_Object;                
	public float spawnTime = 3f;            
	public Transform[] spawnPoints;     
	public GameObject[] Ship1;
	public csEnemyHealth[] Ship1Health;
	float currentTime = 0;
	public int ArraySize = 10;

	void Awake ()
	{
		Ship1 = new GameObject[ArraySize];
		Ship1Health = new csEnemyHealth[ArraySize];
		for (int i = 0; i < ArraySize; i++) 
		{

			int spawnPointIndex = Random.Range (0, spawnPoints.Length);
			Ship1[i] = Instantiate (m_Object, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation) as GameObject;
			Ship1Health[i] = Ship1[i].GetComponent(typeof(csEnemyHealth))as csEnemyHealth;
			Ship1Health[i].isDead = true;
			Ship1[i].SetActive(false);
		}
	}
	
	void Update()
	{
		if (csManagerClass.ManagerClass.isGameRunning == false)
			return;

		currentTime += Time.deltaTime;
		float RealSpawnTime;
		if (csManagerClass.ManagerClass.Weapon_1_UpgradedState <= 3)
			RealSpawnTime = 3 - 0.2f * csManagerClass.ManagerClass.RealLevel;
		else
			RealSpawnTime = 1.5f - 0.12f * csManagerClass.ManagerClass.RealLevel;

		if (currentTime > RealSpawnTime) {
			currentTime = 0;

			for(int i = 0 ; i < 10 ;i++)
			{
				if(Ship1Health[i].isDead == true)
				{
					Ship1Health[i].isDead = false;
					Ship1Health[i].SetInit();
					Ship1[i].SetActive(true);
					break;
				}
			}
		}
	}

	public void InitManager()
	{
		for (int i = 0; i < ArraySize; i++) {
			int spawnPointIndex = Random.Range (0, spawnPoints.Length);
			Ship1Health [i].SetInit (spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
			Ship1Health[i].isDead = true;
			Ship1[i].SetActive(false);
		}
	}
}
