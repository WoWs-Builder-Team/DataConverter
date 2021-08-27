using System.Collections.Generic;

public class SkillsTiers // collection of all the skills and their tier for each class
{
    public Dictionary<int, int> Cruiser { get; set; }
    public Dictionary<int, int> Auxiliary { get; set; }
    public Dictionary<int, int> Destroyer { get; set; }
    public Dictionary<int, int> AirCarrier { get; set; }
    public Dictionary<int, int> Submarine { get; set; }
    public Dictionary<int, int> Battleship { get; set; }
}