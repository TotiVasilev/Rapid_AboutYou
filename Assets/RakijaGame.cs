using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RakijaGame : MonoBehaviour
{
    public Transform well;
    public List<Transform> trees;
    public Transform masher;
    public Transform kazan;
    public GameObject barrelPrefab;
    public Transform[] barrelSpawnPoints;
    public GameObject gameCompleteUI;
    public Text timeText;

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
    private bool isNearKazan = false;
    private Transform nearTree = null;

    private bool bucketFull = false;
    private bool bucketHasPlums = false;
    private bool bucketHasLiquid = false;

    private bool masherProcessing = false;
    private bool liquidReady = false;

    private int kazanLiquidCount = 0;
    private bool kazanProcessing = false;
    private bool rakijaReady = false;

    private float boilingTime = 20f;

    private Dictionary<Transform, int> treeWaterLevels = new Dictionary<Transform, int>();
    private Dictionary<Transform, bool> treeGrowthStatus = new Dictionary<Transform, bool>();

    private float interactionDistance = .5f;

    private int barrelCount = 0;
    private float startTime;
    private bool gameCompleted = false;

    private GameObject interactionTarget = null; // NEW

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

        startTime = Time.time;
        gameCompleteUI.SetActive(false);
    }

    void Update()
    {
        CheckInteractions();

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {
                interactionTarget = hit.collider.gameObject;

                // Move to nearby position near object
                Vector3 objectPos = interactionTarget.transform.position;
                Vector3 dir = (objectPos - player.transform.position).normalized;
                Vector3 stopPosition = objectPos - dir * 0.2f;

                player.MoveTo(stopPosition);
            }
            else
            {
                // No object clicked: just move to clicked position
                player.MoveTo(mouseWorldPos);
                interactionTarget = null; // Clear any existing target
            }
        }

        if (interactionTarget != null)
        {
            float dist = Vector2.Distance(player.transform.position, interactionTarget.transform.position);
            if (!player.IsMoving() && dist <= interactionDistance)
            {
                HandleInteraction(interactionTarget);
                interactionTarget = null;
            }
        }
    }




    void CheckInteractions()
    {
        isNearBucket = Vector2.Distance(player.transform.position, bucket.position) <= interactionDistance;
        isNearWell = Vector2.Distance(player.transform.position, well.position) <= interactionDistance;
        isNearMasher = Vector2.Distance(player.transform.position, masher.position) <= interactionDistance;
        isNearKazan = Vector2.Distance(player.transform.position, kazan.position) <= interactionDistance;

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

    void HandleInteraction(GameObject target)
    {
        if (target == bucket.gameObject)
        {
            ToggleBucket();
        }
        else if (target == well.gameObject)
        {
            TryExtractWater();
        }
        else if (trees.Contains(target.transform))
        {
            TryInteractWithTree(target.transform);
        }
        else if (target == masher.gameObject)
        {
            TryInteractWithMasher();
        }
        else if (target == kazan.gameObject)
        {
            TryInteractWithKazan();
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
        if (!liquidReady || bucketHasPlums || bucketFull)
        {
            Debug.Log("The bucket already contains something!");
            return;
        }

        liquidReady = false;
        bucketHasLiquid = true;
        bucketSpriteRenderer.sprite = bucketWithLiquid;
    }

    void TryInteractWithKazan()
    {
        if (bucketHasLiquid && !kazanProcessing)
        {
            kazanLiquidCount++;
            bucketHasLiquid = false;
            bucketSpriteRenderer.sprite = emptyBucketSprite;

            if (kazanLiquidCount >= 3)
            {
                kazanProcessing = true;
                StartCoroutine(BoilLiquid());
            }
        }
        else if (rakijaReady)
        {
            rakijaReady = false;
            SpawnBarrel();

            barrelCount++;
            if (barrelCount >= 3 && !gameCompleted)
            {
                EndGame();
            }
        }
    }

    IEnumerator BoilLiquid()
    {
        yield return new WaitForSeconds(boilingTime);
        rakijaReady = true;
        kazanProcessing = false;
        kazanLiquidCount = 0;
    }

    void SpawnBarrel()
    {
        Transform spawnPoint = barrelSpawnPoints[barrelCount % barrelSpawnPoints.Length];
        Instantiate(barrelPrefab, spawnPoint.position, Quaternion.identity);
    }

    void EndGame()
    {
        gameCompleted = true;

        float totalTime = Time.time - startTime;
        int minutes = Mathf.FloorToInt(totalTime / 60);
        int seconds = Mathf.FloorToInt(totalTime % 60);

        timeText.text = string.Format("{0}:{1:D2}", minutes, seconds);
        gameCompleteUI.SetActive(true);
    }
}
