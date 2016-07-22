using UnityEngine;
using System.Collections;

public class csPlayerClass : MonoBehaviour {

	private static csPlayerClass m_PlayerClass;
	public static csPlayerClass PlayerClass
	{
		get
		{
			if(!m_PlayerClass)
				m_PlayerClass = FindObjectOfType(typeof(csPlayerClass)) as csPlayerClass;
			return m_PlayerClass;
		}
	}

	int TurningDir = 0;
	[HideInInspector]
	public csPlayerHealth m_PlayerHealth;
	float m_Speed;
	Rigidbody m_PlayerRigidBody;
	Vector3 MoveDir;
	Quaternion OriginalRot = Quaternion.identity;

	#if !MOBILE_INPUT
	int floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
	float camRayLength = 100f;          // The length of the ray from the camera into the scene.
	#endif
	
	void Awake ()
	{
		#if !MOBILE_INPUT
		// Create a layer mask for the floor layer.
		floorMask = LayerMask.GetMask ("Floor");
		#endif

		m_PlayerRigidBody = GetComponent<Rigidbody> ();
		m_PlayerHealth = GetComponent<csPlayerHealth> ();
	}

	public void FixedUpdate()
	{
		if (csManagerClass.ManagerClass.isGameRunning == false)
			return;
		m_Speed = 0.5f + csManagerClass.ManagerClass.Health_1_UpgradedState * 0.1f;
		Move();
		//Turning ();
		Turning2 ();
	}

	void Move()
	{
		Vector3 Velocity = Vector3.zero;
		Velocity = (m_PlayerRigidBody.rotation * Vector3.forward) * (m_Speed*100);
		m_PlayerRigidBody.velocity = Vector3.Lerp(m_PlayerRigidBody.velocity, Velocity, Time.fixedDeltaTime);
	}

	void Turning ()
	{
		Quaternion Rot = Quaternion.identity;
		Rot.eulerAngles = new Vector3(0, TurningDir * Time.deltaTime,0);
		OriginalRot *= Rot;
		m_PlayerRigidBody.rotation = Quaternion.Lerp(transform.rotation, OriginalRot, Time.fixedDeltaTime * 25);
	}

	void Turning2()
	{
		#if !MOBILE_INPUT

		Ray camRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit floorHit;
		if(Physics.Raycast (camRay, out floorHit, camRayLength, floorMask))
		{
			Vector3 playerToMouse = floorHit.point - transform.position;
			playerToMouse.y = 0f;
			Quaternion newRotatation = Quaternion.LookRotation (playerToMouse);
			m_PlayerRigidBody.rotation = Quaternion.Lerp(transform.rotation,newRotatation,Time.fixedDeltaTime * 25);
		}
		#else
		
		Vector3 turnDir = new Vector3(CrossPlatformInputManager.GetAxisRaw("Mouse X") , 0f , CrossPlatformInputManager.GetAxisRaw("Mouse Y"));
		if (turnDir != Vector3.zero)
		{
			Vector3 playerToMouse = (transform.position + turnDir) - transform.position;
			playerToMouse.y = 0f;
			Quaternion newRotatation = Quaternion.LookRotation(playerToMouse);
			m_PlayerRigidBody.rotation = Quaternion.Lerp(transform.rotation,newRotatation,Time.fixedDeltaTime * 25);
		}
		#endif
	}

	public void ChangeTurningDirectToLeft()
	{
		TurningDir = -10;
	}

	public void ChangeTurnindDirectToRight()
	{
		TurningDir = 10;
	}
}
