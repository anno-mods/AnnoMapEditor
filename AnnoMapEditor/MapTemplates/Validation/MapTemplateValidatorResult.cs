namespace AnnoMapEditor.MapTemplates.Validation
{
    public class MapTemplateValidatorResult
    {
        public static readonly MapTemplateValidatorResult Ok = new(MapTemplateValidatorStatus.Ok);


        public MapTemplateValidatorStatus Status { get; }

        public string? Title { get; }

        public string? Message { get; }

        public bool IsError => Status == MapTemplateValidatorStatus.Error;

        public bool IsWarning => Status == MapTemplateValidatorStatus.Warning;

        public bool IsOk => Status == MapTemplateValidatorStatus.Ok;


        public MapTemplateValidatorResult(MapTemplateValidatorStatus status, string? title = null, string? message = null)
        {
            Status = status;
            Title = title;
            Message = message;
        }
    }
}
