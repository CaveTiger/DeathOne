using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using System.Linq;
using System;

public class SkillLoader : MonoBehaviour
{
    public static SkillLoader Instance { get; private set; }
    private bool isInitialized = false;

    private const string BASE_RESOURCE_PATH = "Data/Skill";
    private readonly string[] REQUIRED_FILES = { "BaseSkill" };

    [System.Serializable]
    public class SkillLoadPath
    {
        public string resourcePath;
        public bool isRequired = false;
        public bool isModPath = false;
    }

    [SerializeField]
    private List<SkillLoadPath> additionalLoadPaths = new List<SkillLoadPath>();

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
            Debug.LogWarning("[SkillLoader] 이미 초기화되었습니다.");
            return;
        }

        Debug.Log("[SkillLoader] 초기화 시작");
        LoadAllSkills();
        Debug.Log($"[SkillLoader] 스킬 로딩 완료. 총 {SkillData.skillDict.Count}개");
        Debug.Log($"[SkillLoader] 로드된 스킬 목록:\n{string.Join("\n", SkillData.skillDict.Keys.Select(id => $"  - {id}"))}");
        isInitialized = true;
    }

    public void LoadAllSkills()
    {
        // 기본 경로에서 파일 로드
        TextAsset[] baseFiles = Resources.LoadAll<TextAsset>(BASE_RESOURCE_PATH);
        Debug.Log($"[SkillLoader] 기본 스킬 XML 파일 개수: {baseFiles.Length}");

        List<SkillData> rawList = new List<SkillData>();
        Dictionary<string, SkillData> specimens = new Dictionary<string, SkillData>();

        // 기본 파일 처리
        foreach (TextAsset xml in baseFiles)
        {
            Debug.Log($"[SkillLoader] XML 파일 처리 중: {xml.name}");
            XDocument doc = XDocument.Parse(xml.text);
            var parsed = doc.Descendants("Skill").Select(x => new SkillData
            {
                ID = (string)x.Attribute("ID") ?? "",
                ParentID = (string)x.Attribute("ParentID") ?? "",
                Specimen = bool.TryParse((string)x.Attribute("Specimen"), out bool specimen) ? specimen : false,
                Name = (string)x.Element("Name") ?? "",
                Icon = (string)x.Element("Icon") ?? "",
                Type = Enum.TryParse(x.Element("Type")?.Value, out SkillType result) ? result : SkillType.Damage,
                Description = (string)x.Element("Description") ?? "",
                DamageMin = (int?)x.Element("DamageMin") ?? 0,
                DamageMax = (int?)x.Element("DamageMax") ?? 0,
                Cooldown = (int)((float?)x.Element("Cooldown") ?? 0f),
                Range = (float?)x.Element("Range") ?? 0f,
                SkillTarget = (string)x.Element("SkillTarget") ?? "",
                Motion = (string)x.Element("Motion") ?? "",
                AttackPoint = (string)x.Element("AttackPoint") ?? "",
                AttackEffect = (string)x.Element("AttackEffect") ?? "",
                ManaCost = (int?)x.Element("ManaCost") ?? 0,
                StaminaCost = (int?)x.Element("StaminaCost") ?? 0,
                HealthCost = (int?)x.Element("HealthCost") ?? 0,
                HealAmount = 0, // 힐량은 사용 시점에 계산
                HealMin = GetHealRange(x).healMin,
                HealMax = GetHealRange(x).healMax,
                KnockdownMultiplier = (float?)x.Element("KnockdownMultiplier") ?? 1.0f,
                skillEffects = x.Element("SkillEffect")?
                    .Elements("li")
                    .Select(li => {
                        var effectIdElement = li.Element("EffectID");
                        if (effectIdElement == null) return null;

                        string effectId = (string)effectIdElement;
                        if (string.IsNullOrWhiteSpace(effectId)) return null;

                        int value = (int?)li.Element("Value") ?? 0;
                        int duration = (int?)li.Element("Duration") ?? 0;

                        return new SkillEffectInfo
                        {
                            EffectID = effectId.Trim(),
                            Value = value,
                            Duration = duration
                        };
                    })
                    .Where(e => e != null)
                    .ToList() ?? new List<SkillEffectInfo>()
            }).ToList();
            Debug.Log($"[SkillLoader] {xml.name}에서 파싱된 스킬 수: {parsed.Count}");
            rawList.AddRange(parsed);
        }

        // 추가 경로에서 파일 로드
        foreach (var path in additionalLoadPaths)
        {
            Debug.Log($"[SkillLoader] 추가 경로 처리 중: {path.resourcePath}");
            TextAsset[] additionalFiles = Resources.LoadAll<TextAsset>(path.resourcePath);
            Debug.Log($"[SkillLoader] 추가 경로의 파일 수: {additionalFiles.Length}");
            foreach (TextAsset xml in additionalFiles)
            {
                Debug.Log($"[SkillLoader] 추가 XML 파일 처리 중: {xml.name}");
                XDocument doc = XDocument.Parse(xml.text);
                var parsed = doc.Descendants("Skill").Select(x => new SkillData
                {
                    ID = (string)x.Attribute("ID") ?? "",
                    ParentID = (string)x.Attribute("ParentID") ?? "",
                    Specimen = bool.TryParse((string)x.Attribute("Specimen"), out bool specimen) ? specimen : false,
                    Name = (string)x.Element("Name") ?? "",
                    Icon = (string)x.Element("Icon") ?? "",
                    Type = Enum.TryParse(x.Element("Type")?.Value, out SkillType result) ? result : SkillType.Damage,
                    Description = (string)x.Element("Description") ?? "",
                    DamageMin = (int?)x.Element("DamageMin") ?? 0,
                    DamageMax = (int?)x.Element("DamageMax") ?? 0,
                    Cooldown = (int)((float?)x.Element("Cooldown") ?? 0f),
                    Range = (float?)x.Element("Range") ?? 0f,
                    SkillTarget = (string)x.Element("SkillTarget") ?? "",
                    Motion = (string)x.Element("Motion") ?? "",
                    AttackPoint = (string)x.Element("AttackPoint") ?? "",
                    AttackEffect = (string)x.Element("AttackEffect") ?? "",
                    ManaCost = (int?)x.Element("ManaCost") ?? 0,
                    StaminaCost = (int?)x.Element("StaminaCost") ?? 0,
                    HealthCost = (int?)x.Element("HealthCost") ?? 0,
                    HealAmount = 0, // 힐량은 사용 시점에 계산
                    HealMin = GetHealRange(x).healMin,
                    HealMax = GetHealRange(x).healMax,
                KnockdownMultiplier = (float?)x.Element("KnockdownMultiplier") ?? 1.0f,
                skillEffects = x.Element("SkillEffect")?
                        .Elements("li")
                        .Select(li => {
                            var effectIdElement = li.Element("EffectID");
                            if (effectIdElement == null) return null;

                            string effectId = (string)effectIdElement;
                            if (string.IsNullOrWhiteSpace(effectId)) return null;

                            int value = (int?)li.Element("Value") ?? 0;
                            int duration = (int?)li.Element("Duration") ?? 0;

                            return new SkillEffectInfo
                            {
                                EffectID = effectId.Trim(),
                                Value = value,
                                Duration = duration
                            };
                        })
                        .Where(e => e != null)
                        .ToList() ?? new List<SkillEffectInfo>()
                }).ToList();
                Debug.Log($"[SkillLoader] {xml.name}에서 파싱된 스킬 수: {parsed.Count}");
                rawList.AddRange(parsed);

                // 파싱 직후 skillEffects 로그 출력
                foreach (var skill in parsed)
                {
                    if (skill.skillEffects != null && skill.skillEffects.Count > 0)
                    {
                        Debug.Log($"[SkillLoader] 파싱 직후: {skill.ID} (ParentID: {skill.ParentID}, Specimen: {skill.Specimen}) - skillEffects 개수: {skill.skillEffects.Count}");
                        foreach (var effect in skill.skillEffects)
                        {
                            Debug.Log($"[SkillLoader]   └─ EffectID: {effect.EffectID}, Value: {effect.Value}, Duration: {effect.Duration}");
                        }
                    }
                    else
                    {
                        Debug.Log($"[SkillLoader] 파싱 직후: {skill.ID} (ParentID: {skill.ParentID}, Specimen: {skill.Specimen}) - skillEffects가 비어있음 (null: {skill.skillEffects == null}, Count: {skill.skillEffects?.Count ?? 0})");
                    }
                }
            }
        }

        Debug.Log($"[SkillLoader] 총 파싱된 스킬 수: {rawList.Count}");

        // Specimen 수집
        foreach (var data in rawList)
        {
            if (data.Specimen)
            {
                specimens[data.ID] = data;
            }
        }
        foreach (var data in rawList)
        {
            if (data.Specimen) continue;

            SkillData final;

            if (!string.IsNullOrEmpty(data.ParentID))
            {
                if (specimens.TryGetValue(data.ParentID, out var parent))
                {
                    final = parent.Clone();
                    OverrideSkill(final, data);
                    final.ID = data.ID;
                }
                else
                {
                    Debug.LogError($"[SkillLoader] 부모 ID '{data.ParentID}'를 specimens에서 찾을 수 없음 (자식 ID: {data.ID})");
                    continue;
                }
            }
            else
            {
                // ParentID가 없는 경우에도 Clone()을 사용하여 독립적인 복사본 생성
                final = data.Clone();
            }

            if (!SkillData.skillDict.ContainsKey(final.ID))
            {
                SkillData.skillDict.Add(final.ID, final);
            }
            else
            {
                Debug.LogWarning($"[SkillLoader] 스킬 ID 중복: {final.ID} (이미 SkillDict에 존재)");
            }
        }
    }

    /// <summary>
    /// XML에서 HealMin과 HealMax를 읽어서 범위 힐량을 설정합니다.
    /// </summary>
    private (int healMin, int healMax) GetHealRange(System.Xml.Linq.XElement skillElement)
    {
        int healMin = (int?)skillElement.Element("HealMin") ?? 0;
        int healMax = (int?)skillElement.Element("HealMax") ?? 0;
        
        // 기존 HealAmount 필드가 있으면 범위로 설정
        if (healMin == 0 && healMax == 0)
        {
            int HealAmount = (int?)skillElement.Element("HealAmount") ?? 0;
            if (HealAmount > 0)
            {
                healMin = HealAmount;
                healMax = HealAmount;
            }
        }
        
        return (healMin, healMax);
    }

    private void OverrideSkill(SkillData baseData, SkillData overrideData)
    {
        if (!string.IsNullOrEmpty(overrideData.Name)) baseData.Name = overrideData.Name;
        if (!string.IsNullOrEmpty(overrideData.Icon)) baseData.Icon = overrideData.Icon;
        if (overrideData.DamageMin != 0) baseData.DamageMin = overrideData.DamageMin;
        if (overrideData.DamageMax != 0) baseData.DamageMax = overrideData.DamageMax;
        if (!string.IsNullOrEmpty(overrideData.SkillTarget)) baseData.SkillTarget = overrideData.SkillTarget;
        if (!string.IsNullOrEmpty(overrideData.Motion)) baseData.Motion = overrideData.Motion;
        if (!string.IsNullOrEmpty(overrideData.AttackPoint)) baseData.AttackPoint = overrideData.AttackPoint;
        if (!string.IsNullOrEmpty(overrideData.AttackEffect)) baseData.AttackEffect = overrideData.AttackEffect;
        if (!string.IsNullOrEmpty(overrideData.Group)) baseData.Group = overrideData.Group;
        if (!string.IsNullOrEmpty(overrideData.Description)) baseData.Description = overrideData.Description;
        if (overrideData.Cooldown != 0) baseData.Cooldown = overrideData.Cooldown;
        if (overrideData.Range != 0) baseData.Range = overrideData.Range;
        if (overrideData.ManaCost != 0) baseData.ManaCost = overrideData.ManaCost;
        if (overrideData.StaminaCost != 0) baseData.StaminaCost = overrideData.StaminaCost;
        if (overrideData.HealthCost != 0) baseData.HealthCost = overrideData.HealthCost;
        if (overrideData.HealAmount != 0) baseData.HealAmount = overrideData.HealAmount;
        if (overrideData.HealMin != 0) baseData.HealMin = overrideData.HealMin;
        if (overrideData.HealMax != 0) baseData.HealMax = overrideData.HealMax;
        if (overrideData.KnockdownMultiplier != 1.0f) baseData.KnockdownMultiplier = overrideData.KnockdownMultiplier;
        // CurrentCooldown은 게임 내에서만 관리되므로 파싱하지 않음
        baseData.Type = overrideData.Type;
        
        // skillEffects 덮어쓰기 로직 (자식에 있으면 덮어쓰기, 없으면 부모 것 유지)
        if (overrideData.skillEffects != null && overrideData.skillEffects.Count > 0)
        {
            baseData.skillEffects = new List<SkillEffectInfo>(overrideData.skillEffects);
        }
        else
        {
            // 부모의 skillEffects를 새 리스트로 복사하여 참조 문제 방지
            if (baseData.skillEffects != null && baseData.skillEffects.Count > 0)
            {
                baseData.skillEffects = new List<SkillEffectInfo>(baseData.skillEffects);
            }
        }
    }
}
