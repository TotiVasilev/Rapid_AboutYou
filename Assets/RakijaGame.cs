using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RakijaGame : MonoBehaviour
{
    public Transform well;
    public List<Transform> trees;

    public Sprite treeWithPlums; // Missing this reference
    public Sprite treeWithoutPlums; // Missing this reference

    public PlayerController player; // Reference to player script for position
    public Transform bucket;
    private bool hasBucket = false;
    public Sprite emptyBucketSprite;
    public Sprite fullBucketSprite;
    private SpriteRenderer bucketSpriteRenderer;

    public Slider extractionSlider;
    private bool isExtractingWater = false;
    private float extractionProgress = 0f;

    private bool isNearBucket = false;
    private bool isNearWell = false;
    private Transform nearTree = null;

    private bool bucketFull = false;

    private Dictionary<Transform, int> treeWaterLevels = new Dictionary<Transform, int>();
    private Dictionary<Transform, bool> treeGrowthStatus = new Dictionary<Transform, bool>();

    private float interactionDistance = .5f; // Distance for interaction (e.g., 2 units)

    void Start()
    {
        bucketSpriteRenderer = bucket.GetComponent<SpriteRenderer>();
        bucketSpriteRenderer.sprite = emptyBucketSprite;

        extractionSlider.gameObject.SetActive(false);

        foreach (Transform tree in trees)
        {
            treeWaterLevels[tree] = 0;
            treeGrowthStatus[tree] = false;
            tree.GetComponent<SpriteRenderer>().sprite = treeWithoutPlums; // Set initial tree sprite without plums
        }
    }

    void Update()
    {
        // Update the player's proximity to interact with objects
        CheckInteractions();

        if (Input.GetKeyDown(KeyCode.G) && isNearBucket)
        {
            ToggleBucket();
        }

        if (hasBucket)
        {
            if (Input.GetKey(KeyCode.F) && isNearWell && !bucketFull)
            {
                StartExtractingWater();
            }
            else if (Input.GetKeyUp(KeyCode.F) && isExtractingWater)
            {
                DropWater();
            }
            else if (Input.GetKeyDown(KeyCode.F) && nearTree != null && bucketFull)
            {
                if (!treeGrowthStatus[nearTree])
                    WaterTree(nearTree);
                else
                    CollectPlums(nearTree);
            }
        }
    }

    // Check if player is near any interactable objects
    void CheckInteractions()
    {
        isNearBucket = Vector2.Distance(player.transform.position, bucket.position) <= interactionDistance;
        if (isNearBucket)
        {
            Debug.Log("Player is near the bucket. Press G to pick it up.");
        }

        isNearWell = Vector2.Distance(player.transform.position, well.position) <= interactionDistance;
        if (isNearWell)
        {
            Debug.Log("Player is near the well. Hold F to extract water.");
        }

        foreach (Transform tree in trees)
        {
            if (Vector2.Distance(player.transform.position, tree.position) <= interactionDistance)
            {
                nearTree = tree;
                Debug.Log("Player is near a tree. Hold F to water or collect plums.");
                return; // Only need one tree interaction at a time
            }
        }
    }

    void ToggleBucket()
    {
        hasBucket = !hasBucket;

        if (hasBucket)
        {
            bucket.SetParent(player.transform);
            bucket.localPosition = Vector3.zero;
            player.PickUpBucket();
            Debug.Log("Picked up the bucket.");
        }
        else
        {
            bucket.SetParent(null);
            bucket.position = new Vector3(player.transform.position.x, player.transform.position.y - 0.5f, 0);
            bucket.gameObject.SetActive(true);
            player.DropBucket();
            Debug.Log("Dropped the bucket.");
        }
    }

    void StartExtractingWater()
    {
        if (!bucketFull && !isExtractingWater)
        {
            isExtractingWater = true;
            extractionProgress = 0f;
            extractionSlider.gameObject.SetActive(true);
            StartCoroutine(ExtractWater());
            Debug.Log("Started extracting water...");
        }
    }

    IEnumerator ExtractWater()
    {
        while (extractionProgress < 1f && isExtractingWater)
        {
            extractionProgress += Time.deltaTime / 2f;
            extractionSlider.value = extractionProgress;
            yield return null;
        }

        if (isExtractingWater)
        {
            bucketFull = true;
            bucketSpriteRenderer.sprite = fullBucketSprite;
            Debug.Log("Bucket is now full.");
        }

        extractionSlider.gameObject.SetActive(false);
        isExtractingWater = false;
    }

    void DropWater()
    {
        isExtractingWater = false;
        extractionSlider.gameObject.SetActive(false);
        Debug.Log("Water extraction interrupted!");
    }

    void WaterTree(Transform tree)
    {
        if (bucketFull)
        {
            treeWaterLevels[tree]++;
            bucketFull = false;
            bucketSpriteRenderer.sprite = emptyBucketSprite;
            Debug.Log($"Watered tree: {treeWaterLevels[tree]}/3");

            if (treeWaterLevels[tree] >= 3)
            {
                StartCoroutine(GrowPlums(tree));
            }
        }
    }

    IEnumerator GrowPlums(Transform tree)
    {
        treeGrowthStatus[tree] = true;
        Debug.Log("Plums are growing...");
        yield return new WaitForSeconds(5f);
        tree.GetComponent<SpriteRenderer>().sprite = treeWithPlums; // Set tree sprite to with plums
        Debug.Log("Plums are ready!");
    }

    void CollectPlums(Transform tree)
    {
        Debug.Log("Plums collected! Restarting cycle.");
        tree.GetComponent<SpriteRenderer>().sprite = treeWithoutPlums; // Reset tree sprite to without plums
        treeGrowthStatus[tree] = false;
        treeWaterLevels[tree] = 0;
    }
}
