using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell : MonoBehaviour {

    /// <summary>
    /// Keeps track of the mouse state to determine if the user cancelled their click by dragging off the cell
    /// </summary>
    public enum mouseState
    {
        clear,
        downClick,
        drag,
        upClick,
        cancelledClick
    }
    private mouseState state;
    private Vector3 mouseDragStartPosition;

    /// <summary>
    /// Has the cell been clicked by the player?
    /// </summary>
    public bool clicked = false;

    /// <summary>
    /// Coordinates of this cell in the grid.
    /// </summary>
    public int x; public int y;

    /// <summary>
    /// Has the cell been right-clicked by the player?
    /// </summary>
    public bool flagged = false;

    /// <summary>
    /// Does this cell contain a mine?
    /// </summary>
    public bool hasMine = false;

    /// <summary>
    /// How many mines neighbor this cell?
    /// </summary>
    public List<Vector2> neighborMines;

    //public static Color[] textColors = new Color[Color.black, Color.blue, Color.green, Color.cyan, Color.orange, Color.purple, Color.pink, Color.yellow];

    private GameObject debugText;

    /// <summary>
    /// The Sprite object which primarily occupies this cell.
    /// </summary>
    private SpriteRenderer cellSprite;
    private GameObject cellLayer1;

    // Use this for initialization
	void Start () {
        state = mouseState.clear;
        transform.position = new Vector3(x, y, 0);
        Debug.Log("transform init'd to " + x + ", " + y);
        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();

        SpriteRenderer spriteCellBG = gameObject.AddComponent<SpriteRenderer>();
        spriteCellBG.sprite = Resources.Load<Sprite>("cell_bg");

        cellLayer1 = new GameObject();
        cellLayer1.transform.position = transform.position;
        cellSprite = cellLayer1.AddComponent<SpriteRenderer>();
        
        //todo: debug text
        debugText = new GameObject();
        debugText.AddComponent<TextMesh>();
        /*
           var theText = new GameObject();
           var textMesh = theText.AddComponent<TextMesh>();
           textMesh.text = "" + x + "," + y + "";
           textMesh.color = Color.white;
           textMesh.characterSize = 0.16f;
           textMesh.transform.position = transform.position + new Vector3(-0.35f, 0.25f, -0.15f);
        */
    }

    public void simulateMouseUp()
    {
        OnMouseUp();
    }

    private void OnMouseUp()
    {
        //Debug.Log("OnMouseUp() Cell [" + x + ", " + y + "]");
        if (Grid.state != Grid.gameState.playing)
            return;

        if (state == mouseState.cancelledClick)
        {
            state = mouseState.clear;
            return;
        }

        if(!clicked && !flagged)
        {
            clicked = true;

            if(!hasMine)
            {
                //todo: Debug, replace with sprites
                var theText = new GameObject();
                var textMesh = theText.AddComponent<TextMesh>();
                textMesh.characterSize = 0.01f;
                textMesh.fontSize = 512;
                textMesh.text = neighborMines.Count.ToString();
                if (neighborMines.Count == 1)
                    textMesh.color = Color.blue;
                else if (neighborMines.Count == 2)
                    textMesh.color = Color.green;
                else if (neighborMines.Count == 3)
                    textMesh.color = Color.red;
                else if (neighborMines.Count == 4)
                    textMesh.color = Color.magenta;
                else if (neighborMines.Count == 5)
                    textMesh.color = Color.magenta;
                else
                    textMesh.color = Color.gray;
                
                textMesh.transform.position = transform.position + new Vector3(-0.15f, 0.25f, 0f);
                //end

                triggerNeighborCells();
            }
            else
            {
                cellSprite.sprite = Resources.Load<Sprite>("mine");
            }
        }
    }

    private void OnMouseUpRight()
    {
        if (Grid.state != Grid.gameState.playing)
            return;

        //todo: Debug, replace with sprites
        TextMesh textMesh = debugText.GetComponent<TextMesh>();
        textMesh.transform.position = transform.position;

        if (!flagged)
        {
            flagged = true;

            cellSprite.sprite = Resources.Load<Sprite>("flag");
        }
        else
        {
            flagged = false;

            cellSprite.sprite = null;
        }
    }

    private void OnMouseDown()
    {
        if (Grid.state != Grid.gameState.playing)
            return;
        state = mouseState.downClick;
    }

    private void OnMouseDrag()
    {
        if (state == mouseState.downClick)
        {
            mouseDragStartPosition = Input.mousePosition;
            state = mouseState.drag;
        }
        if (state == mouseState.drag)
        {
            if (Vector3.Distance(mouseDragStartPosition, Input.mousePosition) > 3)
            {
                Debug.Log("Click cancelled, dist: " + Vector3.Distance(mouseDragStartPosition, Input.mousePosition));
                state = mouseState.cancelledClick;
            }
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(1))
        {
            OnMouseUpRight();
        }
    }
    // Update is called once per frame
    void Update () {
        if (Input.touches.Length > 0)
        {
            Touch touch = Input.touches[0];
            float touchTime = 0f;
            if (touch.phase == TouchPhase.Began)
            {
                touchTime = Time.time;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (Time.time - touchTime <= 0.5)
                {
                    //just a tap
                }
                else
                {
                    //long press
                    OnMouseUpRight();
                }
            }
        }
    }

    private void triggerNeighborCells()
    {
        Grid grid = GameObject.Find("Main").GetComponent<Grid>();

        if (x + 1 < Grid.gridSizeX)
            if (!grid.cells[x + 1, y].GetComponent<Cell>().hasMine && grid.cells[x + 1, y].GetComponent<Cell>().neighborMines.Count == 0) //N
                grid.cells[x + 1, y].GetComponent<Cell>().simulateMouseUp();
        if (x + 1 < Grid.gridSizeX && y + 1 < Grid.gridSizeY)
            if (!grid.cells[x + 1, y + 1].GetComponent<Cell>().hasMine && grid.cells[x + 1, y + 1].GetComponent<Cell>().neighborMines.Count == 0) //NE
                grid.cells[x + 1, y + 1].GetComponent<Cell>().simulateMouseUp();
        if (y + 1 < Grid.gridSizeY)
            if (!grid.cells[x, y + 1].GetComponent<Cell>().hasMine && grid.cells[x, y + 1].GetComponent<Cell>().neighborMines.Count == 0) //E
                grid.cells[x, y + 1].GetComponent<Cell>().simulateMouseUp();
        if (x - 1 >= 0)
            if (!grid.cells[x - 1, y].GetComponent<Cell>().hasMine && grid.cells[x - 1, y].GetComponent<Cell>().neighborMines.Count == 0) //W
                grid.cells[x - 1, y].GetComponent<Cell>().simulateMouseUp();
        if (x - 1 >= 0 && y - 1 >= 0)
            if (!grid.cells[x - 1, y - 1].GetComponent<Cell>().hasMine && grid.cells[x - 1, y - 1].GetComponent<Cell>().neighborMines.Count == 0) //SW
                grid.cells[x - 1, y - 1].GetComponent<Cell>().simulateMouseUp();
        if (y - 1 >= 0)
            if (!grid.cells[x, y - 1].GetComponent<Cell>().hasMine && grid.cells[x, y - 1].GetComponent<Cell>().neighborMines.Count == 0) //S
                grid.cells[x, y - 1].GetComponent<Cell>().simulateMouseUp();
        if (x - 1 >= 0 && y + 1 < Grid.gridSizeY)
            if (!grid.cells[x - 1, y + 1].GetComponent<Cell>().hasMine && grid.cells[x - 1, y + 1].GetComponent<Cell>().neighborMines.Count == 0) //NW
                grid.cells[x - 1, y + 1].GetComponent<Cell>().simulateMouseUp();
        if (x + 1 < Grid.gridSizeX && y - 1 >= 0)
            if (!grid.cells[x + 1, y - 1].GetComponent<Cell>().hasMine && grid.cells[x + 1, y - 1].GetComponent<Cell>().neighborMines.Count == 0) //SE
                grid.cells[x + 1, y - 1].GetComponent<Cell>().simulateMouseUp();
    }
}
