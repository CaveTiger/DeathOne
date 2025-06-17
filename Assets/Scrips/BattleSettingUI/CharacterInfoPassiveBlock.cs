using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoPassiveBlock : MonoBehaviour
{

    [SerializeField]
    private Transform passiveBar; //생성될 위치
    [SerializeField]
    private GameObject passiveBlock; //생성될 블럭 자기 자신을 지칭
    [SerializeField]
    private TextMeshProUGUI passiveName; //패시브 이름  
    [SerializeField]
    private TextMeshProUGUI passiveDescription; //패시브 설명   
    
    // 현재 패시브 정보를 임시로 저장하는 필드 (추후 확장 예정)
    [SerializeField]
    private string passive;

    /// <summary>
    /// 패시브 데이터(이름, 설명)를 받아 UI에 표시한다.
    /// </summary>
    /// <param name="name">패시브 이름</param>
    /// <param name="description">패시브 설명</param>
    public void SetPassiveInfo(string name, string description)
    {
        passiveName.text = name;
        passiveDescription.text = description;
        passive = name; // 임시로 이름을 passive 필드에 저장(확장 대비)
    }
}
