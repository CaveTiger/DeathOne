using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using System.Text;
using System.IO;

public class StageLoader : MonoBehaviour
{
    private List<TextAsset> stageXmlFiles = new List<TextAsset>();
    private const string BASE_RESOURCE_PATH = "Data/Stage";
    private readonly string[] REQUIRED_FILES = { "BaseStage", "BaseBlock" };

    [System.Serializable]
    public class StageLoadPath
    {
        public string resourcePath;
        public bool isRequired = false;
        public bool isModPath = false;
        // 추후 외부 모드 지원을 위한 옵션
        //public bool isExternalPath = false;
    }

    [SerializeField]
    private List<StageLoadPath> additionalLoadPaths = new List<StageLoadPath>();

    // 추후 외부 모드 지원을 위한 변수
    //private List<string> externalModPaths = new List<string>();

    public static StageLoader Instance { get; private set; }

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

    // 추후 외부 모드 지원을 위한 메서드
    /*
    public void AddExternalModPath(string absolutePath)
    {
        if (!externalModPaths.Contains(absolutePath))
        {
            externalModPaths.Add(absolutePath);
            // Debug.Log($"[StageLoader] 외부 모드 경로 추가: {absolutePath}");
        }
    }
    */

    public void Initialize()
    {
        if (isInitialized)
        {
            // Debug.LogWarning("[StageLoader] 이미 초기화되었습니다.");
            return;
        }

        // Debug.Log("[StageLoader] 초기화 시작");
        LoadAllStages();
        // Debug.Log($"[StageLoader] 스테이지 로딩 완료. 총 {StageData.stageDict.Count}개");
        isInitialized = true;
    }

