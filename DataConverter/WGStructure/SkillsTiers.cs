using System.Collections.Generic;
using Newtonsoft.Json;

//public class SkillsTiers // collection of all the skills and their tier for each class
//{
//    public Dictionary<int, List<int>> Cruiser { get; set; }
//    public Dictionary<int, List<int>> Auxiliary { get; set; }
//    public Dictionary<int, List<int>> Destroyer { get; set; }
//    public Dictionary<int, List<int>> AirCarrier { get; set; }
//    public Dictionary<int, List<int>> Submarine { get; set; }
//    public Dictionary<int, List<int>> Battleship { get; set; }
//}


public class SkillsTiers
{
    public List<SkillRow> Cruiser { get; set; }
    public List<SkillRow> Auxiliary { get; set; }
    public List<SkillRow> Destroyer { get; set; }
    public List<SkillRow> AirCarrier { get; set; }
    public List<SkillRow> Submarine { get; set; }
    public List<SkillRow> Battleship { get; set; }
}

public class SkillRow
{
    [JsonProperty("0")]
    public List<int> First { get; set; }

    [JsonProperty("1")]
    public List<int> Second { get; set; }
}

