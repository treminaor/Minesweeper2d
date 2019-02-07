using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 Todo list:
 - Settings menu to change gidsize and mine density
 - info menu to show credits
 - figure out how to adjust the size for android screens
 - export an android package and put it on my phone
  
 */

public class Grid : MonoBehaviour {

    public static int gridSizeX = 5;
    public static int gridSizeY = 5;
    private static float mineChance = 0.75f;

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

    // Use this for initialization
    void Start() {

        if (GameSettings.getGridSizePref() > 0)
        {
            gridSizeX = GameSettings.getGridSizePref();
            gridSizeY = GameSettings.getGridSizePref();
        }

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
                if (Random.value > mineChance) //todo: Does Random need to be seeded?
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
                int x = Random.RandomRange(1, gridSizeX);
                int y = Random.RandomRange(1, gridSizeY);
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
        Camera.main.orthographicSize = gridSizeX;
        Camera.main.transform.position = new Vector3(
            ((float)gridSizeX / 2.0f) - 0.5f, //-0.5 is half a grid unit for centering purposes.
            ((float)gridSizeX / 2.0f) - 0.5f, 
            Camera.main.transform.position.z); 

        state = gameState.playing;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonUp(0))
            checkForEndgame();
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
