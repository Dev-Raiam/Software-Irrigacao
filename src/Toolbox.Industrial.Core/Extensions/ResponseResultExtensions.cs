using Toolbox.Core.Mediator;

namespace Toolbox.Industrial.Core.Extensions
{
    public static class ResponseResultExtensions
    {
        public static List<object> GetErrors(this ResponseResult response)
        {
            var result = new List<object>();
            foreach (var error in response.Errors)
            {
                result.AddRange(error.Value);
            }
            return result;
        }
    }
}
