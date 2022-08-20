# DataConverter

Libraries and a console application to extract data from Wargaming's GameParams and localization files and convert them
into the data format used by ShipBuilder.

## Repository Structure

* DataConverter: The main library containing the logic for converting Wargaming data structures into the data structures
  defined in WoWsShipBuilder.DataStructures project.
* DataConverter.Console: A console application that allows to execute the functions from the library projects.
* GetTextNET: A fork of [GetText.NET](https://github.com/perpetualKid/GetText.NET), a library based on GNU Gettext that
  is used to extract localizations from .mo files.
* WoWsShipBuilder.DataStructures: The data structures used by the ShipBuilder application. These structures are provided
  as nuget package so other applications can use them as well.
* WoWsShipBuilder.GameParamsExtractor: A library that extracts data from the Wargaming GameParams file and converts it
  into C# objects that are easier to process. Also handles basic data filtering before passing data to the data
  converter.

## Usage

### Data Structures

The data structures are available as [nuget package](https://www.nuget.org/packages/WoWsShipBuilder.DataStructures) that
can be referenced by other C# applications.
They can be used under the same license as the rest of this repository.

Please note that this package only contains the data structures. Developers are responsible for providing the actual
data themselves.
That data can be created using the DataConverter.Console application from this repository.

### DataConverter.Console

A console application that can extract and convert data from the files extracted from the World of Warships client.
There are two different operations that are currently supported by the application:

* Full data extraction and conversion
* Extracting raw data and writing it to a file.

Both operations require that the provided data files have been extracted from the game client.
For GameParams, use the official unpack tool provided by Wargaming to extract the GameParams.data file.

For localizations, we recommend to copy the entire "texts" directory from the game client.
This directory can be found at <WoWs Game Directory>/bin/<highest number>/res/ and does not require any unpacking with
the official unpack tool.

Always copy the input data from the original directory to another directory outside of the game client directory in
order to avoid any files from being accidentally flagged as illegal modifications by the game.

**While the converter is not supposed to modify input files, we do not guarantee that this won't happen. We do not
accept responsibility for any unintentional modifications of game client files and possible consequences from these
modifications!**

#### Full data extraction and conversion

This operation requires a path to a GameParams file.

Sample command line:
> DataConverter.Console.exe convert -p GameParams.data -l localizations/texts -o out/live -v 0.11.7 -s live
> --writeUnfiltered --writeFiltered -d out/debug

Providing a game params file (-p), an output directory (-o) and the game version (-v) is mandatory. All other parameters
are optional, if they are missing certain features may be disabled.
Use of the writeUnfiltered and writeFiltered flags is not recommended for production use cases. These files are meant
for debugging and adjusting the filtering logic of the data converter.

The version has to contain at least the game version. It may optionally include an iteration number (which will be
ignored) and a suffix specifying a server type.
An example that combines the version and server type from the example above would look like this: "0.11.7-live"

**The -s argument will always override any server type specified as part of the version**

#### Extracting raw data

If the data conversion is not required, it is also possible to only extract the data and write it to files.

Sample command line:
> DataConverter.Console.exe extract -p GameParams.data -l localizations/texts -o out/raw

Providing a game params file (-p) and an output directory (-o) is mandatory, localizations are optional.

## Sponsorships and Support

This project is part of the WoWs ShipBuilder project which is sponsored by the following companies through their
Open-Source sponsorship programs:

Product subscriptions provided by JetBrains through their [Open Source Support](https://jb.gg/OpenSourceSupport)

<img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png" alt="JetBrains Logo (Main) logo." height="120">
<img src="https://resources.jetbrains.com/storage/products/company/brand/logos/Rider.png" alt="Rider logo." height="120">

Free Open-Source subscription of their localization solution provided by [Crowdin](https://crowdin.com/)

Free Open-Source subscription provided by [Sentry](https://sentry.io/)
