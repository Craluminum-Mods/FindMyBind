using HarmonyLib;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;

namespace FindMyBind;

[HarmonyPatch(typeof(GuiCompositeSettings), "OnControlOptions")]
public static class AddSearchFieldToControlsTab
{
    public static string currentSearchText;
    public static bool filterByKey;

    [HarmonyPrefix]
    public static bool Prefix(GuiCompositeSettings __instance, IGameSettingsHandler ___handler, ref GuiComposer ___composer, ref bool ___mousecontrolsTabActive)
    {
        ___mousecontrolsTabActive = false;
        __instance.CallMethod("LoadKeyCombinations");
        ElementBounds configListBounds = ElementBounds.Fixed(0.0, 0.0, 900.0 - 2.0 * GuiStyle.ElementToDialogPadding - 35.0, 400.0);
        ElementBounds insetBounds = configListBounds.ForkBoundingParent(5.0, 5.0, 5.0, 5.0);
        ElementBounds clipBounds = configListBounds.FlatCopy().WithParent(insetBounds);
        ElementBounds scrollbarBounds = ElementStdBounds.VerticalScrollbar(insetBounds);
        ElementBounds leftText = ElementBounds.Fixed(0.0, 41.0, 360.0, 42.0);
        ElementBounds rightSlider;

        ___composer = GuiComposerHelpers.AddSwitch(bounds: rightSlider = ElementBounds.Fixed(490.0, 38.0, 200.0, 20.0).BelowCopy(0.0, 32.0), composer: __instance.CallMethod<GuiComposer>("ComposerHeader", "gamesettings-controls", "controls").AddStaticText(Lang.Get("setting-name-noseparatectrlkeys"), CairoFont.WhiteSmallishText(), leftText = leftText.BelowCopy(0.0, 10.0, 120.0)), onToggle: (on) => onSeparateCtrl(__instance, on), key: "separateCtrl");
        ___composer.AddHoverText(Lang.Get("setting-hover-noseparatectrlkeys"), CairoFont.WhiteSmallText(), 250, leftText.FlatCopy().WithFixedHeight(25.0));
        ___composer.AddStaticText(Lang.Get("keycontrols"), CairoFont.WhiteSmallishText(), leftText = leftText.BelowCopy(0.0, 5.0, -120.0));

        ___composer.AddTextInput(leftText = leftText.BelowCopy(0.0, 5.0), (text) => FilterItemsBySearchText(__instance, text), key: "searchField");
        ___composer.AddToggleButton(Lang.Get("keycontrols-filter-by-key"), CairoFont.WhiteSmallText(), (on) => FilterItemsByKey(__instance, on), leftText.RightCopy(fixedDeltaX: 10.0), key: "searchField-filter");

        ___composer.AddVerticalScrollbar((value) => OnNewScrollbarValue(__instance, value), scrollbarBounds.FixedUnder(leftText, 10.0), "scrollbar");
        ___composer.AddInset(insetBounds.FixedUnder(leftText, 10.0), 3, 0.8f);
        ___composer.BeginClip(clipBounds);
        ___composer.AddConfigList(__instance.GetField<List<ConfigItem>>("keycontrolItems"), (index, indexWithoutTitles) => OnKeyControlItemClick(__instance, index, indexWithoutTitles), CairoFont.WhiteSmallText().WithFontSize(18f), configListBounds, "configlist");
        ___composer.EndClip();
        ___composer.AddButton(Lang.Get("setting-name-setdefault"), () => OnResetControls(__instance), ElementStdBounds.MenuButton(0f, EnumDialogArea.LeftFixed).FixedUnder(insetBounds, 10.0).WithFixedPadding(10.0, 2.0));
        ___composer.AddIf(___handler.IsIngame);
        ___composer.AddButton(Lang.Get("setting-name-macroeditor"), () => OnMacroEditor(__instance), ElementStdBounds.MenuButton(0f, EnumDialogArea.RightFixed).FixedUnder(insetBounds, 10.0).WithFixedPadding(10.0, 2.0));
        ___composer.EndIf();
        ___composer.EndChildElements();
        ___composer.Compose();
        ___handler.LoadComposer(___composer);
        ___composer.GetSwitch("separateCtrl").SetValue(!ClientSettings.SeparateCtrl);
        GuiElementConfigList configlist = ___composer.GetConfigList("configlist");
        configlist.errorFont = configlist.stdFont.Clone();
        configlist.errorFont.Color = GuiStyle.ErrorTextColor;
        configlist.Bounds.CalcWorldBounds();
        clipBounds.CalcWorldBounds();
        __instance.CallMethod("ReLoadKeyCombinations");
        ___composer.GetScrollbar("scrollbar").SetHeights((float)clipBounds.fixedHeight, (float)configlist.innerBounds.fixedHeight);

        ___composer.GetTextInput("searchField").SetPlaceHolderText(Lang.Get("Search..."));
        ___composer.GetTextInput("searchField").SetValue("");
        filterByKey = false;
        return false;
    }

    static void FilterItemsByKey(GuiCompositeSettings instance, bool on)
    {
        filterByKey = on;
        FilterItemsBySearchText(instance, currentSearchText, true);
    }

    static void FilterItemsBySearchText(GuiCompositeSettings instance, string text, bool forceUpdate = false)
    {
        if (currentSearchText == text && forceUpdate == false)
        {
            return;
        }

        currentSearchText = text;
        instance.CallMethod("ReLoadKeyCombinations");
    }

    static void OnNewScrollbarValue(GuiCompositeSettings instance, float value) => instance.CallMethod("OnNewScrollbarValue", value);
    static bool OnMacroEditor(GuiCompositeSettings instance) => instance.CallMethod<bool>("OnMacroEditor");
    static bool OnResetControls(GuiCompositeSettings instance) => instance.CallMethod<bool>("OnResetControls");
    static void OnKeyControlItemClick(GuiCompositeSettings instance, int index, int indexWithoutTitles) => instance.CallMethod("OnKeyControlItemClick", index, indexWithoutTitles);
    static void onSeparateCtrl(GuiCompositeSettings instance, bool on) => instance.CallMethod("onSeparateCtrl", on);
}