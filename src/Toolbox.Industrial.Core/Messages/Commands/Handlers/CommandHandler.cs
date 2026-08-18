using System.Net;
using Toolbox.Core.Data;
using Toolbox.Core.Extensions;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Commands.Handlers
{
    public delegate bool BusinessRule();

    public abstract class CommandHandler
    {
        //private readonly IMediator _mediator;
        private readonly List<Event> _events = [];
        private readonly IDictionary<string, object[]> _errors = new Dictionary<string, object[]>();
        protected ResponseResult ResponseResult { get; set; } = ResponseResult.Success;
        //protected IMediator Mediator => _mediator;

        //protected CommandHandler(IMediator mediator)
        //{
        //    //_mediator = mediator;
        //}

        public ResponseResult BadRequest()
        {
            return ResponseResult.SetHttpStatusCode(HttpStatusCode.BadRequest).AddErrors(_errors);
        }

        public ResponseResult BadRequest(string paramName, params object[] errorMessages)
        {
            return BadRequest().AddError(paramName, errorMessages);
        }

        public ResponseResult NotFound()
        {
            return ResponseResult.SetHttpStatusCode(HttpStatusCode.NotFound);
            //.AddErrors(_errors);
        }

        public ResponseResult NoContent()
        {
            return ResponseResult.SetHttpStatusCode(HttpStatusCode.NoContent);
        }

        public ResponseResult Forbid()
        {
            return ResponseResult.SetHttpStatusCode(HttpStatusCode.Forbidden);
        }

        public ResponseResult Conflict(string paramName, params object[] errorMessages)
        {
            return ResponseResult
                .SetHttpStatusCode(HttpStatusCode.Conflict)
                .AddError(paramName, errorMessages);
        }

        public ResponseResult InternalServerError(Exception exception)
        {
            return ResponseResult.SetHttpStatusCode(HttpStatusCode.InternalServerError);
        }

        public ResponseResult Ok<TResponse>(
            TResponse? payload = null,
            Func<TResponse?, Task<ResponseResult?>>? validate = null,
            DefaultResponse? defaultResponse = null
        )
            where TResponse : class
        {
            if (payload != null || defaultResponse == null)
            {
                if (validate != null && payload != null)
                {
                    if (
                        Task.Run(() => validate(payload)).GetAwaiter().GetResult()
                        is ResponseResult result
                    )
                    {
                        return result;
                    }
                }
                return ResponseResult.SetHttpStatusCode(HttpStatusCode.OK).AddPayload(payload);
            }
            return defaultResponse();
        }

        public ResponseResult Created<TResponse>(
            TResponse? payload = null,
            Func<TResponse?, Task<ResponseResult?>>? validate = null,
            DefaultResponse? defaultResponse = null
        )
            where TResponse : class
        {
            if (payload != null || defaultResponse == null)
            {
                if (validate != null && payload != null)
                {
                    if (
                        Task.Run(() => validate(payload)).GetAwaiter().GetResult()
                        is ResponseResult result
                    )
                    {
                        return result;
                    }
                }
                return ResponseResult.SetHttpStatusCode(HttpStatusCode.Created).AddPayload(payload);
            }
            return defaultResponse();
        }

        public bool IsValid(Toolbox.Core.Messages.Command command)
        {
            var validationResult = command.Validate();
            if (validationResult.IsValid)
                return true;

            validationResult.Errors.ForEach(error =>
                _errors.TryAdd(error.PropertyName.ToLowerFirst(), new[] { error.ErrorMessage })
            );
            return false;
        }

        public bool NotIsValid(Toolbox.Core.Messages.Command command) => IsValid(command) == false;

        public CommandHandler AddError(string propertyName, params object[] errorMessages)
        {
            _errors.TryAdd(propertyName.ToLowerFirst(), errorMessages);
            return this;
        }

        public bool IsDefaultValue<T>(T value) =>
            EqualityComparer<T>.Default.Equals(value, default);

        public bool AddErrorWhenDefaultValue<T>(T value, string propertyName, string errorMessage)
        {
            if (IsDefaultValue(value))
            {
                AddError(propertyName, errorMessage);
                return true;
            }
            return false;
        }

        public bool Assert(
            BusinessRule businessRule,
            string propertyName,
            params object[] errorMessages
        )
        {
            var result = businessRule();
            if (result == false)
                AddError(propertyName, errorMessages);

            return result;
        }

        public bool Fail(
            BusinessRule businessRule,
            string propertyName,
            params object[] errorMessages
        ) => Assert(businessRule, propertyName, errorMessages) == false;

        public CommandHandler AddErrors(IDictionary<string, object[]> errors)
        {
            errors?.ForEach(error => _errors.TryAdd(error.Key.ToLowerFirst(), error.Value));
            return this;
        }

        public void AddEvent(Event @event)
        {
            //if (@event.CorrelationId == Guid.Empty)
            //{
            //    @event.SetCorrelationId();
            //}
            _events.Add(@event);
        }

        public bool HasErrors => _errors.Any();
    }
}
