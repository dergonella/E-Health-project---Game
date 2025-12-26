using UnityEngine;

/// <summary>
/// Manages background sprite for levels.
/// Automatically scales to fit the camera view.
/// Add this to an empty GameObject in your scene or let MainLevelSetup create it.
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    [Header("Background Settings")]
    [Tooltip("The background sprite to display")]
    public Sprite backgroundSprite;

    [Tooltip("Background color if no sprite is assigned")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.15f); // Dark blue-gray

    [Tooltip("Sorting order (negative = behind everything)")]
    public int sortingOrder = -100;

    [Tooltip("Optional: Different backgrounds per persona")]
    public Sprite brightgroveBackground;
    public Sprite silvergroveBackground;
    public Sprite stonegroveBackground;

    // The actual background GameObject
    private GameObject backgroundObject;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        CreateBackground();
    }

    void Start()
    {
        // Select background based on persona if available
        SelectPersonaBackground();

        // Scale to fit camera
        ScaleToFitCamera();
    }

    void CreateBackground()
    {
        // Create background GameObject
        backgroundObject = new GameObject("LevelBackground");
        backgroundObject.transform.SetParent(transform);
        backgroundObject.transform.localPosition = new Vector3(0f, 0f, 10f); // Behind everything

        // Add SpriteRenderer
        spriteRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = sortingOrder;

        // Set sprite or create solid color background
        if (backgroundSprite != null)
        {
            spriteRenderer.sprite = backgroundSprite;
        }
        else
        {
            // Create a simple 1x1 white texture and tint it
            CreateSolidColorBackground();
        }
    }

    void SelectPersonaBackground()
    {
        Sprite selectedSprite = null;

        // Check GameState for current persona
        switch (GameState.SelectedPersona)
        {
            case GameState.Persona.Brightgrove:
                selectedSprite = brightgroveBackground;
                break;
            case GameState.Persona.Silvergrove:
                selectedSprite = silvergroveBackground;
                break;
            case GameState.Persona.Stonegrove:
                selectedSprite = stonegroveBackground;
                break;
        }

        // Use persona-specific background if available, otherwise use default
        if (selectedSprite != null)
        {
            spriteRenderer.sprite = selectedSprite;
            spriteRenderer.color = Color.white; // Reset color tint
        }
        else if (backgroundSprite != null)
        {
            spriteRenderer.sprite = backgroundSprite;
            spriteRenderer.color = Color.white;
        }
    }

    void CreateSolidColorBackground()
    {
        // Create a 1x1 white texture
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        // Create sprite from texture
        Sprite solidSprite = Sprite.Create(
            texture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f // 1 pixel per unit for easy scaling
        );

        spriteRenderer.sprite = solidSprite;
        spriteRenderer.color = backgroundColor;
    }

    void ScaleToFitCamera()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Get camera bounds
        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // Get sprite bounds
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        // Calculate scale to cover entire camera view (with some padding)
        float scaleX = (cameraWidth / spriteSize.x) * 1.1f; // 10% padding
        float scaleY = (cameraHeight / spriteSize.y) * 1.1f;

        // Use the larger scale to ensure full coverage
        float scale = Mathf.Max(scaleX, scaleY);

        backgroundObject.transform.localScale = new Vector3(scale, scale, 1f);

        Debug.Log($"[BackgroundManager] Scaled background to {scale}x to fit camera ({cameraWidth}x{cameraHeight})");
    }

    /// <summary>
    /// Change background sprite at runtime
    /// </summary>
    public void SetBackground(Sprite newSprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = newSprite;
            spriteRenderer.color = Color.white;
            ScaleToFitCamera();
        }
    }

    /// <summary>
    /// Change background color (for solid color backgrounds)
    /// </summary>
    public void SetBackgroundColor(Color color)
    {
        backgroundColor = color;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    /// <summary>
    /// Add parallax scrolling effect (optional)
    /// </summary>
    public void EnableParallax(float parallaxFactor = 0.5f)
    {
        BackgroundParallax parallax = backgroundObject.AddComponent<BackgroundParallax>();
        parallax.parallaxFactor = parallaxFactor;
    }

    void OnDestroy()
    {
        if (backgroundObject != null)
        {
            Destroy(backgroundObject);
        }
    }
}

/// <summary>
/// Optional parallax scrolling for background
/// </summary>
public class BackgroundParallax : MonoBehaviour
{
    public float parallaxFactor = 0.5f;

    private Camera mainCamera;
    private Vector3 lastCameraPosition;
    private Vector3 startPosition;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            lastCameraPosition = mainCamera.transform.position;
            startPosition = transform.position;
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        Vector3 deltaMovement = mainCamera.transform.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxFactor, deltaMovement.y * parallaxFactor, 0f);
        lastCameraPosition = mainCamera.transform.position;
    }
}
