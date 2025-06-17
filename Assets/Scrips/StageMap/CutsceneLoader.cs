using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public class CutsceneLoader : MonoBehaviour
{
    private List<TextAsset> cutsceneXmlFiles = new List<TextAsset>();
    private const string BASE_RESOURCE_PATH = "Data/CutScene";
    private readonly string[] REQUIRED_FILES = { "CutSceneBase" };

    public static CutsceneLoader Instance { get; private set; }
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        LoadAllCutscenes();
        isInitialized = true;
    }

    public void LoadAllCutscenes()
    {
        cutsceneXmlFiles.Clear();

        // 1. 기본 경로에서 필수 파일 로드
        if (!LoadRequiredFiles())
        {
            return;
        }

        // 2. 기본 경로의 나머지 파일 로드
        LoadFromPath(BASE_RESOURCE_PATH);

        if (cutsceneXmlFiles.Count == 0)
        {
            Debug.LogError("[CutsceneLoader] 로드된 컷신 파일이 없습니다.");
            return;
        }

        LoadCutsceneFiles();
    }

    private bool LoadRequiredFiles()
    {
        bool hasAllRequired = true;
        Object[] baseFiles = Resources.LoadAll(BASE_RESOURCE_PATH, typeof(TextAsset));

        foreach (string requiredFile in REQUIRED_FILES)
        {
            TextAsset file = baseFiles.FirstOrDefault(f => f.name == requiredFile) as TextAsset;
            if (file == null)
            {
                Debug.LogError($"[CutsceneLoader] 필수 파일을 찾을 수 없습니다: {requiredFile}");
                hasAllRequired = false;
            }
            else
            {
                cutsceneXmlFiles.Add(file);
            }
        }

        return hasAllRequired;
    }

    private void LoadFromPath(string path)
    {
        Object[] files = Resources.LoadAll(path, typeof(TextAsset));
        
        if (files.Length == 0)
        {
            Debug.LogError($"[CutsceneLoader] 경로에서 파일을 찾을 수 없습니다: {path}");
            return;
        }

        foreach (Object obj in files)
        {
            if (obj is TextAsset textAsset && !REQUIRED_FILES.Contains(textAsset.name))
            {
                cutsceneXmlFiles.Add(textAsset);
            }
        }
    }

    private void LoadCutsceneFiles()
    {
        int loadedCutsceneCount = 0;

        // 1. 필수 파일 먼저 로드
        foreach (string baseFile in REQUIRED_FILES)
        {
            var file = cutsceneXmlFiles.FirstOrDefault(f => f.name == baseFile);
            if (file != null)
            {
                loadedCutsceneCount += LoadCutsceneFile(file);
            }
        }

        // 2. 나머지 파일 로드
        foreach (var xmlFile in cutsceneXmlFiles)
        {
            if (!REQUIRED_FILES.Contains(xmlFile.name))
            {
                loadedCutsceneCount += LoadCutsceneFile(xmlFile);
            }
        }
    }

    private int LoadCutsceneFile(TextAsset xmlFile)
    {
        int cutsceneCount = 0;

        XDocument doc = LoadXmlFile(xmlFile);
        if (doc != null)
        {
            cutsceneCount = LoadCutscenes(doc);
        }

        return cutsceneCount;
    }

    private int LoadCutscenes(XDocument doc)
    {
        int cutsceneCount = 0;
        var cutsceneDict = new Dictionary<string, CutsceneData>();

        foreach (var cutFlowEl in doc.Descendants("CutFlow"))
        {
            string id = (string)cutFlowEl.Attribute("ID") ?? "";
            string parentID = (string)cutFlowEl.Attribute("ParentID") ?? "";

            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("[CutsceneLoader] 컷신 ID가 없습니다!");
                continue;
            }

            CutsceneData cutscene;

            // 부모가 있는 경우
            if (!string.IsNullOrEmpty(parentID) && cutsceneDict.TryGetValue(parentID, out var parent))
            {
                cutscene = CloneCutscene(parent);
                cutscene.ID = id;
            }
            else
            {
                cutscene = new CutsceneData { ID = id };
            }

            cutscene.ParentID = parentID;
            cutscene.CutName = (string)cutFlowEl.Element("CutName") ?? cutscene.CutName;
            cutscene.BGM = (string)cutFlowEl.Element("Bgm") ?? cutscene.BGM;

            // 장면 파싱
            foreach (var sceneEl in cutFlowEl.Elements("li"))
            {
                var scene = new CutsceneScene
                {
                    CutImage = (string)sceneEl.Element("CutImage") ?? ""
                };

                // 대화 파싱
                var dialogueEl = sceneEl.Element("Dialogue");
                if (dialogueEl != null)
                {
                    foreach (var dialogueItemEl in dialogueEl.Elements("li"))
                    {
                        var dialogue = new CutsceneDialogue
                        {
                            Character = (string)dialogueItemEl.Element("Character") ?? "",
                            CharacterImage = (string)dialogueItemEl.Element("CharacterImage") ?? "",
                            Line = (string)dialogueItemEl.Element("Line") ?? "",
                            Sound = (string)dialogueItemEl.Element("Sound") ?? ""
                        };
                        scene.Dialogues.Add(dialogue);
                    }
                }

                cutscene.Scenes.Add(scene);
            }

            cutsceneDict[id] = cutscene;
            CutsceneManager.Instance.AddCutscene(cutscene);
            cutsceneCount++;
        }

        return cutsceneCount;
    }

    private CutsceneData CloneCutscene(CutsceneData original)
    {
        var clone = new CutsceneData
        {
            ID = original.ID,
            ParentID = original.ParentID,
            CutName = original.CutName,
            BGM = original.BGM
        };

        foreach (var scene in original.Scenes)
        {
            var sceneClone = new CutsceneScene
            {
                CutImage = scene.CutImage
            };

            foreach (var dialogue in scene.Dialogues)
            {
                sceneClone.Dialogues.Add(new CutsceneDialogue
                {
                    Character = dialogue.Character,
                    CharacterImage = dialogue.CharacterImage,
                    Line = dialogue.Line,
                    Sound = dialogue.Sound
                });
            }

            clone.Scenes.Add(sceneClone);
        }

        return clone;
    }

    private XDocument LoadXmlFile(TextAsset xmlFile)
    {
        try
        {
            return XDocument.Parse(xmlFile.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CutsceneLoader] XML 파일 파싱 실패: {xmlFile.name}\n{e.Message}");
            return null;
        }
    }
} 