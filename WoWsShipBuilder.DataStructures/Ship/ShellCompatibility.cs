using System.Collections.Immutable;

namespace WoWsShipBuilder.DataStructures.Ship;

public sealed record ShellCompatibility(string ShellName, ImmutableDictionary<string,  ImmutableList<string>> CompatibleHullArtilleryModulesCombo);
