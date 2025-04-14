using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class SkillLoader : MonoBehaviour
{
    public static Dictionary<string, SkillData> LoadSkills(string xmlPath)
    {
        var doc = XDocument.Load(xmlPath);
        var skills = new Dictionary<string, SkillData>();

        foreach (var elem in doc.Descendants("Skill"))
        {
            var specimenAttr = elem.Attribute("Specimen");
            if (specimenAttr != null && specimenAttr.Value == "True")
                continue; // Specimen은 무시

            SkillData skill = new SkillData
            {
                ID = elem.Attribute("ID")?.Value,
                ParentID = elem.Attribute("ParentID")?.Value,
                Name = elem.Element("Name")?.Value,
                DamageMin = int.Parse(elem.Element("DamageMin")?.Value ?? "0"),
                DamageMax = int.Parse(elem.Element("DamageMax")?.Value ?? "0"),
                SkillTarget = elem.Element("SkillTarget")?.Value,
                Motion = elem.Element("Motion")?.Value,
                AttackPoint = elem.Element("AttackPoint")?.Value,
                AttackEffect = elem.Element("AttackEffect")?.Value,
                Specimen = specimenAttr != null && specimenAttr.Value == "True"
            };

            // SkillEffect가 리스트일 수도 있음
            var effectList = elem.Element("SkillEffect");
            if (effectList != null)
            {
                foreach (var li in effectList.Elements("li"))
                {
                    skill.SkillEffects.Add(li.Value);
                }
            }

            skills[skill.ID] = skill;
        }

        return skills;
    }

}
