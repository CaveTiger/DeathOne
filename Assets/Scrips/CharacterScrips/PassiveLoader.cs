using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class PassiveLoader
{
    public static Dictionary<string, PassiveData> passiveDict = new Dictionary<string, PassiveData>();

    /// <summary>
    /// XML에서 패시브 데이터를 로드합니다.
    /// </summary>
    /// <param name="xmlPath">XML 파일 경로</param>
    public static void LoadPassivesFromXML(string xmlPath)
    {
        TextAsset xmlFile = Resources.Load<TextAsset>(xmlPath);
        if (xmlFile == null)
        {
            Debug.LogError($"[PassiveLoader] XML 파일을 찾을 수 없습니다: {xmlPath}");
            return;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlFile.text);

        XmlNodeList passiveNodes = xmlDoc.SelectNodes("//Passive");
        if (passiveNodes == null)
        {
            Debug.LogWarning($"[PassiveLoader] 패시브 노드를 찾을 수 없습니다: {xmlPath}");
            return;
        }

        foreach (XmlNode passiveNode in passiveNodes)
        {
            ParsePassiveNode(passiveNode);
        }

        Debug.Log($"[PassiveLoader] {passiveDict.Count}개의 패시브 데이터를 로드했습니다.");
    }

    /// <summary>
    /// 개별 패시브 노드를 파싱합니다.
    /// </summary>
    /// <param name="passiveNode">패시브 XML 노드</param>
    private static void ParsePassiveNode(XmlNode passiveNode)
    {
        string passiveID = passiveNode.Attributes["ID"]?.Value;
        string parentID = passiveNode.Attributes["ParentID"]?.Value;
        bool isSpecimen = passiveNode.Attributes["Specimen"]?.Value == "True";

        if (string.IsNullOrEmpty(passiveID))
        {
            Debug.LogWarning("[PassiveLoader] 패시브 ID가 없습니다.");
            return;
        }

        // 기본 데이터 생성
        PassiveData passiveData = new PassiveData();
        passiveData.passiveID = passiveID;

        // 부모 데이터가 있다면 상속
        if (!string.IsNullOrEmpty(parentID) && passiveDict.ContainsKey(parentID))
        {
            PassiveData parentData = passiveDict[parentID];
            ClonePassiveData(parentData, passiveData);
        }

        // XML에서 데이터 파싱
        ParsePassiveFields(passiveNode, passiveData);

        // 딕셔너리에 저장
        passiveDict[passiveID] = passiveData;
    }

    /// <summary>
    /// 패시브 데이터를 복제합니다.
    /// </summary>
    /// <param name="source">원본 데이터</param>
    /// <param name="target">대상 데이터</param>
    private static void ClonePassiveData(PassiveData source, PassiveData target)
    {
        target.passiveName = source.passiveName;
        target.description = source.description;
        target.passiveType = source.passiveType;
        target.targetStat = source.targetStat;
        target.value = source.value;
        target.floatValue = source.floatValue;
        target.grantsMana = source.grantsMana;
        target.maxMana = source.maxMana;
        target.manaRegenInterval = source.manaRegenInterval;
        target.rarity = source.rarity;
        target.cost = source.cost;
        target.scriptClass = source.scriptClass;
    }

    /// <summary>
    /// XML 필드들을 파싱하여 패시브 데이터에 적용합니다.
    /// </summary>
    /// <param name="passiveNode">패시브 XML 노드</param>
    /// <param name="passiveData">패시브 데이터</param>
    private static void ParsePassiveFields(XmlNode passiveNode, PassiveData passiveData)
    {
        // 패시브 이름
        XmlNode nameNode = passiveNode.SelectSingleNode("Name");
        if (nameNode != null)
            passiveData.passiveName = nameNode.InnerText;

        // 설명
        XmlNode descNode = passiveNode.SelectSingleNode("Description");
        if (descNode != null)
            passiveData.description = descNode.InnerText;

        // 효과 타입
        XmlNode typeNode = passiveNode.SelectSingleNode("Type");
        if (typeNode != null)
        {
            if (System.Enum.TryParse<PassiveType>(typeNode.InnerText, out PassiveType type))
                passiveData.passiveType = type;
        }

        // 수치
        XmlNode valueNode = passiveNode.SelectSingleNode("Value");
        if (valueNode != null && int.TryParse(valueNode.InnerText, out int value))
            passiveData.value = value;

        // 부가 수치
        XmlNode floatValueNode = passiveNode.SelectSingleNode("FloatValue");
        if (floatValueNode != null && float.TryParse(floatValueNode.InnerText, out float floatValue))
            passiveData.floatValue = floatValue;

        // 마나 관련
        XmlNode grantsManaNode = passiveNode.SelectSingleNode("GrantsMana");
        if (grantsManaNode != null)
            passiveData.grantsMana = grantsManaNode.InnerText == "True";

        XmlNode maxManaNode = passiveNode.SelectSingleNode("MaxMana");
        if (maxManaNode != null && int.TryParse(maxManaNode.InnerText, out int maxMana))
            passiveData.maxMana = maxMana;

        XmlNode manaRegenIntervalNode = passiveNode.SelectSingleNode("ManaRegenInterval");
        if (manaRegenIntervalNode != null && int.TryParse(manaRegenIntervalNode.InnerText, out int manaRegenInterval))
            passiveData.manaRegenInterval = manaRegenInterval;

        // 레어리티
        XmlNode rarityNode = passiveNode.SelectSingleNode("Rarity");
        if (rarityNode != null)
        {
            if (System.Enum.TryParse<RarityList>(rarityNode.InnerText, out RarityList rarity))
                passiveData.rarity = rarity;
        }

        // 코스트
        XmlNode costNode = passiveNode.SelectSingleNode("Cost");
        if (costNode != null && int.TryParse(costNode.InnerText, out int cost))
            passiveData.cost = cost;

        XmlNode targetStatNode = passiveNode.SelectSingleNode("TargetStat");
        if (targetStatNode != null)
        {
            if (System.Enum.TryParse<TargetStat>(targetStatNode.InnerText, out TargetStat stat))
                passiveData.targetStat = stat;
            else
                passiveData.targetStat = TargetStat.None;
        }
        XmlNode scriptClassNode = passiveNode.SelectSingleNode("ScriptClass");
        if (scriptClassNode != null)
            passiveData.scriptClass = scriptClassNode.InnerText;
    }

    /// <summary>
    /// ID로 패시브 데이터를 가져옵니다.
    /// </summary>
    /// <param name="id">패시브 ID</param>
    /// <returns>패시브 데이터</returns>
    public static PassiveData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        passiveDict.TryGetValue(id, out PassiveData data);
        return data;
    }

    /// <summary>
    /// 모든 패시브 데이터를 반환합니다.
    /// </summary>
    /// <returns>패시브 데이터 딕셔너리</returns>
    public static Dictionary<string, PassiveData> GetAllPassives()
    {
        return new Dictionary<string, PassiveData>(passiveDict);
    }
} 