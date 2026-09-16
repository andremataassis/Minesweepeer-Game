using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UIManager : MonoBehaviour
{
    [SerializeField] public GameObject left_panel;
    public float left_panel_height;
    public float left_panel_width;
    public float bot_pos_increments;
    [SerializeField] public GameObject image_prefab;
    [SerializeField] public GameObject submit_button_ref;
    private Button submit_button;
    [SerializeField] public GameObject completion_txt_ref;
    private TextMeshProUGUI completion_txt;
    [SerializeField] public GameObject flag_overlay_ref;
    [SerializeField] public GameObject robot_overlay_ref;
    private bool valid_robot_placement = false;
    [SerializeField] public GameObject code_block_ui_ref;
    [SerializeField] public GameObject win_screen_ref;
    [SerializeField] public GameObject lose_screen_ref;
    [SerializeField] public GameObject currency_UI_ref;
    private TextMeshProUGUI currency_UI_txt;
    public static UIManager Instance { get; private set; }
    private void Awake()
    {
        // Enforce the single-instance rule
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        submit_button = submit_button_ref.GetComponent<Button>();
        completion_txt = completion_txt_ref.GetComponent<TextMeshProUGUI>();
        currency_UI_txt = currency_UI_ref.GetComponent <TextMeshProUGUI>();
        Instance = this;
    }

    public void ToggleValidRobotPlacementUI(bool x)
    {
        valid_robot_placement = x;
    }

    public void UpdateBotListUI()
    {
        //Delete existing images
        for (int i = left_panel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(left_panel.transform.GetChild(i).gameObject);
        }

        int robots = RobotController.Instance.robots.Count;
        for(int i = 0; i < robots; i++)
        {
            SpriteRenderer bot_sr = RobotController.Instance.robots[i].GetComponent<SpriteRenderer>();
            GameObject img_ref = Instantiate(image_prefab, left_panel.transform);
            RectTransform img_rt = img_ref.GetComponent<RectTransform>();
            float selected_mod = RobotController.Instance.robot_selected == i ? left_panel_width/8f : 0;
            img_rt.localPosition = new Vector3(0 + selected_mod, left_panel_height/2f - bot_pos_increments*(i+1), 0);
            Image img = img_ref.GetComponent<Image>();
            img.sprite = bot_sr.sprite;
            img.color = bot_sr.color;

            //Modify button component for selection
            Button select_button = img_ref.GetComponent<Button>();
            if(RobotController.Instance.robot_selected == i) select_button.Select();
            int this_robot = i;
            select_button.onClick.AddListener(() => RobotController.Instance.SelectRobot(this_robot));

            //Set up code button
            Button edit_button = img_ref.transform.GetChild(1).GetComponent<Button>();
            edit_button.onClick.AddListener(() => RobotController.Instance.DisplayCodeBlocks(this_robot));

            TextMeshProUGUI txt = img_ref.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            txt.text = $"{this_robot + 1}";
        }
    }

    public void SubmitButtonOnClick()
    {
        bool result = MinesweeperLogic.Instance.CheckIfWon();
        if (result)
        {
            RunManager.Instance.LevelWin();
            win_screen_ref.SetActive(true);
        }
        else
        {
            lose_screen_ref.SetActive(true);
        }
    }
    
    public void UpdateCurrencyUI(int c)
    {
        currency_UI_txt.text = $"${c}";
    }

    public void ContinueButton()
    {
        Debug.Log("yup");
    }

    public void RetryButton()
    {
        RunManager.Instance.NewRun();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        left_panel_height = Instance.left_panel.GetComponent<RectTransform>().rect.height;
        left_panel_width = Instance.left_panel.GetComponent <RectTransform>().rect.width;
        bot_pos_increments = left_panel_height / 9f;
        UpdateBotListUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (MinesweeperLogic.Instance.paused) return;
        UpdateGameStateUI();
        UpdateFlagOverlay();
        UpdateRobotOverlay();
    }

    public void UpdateGameStateUI()
    {
        submit_button.interactable = MinesweeperLogic.Instance.win_state;
        int flags = MinesweeperLogic.Instance.flags_placed;
        int mines = MinesweeperLogic.Instance.mines_revealed;
        int mines_total = MinesweeperLogic.Instance.mineCount;
        completion_txt.text = $"{flags}<sprite=0> + {mines}<sprite=1>  = {flags + mines} / {mines_total}";
    }

    public void UpdateFlagOverlay()
    {
        flag_overlay_ref.SetActive(false);

        //Debating whether flag placement should be enabled while a robot is selected...
        //if (RobotController.Instance.placing) return;

        Cell cell = MinesweeperLogic.Instance.worldCoordinateToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (cell.type == Cell.Type.Invalid || cell.revealed) return;
        flag_overlay_ref.transform.position = MinesweeperLogic.Instance.cellToWorldCoordinate(cell);
        flag_overlay_ref.SetActive(true);
    }

    public void UpdateRobotOverlay()
    {
        robot_overlay_ref.SetActive(false);

        if (!RobotController.Instance.placing || !valid_robot_placement) return;

        Cell cell = MinesweeperLogic.Instance.worldCoordinateToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (cell.type != Cell.Type.Invalid) return;
        robot_overlay_ref.transform.position = MinesweeperLogic.Instance.cellToWorldCoordinate(cell);
        robot_overlay_ref.SetActive(true);
        SpriteRenderer robot_spr = RobotController.Instance.GetSelectedRobot().GetComponent<SpriteRenderer>();
        robot_overlay_ref.GetComponent<SpriteRenderer>().sprite = robot_spr.sprite;
        robot_overlay_ref.GetComponent<SpriteRenderer>().color = robot_spr.color;
        robot_overlay_ref.GetComponent<SpriteRenderer>().material = robot_spr.material;
        robot_overlay_ref.GetComponent<SpriteRenderer>().sortingLayerID = robot_spr.sortingLayerID;
        robot_overlay_ref.GetComponent<SpriteRenderer>().sortingOrder = robot_spr.sortingOrder;
        robot_overlay_ref.GetComponent<SpriteRenderer>().flipX = robot_spr.flipX;
        robot_overlay_ref.GetComponent<SpriteRenderer>().flipY = robot_spr.flipY;
        robot_overlay_ref.GetComponent<SpriteRenderer>().drawMode = robot_spr.drawMode;
    }

    public void SetCodeBlockUI(bool enabled)
    {
        code_block_ui_ref.SetActive(enabled);
        foreach(Transform child in transform)
        {
            if(child.gameObject.tag == "MainGameUI") child.gameObject.SetActive(!enabled);
        }
    }
}
