using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Level
{

    public static int level = -1;

    public static GameObject tutorial;

    public static void Start() {
        if(level < 0) {
            PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level", 0));
            level = PlayerPrefs.GetInt("level");
            if(level < 0) {
                level = 0;
            }
        }
        LoadLevel(level);
    }

    public static void SwipeLeft() {
        if(level < PlayerPrefs.GetInt("level")) {
            GameManager.Instance.PlaySwipeSound();
            level++;
            LoadLevel(level);
            Board.Instance.CancelHint();
            Board.Instance.Touch();
        }
    }

    public static void SwipeRight() {
        if(level > 0) {
            GameManager.Instance.PlaySwipeSound();
            level--;
            LoadLevel(level);
            Board.Instance.CancelHint();
            Board.Instance.Touch();
        }
    }
    
    public static void NextLevel() {
        level++;
        LoadLevel(level);
    }

    public static void LoadLevel(int level) {
        
        if(level == 0) {
            tutorial = (GameObject)GameObject.Instantiate(GameManager.Instance.tutorialPrefab, Vector3.zero, Quaternion.identity);
        } else {
            if(tutorial != null) {
                GameObject.Destroy(tutorial);
            }
        }

        GameManager.Instance.LoadLevel(level);
    }

    public static int[] GetLevelSize(int level) {
        int tmp = level % 100;
        if(tmp < 10) {
            return new int[] { 2, 2 };
        }else if(tmp < 20) {
            return new int[] { 2, 3 };
        } else if(tmp < 30) {
            return new int[] { 3, 3 };
        } else if(tmp < 40) {
            return new int[] { 3, 4 };
        } else if(tmp < 50) {
            return new int[] { 4, 4 };
        } else if(tmp < 60) {
            return new int[] { 4, 5 };
        } else if(tmp < 70) {
            return new int[] { 5, 5 };
        } else if(tmp < 80) {
            return new int[] { 5, 6 };
        } else if(tmp < 90) {
            return new int[] { 6, 6 };
        }
        return new int[] { 6, 7 }; // < 100
    }

}
