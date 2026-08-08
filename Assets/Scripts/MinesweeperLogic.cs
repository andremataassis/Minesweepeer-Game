using UnityEditor;
using UnityEngine;

//Singleton
public class MinesweeperLogic : MonoBehaviour
{
    public int width = 36;
    public int height = 20;
    public int mineCount = 50;

    private Cell[,] state;

    //Highlight effects
    private Cell selected_cell;
    private int highlighted_row;
    private int highlighted_column;

    public static MinesweeperLogic Instance { get; private set; }

    private void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width * height);
    }

    private void Awake()
    {
        // Enforce the single-instance rule
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        NewGame();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Flag();
        }

        SelectCell();

        Board.Instance.Draw(state);
    }

    //Highlights cell that mouse is over
    private void SelectCell()
    {
        if (selected_cell.type == Cell.Type.Invalid) return;

        //Highlight effect
        selected_cell = GetCell(selected_cell.position.x, selected_cell.position.y);
        selected_cell.highlighted = false;
        state[selected_cell.position.x, selected_cell.position.y] = selected_cell;

        selected_cell = worldCoordinateToCell();
        if (selected_cell.type == Cell.Type.Invalid) return;
        selected_cell.highlighted = true;
        state[selected_cell.position.x, selected_cell.position.y] = selected_cell;
    }

    //To unhighlight, just give an invalid row #
    public void HighlightRow(int row)
    {
        int width = state.GetLength(0);
        //unhighlight previous
        for (int i = 0; i < width; i++)
        {
            Cell cell = GetCell(i, highlighted_row);
            cell.highlighted = false;
            state[cell.position.x, cell.position.y] = cell;
        }

        //highlight new
        for (int i = 0; i < width; i++)
        {
            Cell cell = GetCell(i, row);
            if (cell.type == Cell.Type.Invalid) return;
            cell.highlighted = true;
            state[cell.position.x, cell.position.y] = cell;
        }
        highlighted_row = row;
    }

    //To unhighlight, just give an invalid column #
    public void HighlightColumn(int column)
    {
        int length = state.GetLength(1);
        //unhighlight previous
        for (int i = 0; i < length; i++)
        {
            Cell cell = GetCell(highlighted_column, i);
            cell.highlighted = false;
            state[cell.position.x, cell.position.y] = cell;
        }

        //highlight new
        for (int i = 0; i < length; i++)
        {
            Cell cell = GetCell(column, i);
            if (cell.type == Cell.Type.Invalid) return;
            cell.highlighted = true;
            state[cell.position.x, cell.position.y] = cell;
        }
        highlighted_column = column;
    }

    private Cell worldCoordinateToCell()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = Board.Instance.tilemap.WorldToCell(worldPosition);
        return GetCell(cellPosition.x, cellPosition.y);
    }

    public Vector3 cellToWorldCoordinate(Cell cell)
    {
        return Board.Instance.tilemap.GetCellCenterWorld(cell.position);
    }

    public void Reveal(Cell cell)
    {
        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged)
        {
            return;
        }

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Explode(cell);
                break;
            case Cell.Type.Empty:
                Flood(cell);
                break;
            default: 
                cell.revealed = true;
                state[cell.position.x, cell.position.y] = cell;
                break;
        }
    }

    private void Explode(Cell cell)
    {
        cell.revealed = true;
        cell.flagged = false;
        state[cell.position.x, cell.position.y] = cell;
    }

    private void Flood(Cell cell)
    {
        if(cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        if(cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x, cell.position.y - 1));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
        }
    }

    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = Board.Instance.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if(cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }

        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
    }

    public Cell GetCell(int x, int y)
    {
        if (IsValid(x, y))
        {
            return state[x, y];
        }
        else
        {
            return new Cell();
        }
    }

    public bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private void NewGame()
    {
        state = new Cell[width, height];
        cameraFitBasedOnSize();

        GenerateCells();
        GenerateMines();
        GenerateNumbers();
    }

    private void cameraFitBasedOnSize()
    {
        gameObject.transform.position = new Vector2(width / -2.0f, height / -2.0f);
    }

    private void GenerateCells()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        for(int i = 0; i < mineCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            while (state[x,y].type == Cell.Type.Mine)
            {
                x = Random.Range(0, width);
                y = Random.Range(0, height);
            }

            state[x, y].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumbers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if(cell.type == Cell.Type.Mine)
                {
                    continue;
                }

                cell.number = CountMines(x, y);

                if(cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }

                state[x, y] = cell;
            }
        }
    }

    private int CountMines(int cellX, int cellY)
    {
        int count = 0;

        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0)
                {
                    continue;
                }

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if (GetCell(x, y).type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }

        return count;
    }
}
