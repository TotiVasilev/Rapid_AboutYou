using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RakijaGame : MonoBehaviour
{
    public Transform well;
    public List<Transform> trees;
    public Transform masher;

    public Sprite treeWithPlums;
    public Sprite treeWithoutPlums;

    public PlayerController player;
    public Transform bucket;
    private bool hasBucket = false;

    public Sprite emptyBucketSprite;
    public Sprite fullBucketSprite;
    public Sprite bucketWithPlums;
    public Sprite bucketWithLiquid;

    private SpriteRenderer bucketSpriteRenderer;

    public Slider extractionSlider;
    private bool isExtractingWater = false;
    private float extractionProgress = 0f;

    private bool isNearBucket = false;
    private bool isNearWell = false;
    private bool isNearMasher = false;
    private Transform nearTree = null;

    private bool bucketFull = false;
    private bool bucketHasPlums = false;
    private bool bucketHasLiquid = false;

    private bool masherProcessing = false;
    private bool liquidReady = false;

    private Dictionary<Transform, int> treeWaterLevels = new Dictionary<Transform, int>();
    private Dictionary<Transform, bool> treeGrowthStatus = new Dictionary<Transform, bool>();

    private float interactionDistance = 0.5f;

    void Start()
    {
        bucketSpriteRenderer = bucket.GetComponent<SpriteRenderer>();
        bucketSpriteRenderer.sprite = emptyBucketSprite;
        extractionSlider.gameObject.SetActive(false);

        foreach (Transform tree in trees)
        {
            treeWaterLevels[tree] = 0;
            treeGrowthStatus[tree] = false;
            tree.GetComponent<SpriteRenderer>().sprite = treeWithoutPlums;
        }
    }

    void Update()
    {
        CheckInteractions();

        if (Input.GetKeyDown(KeyCode.G) && isNearBucket)
        {
            ToggleBucket();
        }

        if (hasBucket)
        {
            if (Input.GetKey(KeyCode.F) && isNearWell)
            {
                TryExtractWater();
            }
            else if (Input.GetKeyUp(KeyCode.F) && isExtractingWater)
            {
                StopExtractingWater();
            }
            else if (Input.GetKeyDown(KeyCode.F) && nearTree != null)
            {
                TryInteractWithTree(nearTree);
            }
            else if (Input.GetKeyDown(KeyCode.F) && isNearMasher)
            {
                TryInteractWithMasher();
            }
        }
    }

    void CheckInteractions()
    {
        isNearBucket = Vector2.Distance(player.transform.position, bucket.position) <= interactionDistance;
        isNearWell = Vector2.Distance(player.transform.position, well.position) <= interactionDistance;
        isNearMasher = Vector2.Distance(player.transform.position, masher.position) <= interactionDistance;

        foreach (Transform tree in trees)
        {
            if (Vector2.Distance(player.transform.position, tree.position) <= interactionDistance)
            {
                nearTree = tree;
                return;
            }
        }
        nearTree = null;
    }

    void ToggleBucket()
    {
        hasBucket = !hasBucket;
        if (hasBucket)
        {
            bucket.SetParent(player.transform);
            bucket.localPosition = Vector3.zero;
            player.PickUpBucket();
        }
        else
        {
            bucket.SetParent(null);
            bucket.position = new Vector3(player.transform.position.x, player.transform.position.y - 0.5f, 0);
            bucket.gameObject.SetActive(true);
            player.DropBucket();
        }
    }

    void TryExtractWater()
    {
        if (bucketFull || bucketHasPlums || bucketHasLiquid)
        {
            Debug.Log("The bucket already contains something!");
            return;
        }

        if (!isExtractingWater)
        {
            isExtractingWater = true;
            extractionProgress = 0f;
            extractionSlider.gameObject.SetActive(true);
            StartCoroutine(ExtractWater());
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
        }

        extractionSlider.gameObject.SetActive(false);
        isExtractingWater = false;
    }

    void StopExtractingWater()
    {
        isExtractingWater = false;
        extractionSlider.gameObject.SetActive(false);
    }

    void TryInteractWithTree(Transform tree)
    {
        if (bucketFull)
        {
            WaterTree(tree);
        }
        else if (treeGrowthStatus[tree] && !bucketHasPlums)
        {
            CollectPlums(tree);
        }
    }

    void WaterTree(Transform tree)
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

    IEnumerator GrowPlums(Transform tree)
    {
        treeGrowthStatus[tree] = true;
        yield return new WaitForSeconds(5f);
        tree.GetComponent<SpriteRenderer>().sprite = treeWithPlums;
    }

    void CollectPlums(Transform tree)
    {
        if (bucketFull || bucketHasPlums || bucketHasLiquid)
        {
            Debug.Log("The bucket already contains something!");
            return;
        }

        bucketHasPlums = true;
        bucketSpriteRenderer.sprite = bucketWithPlums;
        tree.GetComponent<SpriteRenderer>().sprite = treeWithoutPlums;
        treeGrowthStatus[tree] = false;
        treeWaterLevels[tree] = 0;
    }

    void TryInteractWithMasher()
    {
        if (bucketHasPlums && !masherProcessing)
        {
            DeliverPlumsToMasher();
        }
        else if (liquidReady)
        {
            CollectLiquid();
        }
    }

    void DeliverPlumsToMasher()
    {
        bucketHasPlums = false;
        bucketSpriteRenderer.sprite = emptyBucketSprite;
        masherProcessing = true;
        StartCoroutine(ProcessPlums());
    }

    IEnumerator ProcessPlums()
    {
        yield return new WaitForSeconds(5f);
        liquidReady = true;
        masherProcessing = false;
    }

    void CollectLiquid()
    {
        if (bucketHasPlums || bucketFull)
        {
            Debug.Log("The bucket already contains something!");
            return;
        }

        liquidReady = false;
        bucketHasLiquid = true;
        bucketSpriteRenderer.sprite = bucketWithLiquid;
    }
}
