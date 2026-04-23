using System.Collections.Generic;
using System.Linq;
using SolastaDMKit.Core.Diagnostics;

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

    public static bool RollSkillCheck(GameLocationCharacter character, string skillName, int dc)
    {
        if (character?.RulesetCharacter == null)
        {
            SxLog.Info("SxCharacters.RollSkillCheck: null character or ruleset");
            return false;
        }

        if (!SkillToAbility.TryGetValue(skillName, out var abilityScoreName))
        {
            SxLog.Error($"SxCharacters.RollSkillCheck: unknown skill '{skillName}'");
            return false;
        }

        var ruleset = character.RulesetCharacter;
        var checkModifier = new ActionModifier();
        var bonus = ruleset.ComputeBaseAbilityCheckBonus(
            abilityScoreName,
            checkModifier.AbilityCheckModifierTrends,
            skillName);

        ruleset.RollAbilityCheck(
            bonus,
            abilityScoreName,
            skillName,
            checkModifier.AbilityCheckModifierTrends,
            checkModifier.AbilityCheckAdvantageTrends,
            checkModifier.AbilityCheckModifier,
            dc,
            false,
            0,
            out _,
            out _,
            out _,
            out var outcome,
            out _,
            true,
            true,
            true);

        return outcome == RuleDefinitions.RollOutcome.Success
               || outcome == RuleDefinitions.RollOutcome.CriticalSuccess;
    }
}
