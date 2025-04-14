using System;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using System.Linq;

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
        foreach (TextAsset xml in xmlFiles)
        {
            XDocument doc = XDocument.Parse(xml.text);

            var characters = doc.Descendants("Character").Select(x => new CharacterData
            {
                ID = (string)x.Attribute("ID") ?? "",
                ParentID = (string)x.Attribute("ParentID") ?? "",
                Specimen = bool.TryParse((string)x.Attribute("Specimen"), out bool specimen) ? specimen : false,

                Label = (string)x.Element("Label") ?? "",
                Description = (string)x.Element("Description") ?? "",

                Hp = (int?)x.Element("Stats")?.Element("Hp") ?? 0,
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


            foreach (var data in characters)
            {
                if (!CharacterData.characterDict.ContainsKey(data.ID))
                    CharacterData.characterDict.Add(data.ID, data);
                else
                    Debug.LogWarning($"중복된 캐릭터 ID 발견: {data.ID}");
            }
            foreach (var kvp in CharacterData.characterDict)
            {
                Debug.Log($"[Dict] 딕셔너리에 들어간 ID: '{kvp.Key}'");
            }
            Debug.Log($"[Load] 캐릭터 총 {characters.Count}명 파싱됨");
            foreach (var c in characters)
            {
                Debug.Log($"[Load] 파싱된 ID: '{c.ID}'");
            }
            Debug.Log($"캐릭터 로딩 완료. 총 {CharacterData.characterDict.Count}개");
            
        }
    }
}
