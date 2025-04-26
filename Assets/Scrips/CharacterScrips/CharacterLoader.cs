using System;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting;

public enum RarityList
{
    One,
    Normal,
    Rare,
    Uniqu,
    Legend
}

public class CharacterLoader : MonoBehaviour
{
    public static CharacterLoader Instance { get; private set; }

    private void Awake()
    { 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void LoadAllCharacters()
    {
        TextAsset[] xmlFiles = Resources.LoadAll<TextAsset>("Data/Characters");
        Debug.Log($"[로드] 캐릭터 XML 파일 개수: {xmlFiles.Length}");

        List<CharacterData> rawList = new(); // 1차 수집용
        Dictionary<string, CharacterData> specimens = new(); // Specimen용

        foreach (TextAsset xml in xmlFiles)
        {
            XDocument doc = XDocument.Parse(xml.text);

            var parsed = doc.Descendants("Character").Select(x => new CharacterData
            {
                ID = (string)x.Attribute("ID") ?? "",
                ParentID = (string)x.Attribute("ParentID") ?? "",
                Specimen = bool.TryParse((string)x.Attribute("Specimen"), out bool specimen) ? specimen : false,

                Label = (string)x.Element("Label") ?? "",
                Description = (string)x.Element("Description") ?? "",

                Hp = (int?)x.Element("Stats")?.Element("Hp") ?? 0,
                MaxHp = (int?)x.Element("Stats")?.Element("Hp") ?? 0,
                Atk = (int?)x.Element("Stats")?.Element("Atk") ?? 0,
                Def = (int?)x.Element("Stats")?.Element("Def") ?? 0,
                EvasionRate = (float?)x.Element("Stats")?.Element("Evasionrate") ?? 0f,
                Accuracy = (float?)x.Element("Stats")?.Element("Accuracy") ?? 0f,
                Speed = (int?)x.Element("Stats")?.Element("Speed") ?? 0,

                Skills = x.Element("Skills")?
           .Elements("li")
           .Select(e => (string)e)
           .Where(s => !string.IsNullOrWhiteSpace(s))
           .ToList() ?? new List<string>(),

                Passives = x.Element("Passives")?
           .Elements("li")
           .Select(e => (string)e)
           .Where(s => !string.IsNullOrWhiteSpace(s))
           .ToList() ?? new List<string>(),

                Rarity = Enum.TryParse((string)x.Element("Rarity"), true, out RarityList rarity) ? rarity : RarityList.Normal,
                Sprite = (string)x.Element("Sprite") ?? "",
                Pattern = (string)x.Element("Pattern") ?? ""

            }).ToList();
            rawList.AddRange(parsed);
            
        }

        foreach (var data in rawList)
        {
            //Debug.Log($"[로드된 캐릭터] ID: {data.ID}, ParentID: {data.ParentID}, Specimen: {data.Specimen}");
            if (data.Specimen)
            {
                //Debug.Log($"[부모 등록] {data.ID}");
                specimens[data.ID] = data;
            }
        }

        foreach (var data in rawList)
        {
            //Debug.Log($"[로드된 캐릭터] ID: {data.ID}, ParentID: {data.ParentID}, Specimen: {data.Specimen}");
            if (data.Specimen) continue; //Debug.Log($"[부모 등록] {data.ID}");

            CharacterData final;

            if (!string.IsNullOrEmpty(data.ParentID))
            {
                if (specimens.TryGetValue(data.ParentID, out var parent))
                {
                    final = parent.Clone();
                    OverrideCharacter(final, data);
                }
                else
                {
                    Debug.LogError($"[Loader] 부모 ID '{data.ParentID}'를 specimens에서 찾을 수 없음 (자식 ID: {data.ID})");
                    continue;
                }
            }
            else
            {
                final = data;
            }

            if (!CharacterData.characterDict.ContainsKey(final.ID))
                CharacterData.characterDict.Add(final.ID, final);
        }


        Debug.Log($"[로드] 캐릭터 로딩 완료. 총 {CharacterData.characterDict.Count}개");
    }
    void OverrideCharacter(CharacterData baseData, CharacterData overrideData)
    {
        if (!string.IsNullOrEmpty(overrideData.Label)) baseData.Label = overrideData.Label;
        if (!string.IsNullOrEmpty(overrideData.Description)) baseData.Description = overrideData.Description;

        if (overrideData.Hp != 0) baseData.Hp = overrideData.Hp;
        if (overrideData.MaxHp != 0) baseData.MaxHp = overrideData.MaxHp;
        if (overrideData.Atk != 0) baseData.Atk = overrideData.Atk;
        if (overrideData.Def != 0) baseData.Def = overrideData.Def;
        if (overrideData.EvasionRate != 0f) baseData.EvasionRate = overrideData.EvasionRate;
        if (overrideData.Accuracy != 0f) baseData.Accuracy = overrideData.Accuracy;
        if (overrideData.Speed != 0) baseData.Speed = overrideData.Speed;

        if (overrideData.Skills.Count > 0) baseData.Skills = overrideData.Skills;
        if (overrideData.Passives.Count > 0) baseData.Passives = overrideData.Passives;

        if (!string.IsNullOrEmpty(overrideData.Sprite)) baseData.Sprite = overrideData.Sprite;
        if (!string.IsNullOrEmpty(overrideData.Pattern)) baseData.Pattern = overrideData.Pattern;

        baseData.Rarity = overrideData.Rarity;
    }
}
