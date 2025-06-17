using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 상태이상 아이콘(껍데기) 핸들러: 실체를 따라다니며 마우스 이벤트를 감지
/// </summary>
public class StatusEffectIconHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Transform target; // 따라갈 실체(캐릭터)
    public Vector3 offset = new Vector3(0, 1.5f, 0); // 실체 기준 오프셋

    public StatusEffectInstance effectInstance; // 연결된 상태이상 인스턴스

    public string description;
    public string Label;
    public Sprite icon; // 상태이상 아이콘
    public int damage; // 데미지 값(혹은 효과 수치)
    public int turns; // 남은 턴 수
    public StatusPopupHandler popupHandler; // 인스펙터에서 연결

    public string effectID; // 상태이상 고유 ID
    public StatusEffectData effectData; // 스크립터브젝트 직접 참조
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 실체가 사라졌으면 자신도 삭제
        if (target == null || (effectInstance != null && effectInstance.gameObject == null))
        {
            Destroy(this.gameObject);
            return;
        }

        // 실체를 따라다님
        transform.position = target.position + offset;
    }

    public void Initialize(StatusEffectData data, int damage, int turns)
    {
        effectID = data.EffectID;
        effectData = data;
        icon = data.icon;
        description = data.description;
        this.damage = damage;
        this.turns = turns;
    }

    // 마우스 오버 시 툴팁 등 표시
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (popupHandler != null && effectData != null)
        {
            popupHandler.ShowStatusPopup(effectData.icon, effectData.description, damage, turns);
        }
    }

    // 마우스가 나가면 툴팁 숨김
    public void OnPointerExit(PointerEventData eventData)
    {
        if (popupHandler != null)
        {
            popupHandler.HideStatusPopup();
        }
    }

    // 클릭 시 추가 인터랙션(선택 등)
    public void OnPointerClick(PointerEventData eventData)
    {
        // 필요시 구현
    }
}
