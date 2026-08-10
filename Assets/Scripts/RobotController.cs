using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

//Singleton that manages robots and robot placement
public class RobotController : MonoBehaviour
{
    [Header("Game State")]
    public List<GameObject> robots = new List<GameObject>();
    public int robot_selected = -1;
    public bool placing = false;

    [Header("Bot Prefabs")]
    public GameObject diggerBot;
    
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
        //For now, acquire three digger bots on start
        for(int i = 0; i < 8; i++)
        {
            GameObject refer = Instantiate(diggerBot, transform);
            refer.GetComponent<SpriteRenderer>().color = Random.ColorHSV();
            ClaimRobot(refer);
        }
        UnselectRobot();
    }

    // Update is called once per frame
    void Update()
    {
        NumKeyRobotSelection();
        RobotPlacement();
    }
    public void NumKeyRobotSelection()
    {
        int key_pressed = GetPressedKeyNumber();
        if (key_pressed == -1) return;
        if(key_pressed - 1 == robot_selected)
        {
            robot_selected = -1;
        }
        else robot_selected = key_pressed - 1;
        if (robot_selected < 0 || robot_selected > robots.Count - 1) placing = false;
        else placing = true;
        UIManager.Instance.UpdateBotListUI();
    }
    public GameObject GetSelectedRobot()
    {
        return robots[robot_selected];
    }
    public void SelectRobot(int select)
    {
        placing = true;
        if(robot_selected == select) UnselectRobot();
        else robot_selected = select;
        UIManager.Instance.UpdateBotListUI();
    }

    public void UnselectRobot()
    {
        placing = false;
        robot_selected = -1;
        UIManager.Instance.UpdateBotListUI();
    }

    private int GetPressedKeyNumber()
    {
        for (int i = 1; i <= 9; i++)
        {
            KeyCode alphaKey = KeyCode.Alpha0 + i;

            if (Input.GetKeyDown(alphaKey))
            {
                return i;
            }
        }
        return -1;
    }
    public void RobotPlacement()
    {
        if (placing == false)
        {
            MinesweeperLogic.Instance.HighlightColumn(-1);
            MinesweeperLogic.Instance.HighlightRow(-1);
            return;
        }

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //To be clear, this is mouse position on the tilemap
        Vector3Int mouse_position = Board.Instance.tilemap.WorldToCell(worldPosition);
        int board_height = MinesweeperLogic.Instance.height;
        int board_width = MinesweeperLogic.Instance.width;

        //Highlight rows/columns for robot placement
        if (mouse_position.x == -1 && mouse_position.y < board_height && mouse_position.y > -1 || mouse_position.x == board_width && mouse_position.y > -1 && mouse_position.y < board_height)
        {
            UIManager.Instance.ToggleValidRobotPlacementUI(true);
            MinesweeperLogic.Instance.HighlightRow(mouse_position.y);
            if (Input.GetMouseButtonDown(0)) PlaceRobot(mouse_position);
        }
        else if (mouse_position.x > -1 && mouse_position.x < board_width && mouse_position.y == board_height || mouse_position.x < board_width && mouse_position.x > -1 && mouse_position.y == -1)
        {
            UIManager.Instance.ToggleValidRobotPlacementUI(true);
            MinesweeperLogic.Instance.HighlightColumn(mouse_position.x);
            if (Input.GetMouseButtonDown(0)) PlaceRobot(mouse_position);
        }
        else
        {
            UIManager.Instance.ToggleValidRobotPlacementUI(false);
            MinesweeperLogic.Instance.HighlightColumn(-1);
            MinesweeperLogic.Instance.HighlightRow(-1);
        }
    }

    public void ClaimRobot(GameObject bot)
    {
        //Store as child
        bot.SetActive(false);
        robots.Add(bot);
        bot.transform.parent = transform;

        UIManager.Instance.UpdateBotListUI();
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

        GameObject bot = robots[robot_selected];
        bot.SetActive(true);
        robots.RemoveAt(robot_selected);
        UnselectRobot();
        IMovement move_ref = bot.GetComponent<IMovement>();
        move_ref.PlaceRobot(bot_pos, direction);

        UIManager.Instance.UpdateBotListUI();
    }
}
