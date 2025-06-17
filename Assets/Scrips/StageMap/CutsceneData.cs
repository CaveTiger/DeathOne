using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CutsceneData
{
    public string ID { get; set; }
    public string ParentID { get; set; }
    public string CutName { get; set; }
    public string BGM { get; set; }
    public List<CutsceneScene> Scenes { get; set; } = new List<CutsceneScene>();
}

[System.Serializable]
public class CutsceneScene
{
    public string CutImage { get; set; }
    public List<CutsceneDialogue> Dialogues { get; set; } = new List<CutsceneDialogue>();
}

[System.Serializable]
public class CutsceneDialogue
{
    public string Character { get; set; }
    public string CharacterImage { get; set; }
    public string Line { get; set; }
    public string Sound { get; set; }
} 