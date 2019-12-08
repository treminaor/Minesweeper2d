using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 Todo list:
 - info menu to show credits
 - Android Package Issues:
    - Grid is pushed to absolute edge of phone screen - need to add some padding
    - Timer and Bomb Count are not completely visible (margins issue)
    - Need to add touch/hold controls so that you can place flags where bombs are.
 - revealAllMines() func should show visual indications for flagged cells if they had a mine or not. Flagged mines should stay as flags, incorrectly flagged mines should get an X over them
 */

public class Grid : MonoBehaviour {

    public static int gridSizeX = 5;
    public static int gridSizeY = 5;
    private static float mineChance = 0.10f;

    private float defaultCameraZoom;

    public enum gameState
    {
        win,
        loss,
        playing,
        newgame
    }

    public static gameState state;

    public GameObject[,] cells;
    public int mineCounter;
    float gameTime = 0f;
    float touchTime = 0f;
    public static float longPressTime = 0.33333f;

    Text MineCountText;
    Text GameTimerText;

    // Use this for initialization
    void Start() {

        //Load & Process Game Settings
        if (GameSettings.getPlayerPref("gridSize") > -1)
        {
            int savedPref = GameSettings.getPlayerPref("gridSize");
            if (savedPref == 0)
                gridSizeX = 5;
            else if (savedPref == 1)
                gridSizeX = 10;
            else
                gridSizeX = 25;

            gridSizeY = gridSizeX;
        }

        if(GameSettings.getPlayerPref("mineDensity") > -1)
        {
            int savedPref = GameSettings.getPlayerPref("mineDensity");
            if (savedPref == 0)
                mineChance = 0.10f;
            else if (savedPref == 1)
                mineChance = 0.18f;
            else
                mineChance = 0.25f;
        }
        //Done Loading Game Settings

        cells = new GameObject[gridSizeX, gridSizeY];

        state = gameState.newgame;
        mineCounter = 0;

        int minimumMinesPerXCells = 9;
        int minimumMineCount = cells.Length % minimumMinesPerXCells;
        if (minimumMineCount == 0)
            minimumMineCount = 1;

        //Build & initialize the grid with randomized mine locations
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                cells[x, y] = new GameObject("Cell [" + x + ", " + y + "]");
                cells[x, y].AddComponent<Cell>();
                Cell thisCell = cells[x, y].GetComponent<Cell>();
                thisCell.x = x;
                thisCell.y = y;
                if (Random.value < mineChance) //todo: Does Random need to be seeded?
                {
                    thisCell.hasMine = true;
                    thisCell.gameObject.name += " *";
                    mineCounter++;
                }
            }
        }

        //Make sure there's at least one mine per X cells (default 9)
        if (mineCounter < minimumMineCount)
        {
            for(int k = 0; k < minimumMineCount - mineCounter; k++)
            {
                int x = Random.Range(1, gridSizeX);
                int y = Random.Range(1, gridSizeY);
                cells[x, y].GetComponent<Cell>().hasMine = true;
                cells[x, y].gameObject.name += " *";
                mineCounter++;
            }
        }
        
        //Tally up the totals for neighboring mines after the grid is built
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Cell thisCell = cells[x, y].GetComponent<Cell>();
                thisCell.neighborMines = getNeighborMinesForCell(cells[x,y]);
            }
        }

        Camera.main.backgroundColor = Color.black;
        //Adjust orthographic camera view based on gridsize
        defaultCameraZoom = gridSizeX + (gridSizeX / 2);
        if (GameSettings.getPlayerPrefFloat("cameraZoom") >= 0)
            Camera.main.orthographicSize = GameSettings.getPlayerPrefFloat("cameraZoom");
        else
            Camera.main.orthographicSize = defaultCameraZoom;
        Camera.main.transform.position = new Vector3(
            ((float)gridSizeX / 2.0f) - 0.5f, //-0.5 is half a grid unit for centering purposes.
            ((float)gridSizeX / 2.0f) - 0.5f, 
            Camera.main.transform.position.z);

        GameSettings.setPlayerPrefFloat("cameraZoom", Camera.main.orthographicSize);

        //Initialize UI Elements
        MineCountText = gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Text>();
        MineCountText.text = mineCounter.ToString();
        MineCountText.gameObject.SetActive(true);

        GameTimerText = gameObject.transform.GetChild(1).GetChild(1).gameObject.GetComponent<Text>();
        GameTimerText.text = "0";
        GameTimerText.gameObject.SetActive(true);

        state = gameState.playing;
    }
	
	// Update is called once per frame
	void Update () {
        updateGameTime();
        handleButtonInput();
        //handleTouchInput();
        handleGridZoom();
    }

    private void handleButtonInput() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.LoadScene("MainMenu");
        }

        if (Input.GetMouseButtonUp(0)) {
            checkForEndgame();
        }
        if (Input.GetMouseButtonUp(1)) {
            updateMineCounterText();
        }
    }

    private void handleGridZoom() {
        float delta = Input.GetAxis("Mouse ScrollWheel");
        if (delta < 0f || Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (Camera.main.orthographicSize < gridSizeX * 2)
            {
                Camera.main.orthographicSize += 0.5f;
                GameSettings.setPlayerPrefFloat("cameraZoom", Camera.main.orthographicSize);
            }
        }
        else if (delta > 0f || Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if (Camera.main.orthographicSize > 1) //don't let them zoom the camera through to negative
            {
                Camera.main.orthographicSize -= 0.5f;
                GameSettings.setPlayerPrefFloat("cameraZoom", Camera.main.orthographicSize);
            }
        }
    }

    private void handleTouchInput() {
        if (Input.touches.Length > 0) {
            Touch touch = Input.touches[0];
            //Debug.Log("<touch> initiated: " + touch.phase);
            
            if (touch.phase == TouchPhase.Began) {
                touchTime = Time.time;
                //Debug.Log("<touch> touchTime " + touchTime);
            }
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
                //Debug.Log("<touch> ended(" + touch.phase + ") Time.time:" + Time.time + ", touchTime: " + touchTime + ", minus: " + (Time.time - touchTime));
                if (Time.time - touchTime <= longPressTime) {
                    //just a tap, already handled by unity input mapping to mouse button input
                    //Debug.Log("<touch> short press");
                } else {
                    //long press
                    //Debug.Log("<touch> long press");
                    updateMineCounterText();
                }
                touchTime = 0f;
            }
        }
    }

    private void updateGameTime() {
        if (state == gameState.win || state == gameState.loss || state == gameState.newgame)
            return;

        gameTime += Time.deltaTime;
        GameTimerText.text = ((int)gameTime).ToString();
    }

    private void updateMineCounterText()
    {
        if (state == gameState.win || state == gameState.loss)
            return;

        int flaggedCount = 0;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Cell cell = cells[x, y].GetComponent<Cell>();
                if (cell.flagged)
                {
                    flaggedCount++;
                }
            }
        }
        MineCountText.text = (mineCounter - flaggedCount).ToString();
    }

    private void checkForEndgame()
    {
        if (state == gameState.win || state == gameState.loss)
            return;

        int count = 0;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Cell cell = cells[x, y].GetComponent<Cell>();
                if(cell.hasMine && cell.clicked)
                {
                    revealAllMines();
                    state = gameState.loss;
                    showGameOver("You Lose!", "");
                }
                else if(cell.clicked && !cell.hasMine)
                {
                    count++;
                }
            }
        }

        if(count >= (cells.Length - mineCounter))
        {
            state = gameState.win;
            showGameOver("You Win!", "");
        }
    }

    private void revealAllMines()
    {
        Debug.Log("revealAllMines()");
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Cell cell = cells[x, y].GetComponent<Cell>();
                if (cell.hasMine)
                {
                    cell.simulateMouseUp();
                }
            }
        }
    }

    public List<Vector2> getNeighborMinesForCell(GameObject cellObj)
    {
        Cell cell = cellObj.GetComponent<Cell>();
        List<Vector2> mines = new List<Vector2>();

        if (cell.x + 1 < gridSizeX)
            if (cells[cell.x + 1, cell.y].GetComponent<Cell>().hasMine) //N
                mines.Add(new Vector2(cell.x + 1, cell.y));
        if (cell.x + 1 < gridSizeX && cell.y + 1 < gridSizeY)
            if (cells[cell.x + 1, cell.y + 1].GetComponent<Cell>().hasMine) //NE
                mines.Add(new Vector2(cell.x + 1, cell.y + 1));
        if (cell.y + 1 < gridSizeY)
            if (cells[cell.x, cell.y + 1].GetComponent<Cell>().hasMine) //E
                mines.Add(new Vector2(cell.x, cell.y + 1));
        if (cell.x - 1 >= 0)
            if (cells[cell.x - 1, cell.y].GetComponent<Cell>().hasMine) //W
                mines.Add(new Vector2(cell.x - 1, cell.y));
        if (cell.x - 1 >= 0 && cell.y - 1 >= 0)
            if (cells[cell.x - 1, cell.y - 1].GetComponent<Cell>().hasMine) //SW
                mines.Add(new Vector2(cell.x - 1, cell.y - 1));
        if (cell.y - 1 >= 0)
            if (cells[cell.x, cell.y - 1].GetComponent<Cell>().hasMine) //S
                mines.Add(new Vector2(cell.x, cell.y - 1));
        if (cell.x - 1 >= 0 && cell.y + 1 < gridSizeY)
            if (cells[cell.x - 1, cell.y + 1].GetComponent<Cell>().hasMine) //NW
                mines.Add(new Vector2(cell.x - 1, cell.y + 1));
        if (cell.x + 1 < gridSizeX && cell.y - 1 >= 0)
            if (cells[cell.x + 1, cell.y - 1].GetComponent<Cell>().hasMine) //SE
                mines.Add(new Vector2(cell.x + 1, cell.y - 1));

        return mines;
    }

    public void showGameOver(string title, string body)
    {
        GameObject gameOverMenu = gameObject.transform.GetChild(0).GetChild(0).gameObject;
        gameOverMenu.SetActive(true);

        Text titleText = gameOverMenu.transform.GetChild(0).GetComponent<Text>();
        titleText.text = title;

        Text bodyText = gameOverMenu.transform.GetChild(1).GetComponent<Text>();
        bodyText.text = body;
    }
}
