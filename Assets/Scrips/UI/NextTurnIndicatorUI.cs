using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NextTurnIndicatorUI : MonoBehaviour
{
    [SerializeField] private GameObject turnblockPrefab;
    [SerializeField] private Transform turnblockSpawnPoint;
    [SerializeField] private Image turnBackground;

    public static NextTurnIndicatorUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void CreateTurnBlocks(List<CharacterStats> turnOrder)
    {
        // 기존 블록 모두 삭제
        foreach (Transform child in turnblockSpawnPoint)
            Destroy(child.gameObject);

        // 새 블록 생성
        foreach (var character in turnOrder)
        {
            var block = Instantiate(turnblockPrefab, turnblockSpawnPoint);
            var text = block.GetComponentInChildren<Text>();
            if (text != null)
                text.text = character.Label;
            // 추가 연출(색상, 아이콘 등)도 여기서!
        }
    }

    public void RemoveCurrentTurnBlock()
    {
        if (turnblockSpawnPoint.childCount > 0)
        {
            Destroy(turnblockSpawnPoint.GetChild(0).gameObject);
        }
    }
}