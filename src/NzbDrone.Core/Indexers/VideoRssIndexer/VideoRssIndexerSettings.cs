using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.MediathekViewRSSIndexer;

public class VideoRssIndexerSettings : IIndexerSettings
{
    private static readonly AbstractValidator<VideoRssIndexerSettings> Validator = new VideoRssIndexerSettingsValidator();

    public VideoRssIndexerSettings()
    {
        EpisodeSearchMask = "{0} S{1:D2} E{2:D2}";
        SeasonSearchMask = "{0} S{1:D2}";
    }

    public NzbDroneValidationResult Validate()
    {
        return new NzbDroneValidationResult(Validator.Validate(this));
    }

    [FieldDefinition(0, Label = "Full RSS Feed URL")]
    public string BaseUrl { get; set; }

    [FieldDefinition(0, Label = "Show Search format")]
    public string EpisodeSearchMask { get; set; }

    [FieldDefinition(0, Label = "Show Search format")]
    public string SeasonSearchMask { get; set; }

    [FieldDefinition(1, Label = "Cookie", HelpText = "If you site requires a login cookie to access the rss, you'll have to retrieve it via a browser.")]
    public string Cookie { get; set; }
}

internal class VideoRssIndexerSettingsValidator : AbstractValidator<VideoRssIndexerSettings>
{
    public VideoRssIndexerSettingsValidator()
    {
        RuleFor(c => c.BaseUrl).ValidRootUrl();

        // RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());
    }
}
