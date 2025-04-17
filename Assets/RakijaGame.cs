using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class RakijaGame : MonoBehaviour
{
    public AudioClip waterSound;
    public AudioClip mashSound;
    public AudioClip boilSound;

    private AudioSource audioSource;

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
    private Dictionary<Transform, bool> plumsReadyToCollect = new Dictionary<Transform, bool>();

    private float interactionDistance = 5f;

    private int barrelCount = 0;
    private float startTime;
    private bool gameCompleted = false;

    private GameObject interactionTarget = null;
    public bool DisableMoving = false;
    public GameObject tipsButton;

    void Start()
    {
        foreach (Transform tree in trees)
        {
            treeWaterLevels[tree] = 0;
            treeGrowthStatus[tree] = false;
            plumsReadyToCollect[tree] = false;
            tree.GetComponent<SpriteRenderer>().sprite = treeWithoutPlums;
        }
        audioSource = gameObject.AddComponent<AudioSource>();
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

        if (DisableMoving || EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {
                interactionTarget = hit.collider.gameObject;

                Collider2D col = hit.collider;
                Vector3 objectPos = col.bounds.center;
                Vector3 dir = (objectPos - player.transform.position).normalized;
                float stopDistance = col.bounds.extents.magnitude + 0.1f;
                Vector3 stopPosition = objectPos - dir * stopDistance;

                player.MoveTo(stopPosition);
            }
            else
            {
                player.MoveTo(mouseWorldPos);
                interactionTarget = null;
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

    public void DontMoove()
    {
        DisableMoving = true;
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
            StartCoroutine(ExtractWater());
        }
    }

    IEnumerator ExtractWater()
    {
        yield return StartCoroutine(ShowProgress(2f, () =>
        {
            bucketFull = true;
            bucketSpriteRenderer.sprite = fullBucketSprite;
        }, waterSound));
    }

    void TryInteractWithTree(Transform tree)
    {
        if (bucketFull)
        {
            WaterTree(tree);
        }
        else if (plumsReadyToCollect.ContainsKey(tree) && plumsReadyToCollect[tree] && !bucketHasPlums)
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

        if (treeWaterLevels[tree] >= 1)
        {
            StartCoroutine(GrowPlums(tree));
        }
    }

    IEnumerator GrowPlums(Transform tree)
    {
        treeGrowthStatus[tree] = true;

        yield return StartCoroutine(ShowProgress(5f, () =>
        {
            tree.GetComponent<SpriteRenderer>().sprite = treeWithPlums;
            plumsReadyToCollect[tree] = true; 
        }));
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
        plumsReadyToCollect[tree] = false;
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
        yield return StartCoroutine(ShowProgress(5f, () =>
        {
            liquidReady = true;
            masherProcessing = false;
        }, mashSound));
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

            if (kazanLiquidCount >= 1)
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
            if (barrelCount >= 1 && !gameCompleted)
            {
                EndGame();
            }
        }
    }

    IEnumerator BoilLiquid()
    {
        yield return StartCoroutine(ShowProgress(boilingTime, () =>
        {
            rakijaReady = true;
            kazanProcessing = false;
            kazanLiquidCount = 0;
        }, boilSound));
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
        tipsButton.SetActive(false);
    }

    IEnumerator ShowProgress(float duration, System.Action onComplete, AudioClip sound = null)
    {
        extractionSlider.value = 0f;
        extractionSlider.gameObject.SetActive(true);

        if (sound != null)
        {
            audioSource.clip = sound;
            audioSource.loop = true;
            audioSource.Play();
        }

        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime / duration;
            extractionSlider.value = progress;
            yield return null;
        }

        audioSource.Stop();
        extractionSlider.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

}
