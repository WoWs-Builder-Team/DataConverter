using System;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.DataStructures;

public static class DataExtensions
{
    public static AntiAirAura AddAura(this AntiAirAura first, AntiAirAura second)
    {
        if (first.MaxRange > 0 && (first.MaxRange != second.MaxRange || first.HitChance != second.HitChance))
        {
            throw new InvalidOperationException("Cannot combine auras with different ranges or accuracies.");
        }

        decimal minRange = second.FlakDamage > 0 ? first.MinRange : second.MinRange; // Use minimum range of new aura only if it is no flak cloud aura

        return new AntiAirAura
        {
            ConstantDps = first.ConstantDps + second.ConstantDps,
            FlakDamage = second.FlakDamage,
            FlakCloudsNumber = first.FlakCloudsNumber + second.FlakCloudsNumber,
            HitChance = second.HitChance,
            MaxRange = second.MaxRange,
            MinRange = minRange,
        };
    }
}
