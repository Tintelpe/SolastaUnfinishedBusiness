﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.CustomInterfaces;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

public abstract class AddExtraAttackBase : IAddExtraAttack
{
    protected readonly ActionDefinitions.ActionType ActionType;
    private readonly List<string> additionalTags = new();
    private readonly bool clearSameType;
    private readonly IsCharacterValidHandler[] validators;

    protected AddExtraAttackBase(
        ActionDefinitions.ActionType actionType,
        bool clearSameType,
        params IsCharacterValidHandler[] validators)
    {
        ActionType = actionType;
        this.clearSameType = clearSameType;
        this.validators = validators;
    }

    protected AddExtraAttackBase(
        ActionDefinitions.ActionType actionType,
        params IsCharacterValidHandler[] validators) :
        this(actionType, false, validators)
    {
    }

    public void TryAddExtraAttack(RulesetCharacterHero hero)
    {
        if (!hero.IsValid(validators))
        {
            return;
        }

        var attackModes = hero.AttackModes;

        if (clearSameType)
        {
            for (var i = attackModes.Count - 1; i > 0; i--)
            {
                var mode = attackModes[i];

                if (mode.ActionType != ActionType)
                {
                    continue;
                }

                RulesetAttackMode.AttackModesPool.Return(mode);
                attackModes.RemoveAt(i);
            }
        }

        var newAttacks = GetAttackModes(hero);

        if (newAttacks == null || newAttacks.Empty())
        {
            return;
        }

        foreach (var attackMode in newAttacks)
        {
            foreach (var tag in additionalTags)
            {
                attackMode.AddAttackTagAsNeeded(tag);
            }

            var same = attackModes.FirstOrDefault(m => ModesEqual(attackMode, m));
            if (same != null)
            {
                //If same attack mode exists, ensure it has max amount of attacks
                same.attacksNumber = Math.Max(attackMode.attacksNumber, same.attacksNumber);
                //and dispose of newly created one
                RulesetAttackMode.AttackModesPool.Return(attackMode);
            }
            else
            {
                attackModes.Add(attackMode);
            }
        }
    }

    [NotNull]
    public AddExtraAttackBase SetTags([NotNull] params string[] tags)
    {
        additionalTags.AddRange(tags);

        return this;
    }

    protected abstract List<RulesetAttackMode> GetAttackModes(RulesetCharacterHero hero);

    //Copied from RulesetAttackMode.IsComparableForNetwork, but not checking for attack number
    private static bool ModesEqual([NotNull] RulesetAttackMode a, RulesetAttackMode b)
    {
        //added all these locals for debug log
        var actionType = a.actionType == b.actionType;
        var sourceDefinition = a.sourceDefinition == b.sourceDefinition;
        var sourceObject = a.sourceObject == b.sourceObject;
        var slotName = a.slotName == b.slotName;
        var ranged = a.ranged == b.ranged;
        var thrown = a.thrown == b.thrown;
        var reach = a.reach == b.reach;
        var reachRange = a.reachRange == b.reachRange;
        var closeRange = a.closeRange == b.closeRange;
        var maxRange = a.maxRange == b.maxRange;
        var toHitBonus = a.toHitBonus == b.toHitBonus;
        //var attacksNumber = a.attacksNumber == b.attacksNumber;
        var useVersatileDamage = a.useVersatileDamage == b.useVersatileDamage;
        var freeOffHand = a.freeOffHand == b.freeOffHand;
        var automaticHit = a.automaticHit == b.automaticHit;
        var afterChargeOnly = a.afterChargeOnly == b.afterChargeOnly;

        if (ValidatorsWeapon.IsUnarmedWeapon(a) && ValidatorsWeapon.IsUnarmedWeapon(b))
        {
            // Main.Log(
            //     $"EQUAL actionType:{actionType}, sourceDefinition: {sourceDefinition}, sourceObject: {sourceObject}, slotName: {slotName}, ranged: {ranged}, thrown: {thrown}, reach: {reach}, reachRange: {reachRange}, closeRange: {closeRange}, maxRange: {maxRange}, toHitBonus: {toHitBonus}, attacksNumber: {attacksNumber}, useVersatileDamage: {useVersatileDamage}, freeOffHand: {freeOffHand}, automaticHit: {automaticHit}, afterChargeOnly: {afterChargeOnly}");
        }

        return actionType
               && sourceDefinition
               && sourceObject
               && slotName
               && ranged
               && thrown
               && reach
               && reachRange
               && closeRange
               && maxRange
               && toHitBonus
               // && attacksNumber
               && useVersatileDamage
               && freeOffHand
               && automaticHit
               && afterChargeOnly;
    }
}

