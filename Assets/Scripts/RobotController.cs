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

    [Header("Code Block Prefabs")]
    public GameObject root_prefab;
    public GameObject node_prefab;

    [Header("Other")]
    public GameObject code_block_display_ref;

    public List<RobotCommand> command_bank = new List<RobotCommand>();
    //What robot we're currently looking at commands for
    private int robot_commands_open = -1;
    
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
        NewRun();
    }

    public void NewRun()
    {
        //For now, acquire three digger bots on start
        for (int i = 0; i < 8; i++)
        {
            GameObject refer = Instantiate(diggerBot, transform);
            refer.GetComponent<SpriteRenderer>().color = Random.ColorHSV();
            ClaimRobot(refer);
        }
        //As well as 2 commands
        command_bank.Add(RobotCommand.TurnRight);
        command_bank.Add(RobotCommand.TurnLeft);
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

    public void DisplayCodeBlocks(int robot)
    {
        if(robot < 0 || robot >= robots.Count) return;
        robot_commands_open = robot;
        UIManager.Instance.SetCodeBlockUI(true);
        Camera.main.transform.position = Camera.main.transform.position + new Vector3(-3, 0 ,0);

        //Fetch everything we need
        GameObject robot_ref = robots[robot];
        IMovement movement_ref = robot_ref.GetComponent<IMovement>();
        List<RobotCommand> flag_command_list = movement_ref.on_flag;

        //Instantiate the command bank code blocks
        for(int i = 0; i < command_bank.Count; i++)
        {
            GameObject node_ref = Instantiate(node_prefab, code_block_display_ref.transform);
            node_ref.GetComponent<RectTransform>().localPosition = new Vector3(0, 0);
            DraggableNode node = node_ref.GetComponent<DraggableNode>();
            node.SetCommand(command_bank[i]);
        }

        //Instantiate root
        GameObject root_ref = Instantiate(root_prefab, code_block_display_ref.transform);
        root_ref.GetComponent<RectTransform>().localPosition = new Vector3(0, 100);
        DropRoot root = root_ref.GetComponent<DropRoot>();
        root.SetTrigger("On Flag");

        for(int i = 0; i < flag_command_list.Count; i++) 
        {
            RobotCommand command = flag_command_list[i];
            GameObject node_ref = Instantiate(node_prefab);
            DraggableNode node = node_ref.GetComponent<DraggableNode>();
            node.SetCommand(command);
            node.AttachToRoot(root);
        }
    }

    //This syncs the new code block state with the robot, stores unused commands in the bank,
    //and closes everything up so we're back to the main game
    public void CloseCodeBlocks()
    {
        List<RobotCommand> new_command_bank = new List<RobotCommand>();

        //Backwards loop is necessary because we destroy the child after loading
        for(int i = code_block_display_ref.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = code_block_display_ref.transform.GetChild(i);
            DropRoot root = child.GetComponent<DropRoot>();

            //Option #1: We're looking at a root (load commands into robot)
            if (root != null)
            {
                if(root.trigger == "On Flag")
                {
                    IMovement robot_script = robots[robot_commands_open].GetComponent<IMovement>();
                    robot_script.on_flag = root.GetCommandList();
                }
            }
            //Option #2: We're looking at an UNATTACHED node (put it in the command bank)
            else if (child.GetComponent<DraggableNode>() != null)
            {
                DraggableNode node = child.GetComponent<DraggableNode>();
                new_command_bank.Add(node.command);
            }
            //Option #3: some other UI element
            else
            {
                continue;
            }

            //Done loading so we can get rid of it
            Destroy(child.gameObject);
        }

        command_bank = new_command_bank;
        robot_commands_open = -1;
        UIManager.Instance.SetCodeBlockUI(false);
        Camera.main.transform.position = Camera.main.transform.position + new Vector3(3, 0, 0);
    }

    public void PauseActiveRobots(bool pause)
    {
        foreach(Transform child in transform)
        {
            //GameObject active means robot active
            if (child.gameObject.activeSelf)
            {
                IMovement robot = child.GetComponent<IMovement>();
                if (robot == null) continue;
                robot.PauseRobot(pause);
                robot.GetComponent<SpriteRenderer>().enabled = !pause;
            }
        }
    }
}
