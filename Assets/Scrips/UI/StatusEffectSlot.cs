using UnityEngine;
using System.Collections.Generic;

public class StatusEffectSlot : MonoBehaviour
{
    // 상태이상 아이콘 프리팹
    public GameObject statusEffectIconPrefab;

    // 그리드 정렬 옵션
    public Vector2 cellSize = new Vector2(0.5f, 0.5f); // 각 셀의 크기(간격)
    public int maxPerRow = 5; // 한 줄에 5개
    public int maxRows = 2;   // 2줄
    public GameObject moreIconPrefab; // ...아이콘 프리팹

    // 최대 표시 개수(2줄, 5+4=9)
    public int maxDisplayCount = 9;

    // 현재 슬롯에 표시 중인 상태이상 아이콘 리스트
    private List<StatusEffectInstance> activeInstances = new List<StatusEffectInstance>();
    private GameObject moreIconInstance;

    /// <summary>
    /// 상태이상 추가
    /// </summary>
    public void AddStatusEffect(StatusEffectData effectData, int duration, int value, CharacterStats owner)
    {
        GameObject icon = Instantiate(statusEffectIconPrefab, transform);
        var instance = icon.GetComponent<StatusEffectInstance>();
        if (instance != null)
        {
            instance.Initialize(effectData, duration, value, owner);
            activeInstances.Add(instance);
            UpdateGridLayout();
        }
        else
        {
            Debug.LogWarning("StatusEffectInstance 컴포넌트가 프리팹에 없습니다.");
        }
    }

    /// <summary>
    /// 상태이상 제거
    /// </summary>
    public void RemoveStatusEffect(StatusEffectData effectData)
    {
        for (int i = activeInstances.Count - 1; i >= 0; i--)
        {
            if (activeInstances[i].EffectData == effectData)
            {
                Destroy(activeInstances[i].gameObject);
                activeInstances.RemoveAt(i);
            }
        }
        UpdateGridLayout();
    }

    /// <summary>
    /// 모든 상태이상 초기화
    /// </summary>
    public void ClearAll()
    {
        foreach (var instance in activeInstances)
            Destroy(instance.gameObject);
        activeInstances.Clear();
        UpdateGridLayout();
    }

    /// <summary>
    /// 그리드 정렬(월드 오브젝트용, 2줄, 5+4+...)
    /// </summary>
    private void UpdateGridLayout()
    {
        // 기존 moreIcon 제거
        if (moreIconInstance != null)
        {
            Destroy(moreIconInstance);
            moreIconInstance = null;
        }

        int displayCount = Mathf.Min(activeInstances.Count, maxDisplayCount);
        for (int i = 0; i < displayCount; i++)
        {
            int row = i / maxPerRow;
            int col = i % maxPerRow;
            // 2줄: 윗줄 5개(0~4), 아랫줄 4개(5~8)
            if (row == 1 && col == 4) break; // 아랫줄 5번째는 ...아이콘 자리
            Vector3 pos = new Vector3(col * cellSize.x, -row * cellSize.y, 0);
            activeInstances[i].transform.localPosition = pos;
            activeInstances[i].gameObject.SetActive(true);
        }
        // 초과분은 숨김
        for (int i = maxDisplayCount; i < activeInstances.Count; i++)
        {
            activeInstances[i].gameObject.SetActive(false);
        }

        // 더보기 아이콘 처리 (상태이상 10개 이상일 때)
        if (activeInstances.Count > maxDisplayCount && moreIconPrefab != null)
        {
            // 아랫줄 5번째(1,4)에 ...아이콘
            Vector3 pos = new Vector3(4 * cellSize.x, -1 * cellSize.y, 0);
            moreIconInstance = Instantiate(moreIconPrefab, transform);
            moreIconInstance.transform.localPosition = pos;
        }
    }
}
