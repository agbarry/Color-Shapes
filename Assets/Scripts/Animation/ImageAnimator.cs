using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimator : MonoBehaviour {

    [SerializeField] private Sprite[] frameArray = null;
    [SerializeField] private float framerate = 0.04f;

    private int currentFrame;
    private float timer;
    private SpriteRenderer spriteRenderer;
    private Image image;
    private bool isPlaying;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
    }

    private void Update() {
        if(!isPlaying) {
            return;
        }

        timer += Time.deltaTime;

        if(timer >= framerate) {
            timer -= framerate;
            currentFrame = (currentFrame + 1) % frameArray.Length;
            if(currentFrame == 0) {
                StopPlaying();
            } else {
                if(spriteRenderer != null) {
                    spriteRenderer.sprite = frameArray[currentFrame];
                } else if(image != null){
                    image.sprite = frameArray[currentFrame];
                }
            }
        }
    }

    public void StopPlaying() {
        isPlaying = false;
    }

    public void PlayAnimation() {
        isPlaying = true;
        currentFrame = 0;
        timer = 0f;
        if(spriteRenderer != null) {
            spriteRenderer.sprite = frameArray[currentFrame];
        } else if(image != null) {
            image.sprite = frameArray[currentFrame];
        }
    }

}
