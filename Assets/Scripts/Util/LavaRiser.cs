using UnityEngine;

public class LavaRiser : MonoBehaviour
{
    [Header("Lava Movement")]
    [SerializeField] private float riseSpeed = 0.5f;
    [SerializeField] private float maxHeight = 10f;
    [SerializeField] private float startHeight = -1f;
    
    [Header("KillZone Settings")]
    [SerializeField] private Transform playerRespawnPoint;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private LayerMask playerLayerMask = 1; // Default to layer 0
    
    [Header("Visualization")]
    [SerializeField] private bool showKillZone = true;
    [SerializeField] private Color killZoneColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Material killZoneMaterial;
    [SerializeField] private string materialName = "Lava_Floor";
    
    private GameObject killZoneObject;
    private GameObject visualizerObject;
    private GameManager gameManager;
    private AudioManager audioManager;
    private int lastLivesCount;
    private Vector3 planeSize;
    private bool isRising = false;
    private bool shouldRise = true;
    
    private void Start()
    {
        // Try to find the material if it's not set
        if (killZoneMaterial == null)
        {
            FindMaterial();
        }
        
        // Create a KillZone object that matches the plane size
        CreateKillZone();
        
        // Find game manager
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogWarning("LavaRiser: GameManager not found! Lives monitoring will not work.");
        }
        else
        {
            lastLivesCount = gameManager.CurrentLives;
        }
        
        // Find audio manager
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogWarning("LavaRiser: AudioManager not found! Sound will not play.");
            
            // Create an AudioManager if one doesn't exist
            GameObject audioManagerObj = new GameObject("AudioManager");
            audioManager = audioManagerObj.AddComponent<AudioManager>();
        }
    }
    
    private void FindMaterial()
    {
        // Try to find the material in the scene first (from any renderer using it)
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat != null && mat.name.Contains(materialName))
                {
                    killZoneMaterial = mat;
                    Debug.Log($"Found material '{materialName}' from scene objects");
                    return;
                }
            }
        }
        
        // If we can't find it in the scene, try to load it as an asset
        // Note: This won't work in a build unless the material is in a Resources folder
        // This is just a fallback for the editor
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Material " + materialName);
        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            killZoneMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            Debug.Log($"Found material '{materialName}' at path: {path}");
        }
