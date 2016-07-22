using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Facebook.Unity;
using SocketIO;

public class csManagerClass : MonoBehaviour {

	//----- Singleton -----
	private static csManagerClass m_ManagerClass;
	public static csManagerClass ManagerClass
	{
		get
		{
			if(!m_ManagerClass)
				m_ManagerClass = FindObjectOfType(typeof(csManagerClass)) as csManagerClass;
			return m_ManagerClass;
		}
	}

	//---- Value of gamestate ----
	public enum GameState
	{
		Title,UpGrade,Running,Over,Pause
	}
	public GameState m_GameState;
	public bool isGameRunning = true;
	
	public int Score;
	public Text ScoreText;
	public Text LevelText;
	public Text CreditTextInRunning;
	float GameTime;
	float LevelTime;

	//---- Saved Info ----
	[HideInInspector]
	public int Level = 1;
	[HideInInspector]
	public int Credit;
	[HideInInspector]
	public int Weapon_1_UpgradedState = 1;
	[HideInInspector]
	public int Health_1_UpgradedState = 1;
	[HideInInspector]
	public int MaxLevel;
	[HideInInspector]
	public int MaxScore;
	[HideInInspector]
	public string UserName;
	//------------------------------
	[HideInInspector]
	public int RealLevel = 1;

	[HideInInspector]
	public bool CheckWithServer = false;

	//--- GameOver Canvas Member ---
	public Canvas GameOverCanvas;
	public Text GameOverScore;
	public Text GameOverMaxScore;
	public Text GameOverLevel;
	public Text GameOverMaxLevel;
	public Text GameOverCredit;
	//------------------------------

	//---- GameUpgrade Canvas Member ---
	public Canvas GameUpgradeCanvas;
	public Text CreditText;
	public Text Weapon_1_Upgraded;
	public Text Health_1_Upgraded;
	//-----------------------------

	//---- Game Ranking Canvas Member ---
	public Canvas RankCanvas;
	public Text rankText;
	[HideInInspector]
	public string rankTextString = "";
	public Text nameText;
	[HideInInspector]
	public string nameTextString = "";
	public Text levelText;
	[HideInInspector]
	public string levelTextString = "";
	public Text scoreText;
	[HideInInspector]
	public string scoreTextString = "";
	//-----------------------------------

	//---- Otehr Canvas Member ----
	public Canvas RunningCanvas;
	public Canvas PauseCanvas;
	//------------------------------

	//----- Title Canavs Member -----
	public Canvas TitleCanvas;
	[HideInInspector]
	public int LoadingValue;
	[HideInInspector]
	public string LoadText;
	public Text LoadCanvasText;
	public Slider loadingSlider;
	//-------------------------------

	//FaceBook && AWS && nodejs variables
	public Canvas ConnectCanvas;
	AWSWithClient AwsConnector;

	[HideInInspector]
	public bool isLoggedIn;
	//SocketIO
	public SocketIOComponent socketIO;
	RankInformationClass[] RankInfoClasses;
	int current_rankArraySize;

	//Time Value;
	[HideInInspector]
	public string GameFirstStartTime = "";
	[HideInInspector]
	public string GamePlayStartTime = "";
	[HideInInspector]
	public string PlayerDeadTime = "";
	[HideInInspector]
	public string GameEndTime = "";
	TimeInformationClass TimeInfoClass;

	//Const value
	const int rankArraySize = 10;

	void Awake()
	{
		LoadText = "";
		GameFirstStartTime = System.DateTime.Now.ToString();

		AwsConnector = GetComponent<AWSWithClient> ();
		TimeInfoClass = new TimeInformationClass ();
		RankInfoClasses = new RankInformationClass[rankArraySize];
		for (int i = 0; i < rankArraySize; i++) {
			RankInfoClasses [i] = new RankInformationClass ();
		}
		//init gamestate to title
		m_GameState = GameState.Title;
		isGameRunning = false;
		isLoggedIn = false;
		current_rankArraySize = 0;

		if (!FB.IsInitialized) 
			FB.Init(InitCallBack);
	}

	void Start()
	{
		socketIO.On ("GetRank", GetRankFromServer);
	}

	//Check FB Script's init success
	void InitCallBack()
	{
		Debug.Log ("FB script has been initiased.");
	}

