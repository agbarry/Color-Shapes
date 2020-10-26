using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Board : MonoBehaviour
{

    public static Board Instance { get; set; }

    public Puzzle puzzle;
    private bool generated = false;

    public GameObject piecePrefab;
    public GameObject[] quarterPrefabs;
    public ColorGroup tutorialColorGroup;
    public ColorGroup[] colorGroups;

    public ColorGroup colorGroup;
    public bool colorChosen = false;

    private float lastTouch = 0;
    public float minHelpTime = 5.0f;
    public float minPiecesRemainingPercentage = 0.3f;

    public Image backToMenu, showCompleted;

    private Color backgroundColor;
    private float backgroundLerp = 0;
    private float backgroundLerpDuration = 5.0f;

    private void Start() {
        Instance = this;
    }

    private void Update() {
        if(!GameManager.Instance || !GameManager.Instance.IsPlaying()) return;

        if(Time.time > lastTouch + minHelpTime) {
            lastTouch = Time.time;
            ShowHint();
        }

        Camera.main.backgroundColor = Color.Lerp(Camera.main.backgroundColor, backgroundColor, backgroundLerp);
        if(backgroundLerp < 1) {
            backgroundLerp += Time.deltaTime / backgroundLerpDuration;
        }
    }

    [System.Serializable]
    public class Puzzle {

        public int winValue;
        public int curValue;

        public int width;
        public int height;
        public Piece[,] pieces;

    }

    public void CreatePuzzle(int level, int width, int height, bool showingCompleted) {
        Random.InitState(level);
        puzzle.width = width;
        puzzle.height = height;

        if(level != 0) {
            if(puzzle.width <= 0 || puzzle.height <= 0) {
                Debug.LogError("Please set the dimensions!");
                Debug.Break();
            }

            GeneratePuzzle();
        } else {
            if(!colorChosen)
                colorGroup = tutorialColorGroup;

            colorChosen = false;

            float cameraX = puzzle.width % 2 == 0 ? puzzle.width / 2 - 0.5f : puzzle.width / 2;
            float cameraY = puzzle.height % 2 == 0 ? puzzle.height / 2 - 0.5f : puzzle.height / 2;
            Camera.main.transform.position = new Vector3(cameraX, cameraY, -10);
            int orthographicSize = Mathf.Max(puzzle.width, puzzle.height);
            Camera.main.orthographicSize = orthographicSize < 3 ? 3 : orthographicSize;
            Camera.main.backgroundColor = colorGroup.colors[4];
            
            backgroundColor = colorGroup.colors[4];
            Camera.main.backgroundColor = colorGroup.colors[4];

            GameManager.Instance.levelText.color = colorGroup.colors[5];
            ParticleSystem.MainModule mainModule = GameManager.Instance.levelClear.main;
            mainModule.startColor = colorGroup.colors[5];
            backToMenu.color = colorGroup.colors[5];
            showCompleted.color = colorGroup.colors[5];

            Vector2 dimensions = CheckDimensions();
            
            puzzle.width = (int)dimensions.x;
            puzzle.height = (int)dimensions.y;

            puzzle.pieces = new Piece[puzzle.width, puzzle.height];
            
            foreach(var piece in GameObject.FindGameObjectsWithTag("Piece")) {
                puzzle.pieces[(int)piece.transform.position.x, (int)piece.transform.position.y] = piece.GetComponent<Piece>();
            }
        }

        puzzle.winValue = GetWinValue();

        if(level != 0 && !showingCompleted) {
            Shuffle();
        }

        puzzle.curValue = Sweep();

        generated = true;
        
        if(level != 0 && puzzle.curValue == puzzle.winValue) {
            GameManager.Instance.Win(showingCompleted);
        }
    }

    private void Shuffle() {
        foreach(var piece in puzzle.pieces) {
            int k = Random.Range(0, 4);
            for(int i =0; i < k;i++) {
                piece.RotatePiece();
            }
        }
    }

    public int Sweep() {

        int value = 0;

        for(int h = 0;h < puzzle.height;++h) {
            for(int w = 0;w < puzzle.width;++w) {

                //Compare top
                if(h != puzzle.height - 1)
                    if(puzzle.pieces[w, h].values[0] == puzzle.pieces[w, h + 1].values[2])
                        value++;

                //Compare right
                if(w != puzzle.width - 1)
                    if(puzzle.pieces[w, h].values[1] == puzzle.pieces[w + 1, h].values[3])
                        value++;
            }
        }

        return value;

    }

    public int QuickSweep(int w, int h) {
        int value = 0;
        //Compare top
        if(h != puzzle.height - 1)
            if(puzzle.pieces[w, h].values[0] == puzzle.pieces[w, h + 1].values[2])
                value++;

        //Compare right
        if(w != puzzle.width - 1)
            if(puzzle.pieces[w, h].values[1] == puzzle.pieces[w + 1, h].values[3])
                value++;

        //Compare bottom
        if(h != 0)
            if(puzzle.pieces[w, h].values[2] == puzzle.pieces[w, h - 1].values[0])
                value++;

        //Compare left
        if(w != 0)
            if(puzzle.pieces[w, h].values[3] == puzzle.pieces[w - 1, h].values[1])
                value++;

        return value;
    }

    private void GeneratePuzzle() {
        if(colorGroups.Length < 1) {
            Debug.LogError("You need at least 1 color group to start!");
            Debug.Break();
        }

        puzzle.pieces = new Piece[puzzle.width, puzzle.height];

        if(!colorChosen) {
            colorGroup = colorGroups[Random.Range(0, colorGroups.Length)];
        } else {
            Random.Range(0, 1);
        }
        colorChosen = false;

        int randomQuarter = Random.Range(0, quarterPrefabs.Length);
        GameObject quarterPrefab = quarterPrefabs[randomQuarter];

        float cameraX = puzzle.width % 2 == 0 ? puzzle.width / 2 - 0.5f : puzzle.width / 2;
        float cameraY = puzzle.height%2 == 0 ? puzzle.height / 2 - 0.5f : puzzle.height / 2;
        Camera.main.transform.position = new Vector3(cameraX, cameraY, -10);
        int orthographicSize = Mathf.Max(puzzle.width, puzzle.height);
        Camera.main.orthographicSize = orthographicSize < 3 ? 3 : orthographicSize;

        backgroundColor = colorGroup.colors[4];

        GameManager.Instance.levelText.color = colorGroup.colors[5];
        ParticleSystem.MainModule mainModule = GameManager.Instance.levelClear.main;
        mainModule.startColor = colorGroup.colors[5];
        backToMenu.color = colorGroup.colors[5];
        showCompleted.color = colorGroup.colors[5];

        int[] auxValues = { 0, 0, 0, 0 };

        for(int h = 0;h < puzzle.height;++h) {
            for(int w = 0;w < puzzle.width;++w) {

                //Width restrictions
                if(w == 0) {
                    auxValues[3] = Random.Range(0, 4);
                } else {
                    auxValues[3] = puzzle.pieces[w - 1, h].values[1];
                }
                if(w == puzzle.width - 1) {
                    auxValues[1] = Random.Range(0, 4);
                } else {
                    auxValues[1] = Random.Range(0, 4);
                }

                //Height restrictions
                if(h == 0) {
                    auxValues[2] = Random.Range(0, 4);
                } else {
                    auxValues[2] = puzzle.pieces[w, h - 1].values[0];
                }
                if(h == puzzle.height - 1) {
                    auxValues[0] = Random.Range(0, 4);
                } else {
                    auxValues[0] = Random.Range(0, 4);
                }
                
                //Piece type
                GameObject piece = (GameObject)Instantiate(piecePrefab, new Vector3(w, h, 0), Quaternion.identity);

                GameObject p1 = (GameObject)Instantiate(quarterPrefab, piece.transform.position, Quaternion.Euler(0, 0, 180));
                p1.transform.parent = piece.transform;
                p1.GetComponent<SpriteRenderer>().color = colorGroup.colors[auxValues[0]];
                piece.GetComponent<Piece>().values[0] = auxValues[0];

                GameObject p2 = (GameObject)Instantiate(quarterPrefab, piece.transform.position, Quaternion.Euler(0, 0, 90));
                p2.transform.parent = piece.transform;
                p2.GetComponent<SpriteRenderer>().color = colorGroup.colors[auxValues[1]];
                piece.GetComponent<Piece>().values[1] = auxValues[1];

                GameObject p3 = (GameObject)Instantiate(quarterPrefab, piece.transform.position, Quaternion.Euler(0, 0, 0));
                p3.transform.parent = piece.transform;
                p3.GetComponent<SpriteRenderer>().color = colorGroup.colors[auxValues[2]];
                piece.GetComponent<Piece>().values[2] = auxValues[2];

                GameObject p4 = (GameObject)Instantiate(quarterPrefab, piece.transform.position, Quaternion.Euler(0, 0, 270));
                p4.transform.parent = piece.transform;
                p4.GetComponent<SpriteRenderer>().color = colorGroup.colors[auxValues[3]];
                piece.GetComponent<Piece>().values[3] = auxValues[3];


                //Rotate the piece correctly in order to count
                while(piece.GetComponent<Piece>().values[0] != auxValues[0] ||
                    piece.GetComponent<Piece>().values[1] != auxValues[1] ||
                    piece.GetComponent<Piece>().values[2] != auxValues[2] ||
                    piece.GetComponent<Piece>().values[3] != auxValues[3]) {

                    piece.GetComponent<Piece>().RotatePiece();
                }

                puzzle.pieces[w, h] = piece.GetComponent<Piece>();

            }
        }

    }

    public IEnumerator ShowPuzzle() {

        List<Piece> randomPieces = new List<Piece>();

        for(int i = 0;i < puzzle.width;i++) {
            for(int j = 0;j < puzzle.height;j++) {
                randomPieces.Add(puzzle.pieces[i, j]);
            }
        }

        Shuffle(randomPieces);

        foreach(Piece p in randomPieces) {
            p.ShowPiece();
            yield return new WaitForSeconds(0.0075f);
        }

        lastTouch = Time.time;
    }

    private Vector2 CheckDimensions() {
        Vector2 aux = Vector2.zero;

        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");

        foreach(var p in pieces) {
            if(p.transform.position.x > aux.x) {
                aux.x = p.transform.position.x;
            }
            if(p.transform.position.y > aux.y) {
                aux.y = p.transform.position.y;
            }
        }

        aux.x++;
        aux.y++;

        return aux;
    }

    private int GetWinValue() {
        int winValue = 0;

        for(int i = 0;i < puzzle.width;i++) {
            for(int j = 0;j < puzzle.height;j++) {
                winValue += QuickSweep(i, j);
            }
        }

        winValue /= 2;

        return winValue;
    }

    private void Shuffle(List<Piece> pieces) {
        for(int i = 0;i < pieces.Count;i++) {
            Piece temp = pieces[i];
            int randomIndex = Random.Range(i, pieces.Count);
            pieces[i] = pieces[randomIndex];
            pieces[randomIndex] = temp;
        }
    }

    public void Clear() {
        if(puzzle.pieces != null) {
            foreach(Piece piece in puzzle.pieces) {
                if(piece != null) {
                    Destroy(piece.gameObject);
                }
            }
            puzzle.pieces = null;
        }

        backgroundLerp = 0;
    }

    public void Touch() {
        lastTouch = Time.time;
    }

    private void ShowHint() {
        if(!GameManager.Instance || !GameManager.Instance.IsPlaying()) return;

        int piecesCount = puzzle.width * puzzle.height;
        int minPieces = (int)Mathf.Ceil(piecesCount * minPiecesRemainingPercentage);
        int count = 0;
        for(int h = 0;h < puzzle.height;++h) {
            for(int w = 0;w < puzzle.width;++w) {

                //Compare top
                if(h != puzzle.height - 1) {
                    if(puzzle.pieces[w, h].values[0] != puzzle.pieces[w, h + 1].values[2]) {
                        continue;
                    }
                }

                //Compare right
                if(w != puzzle.width - 1) {
                    if(puzzle.pieces[w, h].values[1] != puzzle.pieces[w + 1, h].values[3]) {
                        continue;
                    }
                }
                count++;
            }
        }
        for(int h = 0;h < puzzle.height;++h) {
            for(int w = 0;w < puzzle.width;++w) {

                //Compare top
                if(h != puzzle.height - 1) {
                    if(puzzle.pieces[w, h].values[0] != puzzle.pieces[w, h + 1].values[2]) {
                        if(count >= piecesCount - minPieces) {
                            puzzle.pieces[w, h].ShowHint();
                            return;
                        }
                    }
                }

                //Compare right
                if(w != puzzle.width - 1) {
                    if(puzzle.pieces[w, h].values[1] != puzzle.pieces[w + 1, h].values[3]) {
                        if(count >= piecesCount - minPieces) {
                            puzzle.pieces[w, h].ShowHint();
                            return;
                        }
                    }
                }
            }
        }
    }

    public void CancelHint() {
        foreach(Piece p in puzzle.pieces) {
            if(p.IsShowingHint()) {
                p.CancelHint();
            }
        }
    }

    public bool IsGenerated() {
        return generated;
    }

    public void ShowCompleted(bool showingCompleted) {
        if(Level.level != 0) {
            Clear();
        }
        int[] size = Level.GetLevelSize(Level.level);
        CreatePuzzle(Level.level, size[0], size[1], showingCompleted);
        StartCoroutine(ShowPuzzle());
    }

}