#endif
    }
    
    private void CreateKillZone()
    {
        // Create a child object for the KillZone
        killZoneObject = new GameObject("Rising KillZone");
        killZoneObject.transform.SetParent(transform);
        
        // Set initial position to match the plane but at start height
        killZoneObject.transform.position = new Vector3(
            transform.position.x,
            startHeight,
            transform.position.z
        );
        
        // Get the plane's mesh size to match the collider
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            planeSize = renderer.bounds.size;
        }
        else
        {
            planeSize = new Vector3(10, 0.1f, 10); // Default size
            Debug.LogWarning("LavaRiser: Couldn't get plane size, using default.");
        }
        
        // Match the rotation
        killZoneObject.transform.rotation = transform.rotation;
        
        // Create a box collider that matches the plane size
        BoxCollider killZoneCollider = killZoneObject.AddComponent<BoxCollider>();
        killZoneCollider.isTrigger = true;
        killZoneCollider.size = new Vector3(planeSize.x, 0.5f, planeSize.z);
        
        // Add a custom trigger script instead of KillZone
        LavaKillTrigger trigger = killZoneObject.AddComponent<LavaKillTrigger>();
        trigger.Initialize(playerRespawnPoint, playerTag, playerLayerMask);
        
        // Create the kill zone visualizer if enabled
        if (showKillZone)
        {
            CreateKillZoneVisualizer();
        }
    }
    
    private void CreateKillZoneVisualizer()
    {
        // Create a child object for visualization using a primitive cube
        visualizerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualizerObject.name = "KillZone Visualizer";
        
        // Remove the collider from the primitive as we already have one
        Destroy(visualizerObject.GetComponent<Collider>());
        
        // Set it as child of the kill zone
        visualizerObject.transform.SetParent(killZoneObject.transform);
        visualizerObject.transform.localPosition = Vector3.zero;
        visualizerObject.transform.localRotation = Quaternion.identity;
        
        // Scale it to match the kill zone size
        visualizerObject.transform.localScale = new Vector3(planeSize.x, 0.5f, planeSize.z);
        
        // Set up the material
        MeshRenderer meshRenderer = visualizerObject.GetComponent<MeshRenderer>();
        
        // Use the material if we found it
        if (killZoneMaterial != null)
        {
            // Create a copy of the material so we don't modify the original
            Material materialCopy = new Material(killZoneMaterial);
            materialCopy.color = killZoneColor;
            meshRenderer.material = materialCopy;
            
            Debug.Log($"Using material '{materialName}' for kill zone visualization");
        }
        else
        {
            Debug.LogWarning($"Material '{materialName}' not found. Using fallback material.");
            
            // Create a fallback material that works with transparency
            Material fallbackMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (fallbackMaterial == null)
            {
                // Try standard shader if URP shader is not found
                fallbackMaterial = new Material(Shader.Find("Standard"));
            }
            
            // Configure the material for transparency
            fallbackMaterial.SetFloat("_Surface", 1); // 1 = Transparent
            fallbackMaterial.SetFloat("_Blend", 0);  // 0 = Alpha
            fallbackMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            fallbackMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            fallbackMaterial.SetInt("_ZWrite", 0);   // Don't write to depth buffer
            fallbackMaterial.DisableKeyword("_ALPHATEST_ON");
            fallbackMaterial.EnableKeyword("_ALPHABLEND_ON");
            fallbackMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            fallbackMaterial.renderQueue = 3000;
            fallbackMaterial.color = killZoneColor;
            
            meshRenderer.material = fallbackMaterial;
        }
    }
    
    private void Update()
    {
        if (killZoneObject == null) return;
        
        if (gameManager == null)
        {
            // Try to find GameManager again
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                lastLivesCount = gameManager.CurrentLives;
            }
            return;
        }
        
        // Check if player lost a life
        if (gameManager.CurrentLives < lastLivesCount)
        {
            // Player lost a life, reset kill zone position
            ResetKillZonePosition();
            lastLivesCount = gameManager.CurrentLives;
            return;
        }
        
        // Update the lastLivesCount if it increased (e.g. player got an extra life)
        if (gameManager.CurrentLives > lastLivesCount)
        {
            lastLivesCount = gameManager.CurrentLives;
        }
        
        // Only rise if not stopped
        if (!shouldRise) return;
        
        // Move the kill zone up gradually
        if (killZoneObject.transform.position.y < maxHeight)
        {
            Vector3 pos = killZoneObject.transform.position;
            pos.y += riseSpeed * Time.deltaTime;
            killZoneObject.transform.position = pos;
            
            // Start playing sound when lava is rising
            if (!isRising && audioManager != null)
            {
                audioManager.PlayLavaRisingSound();
                isRising = true;
            }
            
            // Update the height of the collider as it rises
            UpdateKillZoneHeight();
            
            // Update the visualizer if it's enabled
            if (showKillZone && visualizerObject != null)
            {
                UpdateVisualizer();
            }
        }
        else if (isRising && audioManager != null)
        {
            // Stop the sound when lava has reached its maximum height
            audioManager.StopLavaRisingSound();
            isRising = false;
        }
    }
    
    private void UpdateKillZoneHeight()
    {
        // Make the kill zone collider taller as it rises
        BoxCollider collider = killZoneObject.GetComponent<BoxCollider>();
        if (collider != null)
        {
            // Calculate the distance from start to current position
            float currentHeight = killZoneObject.transform.position.y - startHeight;
            
            // Make the box collider taller, with the top at the current position
            collider.size = new Vector3(planeSize.x, currentHeight + 0.5f, planeSize.z);
            
            // Adjust the center so the top of the collider is at the current position
            collider.center = new Vector3(0, -currentHeight / 2, 0);
        }
    }
    
    private void UpdateVisualizer()
    {
        // Make sure the visualizer exists
        if (visualizerObject == null) return;
        
        // Get the current collider dimensions
        BoxCollider collider = killZoneObject.GetComponent<BoxCollider>();
        if (collider == null) return;
        
        // Update the visualizer's transform to match the collider
        visualizerObject.transform.localPosition = collider.center;
        visualizerObject.transform.localScale = new Vector3(planeSize.x, collider.size.y, planeSize.z);
    }
    
    private void ResetKillZonePosition()
    {
        Debug.Log("LavaRiser: Resetting kill zone position to start height");
        Vector3 pos = killZoneObject.transform.position;
        pos.y = startHeight;
        killZoneObject.transform.position = pos;
        
        // Stop the lava sound when resetting
        if (isRising && audioManager != null)
        {
            audioManager.StopLavaRisingSound();
            isRising = false;
        }
        
        // Reset the collider size
        BoxCollider collider = killZoneObject.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.size = new Vector3(planeSize.x, 0.5f, planeSize.z);
            collider.center = Vector3.zero;
        }
        
        // Reset the visualizer
        if (showKillZone && visualizerObject != null)
        {
            visualizerObject.transform.localPosition = Vector3.zero;
            visualizerObject.transform.localScale = new Vector3(planeSize.x, 0.5f, planeSize.z);
        }
    }
    
    // Toggle the kill zone visualizer
    public void ToggleKillZoneVisibility(bool visible)
    {
        showKillZone = visible;
        
        if (visualizerObject != null)
        {
            visualizerObject.SetActive(visible);
        }
        else if (visible && killZoneObject != null)
        {
            CreateKillZoneVisualizer();
        }
    }
    
    // Public method to stop the lava from rising
    public void StopRising()
    {
        Debug.Log("LavaRiser: Stopping lava from rising");
        shouldRise = false;
        
        // Stop the sound if it's playing
        if (isRising && audioManager != null)
        {
            audioManager.StopLavaRisingSound();
            isRising = false;
        }
    }
    
    // Public method to resume rising if needed
    public void ResumeRising()
    {
        Debug.Log("LavaRiser: Resuming lava rising");
        shouldRise = true;
    }
} 