using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public class StageLoader : MonoBehaviour
{
    public TextAsset[] stageXmlFiles;

    public static StageLoader Instance { get; private set; }

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void LoadStages()
    {
        TextAsset[] xmlFiles = Resources.LoadAll<TextAsset>("Data/Stage");
        Debug.Log("[StageLoader] LoadStages() 실행됨");
        foreach (TextAsset xmlFile in xmlFiles)
        {
            var doc = XDocument.Parse(xmlFile.text);

            foreach (var stageElement in doc.Descendants("Stage"))
            {
                string id = (string)stageElement.Attribute("ID") ?? "";
                string parentID = (string)stageElement.Attribute("ParentID") ?? "";
                bool isSpecimen = bool.TryParse((string)stageElement.Attribute("Specimen"), out bool sp) && sp;

                if (isSpecimen)
                {
                    Debug.Log($"[Specimen 스킵] ID: {id}");
                    continue;
                }

                StageData stage = new()
                {
                    ID = id,
                    ParentID = parentID,
                    Label = (string)stageElement.Element("Label") ?? "",
                    TotalStageBlock = int.TryParse((string)stageElement.Element("TotalStageBlock"), out int total) ? total : 0
                };

                foreach (var blockElement in stageElement.Descendants("li"))
                {
                    StageBlockData block = new()
                    {
                        StageKey = (string)blockElement.Element("StageKey") ?? "",
                        BlockType = (string)blockElement.Element("BlockType") ?? "",
                        FrontCutID = (string)blockElement.Element("FrontCutID") ?? "",
                        BackCutID = (string)blockElement.Element("BackCutID") ?? "",
                        ParentBlockName = (string)blockElement.Element("ParentBlockName") ?? ""
                    };

                    // SetEnemy
                    block.EnemyIDs = blockElement.Element("SetEnemy")?
                        .Elements("li").Select(e => e.Value.Trim()).Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                        ?? new List<string>();

                    // EventList
                    block.EventList = blockElement.Element("EventList")?
                        .Elements("li").Select(e => e.Value.Trim()).Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                        ?? new List<string>();

                    // SpecialGuests
                    block.SpecialGuests = blockElement.Element("SpecialGuest")?
                        .Elements("li").Select(e => e.Value.Trim()).Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                        ?? new List<string>();

                    stage.Blocks.Add(block);
                    StageManager.Instance.stageBlockDict[block.StageKey] = block;

                    Debug.Log($"[스테이지 블록 로드] {block.StageKey} / {block.BlockType} / {block.EnemyIDs.Count}명");

                }

                StageManager.Instance.stageDict[stage.ID] = stage;
                Debug.Log($"[스테이지 등록] {stage.ID} ({stage.Blocks.Count}개 블록)");
            }
        }
    }
}
