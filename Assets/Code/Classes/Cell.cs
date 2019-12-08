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

    /// <summary>
    /// Used to track how long a touch has lasted
    /// </summary>
    float touchTime = 0f;

    //public static Color[] textColors = new Color[Color.black, Color.blue, Color.green, Color.cyan, Color.orange, Color.purple, Color.pink, Color.yellow];

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

        cellLayer1 = new GameObject("Sprite Layer for Cell [" + x + ", " + y + "]");
        cellLayer1.transform.position = transform.position;
        cellSprite = cellLayer1.AddComponent<SpriteRenderer>();
        cellSprite.sortingOrder = 1;
    }

    public void simulateMouseUp()
    {
        OnMouseUp(true);
    }
    private void OnMouseUp()
    {
        OnMouseUp(false);
    }
    private void OnMouseUp(bool sim)
    {
        //Debug.Log("OnMouseUp() " + sim + ", Cell [" + x + ", " + y + "]");
        if (Grid.state != Grid.gameState.playing)
            return;

        if((Time.time - touchTime) > Grid.longPressTime)
        {
            OnMouseUpRight();
            touchTime = 0f;
            return;
        }
        else if (state == mouseState.cancelledClick)
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
                var theText = new GameObject("TextMesh for Cell [" + x + ", " + y + "]");
                var textMesh = theText.AddComponent<TextMesh>();
                textMesh.characterSize = 0.01f;
                textMesh.fontSize = 512;
                textMesh.text = neighborMines.Count.ToString();
                textMesh.offsetZ = -1;

                if (neighborMines.Count == 1)
                    textMesh.color = Color.blue;
                else if (neighborMines.Count == 2)
                    textMesh.color = Color.green;
                else if (neighborMines.Count == 3)
                    textMesh.color = Color.red;
                else if (neighborMines.Count == 4)
                    textMesh.color = Color.magenta;
                else if (neighborMines.Count == 5)
                    textMesh.color = Color.cyan;
                else
                    textMesh.color = Color.black;
                
                textMesh.transform.position = transform.position + new Vector3(-0.15f, 0.275f, 0f);
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
        //Debug.Log("OnMouseUpRight() Cell [" + x + ", " + y + "]");
        if (Grid.state != Grid.gameState.playing)
            return;

        if (!flagged) {
            flagged = true;

            cellSprite.sprite = Resources.Load<Sprite>("flag");
        } else {
            flagged = false;

            cellSprite.sprite = null;
        }
    }

    private void OnMouseDown()
    {
        if (Grid.state != Grid.gameState.playing)
            return;
        state = mouseState.downClick;

        //Debug.Log("OnMouseDown() for Cell [" + x + ", " + y + "]");
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
                state = mouseState.cancelledClick;
            }
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(1))
        {
            Debug.Log("OnMouseOver() GetMouseButtonUp(1)");
            OnMouseUpRight();
        }
    }
    
    // Update is called once per frame
    void Update ()
    {
        if (Input.touches.Length > 0)
        {
            Touch touch = Input.touches[0];
            //Debug.Log("<touch> for Cell [" + x + ", " + y + "] Phase: " + touch.phase);

            if (touch.phase == TouchPhase.Began)
            {
                touchTime = Time.time;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (Time.time - touchTime <= Grid.longPressTime)
                {
                    //just a tap
                }
                else
                {
                    //long press
                    //Debug.Log("long press OnMouseUpRight()");
                }
                //touchTime = 0f;
            }
        }
        else
            touchTime = 0f;
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
