using ErrorOr;

namespace ss_blog_be.ApiHelpers
{
    public class OnErrorHandler
    {
        public OnErrorHandler() { }

        public static IResult HandleGet<T>(ErrorOr.ErrorOr<T> result)
        {
            if(result.IsError && result.FirstError.Type == ErrorType.NotFound) 
            {
               return Results.NotFound(); 
            }

            return Results.Ok(result.Value);
        }
    }
}
