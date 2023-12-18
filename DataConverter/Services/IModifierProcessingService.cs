using System.Collections.Generic;
using System.Threading.Tasks;
using WoWsShipBuilder.DataStructures.Modifiers;

namespace DataConverter.Services;

public interface IModifierProcessingService
{
    /// <summary>
    /// Read the modifier json and returns a dictionary mapping the name and the modifier object itself.
    /// Used to assign all the parameters, except the actual value, during the modifier processing in the various converters.
    /// </summary>
    /// <param name="modifiersFilePath">The modifiers file path.</param>
    /// <returns>A dictionary mapping the modifier name to the modifier object itself.</returns>
    public Dictionary<string, Modifier> ReadModifiersFile(string modifiersFilePath);

    /// <summary>
    /// Write the following files useful for debugging and generating the modifier json file.<br/>
    /// It generates the following files:
    /// <list type="bullet">
    /// <item>
    /// The actual full modifier.json file, in the same format of the one read by the converter.
    /// </item>
    /// <item>
    /// A file with a list of modifier names with missing translation. Note that there is a basic match for translation keys, since the common case
    /// of a translation key is "MODIFIER_PARAMS_" + modifier name.
    /// </item>
    /// <item>
    /// A file that includes only the new modifiers. It has the same format of the first file, and it's created for convenience.
    /// </item>
    /// </list>
    /// </summary>
    /// <param name="modifierList">List of all the processed modifiers.</param>
    /// <param name="startingModifierDictionary">The starting modifier Dictionary, as read from the modifier.json file.</param>
    /// <param name="localizationKeys">A list containing the localization keys.</param>
    /// <param name="outputPath">The outpath path for the files.</param>
    /// <returns>A task running the operation.</returns>
    public Task WriteDebugModifierFiles(List<Modifier> modifierList, Dictionary<string, Modifier> startingModifierDictionary, List<string> localizationKeys, string outputPath);
}
