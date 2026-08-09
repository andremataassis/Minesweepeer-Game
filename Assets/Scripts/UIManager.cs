using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] public GameObject left_panel;
    public float left_panel_height;
    public float left_panel_width;
    public float bot_pos_increments;
    [SerializeField] public GameObject image_prefab;
    public static UIManager Instance { get; private set; }
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
            Button button = img_ref.GetComponent <Button>();
            int this_robot = i;
            button.onClick.AddListener(() => RobotController.Instance.SelectRobot(this_robot));
        }
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
        
    }
}
