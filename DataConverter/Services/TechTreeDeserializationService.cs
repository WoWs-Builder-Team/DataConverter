using System.Collections.Generic;
using System.Xml;

namespace DataConverter.Services;

internal class TechTreeDeserializationService : ITechTreeDeserializationService
{
    public Dictionary<long, int> ReadTechTreeFile(string path)
    {
        Dictionary<long, int> positions = new();
        using var reader = XmlReader.Create(path);
        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            if (!reader.Name.Equals("ship") || !reader.HasAttributes)
            {
                continue;
            }

            long shipId = -1;
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "shipid":
                    {
                        if (long.TryParse(reader.Value, out shipId))
                        {
                            positions.TryAdd(shipId, -1);
                        }

                        break;
                    }
                    case "x" when shipId != -1:
                    {
                        if (int.TryParse(reader.Value, out var techTreeHorizontalPosition))
                        {
                            positions[shipId] = techTreeHorizontalPosition;
                            shipId = -1;
                        }

                        break;
                    }
                }
            }
        }

        return positions;
    }
}
