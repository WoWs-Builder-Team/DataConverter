// soource for the custom unpickler: https://github.com/CruleD/WoWs-GameParams-Dumper/blob/master/WoWs%20GameParams%20Dumper/CustomUnpicklerClass.cs

using System.Collections;
using Razorvine.Pickle;

namespace WowsShipBuilder.GameParamsExtractor
{
    public class CustomUnpicklerClass : IObjectConstructor
    {
        public readonly string module;
        public readonly string name;

        public CustomUnpicklerClass(string module, string name)
        {
            this.module = module;
            this.name = name;
        }

        public object construct(object[] args)
        {
            return new CustomClassDict(module, name);
        }
    }

    public class CustomClassDict : Dictionary<object, object>
    {
        public string ClassName { get; }

        public CustomClassDict(string modulename, string classname)
        {
            ClassName = (string.IsNullOrEmpty(modulename)) ? classname : modulename + "." + classname;
            //Add("__class__", ClassName);
        }

        public void __setstate__(Hashtable values)
        {
            Clear();
            //Add("__class__", ClassName);
            if (values.ContainsKey("damageDistribution"))
            {
                Hashtable HashtableTemp = (Hashtable)values["damageDistribution"]!;
                Hashtable HashtableTempNew = new Hashtable();
                foreach (DictionaryEntry DictionaryEntryTemp in HashtableTemp)
                {
                    HashtableTempNew.Add(DictionaryEntryTemp.Value!, DictionaryEntryTemp.Key);
                }
                values["damageDistribution"] = HashtableTempNew;
            }
            foreach (string x in values.Keys)
            {
                Add(x, values[x]!);
            }

        }
    }
}
