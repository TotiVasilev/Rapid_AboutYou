using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class ProcessImageLine
    {
        public int lineIndex;
        public Sprite image;
    }

    [Header("UI References")]
    public Text dialogueText;
    public Image speakerPortrait;
    public Image explanationImage;
    public Button continueButton;

    [Header("Dialogue Content")]
    public string[] dialogueLines;

    [Header("Settings")]
    public float textSpeed = 0.05f;

    [Header("Speaker Portraits")]
    public Sprite teacherSprite;
    public Sprite playerSprite;

    [Header("Process Images (Lines 6 to 11)")]
    public Sprite bucketImage;
    public Sprite wellImage;
    public Sprite treeCollectImage;
    public Sprite pressImage;
    public Sprite cauldronImage;
    public Sprite barrelDoneImage;

    private int currentLine = 0;
    private bool isTyping = false;
    private bool cancelTyping = false;

    void Start()
    {
        continueButton.gameObject.SetActive(false); // Hide the button at start
        UpdateVisuals();
        StartCoroutine(TypeLine(dialogueLines[currentLine]));
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isTyping)
            {
                currentLine++;
                if (currentLine < dialogueLines.Length)
                {
                    UpdateVisuals();
                    StartCoroutine(TypeLine(dialogueLines[currentLine]));
                }
                else
                {
                    dialogueText.text = "";
                    speakerPortrait.enabled = false;
                    explanationImage.enabled = false;

                    // Show the continue button
                    continueButton.gameObject.SetActive(true);
                }
            }
            else
            {
                cancelTyping = true;
            }
        }
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        cancelTyping = false;
        dialogueText.text = "";

        for (int i = 0; i < line.Length; i++)
        {
            if (cancelTyping)
            {
                dialogueText.text = line;
                break;
            }

            dialogueText.text += line[i];
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void UpdateVisuals()
    {
        // Reset visuals
        speakerPortrait.enabled = false;
        explanationImage.enabled = false;

        if (currentLine < 2)
        {
            speakerPortrait.sprite = teacherSprite;
            speakerPortrait.enabled = true;
        }
        else if (currentLine < 6)
        {
            speakerPortrait.sprite = playerSprite;
            speakerPortrait.enabled = true;
        }
        else if (currentLine >= 6 && currentLine <= 11)
        {
            switch (currentLine)
            {
                case 6: explanationImage.sprite = bucketImage; break;
                case 7: explanationImage.sprite = wellImage; break;
                case 8: explanationImage.sprite = treeCollectImage; break;
                case 9: explanationImage.sprite = pressImage; break;
                case 10: explanationImage.sprite = cauldronImage; break;
                case 11: explanationImage.sprite = barrelDoneImage; break;
            }
            explanationImage.enabled = true;
        }
        else if (currentLine == 12)
        {
            speakerPortrait.sprite = playerSprite;
            speakerPortrait.enabled = true;
        }
    }

    public void LoadNextScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
