using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures.Ship;

public record ShellCompatibility(string ShellName, Dictionary<string,  List<string>> CompatibleHullArtilleryModulesCombo);