	void GetRankFromServer(SocketIOEvent obj)
	{
		if (current_rankArraySize > rankArraySize-1)
			return;

		RankInfoClasses[current_rankArraySize].user_Rank = current_rankArraySize + 1;
		RankInfoClasses[current_rankArraySize].user_Name = JsonToString (obj.data.GetField ("UserName").ToString (), "\"");
		RankInfoClasses[current_rankArraySize].user_Level =  obj.data.GetField ("MaxLevel").ToString ();
		RankInfoClasses [current_rankArraySize].user_Score =  obj.data.GetField ("MaxScore").ToString ();

		rankTextString += (RankInfoClasses[current_rankArraySize].user_Rank + "\n");
		for (int i = 0; i < RankInfoClasses[current_rankArraySize].user_Name.Length; i++) {
			nameTextString += RankInfoClasses[current_rankArraySize].user_Name[i];
			if(i >= 4)
			{
				nameTextString+= "...";
				break;
			}
		}
		nameTextString += "\n";
		levelTextString += (RankInfoClasses [current_rankArraySize].user_Level + "\n");
		scoreTextString += (RankInfoClasses [current_rankArraySize].user_Score + "\n");

		current_rankArraySize++;
	}

	string JsonToString(string target, string s)
	{
		string[] newString = Regex.Split (target, s);
		return newString [1];
	}
	
	public void Update()
	{
		if (m_GameState == GameState.Title) {
			if(LoadingValue >= 100)
				LoadingValue = 100;
			loadingSlider.value = LoadingValue;
			LoadCanvasText.text = LoadText;
		}
		else if (m_GameState == GameState.Running) {

			if (Input.GetKeyDown (KeyCode.Escape)) {
				m_GameState = GameState.Pause;
				PauseCanvas.enabled = !PauseCanvas.enabled;
				Pause ();
			}

			if (csPlayerClass.PlayerClass.m_PlayerHealth.isDead == true) {
				m_GameState = GameState.Over;
				SaveGameInfo ();
				GameOverCanvas.enabled = !GameOverCanvas.enabled;
				isGameRunning = false;
				PlayerDeadTime = System.DateTime.Now.ToString();
			} else
				isGameRunning = true;

			LevelSystem();
			GameTime += Time.deltaTime;
			ScoreText.text = "Score : " + Score;
			LevelText.text = "Level : " + Level;
			CreditTextInRunning.text = "Credit : " + Credit;

		} 
		else if (m_GameState == GameState.Over) {
			GameOverScore.text = "Score : " + Score;
			GameOverMaxScore.text = "MaxScore : " + MaxScore;
			GameOverLevel.text = "Level : " + Level;
			GameOverMaxLevel.text = "MaxLevel : " + MaxLevel;
			GameOverCredit.text = "Credit : " + Credit;

			rankText.text = rankTextString;
			nameText.text = nameTextString;
			levelText.text = levelTextString;
			scoreText.text = scoreTextString;
		}
		else if (m_GameState == GameState.UpGrade) {
			CreditText.text = "Credit : " + Credit;

			string weapon1 = "";
			for(int i = 0 ; i < Weapon_1_UpgradedState ; i++)
				weapon1+="O";
			Weapon_1_Upgraded.text = weapon1;

			string health1 = "";
			for(int i = 0 ; i < Health_1_UpgradedState ; i++)
				health1+="O";
			Health_1_Upgraded.text = health1;

		}
		else if (m_GameState == GameState.Pause) {
			if(Input.GetKeyDown(KeyCode.Escape)){
				PauseCanvas.enabled = !PauseCanvas.enabled;
				m_GameState = GameState.Running;
				Pause();
			}
		}
	}
	void LevelSystem()
	{
		LevelTime += Time.deltaTime;
		if(LevelTime > 15)
		{
			LevelTime = 0;
			if(Level < 10)
				RealLevel++;
			Level++;
		}
	}

	//UI relative methods
	private void Pause()
	{
		Time.timeScale = Time.timeScale == 0 ? 1 : 0;
	}

	public void Resume()
	{
		m_GameState = GameState.Running;
		PauseCanvas.enabled = !PauseCanvas.enabled;
		Pause ();
	}

	//------------ Rank Canvas Methods --------------
	public void GetRank()
	{
		Debug.Log ("Get Rank Start");
		current_rankArraySize = 0;
		rankTextString = "";
		nameTextString = "";
		levelTextString = "";
		scoreTextString = "";

		RankCanvas.enabled = !RankCanvas.enabled;
		GameOverCanvas.enabled = !GameOverCanvas.enabled;
		socketIO.Emit ("Rank");
	}

	public void CloseRank()
	{
		RankCanvas.enabled = !RankCanvas.enabled;
		GameOverCanvas.enabled = !GameOverCanvas.enabled;
	}
	//-----------------------------------------------

