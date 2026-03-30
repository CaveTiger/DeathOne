using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

/// <summary>
/// 패시브 XML 로드. 필드/타입을 늘릴 때 작업 순서는 <see cref="PassiveSystemExtensionGuide"/>.
/// </summary>
public class PassiveLoader : MonoBehaviour
{
    public static PassiveLoader Instance { get; private set; }
    private const string BASE_RESOURCE_PATH = "Data/Passive/BasePassive";
    [SerializeField] private bool logLoadedPassiveDetails = true;
    [SerializeField] private bool logGetByIdMissDetails = true;
    private bool isInitialized = false;
    private static readonly Dictionary<string, PassiveData> passiveDict = new Dictionary<string, PassiveData>();
    private static readonly Dictionary<string, PassiveData> specimenDict = new Dictionary<string, PassiveData>();
    private static bool hasLoggedStaticMiss = false;

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

        LoadPassivesFromXML(BASE_RESOURCE_PATH);
        isInitialized = true;
    }

    /// <summary>
    /// XML에서 패시브 데이터를 로드합니다.
    /// </summary>
    /// <param name="xmlPath">XML 파일 경로</param>
    public void LoadPassivesFromXML(string xmlPath)
    {
        passiveDict.Clear();
        specimenDict.Clear();

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

        ValidatePassiveDefinitions();

        Debug.Log($"[PassiveLoader] {passiveDict.Count}개의 패시브 데이터를 로드했습니다.");

        if (logLoadedPassiveDetails)
        {
            DebugLogLoadedPassiveDetails();
        }
    }

    /// <summary>
    /// 개별 패시브 노드를 파싱합니다.
    /// </summary>
    /// <param name="passiveNode">패시브 XML 노드</param>
    private void ParsePassiveNode(XmlNode passiveNode)
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
        else if (!string.IsNullOrEmpty(parentID) && specimenDict.ContainsKey(parentID))
        {
            PassiveData parentData = specimenDict[parentID];
            ClonePassiveData(parentData, passiveData);
        }

        // XML에서 데이터 파싱
        ParsePassiveFields(passiveNode, passiveData);

        // Specimen=True는 템플릿으로만 보관하고, 최종 조회 딕셔너리에서는 제외
        if (isSpecimen)
        {
            specimenDict[passiveID] = passiveData;
            return;
        }

        // 딕셔너리에 저장
        passiveDict[passiveID] = passiveData;
    }

    /// <summary>
    /// 패시브 데이터를 복제합니다. <see cref="PassiveData"/>에 필드를 추가했으면 여기에도 반드시 복사 줄을 추가할 것.
    /// </summary>
    /// <param name="source">원본 데이터</param>
    /// <param name="target">대상 데이터</param>
    private void ClonePassiveData(PassiveData source, PassiveData target)
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
        target.useCount = source.useCount;
        target.startUseCount = source.startUseCount;
        target.grantStatusDuration = source.grantStatusDuration;
        target.grantStatusEffectId = source.grantStatusEffectId;
        target.grantStatusValue = source.grantStatusValue;
    }

    /// <summary>
    /// XML 필드들을 파싱하여 패시브 데이터에 적용합니다. 새 XML 태그는 <see cref="PassiveData"/> 필드와 함께 여기에 추가.
    /// </summary>
    /// <param name="passiveNode">패시브 XML 노드</param>
    /// <param name="passiveData">패시브 데이터</param>
    private void ParsePassiveFields(XmlNode passiveNode, PassiveData passiveData)
    {
        // 패시브 이름
        XmlNode nameNode = passiveNode.SelectSingleNode("Name");
        if (nameNode != null)
            passiveData.passiveName = nameNode.InnerText;

        // 설명
        XmlNode descNode = passiveNode.SelectSingleNode("Description");
        if (descNode != null)
            passiveData.description = descNode.InnerText;

        // 효과 타입 — 문자열은 PassiveType enum 이름과 완전 일치해야 함
        XmlNode typeNode = passiveNode.SelectSingleNode("Type");
        if (typeNode != null)
        {
            string typeRaw = typeNode.InnerText?.Trim();
            if (!string.IsNullOrEmpty(typeRaw))
            {
                if (!Enum.TryParse(typeRaw, out PassiveType parsedType))
                {
                    Debug.LogWarning(
                        $"[PassiveLoader] ID={passiveData.passiveID}: <Type> '{typeRaw}' 을(를) PassiveType으로 파싱할 수 없습니다. " +
                        $"C# enum 이름과 동일하게 적었는지 확인하세요. (가이드: PassiveSystemExtensionGuide)");
                }
                else
                    passiveData.passiveType = parsedType;
            }
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
            string statRaw = targetStatNode.InnerText?.Trim();
            if (string.IsNullOrEmpty(statRaw))
                passiveData.targetStat = TargetStat.None;
            else if (!Enum.TryParse(statRaw, out TargetStat stat))
            {
                passiveData.targetStat = TargetStat.None;
                Debug.LogWarning(
                    $"[PassiveLoader] ID={passiveData.passiveID}: <TargetStat> '{statRaw}' 파싱 실패 → None 처리.");
            }
            else
                passiveData.targetStat = stat;
        }
        XmlNode scriptClassNode = passiveNode.SelectSingleNode("ScriptClass");
        if (scriptClassNode != null)
            passiveData.scriptClass = scriptClassNode.InnerText;

        // TurnIntervalGrantStatus: 유즈 임계(0=상시)
        XmlNode useCountNode = passiveNode.SelectSingleNode("UseCount") ??
                               passiveNode.SelectSingleNode("Usecount");
        if (useCountNode != null && int.TryParse(useCountNode.InnerText, out int useCnt))
            passiveData.useCount = useCnt;

        XmlNode startCountNode = passiveNode.SelectSingleNode("StartCount") ??
                                 passiveNode.SelectSingleNode("startcount");
        if (startCountNode != null && int.TryParse(startCountNode.InnerText, out int startCnt))
            passiveData.startUseCount = startCnt;

        // TurnIntervalGrantStatus 등: 부여 상태이상 파라미터
        XmlNode grantDurationNode = passiveNode.SelectSingleNode("Duration");
        if (grantDurationNode != null && int.TryParse(grantDurationNode.InnerText, out int grantDur))
            passiveData.grantStatusDuration = grantDur;

        XmlNode statusIdNode = passiveNode.SelectSingleNode("statusEffectID") ??
                               passiveNode.SelectSingleNode("StatusEffectID");
        if (statusIdNode != null)
            passiveData.grantStatusEffectId = statusIdNode.InnerText?.Trim();

        XmlNode statusValNode = passiveNode.SelectSingleNode("statusEffectValue") ??
                                passiveNode.SelectSingleNode("StatusEffectValue");
        if (statusValNode != null && int.TryParse(statusValNode.InnerText, out int statusVal))
            passiveData.grantStatusValue = statusVal;
    }

    /// <summary>
    /// 로드 직후 타입별 필수 필드가 비어 있으면 경고. 새 PassiveType 추가 시 여기에 검증을 덧붙이면 반복 작업에서 빠르게 발견 가능.
    /// </summary>
    private void ValidatePassiveDefinitions()
    {
        foreach (PassiveData p in passiveDict.Values)
        {
            if (p == null) continue;

            switch (p.passiveType)
            {
                case PassiveType.TurnIntervalGrantStatus:
                    if (string.IsNullOrWhiteSpace(p.grantStatusEffectId))
                    {
                        Debug.LogWarning(
                            $"[PassiveLoader][검증] {p.passiveID} ({p.passiveName}): TurnIntervalGrantStatus — statusEffectID(또는 StatusEffectID)가 비어 있습니다.");
                    }
                    if (p.useCount < 0)
                    {
                        Debug.LogWarning(
                            $"[PassiveLoader][검증] {p.passiveID} ({p.passiveName}): TurnIntervalGrantStatus — UseCount가 음수입니다. 0(상시) 이상으로 맞추세요.");
                    }
                    break;
                case PassiveType.CustomScript:
                    if (string.IsNullOrWhiteSpace(p.scriptClass) ||
                        p.scriptClass.Equals("none", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogWarning(
                            $"[PassiveLoader][검증] {p.passiveID} ({p.passiveName}): CustomScript — ScriptClass가 비어 있거나 none입니다.");
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// ID로 패시브 데이터를 가져옵니다.
    /// </summary>
    /// <param name="id">패시브 ID</param>
    /// <returns>패시브 데이터</returns>
    public PassiveData GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            if (logGetByIdMissDetails)
                Debug.LogWarning($"[PassiveLoader][GetById] 빈 ID 조회 요청. count={passiveDict.Count}, instance={GetInstanceID()}");
            return null;
        }

        passiveDict.TryGetValue(id, out PassiveData data);

        if (data == null && logGetByIdMissDetails)
        {
            string keySample = passiveDict.Count > 0
                ? string.Join(",", passiveDict.Keys.Take(10))
                : "(empty)";
            Debug.LogWarning($"[PassiveLoader][GetByIdMiss] id={id}, count={passiveDict.Count}, keys(sample)={keySample}, initialized={isInitialized}, instance={GetInstanceID()}");
        }

        return data;
    }

    public static PassiveData GetByIdStatic(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        passiveDict.TryGetValue(id, out PassiveData data);
        if (data == null && !hasLoggedStaticMiss)
        {
            string keySample = passiveDict.Count > 0
                ? string.Join(",", passiveDict.Keys.Take(10))
                : "(empty)";
            Debug.LogWarning($"[PassiveLoader][GetByIdStaticMiss] id={id}, count={passiveDict.Count}, keys(sample)={keySample}");
            hasLoggedStaticMiss = true;
        }
        return data;
    }

    /// <summary>
    /// 모든 패시브 데이터를 반환합니다.
    /// </summary>
    /// <returns>패시브 데이터 딕셔너리</returns>
    public Dictionary<string, PassiveData> GetAllPassives()
    {
        return new Dictionary<string, PassiveData>(passiveDict);
    }

    private void DebugLogLoadedPassiveDetails()
    {
        foreach (var pair in passiveDict)
        {
            PassiveData p = pair.Value;
            if (p == null)
            {
                Debug.LogWarning($"[PassiveLoader][Spec] ID={pair.Key} -> null 데이터");
                continue;
            }

            Debug.Log(
                $"[PassiveLoader][Spec] " +
                $"ID={p.passiveID}, Name={p.passiveName}, Type={p.passiveType}, " +
                $"TargetStat={p.targetStat}, Value={p.value}, FloatValue={p.floatValue}, " +
                $"GrantsMana={p.grantsMana}, MaxMana={p.maxMana}, ManaRegenInterval={p.manaRegenInterval}, " +
                $"Rarity={p.rarity}, Cost={p.cost}, ScriptClass={p.scriptClass}, " +
                $"UseCount={p.useCount} StartCount={p.startUseCount} GrantStatus={p.grantStatusEffectId} dur={p.grantStatusDuration} val={p.grantStatusValue}");
        }
    }
} 