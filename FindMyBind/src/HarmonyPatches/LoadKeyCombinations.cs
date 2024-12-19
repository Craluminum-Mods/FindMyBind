using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Util;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;

namespace FindMyBind;

[HarmonyPatch(typeof(GuiCompositeSettings), "LoadKeyCombinations")]
public static class LoadKeyCombinationsPatch
{
    [HarmonyPrefix]
    public static bool Prefix(GuiCompositeSettings __instance, IGameSettingsHandler ___handler, Dictionary<HotkeyType, int> ___sortOrder, HotKey ___keyCombClone, ref List<ConfigItem> ___keycontrolItems, ref int? ___clickedItemIndex, ref string[] ___titles)
    {
        int hotkeyIndex = -1;
        if (___keycontrolItems.Count >= ___clickedItemIndex)
        {
            hotkeyIndex = (int)___keycontrolItems[___clickedItemIndex.Value].Data;
        }
        ___keycontrolItems.Clear();
        int i = 0;
        List<ConfigItem>[] sortedItems = new List<ConfigItem>[___sortOrder.Count];
        for (int j = 0; j < sortedItems.Length; j++)
        {
            sortedItems[j] = new List<ConfigItem>();
        }
        foreach (KeyValuePair<string, HotKey> val in ScreenManager.hotkeyManager.HotKeys)
        {
            HotKey kc = val.Value;
            if (___clickedItemIndex.HasValue && i == hotkeyIndex)
            {
                kc = ___keyCombClone;
            }
            string text = "?";
            if (kc.CurrentMapping != null)
            {
                text = kc.CurrentMapping.ToString();
            }
            ConfigItem item = new ConfigItem
            {
                Code = val.Key,
                Key = kc.Name,
                Value = text,
                Data = i
            };
            int index = ___keycontrolItems.FindIndex((ConfigItem configitem) => configitem.Value == text);
            if (index != -1)
            {
                item.error = true;
                ___keycontrolItems[index].error = true;
            }

            sortedItems[___sortOrder[kc.KeyCombinationType]].Add(item);
            i++;
        }

        //// Very crude example for highlighting duplicate hotkeys
        //IEnumerable<ConfigItem> _all = sortedItems.SelectMany(x => x);
        //_all.Foreach(x =>
        //{
        //    _all.Foreach(y =>
        //    {
        //        if (x.Code != y.Code && x.Value == y.Value)
        //        {
        //            x.error = true;
        //            y.error = true;
        //        }
        //    });
        //});

        for (int j = 0; j < sortedItems.Length; j++)
        {
            if ((j != 1 || ClientSettings.SeparateCtrl) && j != 9)
            {
                string searchText = AddSearchFieldToControlsTab.currentSearchText?.ToSearchFriendly().ToLowerInvariant();
                bool canSearch = !string.IsNullOrEmpty(searchText);

                IEnumerable<ConfigItem> _filteredSortedItems = Enumerable.Empty<ConfigItem>();
                if (canSearch)
                {
                    foreach (ConfigItem item in sortedItems[j])
                    {
                        string key = item.Key.ToSearchFriendly().ToLowerInvariant();
                        if (key.Contains(searchText))
                        {
                            _filteredSortedItems = _filteredSortedItems.AddItem(item);
                        }
                    }
                }

                if (canSearch && _filteredSortedItems?.Any() == false)
                {
                    continue;
                }

                if (j != 7)
                {
                    ___keycontrolItems.Add(new ConfigItem
                    {
                        Type = EnumItemType.Title,
                        Key = ___titles[j]
                    });
                }
                ___keycontrolItems.AddRange(canSearch ? _filteredSortedItems : sortedItems[j]);
            }
        }
        return false;
    }
}