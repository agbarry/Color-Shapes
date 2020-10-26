using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{

    public int[] values;

    private float realRotation;

    private bool showingHint = false;

    private bool touched;

    private bool rotating = false;

    private AudioSource rotateSound;

    private void Awake() {
        realRotation = transform.localEulerAngles.z;
        rotateSound = GetComponent<AudioSource>();
    }

    private void Update() {

        if(MobileInput.Instance.Tap) {
            if(GameManager.Instance != null && !GameManager.Instance.HasWon()) {
                Vector3 wp;
                if(Input.touchCount > 0) {
                    wp = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                } else {
                    wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                Vector2 touchPos = new Vector2(wp.x, wp.y);
                if(GetComponent<Collider2D>() == Physics2D.OverlapPoint(touchPos)) {
                    OnTouch();
                }
            }
        }

        Rotate();
        ShowTouched();
        ShowBlink();
        ShowWin();

    }

    private void Rotate() {
        if(transform.rotation.eulerAngles.z != realRotation) {
            if(rotating) {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, realRotation), 10f * Time.deltaTime);
                return;
            }
            if(!rotating || Mathf.Abs(transform.localEulerAngles.z) == realRotation) {
                transform.rotation = Quaternion.Euler(0, 0, realRotation);
                rotating = true;
            }
        }
    }

    private void ShowTouched() {
        if(touched) {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0.7f, 0.7f, 0.7f), 10f * Time.deltaTime);
            if(transform.localScale.x < 0.81f) {
                transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                touched = false;
            }
        } else {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1, 1, 1), 10f * Time.deltaTime);
        }
    }

    private void ShowBlink() {
        if(showingHint) {
            foreach(Transform t in transform) {
                SpriteRenderer rend = t.GetComponent<SpriteRenderer>();
                rend.color = Color.Lerp(t.GetComponent<SpriteRenderer>().color, new Color(rend.color.r, rend.color.g, rend.color.b, 0), 7.5f * Time.deltaTime);
                if(rend.color.a < 0.01f) {
                    rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, 0);
                    showingHint = false;
                }
            }
        } else {
            foreach(Transform t in transform) {
                SpriteRenderer rend = t.GetComponent<SpriteRenderer>();
                rend.color = Color.Lerp(t.GetComponent<SpriteRenderer>().color, new Color(rend.color.r, rend.color.g, rend.color.b, 1), 7.5f * Time.deltaTime);
                if(rend.color.a > 0.99f) {
                    rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, 1);
                }
            }
        }
    }

    private void ShowWin() {
        if(GameManager.Instance != null && GameManager.Instance.showingWin) {
            foreach(Transform p in transform) {
                p.transform.localScale = Vector3.Lerp(p.transform.localScale, new Vector3(1, 1, 1), 10f * Time.deltaTime);
            }
        }
    }

    private void OnTouch() {
        //Tutorial
        if(Level.level == 0 && (transform.position.x != 0 || transform.position.y != 0)) {
            return;
        }

        touched = true;
        rotating = true;
        Board.Instance.CancelHint();

        int difference = -Board.Instance.QuickSweep((int)transform.position.x, (int)transform.position.y);

        RotatePiece();

        difference += Board.Instance.QuickSweep((int)transform.position.x, (int)transform.position.y);

        Board.Instance.puzzle.curValue += difference;

        Board.Instance.Touch();

        if(Board.Instance.puzzle.curValue >= Board.Instance.puzzle.winValue) {
            GameManager.Instance.Win(false);
        }

        rotateSound.Play();

    }

    public void RotatePiece() {
        realRotation += 90;

        realRotation = realRotation % 360;

        RotateValues();
    }

    public void RotateValues() {

        int aux = values[0];

        for(int i = 0; i < values.Length-1;++i) {
            values[i] = values[i + 1];
        }
        values[3] = aux;
    }

    public void ShowPiece() {
        foreach(Transform t in transform) {
            t.GetComponent<ImageAnimator>().PlayAnimation();
        }
    }

    public void ShowHint() {
        showingHint = true;
    }

    public bool IsShowingHint() {
        return showingHint;
    }

    public void CancelHint() {
        foreach(Transform t in transform) {
            SpriteRenderer rend = t.GetComponent<SpriteRenderer>();
            rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, 1);
        }
        showingHint = false;
    }

}
