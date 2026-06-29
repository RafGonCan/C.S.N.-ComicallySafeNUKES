using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SnakeGame : MonoBehaviour
{
    [SerializeField] private RectTransform gameArea;
    [SerializeField] private GameObject snakeBodySegment;
    [SerializeField] private GameObject snakeHeadSegment;
    [SerializeField] private RectTransform food;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject digitalPlutonium;
    [SerializeField] private GameObject startButton;

    private float gridSize = 20f;
    private Vector2 direction;
    private int applesCollected = 0;
    private bool gameCompleted = false;
    private Queue<Vector2> directionQueue = new Queue<Vector2>();
    private int maxQueueSize = 2;
    private List<RectTransform> snake = new List<RectTransform>();
    private List<GameObject> snakeGameObjects = new List<GameObject>();
    private float moveTimer;
    private float moveDelay = 0.2f;
    private bool gameStarted = false;
    private int maxCellsX;
    private int maxCellsY;

    private void Start()
    {
        gridSize = snakeBodySegment.GetComponent<RectTransform>().rect.width;
        Canvas.ForceUpdateCanvases();
        CalculateMaxCells();
    }
    
    public void StartGame()
    {
        if (gameCompleted || gameStarted) return;

        background.SetActive(false);
        Cursor.visible = false;

        CalculateMaxCells();

        gameStarted = true;
        direction = Vector2.right;
        moveTimer = 0;

        foreach (RectTransform segment in snake)
            Destroy(segment.gameObject);

        snake.Clear();
        snakeGameObjects.Clear();

        AddSegment();
        AddSegment();
        SpawnFood();
        
        UpdateHeadRotation();
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
            if (applesCollected >= 12)
            {
                gameCompleted = true;
                digitalPlutonium.SetActive(true);
                startButton.SetActive(false);
                background.SetActive(true);
                Cursor.visible = true;
            }
        }
    }

    private void HandleInput()
    {
        if (directionQueue.Count >= maxQueueSize) return;
        
        if (Input.GetKeyDown(KeyCode.W) && direction != Vector2.down) 
            directionQueue.Enqueue(Vector2.up);
        if (Input.GetKeyDown(KeyCode.S) && direction != Vector2.up) 
            directionQueue.Enqueue(Vector2.down);
        if (Input.GetKeyDown(KeyCode.D) && direction != Vector2.right) 
            directionQueue.Enqueue(Vector2.left);
        if (Input.GetKeyDown(KeyCode.A) && direction != Vector2.left) 
            directionQueue.Enqueue(Vector2.right);
    }

    private void MoveSnake()
    {
        if (directionQueue.Count > 0)
        {
            direction = directionQueue.Dequeue();
            UpdateHeadRotation();
        }
        
        Vector2 newPos = snake[0].anchoredPosition + direction * gridSize;
        newPos = WrapPosition(newPos);

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
                background.SetActive(true);
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
            applesCollected++;
        }
    }

    private void AddSegment()
    {
        GameObject segmentPrefab = snake.Count == 0 ? snakeHeadSegment : snakeBodySegment;
        
        if (segmentPrefab == null)
        {
            Debug.LogError("Snake segment prefab is null!");
            return;
        }
        
        GameObject segment = Instantiate(segmentPrefab, gameArea);
        RectTransform rectTransform = segment.GetComponent<RectTransform>();
        
        rectTransform.anchoredPosition = snake.Count == 0 ? Vector2.zero : snake[snake.Count - 1].anchoredPosition;
        
        snake.Add(rectTransform);
        snakeGameObjects.Add(segment);
        
        if (snake.Count == 1)
        {
            UpdateHeadRotation();
        }
    }

    private void UpdateHeadRotation()
    {
        if (snakeGameObjects.Count == 0) return;
        
        GameObject head = snakeGameObjects[0];
        float rotationAngle = 0f;
        
        if (direction == Vector2.up)
            rotationAngle = 0f;
        else if (direction == Vector2.down)
            rotationAngle = 180f;
        else if (direction == Vector2.right)
            rotationAngle = -90f;
        else if (direction == Vector2.left)
            rotationAngle = 90f;
        
        head.transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);
    }

    private void SpawnFood()
    {
        Vector2 foodPos;
        bool positionIsValid;
        int maxAttempts = 100;
        
        do
        {
            int x = Random.Range(-maxCellsX + 1, maxCellsX);
            int y = Random.Range(-maxCellsY + 1, maxCellsY);
            foodPos = new Vector2(x * gridSize, y * gridSize);
            
            positionIsValid = true;
            foreach (RectTransform segment in snake)
            {
                if (segment.anchoredPosition == foodPos)
                {
                    positionIsValid = false;
                    break;
                }
            }
            
            maxAttempts--;
        } 
        while (!positionIsValid && maxAttempts > 0);
        
        if (!positionIsValid)
        {
            Debug.Log("Could not find empty spot for food!");
            return;
        }
        
        food.anchoredPosition = foodPos;
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
    
    private Vector2 WrapPosition(Vector2 position)
    {
        float halfWidth = maxCellsX * gridSize;
        float halfHeight = maxCellsY * gridSize;
        Vector2 wrappedPos = position;
        
        if (position.x >= halfWidth)
            wrappedPos.x = -halfWidth + gridSize;
        else if (position.x < -halfWidth)
            wrappedPos.x = halfWidth - gridSize;
        

        if (position.y >= halfHeight)
            wrappedPos.y = -halfHeight + gridSize;
        else if (position.y < -halfHeight)
            wrappedPos.y = halfHeight - gridSize;
        
        return wrappedPos;
    }
}