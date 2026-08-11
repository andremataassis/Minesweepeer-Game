using UnityEditor;
using UnityEngine;

//Singleton
public class MinesweeperLogic : MonoBehaviour
{
    [Header("Game Settings")]
    public int width = 36;
    public int height = 20;
    public int mineCount = 50;

    [Header("Game State")]
    public int mines_revealed = 0;
    public int flags_placed = 0;
    //Whether mines_revealed + flags_placed == mineCount -- this being true does NOT mean the game is won
    public bool win_state = false;

    private Cell[,] state;

    //Highlight effects
    public int highlighted_row;
    public int highlighted_column;

    public bool paused = false;

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
        if (paused) return;
        if (Input.GetMouseButtonDown(0))
        {
            Flag();
        }
        if (Input.GetKey(KeyCode.Keypad0))
        {
            RevealAllCells();
        }
        Board.Instance.Draw(state);
        win_state = CheckIfCanWin();
    }

    public void RevealAllCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = GetCell(x, y);
                Reveal(cell);
            }
        }
    }

    //To unhighlight, just give an invalid row #
    public void HighlightRow(int row)
    {
        //unhighlight previous
        for (int i = 0; i < width; i++)
        {
            if(highlighted_row == -1) break;
            Cell cell = GetCell(i, highlighted_row);
            cell.highlighted = false;
            state[cell.position.x, cell.position.y] = cell;
        }

        //highlight new
        for (int i = 0; i < width; i++)
        {
            Cell cell = GetCell(i, row);
            if (cell.type == Cell.Type.Invalid)
            {
                highlighted_row = -1;
                return;
            }
            cell.highlighted = true;
            state[cell.position.x, cell.position.y] = cell;
        }
        highlighted_row = row;
    }

    //To unhighlight, just give an invalid column #
    public void HighlightColumn(int column)
    {
        //unhighlight previous
        for (int i = 0; i < height; i++)
        {
            if (highlighted_column == -1) break;
            Cell cell = GetCell(highlighted_column, i);
            cell.highlighted = false;
            state[cell.position.x, cell.position.y] = cell;
        }

        //highlight new
        for (int i = 0; i < height; i++)
        {
            Cell cell = GetCell(column, i);
            if (cell.type == Cell.Type.Invalid)
            {
                highlighted_column = -1;
                return;
            }
            cell.highlighted = true;
            state[cell.position.x, cell.position.y] = cell;
        }
        highlighted_column = column;
    }

    public Cell worldCoordinateToCell(Vector3 worldPosition)
    {
        Vector3Int cellPosition = Board.Instance.tilemap.WorldToCell(worldPosition);
        return GetCell(cellPosition.x, cellPosition.y);
    }

    public Vector3 cellToWorldCoordinate(Cell cell)
    {
        return Board.Instance.tilemap.GetCellCenterWorld(cell.position);
    }

    public void Reveal(Cell cell)
    {
        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }
        else if (cell.flagged)
        {
            cell.flagged = false;
            flags_placed--;
        }

        switch (cell.type)
        {
            case Cell.Type.Mine:
                mines_revealed++;
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
        if (cell.flagged) { cell.flagged = false; flags_placed--; }

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
        if (cell.flagged) flags_placed++;
        else flags_placed--;
        state[cellPosition.x, cellPosition.y] = cell;
    }

    private void Flag(Cell cell)
    {
        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }

        cell.flagged = !cell.flagged;
        if (cell.flagged) flags_placed++;
        else flags_placed--;
        state[cell.position.x, cell.position.y] = cell;
    }

    public Cell GetCell(int x, int y)
    {
        if (IsValid(x, y))
        {
            return state[x, y];
        }
        else
        {
            Cell cell = new Cell();
            cell.position.x = x;
            cell.position.y = y;
            cell.type = Cell.Type.Invalid;
            return cell;
        }
    }

    public bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
    public bool CheckIfCanWin()
    {
        return flags_placed + mines_revealed == mineCount;
    }

    public bool CheckIfWon()
    {
        if(win_state == false) return false;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = GetCell(x, y);
                if (cell.flagged && cell.type != Cell.Type.Mine || cell.type == Cell.Type.Mine && cell.revealed == false && cell.flagged == false) return false;
            }
        }
        return true;
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

    public void PauseGameplay(bool p)
    {
        paused = p;
        Board.Instance.gameObject.SetActive(!paused);
    }
}
