using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Ffmpeg;

public class FileSettings : IProviderConfig
{
    public FileSettings()
    {
        StorageFolder = "/file_cache/";
    }

    public NzbDroneValidationResult Validate()
    {
        return new NzbDroneValidationResult();
    }

    [FieldDefinition(7, Label = "Directory", Type = FieldType.Textbox, Advanced = true, HelpText = "Optional location to put downloads in, leave blank to use the default Transmission location")]
    public string StorageFolder { get; set; }
}
