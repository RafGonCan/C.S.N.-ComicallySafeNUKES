using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SnakeGame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform gameArea;
    [SerializeField] private GameObject snakeBodySegment;
    [SerializeField] private GameObject snakeHeadSegment;
    [SerializeField] private RectTransform food;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject digitalPlutonium;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private PlayerMovement playerMovement;

    private float gridSize = 20f;
    private Vector2 direction;
    private int _applesCollected = 0;
    private Queue<Vector2> directionQueue = new Queue<Vector2>();
    private int maxQueueSize = 2;
    private List<RectTransform> snake = new List<RectTransform>();
    private List<GameObject> snakeGameObjects = new List<GameObject>();
    private float moveTimer;
    private float moveDelay = 0.2f;
    private bool gameStarted = false;
    private int maxCellsX;
    private int maxCellsY;

    private InputSystem_Actions _inputActions;
    private InputAction _moveAction;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();

        _moveAction = _inputActions.Player.Move;
        _moveAction.performed += OnMove;
        _moveAction.canceled += OnMove;
    }

    private void Start()
    {
        gridSize = snakeBodySegment.GetComponent<RectTransform>().rect.width;
        Canvas.ForceUpdateCanvases();
        CalculateMaxCells();
        ShowMenu();
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _moveAction.performed -= OnMove;
            _moveAction.canceled -= OnMove;
            _inputActions.Disable();
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (!gameStarted) return;

        Vector2 input = context.ReadValue<Vector2>();
        if (input.magnitude < 0.1f) return;

        Vector2 newDir = Vector2.zero;
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            newDir = input.x > 0 ? Vector2.right : Vector2.left;
        else
            newDir = input.y > 0 ? Vector2.up : Vector2.down;

        if (directionQueue.Count < maxQueueSize)
        {
            Vector2 lastDir = directionQueue.Count > 0 ? directionQueue.ToArray()[directionQueue.Count - 1] : direction;
            if (newDir != -lastDir && newDir != lastDir)
                directionQueue.Enqueue(newDir);
        }
    }

    // ----- Menu / UI -----

    private void ShowMenu()
    {
        background.SetActive(true);
        if (exitButton != null) exitButton.SetActive(true);
        if (_applesCollected >= 12)
        {
            startButton.SetActive(false);
            digitalPlutonium.SetActive(true);
            EventSystem.current?.SetSelectedGameObject(exitButton);
        }
        else
        {
            startButton.SetActive(true);
            _applesCollected = 0;
        }
        if (playerMovement != null)
            playerMovement.CanMove = false;

        gameStarted = false;

        InteractionManager.instance.SetCursorAllowed(true);
        EventSystem.current?.SetSelectedGameObject(startButton);
    }

    public void StartGame()
    {
        background.SetActive(false);
        startButton.SetActive(false);
        if (exitButton != null) exitButton.SetActive(false);

        if (playerMovement != null)
            playerMovement.CanMove = false;

        CalculateMaxCells();

        gameStarted = true;
        direction = Vector2.right;
        moveTimer = 0;
        _applesCollected = 0;

        foreach (RectTransform segment in snake)
            Destroy(segment.gameObject);
        snake.Clear();
        snakeGameObjects.Clear();

        AddSegment();
        AddSegment();
        SpawnFood();
        UpdateHeadRotation();

        // Hide cursor during gameplay
        InteractionManager.instance.SetCursorAllowed(false);

        EventSystem.current?.SetSelectedGameObject(null);
    }

    // ----- Game Loop -----

    private void Update()
    {
        if (!gameStarted) return;

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveDelay)
        {
            moveTimer = 0;
            MoveSnake();

            if (_applesCollected >= 12)
            {
                WinGame();
                return;
            }
        }
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

        for (int i = 1; i < snake.Count; i++)
        {
            if (snake[i].anchoredPosition == newPos)
            {
                Debug.Log("Game Over: Snake hit itself");
                GameOver();
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
            _applesCollected++;
        }
    }

    // ----- Game Over / Win -----

    private void GameOver()
    {
        gameStarted = false;
        ShowMenu();
    }

    private void WinGame()
    {
        gameStarted = false;
        ShowMenu();
    }

    public void ExitGame()
    {
        background.SetActive(false);
        startButton.SetActive(false);
        if (exitButton != null) exitButton.SetActive(false);
        digitalPlutonium.SetActive(false);

        if (playerMovement != null)
            playerMovement.CanMove = true;

        InteractionManager.instance.SetCursorAllowed(false);

        EventSystem.current?.SetSelectedGameObject(null);

        gameStarted = false;
        foreach (RectTransform segment in snake)
            Destroy(segment.gameObject);
        snake.Clear();
        snakeGameObjects.Clear();
    }

    // ----- Snake Body Methods -----

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

        if (snake.Count == 1) UpdateHeadRotation();
    }

    private void UpdateHeadRotation()
    {
        if (snakeGameObjects.Count == 0) return;
        GameObject head = snakeGameObjects[0];
        float rotationAngle = 0f;
        if (direction == Vector2.up) rotationAngle = 0f;
        else if (direction == Vector2.down) rotationAngle = 180f;
        else if (direction == Vector2.right) rotationAngle = 90f;
        else if (direction == Vector2.left) rotationAngle = -90f;
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
        return pos.x >= -halfWidth && pos.x < halfWidth &&
               pos.y >= -halfHeight && pos.y < halfHeight;
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