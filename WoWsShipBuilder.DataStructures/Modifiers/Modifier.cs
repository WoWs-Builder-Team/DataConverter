using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace WoWsShipBuilder.DataStructures.Modifiers;

[PublicAPI]
public sealed class Modifier
{
    [JsonConstructor]
    public Modifier(string name, float value, string? gameLocalizationKey, string? appLocalizationKey, Unit unit, ImmutableHashSet<string> affectedProperties, DisplayValueProcessingKind displayValueProcessingKind, ValueProcessingKind valueProcessingKind)
    {
        Location = string.Empty;
        Name = name;
        Value = value;
        GameLocalizationKey = gameLocalizationKey;
        AppLocalizationKey = appLocalizationKey;
        Unit = unit;
        AffectedProperties = affectedProperties;
        DisplayValueProcessingKind = displayValueProcessingKind;
        ValueProcessingKind = valueProcessingKind;
    }

    public Modifier(string name, float value, string location, Modifier? modifierData)
    {
        Name = name;
        Value = value;
        Location = location;
        if (modifierData is null)
        {
            return;
        }

        GameLocalizationKey = modifierData.GameLocalizationKey;
        AppLocalizationKey = modifierData.AppLocalizationKey;
        Unit = modifierData.Unit;
        AffectedProperties = modifierData.AffectedProperties;
        DisplayValueProcessingKind = modifierData.DisplayValueProcessingKind;
        ValueProcessingKind = modifierData.ValueProcessingKind;
    }

    public string Name { get; }

    public float Value { get; }

    // ONLY one of the following two need to have a value, based on where the localization key needs to be searched in.
    public string? GameLocalizationKey { get; internal set; }

    public string? AppLocalizationKey { get; internal set; }

    public Unit Unit { get; internal set; }

    /// <summary>
    /// The properties that this modifier affects.
    /// </summary>
    /// <remarks>
    /// Format: DataContainerName.Property.AdditionalSelector
    /// For example: ShellDataContainer.Damage.HE would identify a modifier that applies to the Damage parameter only for HE shells.
    /// ShellDataContainer.Damage would identify a modifier that applies to the Damage parameter of all shells.
    /// </remarks>
    public ImmutableHashSet<string> AffectedProperties { get; } = ImmutableHashSet<string>.Empty;

    public DisplayValueProcessingKind DisplayValueProcessingKind { get; internal set; }

    public ValueProcessingKind ValueProcessingKind { get; internal set; }

    [JsonIgnore]
    internal string Location { get; }

    [PublicAPI]
    public string ToDisplayValue()
    {
        double modifierValue;
        switch (DisplayValueProcessingKind)
        {
            case DisplayValueProcessingKind.Raw:
                return $"{Value}";
            case DisplayValueProcessingKind.ToNegative:
                return $"-{Value}";
            case DisplayValueProcessingKind.ToPositive:
                return $"+{Value}";
            case DisplayValueProcessingKind.Discard:
                return string.Empty;
            case DisplayValueProcessingKind.ToInt:
                return (int)Value > 0 ? $"+{(int)Value}" : $"{(int)Value}";
            case DisplayValueProcessingKind.RawOrPercentage:
                return Value > 1 ? $"+{Value}" : $"+{Math.Round(Value * 100, 2)}";
            case DisplayValueProcessingKind.PositivePercentage:
                return $"+{(Value - 1) * 100}";
            case DisplayValueProcessingKind.NegativePercentage:
                return $"-{(int)Math.Round(Value * 100)}";
            case DisplayValueProcessingKind.InverseNegativePercentage:
                return $"-{Math.Round((1 - Value) * 100)}";
            case DisplayValueProcessingKind.VariablePercentage:
                modifierValue = Math.Round(Value * 100, 2) - 100;
                return modifierValue > 0 ? $"+{modifierValue}" : $"{modifierValue}";
            case DisplayValueProcessingKind.IntVariablePercentage:
                modifierValue = (int)(Math.Round(Value * 100, 2) - 100);
                return modifierValue > 0 ? $"+{modifierValue}" : $"{modifierValue}";
            case DisplayValueProcessingKind.DecimalRoundedPercentage:
                return Value > 1 ? $"+{Math.Round((Value - 1) * 100, 2)}" : $"-{Math.Round((1 - Value) * 100, 2)}";
            case DisplayValueProcessingKind.RoundedPercentage:
                modifierValue = Math.Round(Value * 100);
                return modifierValue > 0 ? $"+{modifierValue}" : $"{modifierValue}";
            case DisplayValueProcessingKind.BigWorldToKm:
                return $"{(Value * 30) / 1000}";
            case DisplayValueProcessingKind.BigWorldToKmDecimal:
                return $"{(Value * 30) / 1000:#0.##}";
            case DisplayValueProcessingKind.MeterToKm:
                return $"{Value / 1000}";
            case DisplayValueProcessingKind.NotAssigned:
            default:
                throw new ArgumentOutOfRangeException(null, $"DisplayValueProcessing for the modifier {Name} is not valid or not assigned.");
        }
    }

    [PublicAPI]
    public decimal ApplyModifier(decimal baseValue)
    {
        switch (ValueProcessingKind)
        {
            case ValueProcessingKind.AddPercentage:
                return baseValue + (decimal)Value * 100;
            case ValueProcessingKind.SubtractPercentage:
                return baseValue - (decimal)Value / 100;
            case ValueProcessingKind.Multiplier:
                return baseValue * (decimal)Value;
            case ValueProcessingKind.PositiveMultiplier:
                return baseValue * (1 + ((decimal)Value / 100));
            case ValueProcessingKind.NegativeMultiplier:
                return baseValue * (1 - ((decimal)Value / 100));
            case ValueProcessingKind.RawAdd:
                return baseValue + (decimal)Value;
            case ValueProcessingKind.None:
                return baseValue;
            case ValueProcessingKind.NotAssigned:
            default:
                throw new ArgumentOutOfRangeException(null, $"ApplyValueProcessing for the modifier {Name} is not valid or not assigned.");
        }
    }

    [PublicAPI]
    public int ApplyModifier(int baseValue)
    {
        switch (ValueProcessingKind)
        {
            case ValueProcessingKind.AddPercentage:
                return baseValue + (int)Value * 100;
            case ValueProcessingKind.SubtractPercentage:
                return baseValue - (int)Value / 100;
            case ValueProcessingKind.Multiplier:
                return baseValue * (int)Value;
            case ValueProcessingKind.PositiveMultiplier:
                return baseValue * (1 + ((int)Value / 100));
            case ValueProcessingKind.RawAdd:
                return baseValue + (int)Value;
            case ValueProcessingKind.None:
                return baseValue;
            case ValueProcessingKind.NotAssigned:
            default:
                throw new ArgumentOutOfRangeException(null, $"ApplyValueProcessing for the modifier {Name} is not valid or not assigned.");
        }
    }
}
