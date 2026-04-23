using System.Collections.Generic;
using System.Linq;
using SolastaDMKit.Core.Diagnostics;
using TA;

namespace SolastaDMKit.Core.Runtime;

public static class SxCharacters
{
    private static readonly Dictionary<string, string> SkillToAbility = new()
    {
        { "Acrobatics", "Dexterity" },
        { "AnimalHandling", "Wisdom" },
        { "Arcana", "Intelligence" },
        { "Athletics", "Strength" },
        { "Deception", "Charisma" },
        { "History", "Intelligence" },
        { "Insight", "Wisdom" },
        { "Intimidation", "Charisma" },
        { "Investigation", "Intelligence" },
        { "Medecine", "Wisdom" },
        { "Nature", "Intelligence" },
        { "Perception", "Wisdom" },
        { "Performance", "Charisma" },
        { "Persuasion", "Charisma" },
        { "Religion", "Intelligence" },
        { "SleightOfHand", "Dexterity" },
        { "Stealth", "Dexterity" },
        { "Survival", "Wisdom" },
    };

    public static GameLocationCharacter FindByName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        var service = ServiceRepository.GetService<IGameLocationCharacterService>();
        if (service == null)
        {
            return null;
        }

        return service.PartyCharacters
            .Concat(service.GuestCharacters)
            .FirstOrDefault(c => c.Name == name || c.RulesetCharacter?.Name == name);
    }

    public static string GetName(GameLocationCharacter character)
    {
        return character?.RulesetCharacter?.Name ?? character?.Name ?? "(unknown)";
    }

    public static SxCheckResult RollSkillCheck(GameLocationCharacter character, string skillName, int dc)
    {
        if (character?.RulesetCharacter == null)
        {
            SxLog.Info("SxCharacters.RollSkillCheck: null character or ruleset");
            return SxCheckResult.Invalid(dc);
        }

        if (!SkillToAbility.TryGetValue(skillName, out var abilityScoreName))
        {
            SxLog.Error($"SxCharacters.RollSkillCheck: unknown skill '{skillName}'");
            return SxCheckResult.Invalid(dc);
        }

        var ruleset = character.RulesetCharacter;
        var checkModifier = new ActionModifier();
        var bonus = ruleset.ComputeBaseAbilityCheckBonus(
            abilityScoreName,
            checkModifier.AbilityCheckModifierTrends,
            skillName);

        var total = ruleset.RollAbilityCheck(
            bonus,
            abilityScoreName,
            skillName,
            checkModifier.AbilityCheckModifierTrends,
            checkModifier.AbilityCheckAdvantageTrends,
            checkModifier.AbilityCheckModifier,
            dc,
            false,
            0,
            out var rawRoll,
            out _,
            out _,
            out var outcome,
            out _,
            true,
            true,
            true);

        var success = outcome == RuleDefinitions.RollOutcome.Success
                      || outcome == RuleDefinitions.RollOutcome.CriticalSuccess;

        return new SxCheckResult(
            success,
            rawRoll,
            total,
            dc,
            outcome == RuleDefinitions.RollOutcome.CriticalSuccess,
            outcome == RuleDefinitions.RollOutcome.CriticalFailure);
    }

    public static bool AddCondition(
        GameLocationCharacter target,
        string conditionName,
        int durationRounds = 1,
        GameLocationCharacter source = null)
    {
        if (target?.RulesetCharacter == null)
        {
            SxLog.Error("SxCharacters.AddCondition: null target");
            return false;
        }

        if (string.IsNullOrEmpty(conditionName))
        {
            SxLog.Error("SxCharacters.AddCondition: empty conditionName");
            return false;
        }

        var sourceRuleset = source?.RulesetCharacter ?? target.RulesetCharacter;

        target.RulesetCharacter.InflictCondition(
            conditionName,
            RuleDefinitions.DurationType.Round,
            durationRounds,
            RuleDefinitions.TurnOccurenceType.StartOfTurn,
            AttributeDefinitions.TagEffect,
            sourceRuleset.Guid,
            sourceRuleset.CurrentFaction?.Name ?? string.Empty,
            1,
            conditionName,
            0,
            0,
            0);

        return true;
    }

    public static int RemoveCondition(GameLocationCharacter target, string conditionName)
    {
        if (target?.RulesetCharacter == null || string.IsNullOrEmpty(conditionName))
        {
            return 0;
        }

        var toRemove = new List<RulesetCondition>();
        foreach (var category in target.RulesetCharacter.ConditionsByCategory.Values)
        {
            foreach (var cond in category)
            {
                if (cond.ConditionDefinition?.Name == conditionName)
                {
                    toRemove.Add(cond);
                }
            }
        }

        foreach (var cond in toRemove)
        {
            target.RulesetCharacter.RemoveCondition(cond);
        }

        return toRemove.Count;
    }

    public static bool TeleportTo(
        GameLocationCharacter character,
        int3 position,
        LocationDefinitions.Orientation orientation = LocationDefinitions.Orientation.North)
    {
        if (character == null)
        {
            return false;
        }

        var service = ServiceRepository.GetService<IGameLocationPositioningService>();
        if (service == null)
        {
            SxLog.Error("SxCharacters.TeleportTo: no positioning service");
            return false;
        }

        service.TeleportCharacter(character, position, orientation);
        return true;
    }
}
