using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField, Tooltip("Reference to the BoardManager")]
    private BoardManager boardManager;
    [SerializeField, Tooltip("Reference to the PlayerController")]
    private PlayerController playerController;
    [SerializeField, Tooltip("Reference to the UIDocument for UI Toolkit")]
    private UIDocument uiDocument;

    [SerializeField, Range(50, 200), Tooltip("Starting food amount")]
    private int startingFood = 100;

    private TurnManager turnManager;
    private int currentFood;
    private int currentLevel;
    private bool isGameOver;

    private Label foodLabel;
    private VisualElement gameOverPanel;
    private Label levelLabel;

    public TurnManager TurnManager => turnManager;
    public BoardManager BoardManager => boardManager;
    public PlayerController PlayerController => playerController;
    public int CurrentFood => currentFood;
    public int CurrentLevel => currentLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        turnManager = new TurnManager();
        currentFood = startingFood;
        currentLevel = 1;
        isGameOver = false;

        SetupUI();
        StartLevel();
    }

    private void SetupUI()
    {
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        foodLabel = root.Q<Label>("FoodLabel");
        gameOverPanel = root.Q<VisualElement>("GameOverPanel");
        levelLabel = root.Q<Label>("LevelLabel");

        if (gameOverPanel != null)
            gameOverPanel.style.display = DisplayStyle.None;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (foodLabel != null)
            foodLabel.text = "Food: " + currentFood;
        if (levelLabel != null)
            levelLabel.text = "Day " + currentLevel;
    }

    private void StartLevel()
    {
        boardManager.Init();
        playerController.Spawn(1, 1);
        playerController.EnableInput(true);
        UpdateUI();
    }

    public void NextLevel()
    {
        currentLevel++;
        boardManager.Clean();
        StartLevel();
    }

    public void ChangeFood(int amount)
    {
        currentFood += amount;

        if (currentFood <= 0)
        {
            currentFood = 0;
            GameOver();
        }

        UpdateUI();
    }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        playerController.EnableInput(false);
        AudioManager.Instance?.PlayGameOver();

        if (gameOverPanel != null)
            gameOverPanel.style.display = DisplayStyle.Flex;
    }

    private void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.Return))
        {
            RestartGame();
        }
    }

    private void RestartGame()
    {
        isGameOver = false;
        currentFood = startingFood;
        currentLevel = 1;

        boardManager.Clean();

        if (gameOverPanel != null)
            gameOverPanel.style.display = DisplayStyle.None;

        turnManager = new TurnManager();
        StartLevel();
    }
}
