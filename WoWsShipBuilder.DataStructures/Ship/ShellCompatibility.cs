using System.Collections.Immutable;

namespace WoWsShipBuilder.DataStructures.Ship;

public record ShellCompatibility(string ShellName, ImmutableDictionary<string,  ImmutableList<string>> CompatibleHullArtilleryModulesCombo);