public sealed class AddExtraUnarmedAttack : AddExtraAttackBase
{
    public AddExtraUnarmedAttack(
        ActionDefinitions.ActionType actionType,
        bool clearSameType,
        params IsCharacterValidHandler[] validators) : base(actionType, clearSameType, validators)
    {
    }

    public AddExtraUnarmedAttack(ActionDefinitions.ActionType actionType, params IsCharacterValidHandler[] validators) :
        base(actionType, validators)
    {
    }

    [NotNull]
    protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacterHero hero)
    {
        var mainHandItem = hero.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand]
            .EquipedItem;

        var isUnarmedWeapon = mainHandItem != null && ValidatorsWeapon.IsUnarmedWeapon(mainHandItem);
        var strikeDefinition = isUnarmedWeapon
            ? mainHandItem.ItemDefinition
            : hero.UnarmedStrikeDefinition;

        var attackModifiers = hero.attackModifiers;


        var attackMode = hero.RefreshAttackMode(
            ActionType,
            strikeDefinition,
            strikeDefinition.WeaponDescription,
            ValidatorsCharacter.IsFreeOffhandForUnarmedTa(hero),
            true,
            EquipmentDefinitions.SlotTypeMainHand,
            attackModifiers,
            hero.FeaturesOrigin,
            isUnarmedWeapon ? mainHandItem : null
        );

        return new List<RulesetAttackMode> { attackMode };
    }
}

// public sealed class AddExtraMainHandAttack : AddExtraAttackBase
// {
//     public AddExtraMainHandAttack(
//         ActionDefinitions.ActionType actionType,
//         bool clearSameType,
//         params CharacterValidator[] validators) : base(actionType, clearSameType, validators)
//     {
//     }
//
//     public AddExtraMainHandAttack(ActionDefinitions.ActionType actionType, params CharacterValidator[] validators) :
//         base(actionType, validators)
//     {
//     }
//
//     [NotNull]
//     protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacterHero hero)
//     {
//         var mainHandItem = hero.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand]
//             .EquipedItem;
//
//         var strikeDefinition = mainHandItem.ItemDefinition;
//
//         var attackModifiers = hero.attackModifiers;
//
//         var attackMode = hero.RefreshAttackModePublic(
//             ActionType,
//             strikeDefinition,
//             strikeDefinition.WeaponDescription,
//             CharacterValidators.IsFreeOffhand(hero),
//             true,
//             EquipmentDefinitions.SlotTypeMainHand,
//             attackModifiers,
//             hero.FeaturesOrigin,
//             mainHandItem
//         );
//
//         return new List<RulesetAttackMode> { attackMode };
//     }
// }

public sealed class AddExtraRangedAttack : AddExtraAttackBase
{
    private readonly IsWeaponValidHandler weaponValidator;

    public AddExtraRangedAttack(
        IsWeaponValidHandler weaponValidator,
        ActionDefinitions.ActionType actionType,
        params IsCharacterValidHandler[] validators) : base(actionType, validators)
    {
        this.weaponValidator = weaponValidator;
    }

    [NotNull]
    protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacterHero hero)
    {
        var result = new List<RulesetAttackMode>();

        AddItemAttack(result, EquipmentDefinitions.SlotTypeMainHand, hero);
        AddItemAttack(result, EquipmentDefinitions.SlotTypeOffHand, hero);

        return result;
    }

    private void AddItemAttack(
        ICollection<RulesetAttackMode> attackModes,
        [NotNull] string slot,
        [NotNull] RulesetCharacterHero hero)
    {
        var item = hero.CharacterInventory.InventorySlotsByName[slot].EquipedItem;

        if (item == null || !weaponValidator.Invoke(null, item, hero))
        {
            return;
        }

        var strikeDefinition = item.ItemDefinition;
        var attackMode = hero.RefreshAttackMode(
            ActionType,
            strikeDefinition,
            strikeDefinition.WeaponDescription,
            ValidatorsCharacter.IsFreeOffhand(hero),
            true,
            slot,
            hero.attackModifiers,
            hero.FeaturesOrigin,
            item
        );

        attackMode.Reach = false;
        attackMode.Ranged = true;
        attackMode.Thrown = ValidatorsWeapon.IsThrownWeapon(item);
        attackMode.AttackTags.Remove(TagsDefinitions.WeaponTagMelee);

        attackModes.Add(attackMode);
    }
}

