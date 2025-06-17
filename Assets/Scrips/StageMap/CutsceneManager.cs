using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject cutscenePanel;
    public Image backgroundImage;
    public TextMeshProUGUI characterText;
    public TextMeshProUGUI dialogueText;
    public Image characterImage;
    public GameObject skipButton;

    [Header("Settings")]
    public float textSpeed = 0.05f;
    public float fadeSpeed = 0.5f;

    private Dictionary<string, CutsceneData> cutsceneDict = new Dictionary<string, CutsceneData>();
    private CutsceneData currentCutscene;
    private int currentSceneIndex = 0;
    private int currentDialogueIndex = 0;
    private bool isPlaying = false;
    private bool isTyping = false;
    private System.Action onComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
        }
    }

    public void AddCutscene(CutsceneData cutscene)
    {
        if (!cutsceneDict.ContainsKey(cutscene.ID))
        {
            cutsceneDict.Add(cutscene.ID, cutscene);
        }
        else
        {
            Debug.LogWarning($"[CutsceneManager] 이미 존재하는 컷신 ID입니다: {cutscene.ID}");
        }
    }

    public void PlayCutscene(string cutsceneID, System.Action onComplete = null)
    {
        if (!cutsceneDict.TryGetValue(cutsceneID, out currentCutscene))
        {
            Debug.LogError($"[CutsceneManager] 컷신을 찾을 수 없습니다: {cutsceneID}");
            return;
        }

        this.onComplete = onComplete;
        currentSceneIndex = 0;
        currentDialogueIndex = 0;
        isPlaying = true;

        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(true);
        }

        // BGM 재생
        if (!string.IsNullOrEmpty(currentCutscene.BGM))
        {
            // TODO: BGM 재생 로직 구현
        }

        StartCoroutine(PlayNextScene());
    }

    private IEnumerator PlayNextScene()
    {
        if (currentSceneIndex >= currentCutscene.Scenes.Count)
        {
            EndCutscene();
            yield break;
        }

        var scene = currentCutscene.Scenes[currentSceneIndex];
        
        // 배경 이미지 로드
        if (!string.IsNullOrEmpty(scene.CutImage))
        {
            var bgSprite = Resources.Load<Sprite>(scene.CutImage);
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
            }
        }

        currentDialogueIndex = 0;
        yield return StartCoroutine(PlayNextDialogue());
    }

    private IEnumerator PlayNextDialogue()
    {
        var scene = currentCutscene.Scenes[currentSceneIndex];
        
        if (currentDialogueIndex >= scene.Dialogues.Count)
        {
            currentSceneIndex++;
            StartCoroutine(PlayNextScene());
            yield break;
        }

        var dialogue = scene.Dialogues[currentDialogueIndex];
        
        // 캐릭터 이미지 로드
        if (!string.IsNullOrEmpty(dialogue.CharacterImage) && dialogue.CharacterImage != "None")
        {
            var charSprite = Resources.Load<Sprite>(dialogue.CharacterImage);
            if (charSprite != null)
            {
                characterImage.sprite = charSprite;
            }
        }

        // 캐릭터 이름 표시
        if (characterText != null)
        {
            characterText.text = dialogue.Character != "None" ? dialogue.Character : "";
        }

        // 대사 타이핑 효과
        if (dialogueText != null)
        {
            isTyping = true;
            dialogueText.text = "";
            foreach (char c in dialogue.Line)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(textSpeed);
            }
            isTyping = false;
        }

        // 음성 재생
        if (!string.IsNullOrEmpty(dialogue.Sound) && dialogue.Sound != "None")
        {
            // TODO: 음성 재생 로직 구현
        }

        // 클릭 대기
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        currentDialogueIndex++;
        StartCoroutine(PlayNextDialogue());
    }

    public void SkipCutscene()
    {
        if (!isPlaying)
            return;

        EndCutscene();
    }

    private void EndCutscene()
    {
        isPlaying = false;
        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
        }

        onComplete?.Invoke();
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        // 스킵 버튼 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SkipCutscene();
        }

        // 타이핑 중 스킵
        if (isTyping && Input.GetMouseButtonDown(0))
        {
            StopAllCoroutines();
            isTyping = false;
            if (dialogueText != null)
            {
                dialogueText.text = currentCutscene.Scenes[currentSceneIndex].Dialogues[currentDialogueIndex].Line;
            }
        }
    }
} 