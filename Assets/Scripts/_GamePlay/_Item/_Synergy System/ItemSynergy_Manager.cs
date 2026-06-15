using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSynergy_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private ItemSynergy_ScrObj[] _placeTriggerSynergies;
    [SerializeField] private ItemSynergy_ScrObj[] _useTriggerSynergies;
    [SerializeField] private ItemSynergy_ScrObj[] _timeCountTriggerSynergies;

    private Dictionary<ItemSynergy_EffectType, IItemSynergy_EffectRunner> _effectRunners = new();


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        InGame_Manager manager = InGame_Manager.instance;

        manager.tilesController.OnTileItemsUpdate -= Trigger_onItemPlace;
        manager.cursor.itemCursor.OnItemUse -= Trigger_onItemUse;
        manager.time.UnRegister(ActionUpdateBus.AwakeUpdate, Trigger_onTimeCount);
    }


    // Data
    private void Set_Data()
    {
        InGame_Manager manager = InGame_Manager.instance;

        manager.tilesController.OnTileItemsUpdate += Trigger_onItemPlace;
        manager.cursor.itemCursor.OnItemUse += Trigger_onItemUse;
        manager.time.Register(ActionUpdateBus.AwakeUpdate, Trigger_onTimeCount);

        Load_EffectRunners();
    }


    // Effect Runner
    private void Load_EffectRunners()
    {
        _effectRunners[ItemSynergy_EffectType.playerDataUpdate] = new PlayerDataUpdate_EffectRunner();
        _effectRunners[ItemSynergy_EffectType.playerTemperatureSustain] = new PlayerTemperatureSustain_EffectRunner();
        _effectRunners[ItemSynergy_EffectType.tileStateUpdate] = new TileStateUpdate_EffectRunner();
        _effectRunners[ItemSynergy_EffectType.placeItem] = new PlaceItem_EffectRunner();
        _effectRunners[ItemSynergy_EffectType.replaceItem] = new ReplaceItem_EffectRunner();
    }

    private void Run_Effect(Tile targetTile, ItemSynergy_EffectData effectData)
    {
        if (targetTile == null || effectData == null) return;

        if (_effectRunners.TryGetValue(effectData.effectType, out IItemSynergy_EffectRunner runner) == false) return;
        runner.Run_Effect(targetTile, effectData);
    }
    private void Run_Effect(Tile targetTile, ItemSynergy_ScrObj targetSynergy)
    {
        ItemSynergy_EffectData[] effectDatas = targetSynergy.effectDatas;

        for (int i = 0; i < effectDatas.Length; i++)
        {
            Run_Effect(targetTile, effectDatas[i]);
        }
    }


    // Trigger Types
    private void Trigger_onItemPlace(Tile itemPlacedTile)
    {
        List<ItemData> placedItemDatas = itemPlacedTile.Placed_ItemDatas();

        for (int i = 0; i < _placeTriggerSynergies.Length; i++)
        {
            ItemSynergy_ScrObj triggerSynergy = _placeTriggerSynergies[i];

            if (triggerSynergy.TargetTile_Match(itemPlacedTile.data.tileScrObj) == false) continue;
            if (_placeTriggerSynergies[i].RequiredItems_Match(placedItemDatas) == false) continue;

            Run_Effect(itemPlacedTile, triggerSynergy);
            return;
        }
    }

    private void Trigger_onItemUse(Tile itemUseTile)
    {
        ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;
        List<ItemData> usedHisusedItemHistoryDatas = new(itemCursor.usedItemHistoryDatas);

        for (int i = 0; i < _placeTriggerSynergies.Length; i++)
        {
            ItemSynergy_ScrObj triggerSynergy = _placeTriggerSynergies[i];

            if (triggerSynergy.TargetTile_Match(itemUseTile.data.tileScrObj) == false) continue;
            if (_placeTriggerSynergies[i].RequiredItems_Match(usedHisusedItemHistoryDatas) == false) continue;

            List<ItemData> requiredItemDatas = triggerSynergy.Required_ItemDatas();

            foreach (ItemData requiredData in requiredItemDatas)
            {
                itemCursor.Remove_UseItemHistory(requiredData.itemScrObj);
            }
            Run_Effect(itemUseTile, triggerSynergy);
            break;
        }
    }

    private void Trigger_onTimeCount()
    {

    }
}