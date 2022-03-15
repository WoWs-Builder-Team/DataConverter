// soource for the custom unpickler: https://github.com/CruleD/WoWs-GameParams-Dumper/blob/master/WoWs%20GameParams%20Dumper/CustomUnpicklerClass.cs

using System.Collections;
using Razorvine.Pickle;

namespace WowsShipBuilder.GameParamsExtractor;

public class PythonDictionary : Dictionary<string, object>
{
    public void __setstate__(Hashtable values)
    {
        Clear();
        if (values.ContainsKey("damageDistribution"))
        {
            var hashtableTemp = (Hashtable)values["damageDistribution"]!;
            var hashtableTempNew = new Hashtable();
            foreach (DictionaryEntry dictionaryEntryTemp in hashtableTemp)
            {
                hashtableTempNew.Add(dictionaryEntryTemp.Value!, dictionaryEntryTemp.Key);
            }
            values["damageDistribution"] = hashtableTempNew;
        }
        foreach (string x in values.Keys)
        {
            Add(x, values[x]!);
        }
    }
}

public class PythonDictionaryConstructor : IObjectConstructor
{
    public object construct(object[] args)
    {
        return new PythonDictionary();
    }
}
