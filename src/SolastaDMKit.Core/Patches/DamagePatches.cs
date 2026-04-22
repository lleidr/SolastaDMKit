using HarmonyLib;
using SolastaDMKit.Core.Events;

namespace SolastaDMKit.Core.Patches;

[HarmonyPatch(typeof(RulesetActor), nameof(RulesetActor.InflictDamage))]
internal static class RulesetActorInflictDamagePatch
{
    internal static bool Prefix(
        ref int rolledDamage,
        string damageType,
        RulesetImplementationDefinitions.ApplyFormsParams formsParams,
        RollInfo rollInfo)
    {
        var evt = new DamageAbout
        {
            Target = formsParams.targetCharacter,
            RolledDamage = rolledDamage,
            DamageType = damageType,
            FormsParams = formsParams,
            RollInfo = rollInfo,
        };
        EventBus.Publish(evt);

        if (evt.OverrideAmount.HasValue)
        {
            rolledDamage = evt.OverrideAmount.Value;
        }

        return !evt.Skip;
    }

    internal static void Postfix(
        int rolledDamage,
        string damageType,
        RulesetImplementationDefinitions.ApplyFormsParams formsParams)
    {
        EventBus.Publish(new DamageApplied
        {
            Target = formsParams.targetCharacter,
            RolledDamage = rolledDamage,
            DamageType = damageType,
            FormsParams = formsParams,
        });
    }
}
