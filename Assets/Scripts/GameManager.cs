using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private int mineCount = 15;
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private Sprite mineSprite;
    [SerializeField] private Sprite explosionSprite;
    [SerializeField] private Sprite flagSprite;
    [SerializeField] private Sprite notmineSprite;
    [SerializeField] private Sprite[] numberSprites;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject customPanel;
    [SerializeField] private GameObject gameWonPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TMP_InputField widthInput;
    [SerializeField] private TMP_InputField heightInput;
    [SerializeField] private TMP_InputField mineCountInput;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text mineCounterText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Transform boardParent;
    [SerializeField] private TMP_Text winHighScoreText;
    [SerializeField] private TMP_Text loseHighScoreText;
    private Cell[,] grid;
    private bool gameOver;
    private bool gameWon;
    private bool firstMove = true;
    private int remainingMines;
    private int safeCellCount;
    private bool timerRunning;
    private double elapsedTime;
    private bool newHighScore;
    private Difficulty currentDifficulty;

    private void Start()
    {
    }

    private void Update()
    {
        if(gameOver || gameWon ) return;

        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cell cell = GetClickedCell();
            if (cell != null && !cell.isFlagged) OnCellClicked(cell);
        }
        if (Input.GetMouseButtonDown(1))
        {
            Cell cell = GetClickedCell();
            if (cell != null && !cell.isRevealed) OnCellRightClicked(cell);
        }
    }

    private void Awake()
    {
        Instance = this;
        customPanel.SetActive(false);
        gameWonPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
    }
    
    private Cell GetClickedCell()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if(hit == null) return null;
        return hit.GetComponent<Cell>();
    }

    private void StartGame(int width, int height, int mineCount)
    {
        this.width = width;
        this.height = height;
        this.mineCount = mineCount;
        remainingMines = mineCount;

        mainMenuPanel.SetActive(false);
        gamePanel.SetActive(true);
        mineCounterText.text = remainingMines.ToString();

        Restart();
        AdjustCamera();
    }

    public void MainMenu()
    {
        gameOverPanel.SetActive(false);
        gameWonPanel.SetActive(false);
        customPanel.SetActive(false);
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        boardParent.gameObject.SetActive(true);
        DestroyGrid();

        mainMenuPanel.SetActive(true);
    }

    public void RestartButton()
    {
        gameWonPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        gamePanel.SetActive(true);
        boardParent.gameObject.SetActive(true);

        Restart();

        mineCounterText.text = remainingMines.ToString();
    }

    public void Custom()
    {
        mainMenuPanel.SetActive(false);
        customPanel.SetActive(true);
    }

    public void Noob()
    {
        currentDifficulty = Difficulty.Noob;
        StartGame(10,12,16);
    }

    public void Pro()
    {
        currentDifficulty = Difficulty.Pro;
        StartGame(16,20,40);
    }

    public void Hacker()
    {
        currentDifficulty = Difficulty.Hacker;
        StartGame(25,32,100);
    }

    public void God()
    {
        currentDifficulty = Difficulty.God;
        StartGame(40,70,300);
    }

    public void Pause()
    {
        if(gameWon || gameOver) return;
        gamePanel.SetActive(false);
        pausePanel.SetActive(true);
        boardParent.gameObject.SetActive(false);

        timerRunning = false;
    }

    public void Continue()
    {
        gamePanel.SetActive(true);
        pausePanel.SetActive(false);
        boardParent.gameObject.SetActive(true);

        if(!firstMove) timerRunning = true;
    }

    private void AdjustCamera()
    {
        Camera.main.transform.position = new Vector3(width/2.0f - 0.5f, height/2.0f - 0.5f, -10.0f);
        
        float verticalSize = height / 2f;
        float horizontalSize = width / (2f * Camera.main.aspect);

        Camera.main.orthographicSize = Mathf.Max(verticalSize, horizontalSize) + 1.0f;
    }

    public void StartCustomGame()
    {
        if(!int.TryParse(widthInput.text , out int width)) return;
        if(!int.TryParse(heightInput.text , out int height)) return;
        if(!int.TryParse(mineCountInput.text , out int mineCount)) return;

        if(width < 5 || height < 5)
        {
            Debug.Log("Width and height cannot be less than 5.");
            return;
        } 
        if(width > 100 || height > 100)
        {
            Debug.Log("Width and height cannot be more than 100.");
            return;
        } 
        if(mineCount <= 0)
        {
            Debug.Log("Mine Count has to be at least 1.");
            return;
        } 
        if(mineCount >= width * height - 9)
        {
            Debug.Log("Mine Count is too much.");
            return;
        } 

        customPanel.SetActive(false);

        currentDifficulty = Difficulty.Custom;
        StartGame(width, height, mineCount);
    }

    public Sprite GetNumberSprite(int number)
    {
        if (number < 1 || number > 8)
            return null;

        return numberSprites[number - 1];
    }

    public Sprite GetMineSprite()
    {
        return mineSprite;
    }

    public Sprite GetExplosionSprite()
    {
        return explosionSprite;
    }

    public Sprite GetFlagSprite()
    {
        return flagSprite;
    }

    public Sprite GetNotmineSprite()
    {
        return notmineSprite;
    }

    private void UpdateTimerUI()
    {
        timerText.text = FormatTime((int)elapsedTime);
    }

    private void FloodFill(Cell cell)
    {
        if(!cell.isRevealed && !cell.isMine)
        {
            cell.Reveal();
            safeCellCount++;

            CheckWin();
            if(gameWon) return;

            if(cell.adjacentMines == 0)
            {
                for(int dx=-1; dx <= 1; dx++)
                {
                    for(int dy = -1; dy <= 1; dy++)
                    {
                        if(dx==0 && dy==0) continue;

                        int x = cell.x + dx;
                        int y = cell.y + dy;

                        if(0 <= x && x < width && 0 <= y && y < height) FloodFill(grid[x,y]);
                    }
                }   
            }

        }
    }

    private void SaveHighScore()
    {
        if (currentDifficulty == Difficulty.Custom) return;

        string key = $"HighScore_{currentDifficulty}";

        int bestTime = PlayerPrefs.GetInt(key, int.MaxValue);
        int currentTime = (int)elapsedTime; 

        if (currentTime < bestTime)
        {
            PlayerPrefs.SetInt(key, currentTime);
            PlayerPrefs.Save();
            newHighScore = true;
        }
    }

    private void PrintHighScore()
    {
        string text;

        if (currentDifficulty == Difficulty.Custom)
        {
            text = "";
        }
        else if (newHighScore)
        {
            text = "New High Score!";
        }
        else
        {
            int bestTime = PlayerPrefs.GetInt($"HighScore_{currentDifficulty}", int.MaxValue);

            if (bestTime == int.MaxValue) text = "Best: --:--";
            else text = "Best: " + FormatTime(bestTime);
        }

        winHighScoreText.text = text;
        loseHighScoreText.text = text;
    }

    private int GetHighScore(Difficulty difficulty)
    {
        return PlayerPrefs.GetInt($"HighScore_{difficulty}", int.MaxValue);
    }

    private string FormatTime(int seconds)
    {
        if (seconds == int.MaxValue) return "--:--";

        int minutes = seconds / 60;
        int secs = seconds % 60;

        return $"{minutes:00}:{secs:00}";
    }

    private void OnCellRightClicked(Cell cell)
    {
        if(cell.isRevealed || firstMove) return;

        if(cell.ToggleFlag()) remainingMines--;
        else remainingMines++;
        mineCounterText.text = remainingMines.ToString();
    }

    private void OnCellClicked(Cell cell)
    {
        if (firstMove)
        {
            PlaceMines(cell);
            CalculateAdjacentMines();
            timerRunning = true;
            firstMove = false;
            FloodFill(cell);
            return;    
        }

        if (cell.isMine)
        {
            GameOver(cell);
            return;
        }

        FloodFill(cell);

        if (cell.isRevealed && CountAdjacentFlags(cell) >= cell.adjacentMines ) Chord(cell);
    }

    private void Restart()
    {
        DestroyGrid();
        ResetVariables();
        GenerateGrid();
        UpdateTimerUI();
    }

    private void DestroyGrid()
    {
        if(grid == null) return;

        foreach (Cell cell in grid)
        {
            if(cell != null) Destroy(cell.gameObject);
        }

        grid = null;
    }

    private void ResetVariables()
    {
        gameOver = false;
        gameWon = false;
        timerRunning = false;
        newHighScore = false;
        firstMove = true;
        remainingMines = mineCount;
        safeCellCount = 0;
        elapsedTime = 0;
    }

    private void Chord(Cell cell)
    {
        for(int dx = -1; dx <= 1; dx++)
        {
            for(int dy = -1; dy <= 1; dy++)
            {
                if(dx==0 && dy==0) continue;

                int x = dx + cell.x;
                int y = dy + cell.y;

                if(0 <= x && x < width && 0 <= y && y < height && !grid[x, y].isFlagged)
                {
                    if (grid[x, y].isMine)
                    {
                        GameOver(grid[x,y]);
                        return;
                    } 
                    else FloodFill(grid[x,y]);
                }
            }
        }
    }
    private int CountAdjacentFlags(Cell cell)
    {
        int count = 0;
        for(int dx = -1; dx <= 1; dx++)
        {
            for(int dy = -1; dy <= 1; dy++)
            {
                if(dx==0 && dy==0) continue;

                int x = dx + cell.x;
                int y = dy + cell.y;

                if(0 <= x && x < width && 0 <= y && y < height && grid[x,y].isFlagged) count++;
            }
        }
        return count;
    }

    private void GameOver(Cell cell)
    {
        gameOver = true;
        Debug.Log("BOOOOOOOOOOOOOOOM!");
        cell.Reveal();
        timerRunning = false;
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                grid[x,y].Mine();
            }
        }
        StartCoroutine(ShowGameOver());
    }

    private void FlagEveryCell()
    {
        foreach(Cell cell in grid)
        {
            if(!cell.isRevealed && !cell.isFlagged) cell.ToggleFlag();
        }
    }
    private void CheckWin()
    {
        if(gameOver || safeCellCount+mineCount != width*height) return;

        FlagEveryCell();
        remainingMines = 0;
        timerRunning = false;
        gameWon = true;

        SaveHighScore();

        StartCoroutine(ShowGameWon());
    }
    private void CalculateAdjacentMines()
    {
        foreach(Cell cell in grid)
        {
            if(cell.isMine) continue;

            for(int dx=-1; dx <= 1; dx++)
            {
                for(int dy = -1; dy <=1; dy++)
                {
                    if(dx==0 && dy==0) continue;

                    int x = cell.x + dx;
                    int y = cell.y + dy;

                    if(0 <= x && x < width && 0 <= y && y < height && grid[x,y].isMine) cell.adjacentMines++;
                }
            }
        }
    }
    private void PlaceMines(Cell cell)
    {
        int count = 0;

        while (count < mineCount)
        {
            int x = Random.Range(0,width);
            int y = Random.Range(0,height);

            if (!grid[x, y].isMine && !(Mathf.Abs(x-cell.x) <= 1 && Mathf.Abs(y-cell.y) <= 1))
            {
                grid[x,y].isMine = true;
                count++;
            }
        }

        foreach(Cell blank in grid)
        {
            if(blank.isMine) Debug.Log($"Mine at ({blank.x},{blank.y})");
        }
    }
    private void GenerateGrid()
    {
        grid = new Cell[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject obj = Instantiate(cellPrefab,
                                             new Vector3(x, y, 0),
                                             Quaternion.identity, boardParent);

                Cell cell = obj.GetComponent<Cell>();

                cell.x = x;
                cell.y = y;

                grid[x, y] = cell;
            }
        }
    }

    public enum Difficulty
    {
        Noob, Pro, Hacker, God, Custom
    }

    private IEnumerator ShowGameOver()
    {
        yield return new WaitForSeconds(2f);

        DestroyGrid();
        gameOverPanel.SetActive(true);
        gamePanel.SetActive(false);
        PrintHighScore();
    }

    private IEnumerator ShowGameWon()
    {
        yield return new WaitForSeconds(2f);

        DestroyGrid();
        gameWonPanel.SetActive(true);
        gamePanel.SetActive(false);
        timeText.text = "Completed in: " + timerText.text;
        PrintHighScore();
    }
}