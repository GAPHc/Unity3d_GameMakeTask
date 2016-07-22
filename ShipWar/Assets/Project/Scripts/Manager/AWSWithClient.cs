using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Amazon;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.CognitoSync;
using Amazon.CognitoSync.SyncManager;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

public class AWSWithClient : MonoBehaviour {
	
	int Credit;
	int WeaponUpdatedState;
	int HealthUpdatedState;
	int Level;
	int MaxLevel;
	int Score;
	int MaxScore;

	public string identityPoolId;
	Dataset PlayerInfo;
	CognitoSyncManager syncManager;
	CognitoAWSCredentials credentials;
	AmazonDynamoDBClient client;
	DynamoDBContext context;
	InformationSave dbInfoSave;
	bool SyncWithCognito = false;

	void Awake()
	{
		//Set AWS Unity Initializer
		UnityInitializer.AttachToGameObject (this.gameObject);

		//Remove if you want to build on an IOS device.
		AWSConfigs.LoggingConfig.LogTo = LoggingOptions.UnityLogger;

		//Set Credentials, SyncManager information
		credentials = new CognitoAWSCredentials(identityPoolId,RegionEndpoint.USEast1);
		syncManager = new CognitoSyncManager (credentials, RegionEndpoint.USEast1);

		//Set Dataset
		PlayerInfo = syncManager.OpenOrCreateDataset ("PlayerInfo");
		PlayerInfo.OnSyncSuccess += SyncSuccessCallback;

		//Make DynamoDB connection value
		client =  new AmazonDynamoDBClient (credentials, RegionEndpoint.USEast1);
		context =  new DynamoDBContext (client);

		dbInfoSave = new InformationSave();
	}

	public void ChangeValueFromClient()
	{
		//Get information from ManagerClass.
		Credit = csManagerClass.ManagerClass.Credit;
		WeaponUpdatedState = csManagerClass.ManagerClass.Weapon_1_UpgradedState;
		HealthUpdatedState = csManagerClass.ManagerClass.Health_1_UpgradedState;
		Level = csManagerClass.ManagerClass.Level;
		MaxLevel = csManagerClass.ManagerClass.MaxLevel;
		Score = csManagerClass.ManagerClass.Score;
		MaxScore = csManagerClass.ManagerClass.MaxScore;

		//Set PlayerInfo
		PlayerInfo.Put ("Credit", Credit.ToString());
		PlayerInfo.Put ("WeaponUpdatedState", WeaponUpdatedState.ToString());
		PlayerInfo.Put ("HealthUpdatedState", HealthUpdatedState.ToString());
		PlayerInfo.Put ("Level", Level.ToString());
		PlayerInfo.Put ("MaxLevel", MaxLevel.ToString());
		PlayerInfo.Put ("Score", Score.ToString());
		PlayerInfo.Put ("MaxScore", MaxScore.ToString());
		//PlayerInfo.Put ("Name",

		//Set information for DynamoDB
		dbInfoSave.Credit = Credit;
		dbInfoSave.WeaponUpdatedState = WeaponUpdatedState;
		dbInfoSave.HealthUpdatedState = HealthUpdatedState;
		dbInfoSave.Level = Level;
		dbInfoSave.MaxLevel = MaxLevel;
		dbInfoSave.Score = Score;
		dbInfoSave.MaxScore = MaxScore;
		Debug.Log ("Success Save Information");
	}

	public void UpdateToServer()
	{
		//if not loggedin, 
		if (!string.IsNullOrEmpty (PlayerInfo.Get ("FacebookId")) && !csManagerClass.ManagerClass.isLoggedIn) {
			Debug.Log ("You must Logged in to sync");
		} else {
			SyncWithCognito = false;
			ChangeValueFromClient ();
			PlayerInfo.SynchronizeOnConnectivity ();
			if(!csManagerClass.ManagerClass.CheckWithServer)
				csManagerClass.ManagerClass.CheckWithServer = true;
			Debug.Log("Success Update To Server");
		}
	}
	public void CheckInformationWithServer()
	{
		if (!string.IsNullOrEmpty (PlayerInfo.Get ("FacebookId")) && !csManagerClass.ManagerClass.isLoggedIn) 
			Debug.Log ("You must Logged in to sync");
		else {
			SyncWithCognito = true;
			Debug.Log("Start Check Information with Cognito Server");

			csManagerClass.ManagerClass.LoadingValue += 15;
			csManagerClass.ManagerClass.LoadText = "Start check information with cognito server";

			PlayerInfo.SynchronizeOnConnectivity ();
		}
	}

