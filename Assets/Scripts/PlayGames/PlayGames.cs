using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames.BasicApi.SavedGame;
using System.Text;
using System.Collections;

public class PlayGames : MonoBehaviour {

    public static PlayGames Instance { set; get; }

    const string SAVE_NAME = "Color Shapes";
    bool isSaving;
    bool isCloudDataLoaded;


    private void Start() {
        Instance = this;

        if(!PlayerPrefs.HasKey(SAVE_NAME)) {
            PlayerPrefs.SetString(SAVE_NAME, "0");
        }
        if(!PlayerPrefs.HasKey("IsFirstTime")) {
            PlayerPrefs.SetInt("IsFirstTime", 1);
        }

        LoadLocal();

        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .EnableSavedGames().Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();

        SignIn();
        StartCoroutine(StartSigningIn());
    }

    private IEnumerator StartSigningIn() {
        yield return new WaitForSeconds(1);
        SignIn();
    }

    void SignIn() {
        Social.localUser.Authenticate(success => { LoadData(); });
    }

    #region Achievements

    public static void UnlockAchievement(string id) {
        Social.ReportProgress(id, 100, success => { });
    }

    public static void IncrementAchievement(string id, int stepsToIncrement) {
        PlayGamesPlatform.Instance.IncrementAchievement(id, stepsToIncrement, success => { });
    }

    public static void ShowAchievementsUI() {
        Social.ShowAchievementsUI();
    }

    #endregion Achievements

    #region Leaderboards

    public static void AddScoreToLeaderboard(string id, long score) {
        Social.ReportScore(score, id, success => { });
    }

    public static void ShowLeaderboardUI() {
        Social.ShowLeaderboardUI();
    }

    #endregion Leaderboards

    #region Saved Games
    //making a string out of game data (highscores...)
    string GameDataToString() {
        return CloudVariables.Level.ToString();
    }

    //this overload is used when user is connected to the internet
    //parsing string to game data (stored in CloudVariables), also deciding if we should use local or cloud save
    void StringToGameData(string cloudData, string localData) {
        //if it's the first time that game has been launched after installing it and successfuly logging into Google Play Games
        if(PlayerPrefs.GetInt("IsFirstTime") == 1) {
            //set playerpref to be 0 (false)
            PlayerPrefs.SetInt("IsFirstTime", 0);
            if(int.Parse(cloudData) > int.Parse(localData)) //cloud save is more up to date
            {
                //set local save to be equal to the cloud save
                PlayerPrefs.SetString(SAVE_NAME, cloudData);
            }
        }
        //if it's not the first time, start comparing
        else {
            //comparing integers, if one int has higher score in it than the other, we update it
            if(int.Parse(localData) > int.Parse(cloudData)) {
                //update the cloud save, first set CloudVariables to be equal to localSave
                CloudVariables.Level = int.Parse(localData);
                PlayerPrefs.SetInt("level", CloudVariables.Level);
                //also send the more up to date high score to leaderboard
                AddScoreToLeaderboard(GPGSIds.leaderboard_level, CloudVariables.Level);
                if(CloudVariables.Level > 10)
                    GameManager.Instance.CompleteAchievements(10);
                if(CloudVariables.Level > 20)
                    GameManager.Instance.CompleteAchievements(20);
                if(CloudVariables.Level > 50)
                    GameManager.Instance.CompleteAchievements(50);
                if(CloudVariables.Level > 100)
                    GameManager.Instance.CompleteAchievements(100);
                if(CloudVariables.Level > 200)
                    GameManager.Instance.CompleteAchievements(200);
                if(CloudVariables.Level > 500)
                    GameManager.Instance.CompleteAchievements(500);
                if(CloudVariables.Level > 1000)
                    GameManager.Instance.CompleteAchievements(1000);
                if(CloudVariables.Level > 2000)
                    GameManager.Instance.CompleteAchievements(2000);
                if(CloudVariables.Level > 5000)
                    GameManager.Instance.CompleteAchievements(5000);
                if(CloudVariables.Level > 9999)
                    GameManager.Instance.CompleteAchievements(9999);
                isCloudDataLoaded = true;
                //saving the updated CloudVariables to the cloud
                SaveData();
                return;
            }
        }
        //if the code above doesn't trigger return and the code below executes,
        //cloud save and local save are identical, so we can load either one
        CloudVariables.Level = int.Parse(cloudData);
        PlayerPrefs.SetInt("level", CloudVariables.Level);
        isCloudDataLoaded = true;
    }

