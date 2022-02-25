// soource for the custom unpickler: https://github.com/CruleD/WoWs-GameParams-Dumper/blob/master/WoWs%20GameParams%20Dumper/CustomUnpicklerClass.cs

using System.Collections;
using Razorvine.Pickle;

namespace WowsShipBuilder.GameParamsExtractor
{
    public class CustomUnpicklerClass : IObjectConstructor
    {
        public readonly string Module;
        public readonly string Name;

        public CustomUnpicklerClass(string module, string name)
        {
            Module = module;
            Name = name;
        }

        public object construct(object[] args)
        {
            return new CustomClassDict(Module, Name);
        }
    }

    public class CustomClassDict : Dictionary<string, object>
    {
        public string ClassName { get; }

        public CustomClassDict(string modulename, string classname)
        {
            ClassName = string.IsNullOrEmpty(modulename) ? classname : modulename + "." + classname;
            //Add("__class__", ClassName);
        }

        public void __setstate__(Hashtable values)
        {
            Clear();
            //Add("__class__", ClassName);
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
}