	public void ChangeValueFromeAWSCognito()
	{
		//Check that PlayerInfo's contents contain values
		//------------------------------------------------------
		if (!string.IsNullOrEmpty (PlayerInfo.Get ("Credit")) && !string.IsNullOrEmpty (PlayerInfo.Get ("WeaponUpdatedState")) 
			&& !string.IsNullOrEmpty (PlayerInfo.Get ("HealthUpdatedState")) && !string.IsNullOrEmpty (PlayerInfo.Get ("Level"))
			&& !string.IsNullOrEmpty (PlayerInfo.Get ("MaxLevel")) && !string.IsNullOrEmpty (PlayerInfo.Get ("Score"))
			&& !string.IsNullOrEmpty (PlayerInfo.Get ("MaxScore"))) {

			Credit = int.Parse (PlayerInfo.Get ("Credit"));
			csManagerClass.ManagerClass.Credit = Credit;
			dbInfoSave.Credit = Credit;

			WeaponUpdatedState = int.Parse (PlayerInfo.Get ("WeaponUpdatedState"));
			csManagerClass.ManagerClass.Weapon_1_UpgradedState = WeaponUpdatedState;
			dbInfoSave.WeaponUpdatedState = WeaponUpdatedState;

			HealthUpdatedState = int.Parse (PlayerInfo.Get ("HealthUpdatedState"));
			csManagerClass.ManagerClass.Health_1_UpgradedState = HealthUpdatedState;
			dbInfoSave.HealthUpdatedState = HealthUpdatedState;

			Level = int.Parse (PlayerInfo.Get ("Level"));
			dbInfoSave.Level = Level;

			MaxLevel = int.Parse (PlayerInfo.Get ("MaxLevel"));
			csManagerClass.ManagerClass.MaxLevel = MaxLevel;
			dbInfoSave.MaxLevel = MaxLevel;

			Score = int.Parse (PlayerInfo.Get ("Score"));
			dbInfoSave.Score = Score;

			MaxScore = int.Parse (PlayerInfo.Get ("MaxScore"));
			csManagerClass.ManagerClass.MaxScore = MaxScore;
			dbInfoSave.MaxScore = MaxScore;
		} else
			ChangeValueFromClient ();
	}

	void SyncSuccessCallback(object sender, SyncSuccessEventArgs e)
	{
		List<Record> newRecords = e.UpdatedRecords;
		for (int i = 0; i < newRecords.Count; i++) {
			Debug.Log (newRecords [i].Key + " was updated : " + newRecords [i].Value);
		}

		if (SyncWithCognito) 
		{
			SyncWithCognito = false;
			ChangeValueFromeAWSCognito ();
		}
		SaveInformationToDynamoDB (dbInfoSave);

		csManagerClass.ManagerClass.LoadingValue += 20;
		csManagerClass.ManagerClass.LoadText = "Succes Sync information with AWS";

		if(!csManagerClass.ManagerClass.CheckWithServer){
			csManagerClass.ManagerClass.CheckWithServer = true;
			csManagerClass.ManagerClass.TitleUpdate();
		}

		Debug.Log ("Success Sync With Server");
	}

	public void FBHasLoggedIn(string token, string id, string name)
	{
		credentials.Clear ();
		credentials.AddLogin ("graph.facebook.com", token);
		csManagerClass.ManagerClass.LoadingValue += 20;
		csManagerClass.ManagerClass.LoadText = "Connect to AWS with facebook info";
		RetriveInformationFromDynamoDB (id,name);
	}

	public void SaveInformationToDynamoDB(InformationSave infosv)
	{
		context.SaveAsync(dbInfoSave,
		           (result) => {
			if (result.Exception != null) {
				Debug.LogError ("Save Error");
				Debug.LogException (result.Exception);
				return;
			}
			else
				Debug.Log ("Success Update Information to DynamoDB");
		});
	}

	public void RetriveInformationFromDynamoDB(string id, string name)
	{
		context.LoadAsync<InformationSave>(id, (result) =>
		{
			if (result.Exception == null) {
				Debug.Log("Success Load & Connection Check From DynamoDB");
				Debug.Log("Welcome " + name + " to ShipWar");

				csManagerClass.ManagerClass.LoadingValue += 20;
				csManagerClass.ManagerClass.LoadText = "Success Connect with AWS DB";

				dbInfoSave.FacebookId = id;
				dbInfoSave.UserName = name;
				PlayerInfo.Put("FacebookId",id);
				PlayerInfo.Put("UserName",name);
				if(result.Result as InformationSave == null) {
					Debug.Log("NewUser");
					ChangeValueFromClient();
					SaveInformationToDynamoDB (dbInfoSave);
				}
			} else {
				Debug.Log("DynamoDB Load Error Error");
			}
			CheckInformationWithServer();
		});
	}
}

[DynamoDBTable("GameMakeTask")]
public class InformationSave
{
	[DynamoDBHashKey]
	public string FacebookId { get; set; }

	[DynamoDBProperty]
	public string UserName { get; set; }

	[DynamoDBProperty]
	public int Credit { get; set; }

	[DynamoDBProperty]
	public int WeaponUpdatedState { get; set; }

	[DynamoDBProperty]
	public int HealthUpdatedState { get; set; }

	[DynamoDBProperty]
	public int Level { get; set; }

	[DynamoDBProperty]
	public int MaxLevel { get; set; }

	[DynamoDBProperty]
	public int Score { get; set; }

	[DynamoDBProperty]
	public int MaxScore { get; set; }
}

[DynamoDBTable("GameMakeTaskTimeTable")]
public class TimeClass
{
	[DynamoDBHashKey]
	public string FacebookId{ get; set; }

	[DynamoDBProperty]
	public string FirstGameInstallTime { get; set; }

	[DynamoDBProperty]
	public string GamePlayStartTime { get ; set; }

	[DynamoDBProperty]
	public string PlayerExistTime { get; set; }

	[DynamoDBProperty]
	public string GameEndTime{get; set;}

	[DynamoDBProperty]
	public string GameLastPlayTime{get; set;}

}
