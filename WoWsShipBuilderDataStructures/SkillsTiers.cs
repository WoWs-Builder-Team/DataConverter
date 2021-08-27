using System.Collections.Generic;

public class SkillsTiers // collection of all the skills and their tier for each class
{
    public Dictionary<int, List<int>> Cruiser { get; set; }
    public Dictionary<int, List<int>> Auxiliary { get; set; }
    public Dictionary<int, List<int>> Destroyer { get; set; }
    public Dictionary<int, List<int>> AirCarrier { get; set; }
    public Dictionary<int, List<int>> Submarine { get; set; }
    public Dictionary<int, List<int>> Battleship { get; set; }
}