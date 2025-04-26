using System.Collections.Generic;
using UnityEngine;
public class PartySlot
{
    public int SlotIndex;         // ��: 0~3
    public Vector2 Position;      // �� �� ���� ��ġ
    public string CharacterID;    // ���� ���Կ� ���� �����ƴ���
}

public class InStageCharacterData
{
    public string CharacterID;
    public int CurrentHP;
    // ���߿� Ȯ�� ����: �����̻�, ��ų ��Ÿ�� ��
}

public class InStageData
{
    public List<PartySlot> Slots = new();
    public Dictionary<string, InStageCharacterData> Characters = new(); // CharacterID ����
}

