using UnityEngine;
using UnityEngine.Tilemaps;

public class RobotController : MonoBehaviour
{
    public GameObject digger_bot;
    
    public static RobotController Instance { get; private set; }
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mouse_position = Board.Instance.tilemap.WorldToCell(worldPosition);
        int board_height = MinesweeperLogic.Instance.height;
        int board_width = MinesweeperLogic.Instance.width;

        //Highlight rows/columns for robot placement
        if(mouse_position.x == -1 && mouse_position.y != board_height || mouse_position.x == board_width && mouse_position.y != -1)
        {
            MinesweeperLogic.Instance.HighlightRow(mouse_position.y);
            if (Input.GetMouseButtonDown(0)) PlaceRobot(mouse_position);
        }
        else if (mouse_position.x != -1 && mouse_position.y == board_height || mouse_position.x != board_width && mouse_position.y == -1)
        {
            MinesweeperLogic.Instance.HighlightColumn(mouse_position.x);
            if (Input.GetMouseButtonDown(0)) PlaceRobot(mouse_position);
        }
        else
        {
            MinesweeperLogic.Instance.HighlightColumn(-1);
            MinesweeperLogic.Instance.HighlightRow(-1);
        }
    }

    public void PlaceRobot(Vector3Int mouse_position)
    {
        Vector2Int direction = Vector2Int.zero;
        int board_height = MinesweeperLogic.Instance.height;
        int board_width = MinesweeperLogic.Instance.width;

        if (mouse_position.x == -1 && mouse_position.y != board_height) direction = Vector2Int.right;
        else if (mouse_position.x == board_width && mouse_position.y != -1) direction = Vector2Int.left;
        else if (mouse_position.x != -1 && mouse_position.y == board_height) direction = Vector2Int.down;
        else if (mouse_position.x != board_width && mouse_position.y == -1) direction = Vector2Int.up;

        Vector2Int bot_pos = new Vector2Int(mouse_position.x + direction.x, mouse_position.y +  direction.y);

        GameObject bot_ref = Instantiate(digger_bot);
        IMovement move_ref = bot_ref.GetComponent<IMovement>();
        move_ref.PlaceRobot(bot_pos, direction);
    }
}
