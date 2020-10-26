using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{

    public GameManager manager;
    public Board board;

    public void Clear() {
        GameObject outline = GameObject.FindGameObjectWithTag("Outline");
        Destroy(outline);
    }

    public void Show() {
        for(int i = 0;i < 3;i++) {
            board.puzzle.pieces[0, 0].RotatePiece();
        }
        board.puzzle.curValue = board.Sweep();
    }

}
