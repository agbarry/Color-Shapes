using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

    public GameObject ingameUI;
    public Board board;

    public Image title, play, achievements, sound, leaderboard;
    public Sprite soundOn, soundOff;

    private void Start() {
        PlayerPrefs.SetInt("Sound", PlayerPrefs.GetInt("Sound", 1));
        ingameUI.SetActive(false);
        ChangeColors();

        UpdateSound();
    }

    private void UpdateSound() {
        bool sounded = PlayerPrefs.GetInt("Sound") == 1;
        sound.sprite = sounded ? soundOn : soundOff;
        if(sounded) {
            AudioListener.pause = false;
        } else {
            AudioListener.pause = true;
        }
    }

    public void Play() {
        ingameUI.SetActive(true);
        GameManager.Instance.Play();
        gameObject.SetActive(false);
    }

    public void ChangeColors() {
        ColorGroup colorGroup;
        if(board.colorGroup == null) {
            PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level", 0));
            int level = PlayerPrefs.GetInt("level");
            if(level < 0) {
                Level.level = 0;
                level = 0;
            }
            Random.InitState(level);
            if(level <= 0) {
                board.colorGroup = board.tutorialColorGroup;
            } else {
                board.colorGroup = board.colorGroups[Random.Range(0, board.colorGroups.Length)];
            }
            board.colorChosen = true;
        }

        colorGroup = board.colorGroup;

        Camera.main.backgroundColor = colorGroup.colors[4];

        Color buttonColor = colorGroup.colors[5];

        title.color = buttonColor;
        play.color = buttonColor;
        achievements.color = buttonColor;
        sound.color = buttonColor;
        leaderboard.color = buttonColor;
    }

    public void ShowAchievements() {

    }

    public void ChangeSound() {
        bool sounded = PlayerPrefs.GetInt("Sound") == 1;
        int sound = 0;
        if(!sounded) {
            sound = 1;
        }
        PlayerPrefs.SetInt("Sound", sound);
        UpdateSound();
    }

    public void ShowLeaderboard() {

    }

}
