﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

public static class SpellSelectionPanelPatcher
{
    [HarmonyPatch(typeof(SpellSelectionPanel), "Bind")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class Bind_Patch
    {
        public static void Prefix(
            GuiCharacter caster,
            ref bool cantripOnly,
            ActionDefinitions.ActionType actionType)
        {
            //PATCH: supports `IReplaceAttackWithCantrip`
            var gameLocationCaster = caster.GameLocationCharacter;

            if (gameLocationCaster.RulesetCharacter.HasSubFeatureOfType<IReplaceAttackWithCantrip>()
                && gameLocationCaster.UsedMainAttacks > 0 && actionType == ActionDefinitions.ActionType.Main)
            {
                cantripOnly = true;
            }
        }

        public static void Postfix(
            SpellSelectionPanel __instance,
            GuiCharacter caster,
            SpellsByLevelBox.SpellCastEngagedHandler spellCastEngaged,
            ActionDefinitions.ActionType actionType,
            bool cantripOnly)
        {
            //PATCH: shows spell selection on multiple rows
            if (!Main.Settings.EnableMultiLineSpellPanel)
            {
                return;
            }

            GameUiContext.SpellSelectionPanelMultilineBind(
                __instance, caster, spellCastEngaged, actionType, cantripOnly);
        }

        [NotNull]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: hide spell panels for repertoires that have hidden spell casting feature
            var getRepertoires = typeof(RulesetCharacter).GetMethod("get_SpellRepertoires");
            var getVisiblerepertoires = new Func<RulesetCharacter, List<RulesetSpellRepertoire>>(GetRepertoires).Method;

            return instructions.ReplaceCalls(getRepertoires, "SpellSelectionPanel.Bind",
                new CodeInstruction(OpCodes.Call, getVisiblerepertoires));
        }

        private static List<RulesetSpellRepertoire> GetRepertoires(RulesetCharacter character)
        {
            return character.SpellRepertoires
                .Where(r => !r.SpellCastingFeature.GuiPresentation.Hidden)
                .ToList();
        }
    }

    [HarmonyPatch(typeof(SpellSelectionPanel), "Unbind")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class Unbind_Patch
    {
        public static void Postfix()
        {
            //PATCH: shows spell selection on multiple rows
            if (!Main.Settings.EnableMultiLineSpellPanel)
            {
                return;
            }

            GameUiContext.SpellSelectionPanelMultilineUnbind();
        }
    }
}