public sealed class AddPolearmFollowupAttack : AddExtraAttackBase
{
    public AddPolearmFollowupAttack() : base(ActionDefinitions.ActionType.Bonus, false,
        ValidatorsCharacter.HasAttacked, ValidatorsCharacter.HasPolearm)
    {
    }

    [NotNull]
    protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacterHero hero)
    {
        var result = new List<RulesetAttackMode>();

        AddItemAttack(result, EquipmentDefinitions.SlotTypeMainHand, hero);
        AddItemAttack(result, EquipmentDefinitions.SlotTypeOffHand, hero);

        return result;
    }

    private void AddItemAttack(
        ICollection<RulesetAttackMode> attackModes,
        [NotNull] string slot,
        [NotNull] RulesetCharacterHero hero)
    {
        var item = hero.CharacterInventory.InventorySlotsByName[slot].EquipedItem;

        if (item == null || !ValidatorsWeapon.IsPolearm(item))
        {
            return;
        }

        var strikeDefinition = item.ItemDefinition;
        var attackMode = hero.RefreshAttackMode(
            ActionType,
            strikeDefinition,
            strikeDefinition.WeaponDescription,
            ValidatorsCharacter.IsFreeOffhand(hero),
            true,
            slot,
            hero.attackModifiers,
            hero.FeaturesOrigin,
            item
        );

        attackMode.Reach = true;
        attackMode.Ranged = false;
        attackMode.Thrown = false;

        var damage = DamageForm.GetCopy(attackMode.EffectDescription.FindFirstDamageForm());

        damage.DieType = DieType.D4;
        damage.VersatileDieType = DieType.D4;
        damage.versatile = false;
        damage.DiceNumber = 1;
        damage.DamageType = DamageTypeBludgeoning;

        var effectForm = EffectForm.Get();

        effectForm.FormType = EffectForm.EffectFormType.Damage;
        effectForm.DamageForm = damage;
        attackMode.EffectDescription.Clear();
        attackMode.EffectDescription.EffectForms.Add(effectForm);

        attackModes.Add(attackMode);
    }
}

public sealed class AddBonusShieldAttack : AddExtraAttackBase
{
    public AddBonusShieldAttack() : base(ActionDefinitions.ActionType.Bonus, false)
    {
    }

    [CanBeNull]
    protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacterHero hero)
    {
        var inventorySlotsByName = hero.CharacterInventory.InventorySlotsByName;
        var offHandItem = inventorySlotsByName[EquipmentDefinitions.SlotTypeOffHand].EquipedItem;

        if (!ShieldStrike.IsShield(offHandItem))
        {
            return null;
        }

        var attackModifiers = hero.attackModifiers;
        var attackMode = hero.RefreshAttackMode(
            ActionDefinitions.ActionType.Bonus,
            offHandItem.ItemDefinition,
            ShieldStrike.ShieldWeaponDescription,
            ValidatorsCharacter.IsFreeOffhand(hero),
            hero.CanAddAbilityBonusToOffhand(),
            EquipmentDefinitions.SlotTypeOffHand,
            attackModifiers,
            hero.FeaturesOrigin,
            offHandItem
        );

        var features = new List<FeatureDefinition>();

        offHandItem.EnumerateFeaturesToBrowse<FeatureDefinitionAttributeModifier>(features);

        var bonus = (from modifier in features.OfType<FeatureDefinitionAttributeModifier>()
            where modifier.ModifiedAttribute == AttributeDefinitions.ArmorClass
            where modifier.ModifierOperation == FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive
            select modifier.ModifierValue).Sum();

        if (bonus == 0)
        {
            return new List<RulesetAttackMode> { attackMode };
        }

        var damage = attackMode.EffectDescription?.FindFirstDamageForm();
        var trendInfo = new TrendInfo(bonus, FeatureSourceType.Equipment, offHandItem.Name, null);

        attackMode.ToHitBonus += bonus;
        attackMode.ToHitBonusTrends.Add(trendInfo);

        if (damage == null)
        {
            return new List<RulesetAttackMode> { attackMode };
        }

        damage.BonusDamage += bonus;
        damage.DamageBonusTrends.Add(trendInfo);

        return new List<RulesetAttackMode> { attackMode };
    }
}