    //this overload is used when there's no internet connection - loading only local data
    void StringToGameData(string localData) {
        CloudVariables.Level = int.Parse(localData);
    }

    //used for loading data from the cloud or locally
    public void LoadData() {
        //basically if we're connected to the internet, do everything on the cloud
        if(Social.localUser.authenticated) {
            isSaving = false;
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithManualConflictResolution(SAVE_NAME,
                DataSource.ReadCacheOrNetwork, true, ResolveConflict, OnSavedGameOpened);
        }
        //this will basically only run in Unity Editor, as on device,
        //localUser will be authenticated even if he's not connected to the internet (if the player is using GPG)
        else {
            LoadLocal();
        }
    }

    private void LoadLocal() {
        StringToGameData(PlayerPrefs.GetString(SAVE_NAME));
    }

    //used for saving data to the cloud or locally
    public void SaveData() {
        //if we're still running on local data (cloud data has not been loaded yet), we also want to save only locally
        if(!isCloudDataLoaded) {
            SaveLocal();
            return;
        }
        //same as in LoadData
        if(Social.localUser.authenticated) {
            isSaving = true;
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithManualConflictResolution(SAVE_NAME,
                DataSource.ReadCacheOrNetwork, true, ResolveConflict, OnSavedGameOpened);
        } else {
            SaveLocal();
        }
    }

    private void SaveLocal() {
        PlayerPrefs.SetString(SAVE_NAME, GameDataToString());
    }

    private void ResolveConflict(IConflictResolver resolver, ISavedGameMetadata original, byte[] originalData,
        ISavedGameMetadata unmerged, byte[] unmergedData) {
        if(originalData == null)
            resolver.ChooseMetadata(unmerged);
        else if(unmergedData == null)
            resolver.ChooseMetadata(original);
        else {
            //decoding byte data into string
            string originalStr = Encoding.ASCII.GetString(originalData);
            string unmergedStr = Encoding.ASCII.GetString(unmergedData);

            //parsing
            int originalNum = int.Parse(originalStr);
            int unmergedNum = int.Parse(unmergedStr);

            //if original score is greater than unmerged
            if(originalNum > unmergedNum) {
                resolver.ChooseMetadata(original);
                return;
            }
            //else (unmerged score is greater than original)
            else if(unmergedNum > originalNum) {
                resolver.ChooseMetadata(unmerged);
                return;
            }
            //if return doesn't get called, original and unmerged are identical
            //we can keep either one
            resolver.ChooseMetadata(original);
        }
    }

    private void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game) {
        //if we are connected to the internet
        if(status == SavedGameRequestStatus.Success) {
            //if we're LOADING game data
            if(!isSaving)
                LoadGame(game);
            //if we're SAVING game data
            else
                SaveGame(game);
        }
        //if we couldn't successfully connect to the cloud, runs while on device,
        //the same code that is in else statements in LoadData() and SaveData()
        else {
            if(!isSaving)
                LoadLocal();
            else
                SaveLocal();
        }
    }

    private void LoadGame(ISavedGameMetadata game) {
        ((PlayGamesPlatform)Social.Active).SavedGame.ReadBinaryData(game, OnSavedGameDataRead);
    }

    private void SaveGame(ISavedGameMetadata game) {
        string stringToSave = GameDataToString();
        //saving also locally (can also call SaveLocal() instead)
        PlayerPrefs.SetString(SAVE_NAME, stringToSave);

        //encoding to byte array
        byte[] dataToSave = Encoding.ASCII.GetBytes(stringToSave);
        //updating metadata with new description
        SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder().Build();
        //uploading data to the cloud
        ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(game, update, dataToSave,
            OnSavedGameDataWritten);
    }

    //callback for ReadBinaryData
    private void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] savedData) {
        //if reading of the data was successful
        if(status == SavedGameRequestStatus.Success) {
            string cloudDataString;
            //if we've never played the game before, savedData will have length of 0
            if(savedData.Length == 0)
                //in such case, we want to assign "0" to our string
                cloudDataString = "0";
            //otherwise take the byte[] of data and encode it to string
            else
                cloudDataString = Encoding.ASCII.GetString(savedData);

            //getting local data (if we've never played before on this device, localData is already
            //"0", so there's no need for checking as with cloudDataString)
            string localDataString = PlayerPrefs.GetString(SAVE_NAME);

            //this method will compare cloud and local data
            StringToGameData(cloudDataString, localDataString);
        }
    }

    //callback for CommitUpdate
    private void OnSavedGameDataWritten(SavedGameRequestStatus status, ISavedGameMetadata game) {

    }
    #endregion /Saved Games

}
