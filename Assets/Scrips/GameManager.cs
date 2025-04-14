using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Debug.Log("GameManager Awake");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameManager �ߺ�! ������");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CharacterLoader.Instance.LoadAllCharacters();
        if (CharacterData.characterDict != null)
        {
            Debug.Log("ĳ���� ������ �ε� �Ϸ�");
        }
    }
    private void Start()
    {
        StageLoader.Instance.LoadStages();
        foreach (var kvp in CharacterData.characterDict)
        {
            var data = kvp.Value;
            Debug.Log($"[��ųʸ� üũ] ID: {data.ID}, HP: {data.Hp}, Atk: {data.Atk}, Def: {data.Def}, Speed{data.Speed}, Sprite: {data.Sprite}, EvasionRate{data.EvasionRate }, Accuracy{data.Accuracy}");
        } //�������� üũ
        foreach (var kvp in StageManager.Instance.stageDict)
        {
            var stage = kvp.Value;
            Debug.Log($"[��������] ID: {stage.ID}, Label: {stage.Label}, Block ��: {stage.Blocks.Count}");
        }
        //�������� ��� üũ
        foreach (var kvp in StageManager.Instance.stageBlockDict)
        {
            var block = kvp.Value;
            Debug.Log($"[���] StageKey: {block.StageKey}, BlockType: {block.BlockType}, ParentBlockName: {block.ParentBlockName}");

            if (block.EnemyIDs.Count > 0)
                Debug.Log($" �� �� ID: {string.Join(", ", block.EnemyIDs)}");

            if (block.EventList.Count > 0)
                Debug.Log($" �� �̺�Ʈ: {string.Join(", ", block.EventList)}");

            if (block.SpecialGuests.Count > 0)
                Debug.Log($" �� ����� �Խ�Ʈ: {string.Join(", ", block.SpecialGuests)}");

            if (!string.IsNullOrEmpty(block.FrontCutID) || !string.IsNullOrEmpty(block.BackCutID))
                Debug.Log($" �� �ƽ�: Front={block.FrontCutID}, Back={block.BackCutID}");
        }
    }
}
