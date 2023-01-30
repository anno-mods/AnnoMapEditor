namespace AnnoMapEditor.MapTemplates.Validation
{
    public class SessionValidatorResult
    {
        public static readonly SessionValidatorResult Ok = new(SessionValidatorStatus.Ok);


        public SessionValidatorStatus Status { get; }

        public string? Title { get; }

        public string? Message { get; }

        public bool IsError => Status == SessionValidatorStatus.Error;

        public bool IsWarning => Status == SessionValidatorStatus.Warning;

        public bool IsOk => Status == SessionValidatorStatus.Ok;


        public SessionValidatorResult(SessionValidatorStatus status, string? title = null, string? message = null)
        {
            Status = status;
            Title = title;
            Message = message;
        }
    }
}
