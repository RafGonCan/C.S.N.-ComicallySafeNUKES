using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class SnakeGame : MonoBehaviour
{
    [SerializeField] private RectTransform gameArea;
    [SerializeField] private GameObject snakeSegment;
    [SerializeField] private RectTransform food;

    private float gridSize = 20f;
    private Vector2 direction;
    private List<RectTransform> snake = new List<RectTransform>();
    private float moveTimer;
    private float moveDelay = 0.2f;
    private bool gameStarted = false;
    private int maxCellsX;
    private int maxCellsY;

    private void Start()
    {
        gridSize = snakeSegment.GetComponent<RectTransform>().rect.width;
        Canvas.ForceUpdateCanvases();
        CalculateMaxCells();
    }
    public void StartGame()
    {
        if (gameStarted) return;

        CalculateMaxCells();

        gameStarted = true;
        direction = Vector2.right;
        moveTimer = 0;

        foreach (RectTransform segment in snake)
            Destroy(segment.gameObject);

        snake.Clear();

        AddSegment();
        AddSegment();
        SpawnFood();
    }

    private void Update()
    {
        if (!gameStarted) return;

        HandleInput();

        moveTimer += Time.deltaTime;

        if (moveTimer >= moveDelay)
        {
            moveTimer = 0;
            MoveSnake();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector2.up;
        if (Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector2.down;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector2.right;
        if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector2.left;
    }

    private void MoveSnake()
    {
        {
            Vector2 newPos = snake[0].anchoredPosition + direction * gridSize;

            if (!IsInsideBounds(newPos))
            {
                Debug.Log("Game Over: Snake hit wall");
                gameStarted = false;
                return;
            }

            for (int i = 1; i < snake.Count; i++)
            {
                if (snake[i].anchoredPosition == newPos)
                {
                    Debug.Log("Game Over: Snake hit itself");
                    gameStarted = false;
                    return;
                }
            }

            for (int i = snake.Count - 1; i > 0; i--)
                snake[i].anchoredPosition = snake[i - 1].anchoredPosition;

            snake[0].anchoredPosition = newPos;

            if (snake[0].anchoredPosition == food.anchoredPosition)
            {
                AddSegment();
                SpawnFood();

            }
        }
    }

    private void AddSegment()
    {
        GameObject segment = Instantiate(snakeSegment, gameArea);
        RectTransform rectTransform = segment.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = snake.Count == 0 ? Vector2.zero : snake[snake.Count - 1].anchoredPosition;
        snake.Add(rectTransform);
    }

    private void SpawnFood()
    {
        int x = Random.Range(-maxCellsX + 1, maxCellsX);
        int y = Random.Range(-maxCellsY + 1, maxCellsY);

        float foodX = x * gridSize;
        float foodY = y * gridSize;

        food.anchoredPosition = new Vector2(foodX, foodY);

        food.SetAsLastSibling();
    }

    private bool IsInsideBounds(Vector2 pos)
    {
        float halfWidth = maxCellsX * gridSize;
        float halfHeight = maxCellsY * gridSize;

        return pos.x >= -halfWidth &&
               pos.x < halfWidth &&
               pos.y >= -halfHeight &&
               pos.y < halfHeight;
    }

    private void CalculateMaxCells()
    {
        maxCellsX = Mathf.FloorToInt(gameArea.rect.width / gridSize / 2f);
        maxCellsY = Mathf.FloorToInt(gameArea.rect.height / gridSize / 2f);
    }

}
