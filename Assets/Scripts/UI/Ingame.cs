using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ingame : MonoBehaviour
{

    public static Ingame Instance { set; get; }

    public GameObject menuUI;

    public Tutorial tutorial;

    public Image showCompletedImage;
    public Sprite showCompletedSprite, showNotCompletedSprite;

    private void Start() {
        Instance = this;
        Show();
    }

    public void ToMenu() {
        GameManager.Instance.Stop();
        menuUI.SetActive(true);
        menuUI.GetComponent<Menu>().ChangeColors();
        gameObject.SetActive(false);
        if(Level.tutorial != null) {
            GameObject.Destroy(Level.tutorial);
        }
    }

    public void ShowCompleted(bool showResult) {
        bool showCompleted = PlayerPrefs.GetInt("ShowCompleted") == 1;

        int completed = 0;
        if(!showCompleted) {
            completed = 1;
        }

        PlayerPrefs.SetInt("ShowCompleted", completed);

        Show();

        if(showResult) {
            GameManager.Instance.ShowCompleted();
        }
    }

    private void Show() {
        if(PlayerPrefs.GetInt("ShowCompleted") == 1) {
            showCompletedImage.sprite = showCompletedSprite;
        } else {
            showCompletedImage.sprite = showNotCompletedSprite;
        }
    }

}
