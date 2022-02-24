using System.Collections;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Razorvine.Pickle;

namespace WowsShipBuilder.GameParamsExtractor
{
    public class GameParamsUtility
    {
        public static byte[] Decompress(byte[] data)
        {
            var outputStream = new MemoryStream();
            using (var compressedStream = new MemoryStream(data))
            using (var inputStream = new InflaterInputStream(compressedStream))
            {
                inputStream.CopyTo(outputStream);
                outputStream.Position = 0;
                return outputStream.ToArray();
            }
        }

        public static Hashtable UnpickleGameParams(byte[] decompressedGpBytes)
        {
            Hashtable UnpickledGP = new Hashtable();
            using (Unpickler UnpicklerTemp = new Unpickler())
            {
                Unpickler.registerConstructor("copy_reg", "_reconstructor", new CustomUnpicklerClass("copy_reg", "_reconstructor"));
                object[] UnpickledObjectTemp = (object[])UnpicklerTemp.loads(decompressedGpBytes);
                UnpickledGP = (Hashtable)UnpickledObjectTemp[0];
                UnpicklerTemp.close();
            }
            return UnpickledGP;
        }

        public static Dictionary<object, object> ConvertDataValue(object ObjectPass)
        {
            switch (ObjectPass.GetType().Name)
            {
                case "Hashtable":
                    {
                        return new Dictionary<object, object>(((Hashtable)ObjectPass).Cast<DictionaryEntry>().ToDictionary(kvp => kvp.Key, kvp => kvp.Value!));
                    }

                case "ClassDict": //Unable to cast object of type 'Razorvine.Pickle.Objects.ClassDict' to type 'System.Collections.Generic.Dictionary`2[System.Object,System.Object]'.
                    {

                        Dictionary<object, object> SortedDictionaryTemp = new Dictionary<object, object>();
                        foreach (KeyValuePair<string, object> kvp in (Dictionary<string, object>)ObjectPass)
                        {
                            SortedDictionaryTemp.Add(kvp.Key, kvp.Value);
                        }
                        return SortedDictionaryTemp;
                    }

                default:
                    {
                        return new Dictionary<object, object>((Dictionary<object, object>)ObjectPass);
                    }
            }
        }
    }
}
