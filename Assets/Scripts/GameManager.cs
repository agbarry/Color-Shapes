using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance { get; set; }

    public Board board;

    public GameObject tutorialPrefab;
    public Tutorial tutorial;

    public Text levelText;
    public Button menuButton;
    public Button showCompletedButton;

    public ParticleSystem levelClear;

    private bool playing = false;

    private bool won = false;
    public bool showingWin = false;
    private bool canGoNextLevel = false;

    private AudioSource[] sounds;

    private void Start() {
        Instance = this;
        sounds = GetComponents<AudioSource>();
    }

    public void Play() {
        playing = true;
        Level.Start();
        AdManager.Instance.ShowBanner();
    }

    public void Stop() {
        board.Clear();
        playing = false;
    }

    private void Update() {
        if(!playing) return;

        Tap();
        Swipe();

    }

    private void Tap() {
        if(MobileInput.Instance.Tap) {
            bool noUIcontrolsInUse = EventSystem.current.currentSelectedGameObject == null;
            if(noUIcontrolsInUse) {
                if(won && canGoNextLevel) {
                    Level.NextLevel();
                    sounds[1].Play();
                }
            }
        }
    }

    private void Swipe() {
        if(MobileInput.Instance.SwipeRight) {
            Level.SwipeRight();
        } else if(MobileInput.Instance.SwipeLeft) {
            Level.SwipeLeft();
        }
    }

    public void Win(bool showingCompleted) {
        won = true;
        StartCoroutine(ShowWin(showingCompleted));
        //Increase
        if(Level.level == PlayerPrefs.GetInt("level") && !showingCompleted) {
            PlayerPrefs.SetInt("level", Level.level + 1);
            PlayGames.AddScoreToLeaderboard(GPGSIds.leaderboard_level, PlayerPrefs.GetInt("level"));
            CloudVariables.Level = PlayerPrefs.GetInt("level");
            PlayGames.Instance.SaveData();
        }

        if(Level.level == 0) {
            Invoke("TutorialWin", 0.5f);
        }

    }

    private void TutorialWin() {
        tutorial.Clear();
    }

    private IEnumerator ShowWin(bool showingCompleted) {
        if(!playing) yield return null;

        yield return new WaitForSeconds(0.5f);
        showingWin = true;
        if(!showingCompleted) {
            canGoNextLevel = true;
        }

        sounds[0].Play();

        yield return new WaitForSeconds(0.3f);

        //Particles
        levelClear.Play();

        if(PlayerPrefs.GetInt("ShowCompleted") != 1) {
            Ingame.Instance.ShowCompleted(false);
        }

        UpdateShowCompletedButton();
        CompleteAchievements(Level.level);
    }

    public void CompleteAchievements(int level) {
        int maxLevel = PlayerPrefs.GetInt("level");
        if(level == maxLevel - 1) {//New highscore
            switch(level) {
                case 10:
                    PlayGames.UnlockAchievement(GPGSIds.achievement_level_10);
                    break;
                case 20:
                    PlayGames.UnlockAchievement(GPGSIds.achievement_level_20);
                    break;
                case 50:
                    PlayGames.UnlockAchievement(GPGSIds.achievement_level_50);
                    break;
                case 100:
                    PlayGames.UnlockAchievement(GPGSIds.achievement_level_100);
                    break;
                case 200:
                    PlayGames.UnlockAchievement(GPGSIds.achievement_level_200);
                    break;
                case 500:
                    PlayGames.UnlockAchievement(GPGSIds.achievement_level_500);
                    break;
                case 1000:
                    PlayGames.UnlockAchievement(GPGSIds.achievement_level_1_000);
                    break;
                case 2000:
                    PlayGames.UnlockAchievement(GPGSIds.achievement_level_2_000);
                    break;
                case 5000:
                    PlayGames.UnlockAchievement(GPGSIds.achievement_level_5_000);
                    break;
                case 9999:
                    PlayGames.UnlockAchievement(GPGSIds.achievement_level_9_999);
                    break;
            }
        }
    }

    public void LoadLevel(int level) {
        if(!playing) return;

        //Reinit values
        StopAllCoroutines();
        won = false;
        showingWin = false;
        canGoNextLevel = false;

        if(level > 0) {
            levelText.text = level.ToString();
        } else {
            levelText.text = "";
        }
        levelText.gameObject.GetComponent<Animator>().Play("Level");

        //Clear
        board.Clear();

        //Create
        int[] size = Level.GetLevelSize(level);
        board.CreatePuzzle(level, size[0], size[1], false);
        StartCoroutine(board.ShowPuzzle());

        if(Level.level == 0) {
            tutorial.Show();
        }

        //Particles
        float count = board.puzzle.width * board.puzzle.height;
        levelClear.transform.localScale = new Vector3(board.puzzle.width + 0.2f, board.puzzle.height + 0.2f, 0);
        levelClear.startSize = count/750;
        levelClear.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);

        ShowCompleted();
        UpdateShowCompletedButton();
    }

    public bool HasWon() {
        return won;
    }

    public bool IsPlaying() {
        return playing;
    }

    public void PlayClickSound() {
        sounds[2].Play();
    }

    public void PlaySwipeSound() {
        sounds[3].Play();
    }

    public void ShowCompleted() {
        if(Level.level < PlayerPrefs.GetInt("level")) {
            bool showWin = PlayerPrefs.GetInt("ShowCompleted") == 1;
            StopAllCoroutines();
            if(!showWin) {
                won = false;
                showingWin = false;
            }
            board.ShowCompleted(showWin);
        }
    }

    public void UpdateShowCompletedButton() {
        showCompletedButton.gameObject.SetActive(Level.level < PlayerPrefs.GetInt("level") && Level.level != 0);
    }
    
}