    public void LoadAllStages()
    {
        stageXmlFiles.Clear();

        // 1. 기본 경로에서 필수 파일 로드
        if (!LoadRequiredFiles())
        {
            return;
        }

        // 2. 기본 경로의 나머지 파일 로드
        LoadFromPath(BASE_RESOURCE_PATH);

        // 3. Resources 내부의 추가 경로에서 파일 로드
        foreach (var path in additionalLoadPaths)
        {
            LoadFromPath(path.resourcePath, path.isRequired, path.isModPath);
        }

        if (stageXmlFiles.Count == 0)
        {
            // Debug.LogError("[StageLoader] 로드된 스테이지 파일이 없습니다.");
            return;
        }

        // Debug.Log($"[StageLoader] 총 {stageXmlFiles.Count}개의 스테이지 파일을 찾았습니다.");
        LoadStageFiles();
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
                // Debug.LogError($"[StageLoader] 필수 파일을 찾을 수 없습니다: {requiredFile}.xml");
                hasAllRequired = false;
            }
            else
            {
                stageXmlFiles.Add(file);
                // Debug.Log($"[StageLoader] 필수 파일 로드: {file.name}");
            }
        }

        return hasAllRequired;
    }

    private void LoadFromPath(string path, bool isRequired = false, bool isModPath = false)
    {
        Object[] files = Resources.LoadAll(path, typeof(TextAsset));
        
        if (files.Length == 0 && isRequired)
        {
            // Debug.LogError($"[StageLoader] 필수 경로에서 파일을 찾을 수 없습니다: {path}");
            return;
        }

        foreach (Object obj in files)
        {
            if (obj is TextAsset textAsset && !REQUIRED_FILES.Contains(textAsset.name))
            {
                // 중복 파일 체크 (모드 파일이 기존 파일을 덮어쓸 수 있음)
                if (isModPath)
                {
                    var existingFile = stageXmlFiles.FirstOrDefault(f => f.name == textAsset.name);
                    if (existingFile != null)
                    {
                        stageXmlFiles.Remove(existingFile);
                        // Debug.Log($"[StageLoader] 모드 파일이 기존 파일을 대체: {textAsset.name}");
                    }
                }

                stageXmlFiles.Add(textAsset);
                // Debug.Log($"[StageLoader] 파일 발견: {textAsset.name} ({(isModPath ? "모드" : "기본")})");
            }
        }
    }

    private void LoadStageFiles()
    {
        int loadedStageCount = 0;
        int loadedBlockCount = 0;

        // Debug.Log(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"[StageLoader] 스테이지 파일 로드 시작 (총 {stageXmlFiles.Count}개 파일)")));

        // 1. 필수 파일 먼저 로드
        foreach (string baseFile in REQUIRED_FILES)
        {
            var file = stageXmlFiles.FirstOrDefault(f => f.name == baseFile);
            if (file != null)
            {
                // Debug.Log(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"[StageLoader] 필수 파일 로드: {file.name}")));
                var (stages, blocks) = LoadStageFile(file);
                loadedStageCount += stages;
                loadedBlockCount += blocks;
            }
        }

        // 2. 나머지 파일 로드 (이미 로드된 필수 파일 제외)
        foreach (var xmlFile in stageXmlFiles)
        {
            if (!REQUIRED_FILES.Contains(xmlFile.name))
            {
                // Debug.Log(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"[StageLoader] 추가 파일 로드: {xmlFile.name}")));
                var (stages, blocks) = LoadStageFile(xmlFile);
                loadedStageCount += stages;
                loadedBlockCount += blocks;
            }
        }
    }

    private (int stageCount, int blockCount) LoadStageFile(TextAsset xmlFile)
    {
        int stageCount = 0;
        int blockCount = 0;

        XDocument doc = LoadXmlFile(xmlFile);
        if (doc != null)
        {
            // Debug.Log(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"[StageLoader] {xmlFile.name} 파일 파싱 시작")));
            (stageCount, blockCount) = LoadStageAndBlock(doc);
        }

        return (stageCount, blockCount);
    }

    private (int stageCount, int blockCount) LoadStageAndBlock(XDocument doc)
    {
        int stageCount = 0;
        int blockCount = 0;
        var stageDict = new Dictionary<string, StageData>();
        var blockDict = new Dictionary<string, StageBlockData>();

        // ===== 1. Block 정보 파싱 =====
        foreach (var blockEl in doc.Descendants("Block"))
        {
            string id = (string)blockEl.Attribute("ID") ?? "";
            string parentID = (string)blockEl.Attribute("ParentID") ?? "";

            // Debug.Log(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"[StageLoader] 블록 파싱: ID={id}, ParentID={parentID}")));

            if (string.IsNullOrEmpty(id))
            {
                // Debug.LogError(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes("[StageLoader] 블록 ID가 없습니다!")));
                continue;
            }

            StageBlockData block;

            // 부모가 있는 경우
            if (!string.IsNullOrEmpty(parentID) && blockDict.TryGetValue(parentID, out var parent))
            {
                block = CloneBlock(parent);
                block.ID = id;
                // Debug.Log(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"[StageLoader] 부모 블록에서 복제: ParentID={parentID}")));
            }
            else
            {
                block = new StageBlockData { ID = id };
            }

            block.ParentID = parentID;
            block.BlockType = (string)blockEl.Element("BlockType") ?? block.BlockType;
            block.FrontCutID = (string)blockEl.Element("FrontCutID") ?? block.FrontCutID;
            block.BackCutID = (string)blockEl.Element("BackCutID") ?? block.BackCutID;

            // Debug.Log(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(
            //     $"[StageLoader] 블록 기본 정보:\n" +
            //     $"  - BlockType: {block.BlockType}\n" +
            //     $"  - FrontCutID: {block.FrontCutID}\n" +
            //     $"  - BackCutID: {block.BackCutID}"
            // )));

            var posEl = blockEl.Element("Position");
            if (posEl != null)
            {
                int x = int.TryParse(posEl.Attribute("x")?.Value, out var px) ? px : 0;
                int y = int.TryParse(posEl.Attribute("y")?.Value, out var py) ? py : 0;
                block.Position = new Vector2Int(x, y);
                // Debug.Log(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"[StageLoader] 블록 위치: ({x}, {y})")));
            }

            var enemyEl = blockEl.Element("SetEnemy");
            if (enemyEl != null)
            {
                block.EnemyIDs = enemyEl.Elements("li")
                    .Select(e => e.Value.Trim())
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();
                // Debug.Log(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(
                //     $"[StageLoader] 적 리스트 ({block.EnemyIDs.Count}개):\n" +
                //     string.Join("\n", block.EnemyIDs.Select(id => $"  - {id}"))
                // )));
            }

            var nextEl = blockEl.Element("NextBlock");
            if (nextEl != null)
            {
                block.NextBlockIDs = nextEl.Elements("li")
                    .Select(e => e.Value.Trim())
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();
                // Debug.Log(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(
                //     $"[StageLoader] 다음 블록 리스트 ({block.NextBlockIDs.Count}개):\n" +
                //     string.Join("\n", block.NextBlockIDs.Select(id => $"  - {id}"))
                // )));
            }

            // 블록 데이터 추가
            if (StageManager.Instance.AddBlock(block))
            {
                blockCount++;
                blockDict[id] = block;
            }
        }

        // ===== 2. Stage 정보 파싱 =====
        foreach (var stageEl in doc.Descendants("Stage"))
        {
            string id = (string)stageEl.Attribute("ID") ?? "";
            
            // Debug.Log(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"[StageLoader] 스테이지 파싱: ID={id}")));

            if (string.IsNullOrEmpty(id))
            {
                // Debug.LogError(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes("[StageLoader] 스테이지 ID가 없습니다!")));
                continue;
            }

            var stage = new StageData { ID = id };

            var blockListEl = stageEl.Element("StageBlock");
            if (blockListEl != null)
            {
                stage.BlockIDs = blockListEl.Elements("li")
                    .Select(e => e.Value.Trim())
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();
            }

            // 스테이지 데이터 추가
            if (StageManager.Instance.AddStage(stage))
            {
                stageCount++;
                stageDict[id] = stage;
            }
        }

        return (stageCount, blockCount);
    }

    private StageBlockData CloneBlock(StageBlockData original)
    {
        return new StageBlockData
        {
            ID = original.ID,
            ParentID = original.ParentID,
            BlockType = original.BlockType,
            FrontCutID = original.FrontCutID,
            BackCutID = original.BackCutID,
            Position = original.Position,
            EnemyIDs = new List<string>(original.EnemyIDs),
            NextBlockIDs = new List<string>(original.NextBlockIDs)
        };
    }

    private StageData CloneStage(StageData original)
    {
        return new StageData
        {
            ID = original.ID,
            ParentID = original.ParentID,
            BlockIDs = new List<string>(original.BlockIDs)
        };
    }

    public XDocument LoadXmlFile(TextAsset xmlFile)
    {
        try
        {
            // UTF-8 인코딩으로 XML 파일 읽기
            byte[] bytes = Encoding.UTF8.GetBytes(xmlFile.text);
            using (var stream = new System.IO.MemoryStream(bytes))
            {
                return XDocument.Load(stream);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[StageLoader] XML 파일 로드 실패: {ex.Message}");
            return null;
        }
    }
}