	//----------- Title Canvas Methods ---------------
	public void LoginWithFaceBook()
	{
		Debug.Log ("Start login with facebook");
		/*
		 * Get FaceBook Info from user's facebook data
		 * If user's Info is not saved in DynamicDB, make new information to DB
		 * if user's Info is already saved in DynamicDB, get information from DB to Client.
		*/
		if (!FB.IsLoggedIn) {
			LoadingValue += 15;
			LoadText = "Check login with user's facebook";
			FB.LogInWithReadPermissions (new List<string>{"public_profile", "email", "user_friends"}, LoginCallBack);
		}
	}

	void LoginCallBack(ILoginResult result)
	{
		if (result.Error == null) {
			TitleCanvas.enabled = !TitleCanvas.enabled;
			ConnectCanvas.enabled = !ConnectCanvas.enabled;
			isLoggedIn = true;
			Debug.Log("FB has logged in");
			LoadText = "Success login with facebok";
			LoadingValue += 20;
			FB.API ("me?fields=name",HttpMethod.GET,NameCallBack);
		} else {
			Debug.Log("Error during Login: " + result.Error);
		}
	}
	void NameCallBack(IGraphResult result)
	{
		IDictionary<string,object> profile = result.ResultDictionary;
		UserName = profile["name"].ToString();
		AwsConnector.FBHasLoggedIn(AccessToken.CurrentAccessToken.TokenString,AccessToken.CurrentAccessToken.UserId, UserName);
	}

	
	public void TitleUpdate()
	{
		ConnectCanvas.enabled = !ConnectCanvas.enabled;
		RunningCanvas.enabled = !RunningCanvas.enabled;
		StartGame ();
		csManagerClass.ManagerClass.LoadText = "Ready to start game";
		socketIO.Emit ("Rank");
	}

	//-----------------------------------------------

	//----- Upgrade Canvas Methods -------
	public void GameUpgradeCheck()
	{
		GameOverCanvas.enabled = !GameOverCanvas.enabled;
		GameUpgradeCanvas.enabled = !GameUpgradeCanvas.enabled;
		m_GameState = GameState.UpGrade;
	}

	public void UpgradeWeapon1()
	{
		if (Credit > 100) 
		{
			if(Weapon_1_UpgradedState < 10)
			{
				Credit-= 100;
				Weapon_1_UpgradedState++;
			}
		}
	}

	public void UpgradedHealth1()
	{
		if(Credit > 100)
		{
			if(Health_1_UpgradedState < 10)
			{
				Credit -= 100;
				Health_1_UpgradedState++;
			}
		}
	}
	//--------------------------------------

	public void SaveGameInfo()
	{
		/*
		 * Save Score, Level, CreditInfo, WeaponUpgradedState, HealthUpgradedState
		 * to the DynamicDB.
		 */
		if (Score > MaxScore)
			MaxScore = Score;
		if (Level > MaxLevel)
			MaxLevel = Level;
		
		AwsConnector.UpdateToServer ();
	}
	
	public void StartGame()
	{
		if(GameUpgradeCanvas.enabled)
			GameUpgradeCanvas.enabled = !GameUpgradeCanvas.enabled;
		if (GameOverCanvas.enabled)
			GameOverCanvas.enabled = !GameOverCanvas.enabled;
		
		//Init player, enemyinformation
		csEnemyManagerClass.EnemyManagerClass.InitAllEnemyManager ();
		csPlayerClass.PlayerClass.m_PlayerHealth.setInit ();
		
		Score = 0;
		Level = 1;
		RealLevel = 1;
		m_GameState = GameState.Running;
		isGameRunning = true;
		GamePlayStartTime = System.DateTime.Now.ToString();
		
	}

	public void OnApplicationQuit()
	{
		GameEndTime = System.DateTime.Now.ToString();
		SaveTimeInfo ();
	}

	public void SaveTimeInfo()
	{
		Dictionary<string,string> TimeData= new Dictionary<string, string> ();
		TimeData ["FacebookId"] = AccessToken.CurrentAccessToken.UserId;
		FB.LogOut();
		TimeData ["GameFirstStartTime"] = GameFirstStartTime;
		TimeData ["GamePlayStartTime"] = GamePlayStartTime;
		TimeData ["PlayerDeadTime"] = PlayerDeadTime;
		TimeData ["GameEndTime"] = GameEndTime;
		
		socketIO.Emit ("TimeSave", new JSONObject(TimeData));
		Debug.Log ("Send Time Information To NodejsServer");
		Debug.Log ("And Quit Game Application");
	}

	public void Quit()
	{
#if UNITY_EDIROT
		EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
}

public class TimeInformationClass
{
	public string GameFirstStartTime;
	public string GamePlayStartTime;
	public string PlayerDeadTime;
	public string GameEndTime;
}

public class RankInformationClass
{
	public int user_Rank;
	public string user_Name;
	public string user_Level;
	public string user_Score;
}