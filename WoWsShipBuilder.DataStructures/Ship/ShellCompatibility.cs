using System.Collections.Generic;
using System.Linq;

namespace WoWsShipBuilder.DataStructures.Ship;

public record ShellCompatibility(string ShellName, Dictionary<string,  IOrderedEnumerable<string>> CompatibleHullArtilleryModulesCombo);
