using UnityEngine;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
    public int currency = 0;
    private int FLAGGED_MINES_COST = 2;
    private int EXPLODED_MINES_COST = 1;
    public static RunManager Instance { get; private set; }
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
        
    }
    public void LevelWin()
    {
        currency += FLAGGED_MINES_COST * MinesweeperLogic.Instance.flags_placed + EXPLODED_MINES_COST * MinesweeperLogic.Instance.mines_revealed;
        UIManager.Instance.UpdateCurrencyUI(currency);
    }
    public void NewRun()
    {
        //MinesweeperLogic.Instance.NewGame();
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
