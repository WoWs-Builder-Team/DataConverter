using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WoWsShipBuilder.DataStructures.Modifiers;

public class Modifier
{
    [JsonConstructor]
    public Modifier(string location, string name, float value, string? gameLocalizationKey, string? appLocalizationKey, Unit unit, HashSet<string> affectedProperties, DisplayValueProcessingKind displayedValueProcessingKind, ValueProcessingKind valueProcessingKind)
    {
        Location = location;
        Name = name;
        Value = value;
        GameLocalizationKey = gameLocalizationKey;
        AppLocalizationKey = appLocalizationKey;
        Unit = unit;
        AffectedProperties = affectedProperties;
        DisplayedValueProcessingKind = displayedValueProcessingKind;
        ValueProcessingKind = valueProcessingKind;
    }

    public Modifier(string name, float value, string location, Modifier? modifierData)
    {
        Name = name;
        Value = value;
        Location = location;
        if (modifierData is not null)
        {
            GameLocalizationKey = modifierData.GameLocalizationKey;
            AppLocalizationKey = modifierData.AppLocalizationKey;
            Unit = modifierData.Unit;
            AffectedProperties = modifierData.AffectedProperties;
            DisplayedValueProcessingKind = modifierData.DisplayedValueProcessingKind;
            ValueProcessingKind = modifierData.ValueProcessingKind;
        }
    }

    [JsonIgnore]
    public string Location { get; }
    public string Name { get; }
    public float Value { get; }

    // ONLY one of the following two need to have a value, based on where the localization key needs to be searched in.
    public string? GameLocalizationKey { get; internal set; }
    public string? AppLocalizationKey { get; internal set; }
    public Unit Unit { get; internal set; }

    // Format: DataContainerName.Property.AdditionalSelector
    // For example: ShellDataContainer.Damage.HE would identify a modifier that applies to the Damage parameter only for HE shells.
    // ShellDataContainer.Damage would identify a modifier that applies to the Damage parameter of all shells.
    public HashSet<string> AffectedProperties { get; } = new();
    public DisplayValueProcessingKind DisplayedValueProcessingKind { get; internal set; }
    public ValueProcessingKind ValueProcessingKind { get; internal set; }

    public string ToDisplayValue()
    {
        double modifierValue;
        switch (DisplayedValueProcessingKind)
        {
            case DisplayValueProcessingKind.None:
                return $"{Value}";
            case DisplayValueProcessingKind.ToNegative:
                return $"-{Value}";
            case DisplayValueProcessingKind.ToPositive:
                return $"+{Value}";
            case DisplayValueProcessingKind.Empty:
                return string.Empty;
            case DisplayValueProcessingKind.ToInt:
                return (int)Value > 0 ? $"+{(int)Value}" : $"{(int)Value}";
            case DisplayValueProcessingKind.NoneOrPercentage:
                return Value > 1 ? $"+{Value}" : $"+{Math.Round(Value * 100, 2)} %";
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
            case DisplayValueProcessingKind.DrainPercentage:
                modifierValue = Math.Round(((1 / Value) - 1) * 100, 2);
                return modifierValue > 0 ? $"+{modifierValue}" : $"{modifierValue}";
            case DisplayValueProcessingKind.DecimalRoundedPercentage:
                return Value > 1 ? $"+{Math.Round((Value - 1) * 100, 2)}" : $"-{Math.Round((1 - Value) * 100, 2)}";
            case DisplayValueProcessingKind.RoundedPercentage:
                modifierValue = Math.Round(Value * 100);
                return modifierValue > 0 ? $"+{modifierValue}" : $"{modifierValue}";
            case DisplayValueProcessingKind.BigWorldToKm:
                return $"{(Value * 30) / 1000}";
            case DisplayValueProcessingKind.BigWorldToKmDecimal:
                return $"{(Value * 30) / 1000:.##}";
            case DisplayValueProcessingKind.MeterToKm:
                return $"{Value / 1000}";
            case DisplayValueProcessingKind.NotAssigned:
            default:
                throw new ArgumentOutOfRangeException(null, $"DisplayedValueProcessing for the modifier {Name} is not valid or not assigned.");
        }
    }

    public decimal ApplyModifier(decimal baseValue)
    {
        switch (ValueProcessingKind)
        {
            case ValueProcessingKind.SumPercentage:
                return baseValue + (decimal)Value * 100;
            case ValueProcessingKind.SubtractPercentage:
                return baseValue - (decimal)Value / 100;
            case ValueProcessingKind.Multiplier:
                return baseValue * (decimal)Value;
            case ValueProcessingKind.PositiveMultiplier:
                return baseValue * (1 + ((decimal)Value / 100));
            case ValueProcessingKind.NegativeMultiplier:
                return baseValue * (1 - ((decimal)Value / 100));
            case ValueProcessingKind.DirectAdd:
                return baseValue + (decimal)Value;
            case ValueProcessingKind.None:
                return baseValue;
            case ValueProcessingKind.NotAssigned:
            default:
                throw new ArgumentOutOfRangeException(null, $"ApplyValueProcessing for the modifier {Name} is not valid or not assigned.");
        }
    }

    public int ApplyModifier(int baseValue)
    {
        switch (ValueProcessingKind)
        {
            case ValueProcessingKind.SumPercentage:
                return baseValue + (int)Value * 100;
            case ValueProcessingKind.SubtractPercentage:
                return baseValue - (int)Value / 100;
            case ValueProcessingKind.Multiplier:
                return baseValue * (int)Value;
            case ValueProcessingKind.PositiveMultiplier:
                return baseValue * (1 + ((int)Value / 100));
            case ValueProcessingKind.DirectAdd:
                return baseValue + (int)Value;
            case ValueProcessingKind.None:
                return baseValue;
            case ValueProcessingKind.NotAssigned:
            default:
                throw new ArgumentOutOfRangeException(null, $"ApplyValueProcessing for the modifier {Name} is not valid or not assigned.");
        }
    }
}
