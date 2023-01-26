using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures.Ship;

public record ShellCompatibility(string ShellName, IEnumerable<string> CompatibleArtilleryModules, IEnumerable<string> CompatibleHullModules);
