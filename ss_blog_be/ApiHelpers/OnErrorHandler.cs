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

        public static IResult HandlePost<T>(ErrorOr.ErrorOr<T> result)
        {
            if (result.IsError && result.FirstError.Type == ErrorType.Validation)
            {
                return Results.BadRequest(result.FirstError.Code);
            }

            if (result.IsError && result.FirstError.Type == ErrorType.Conflict)
            {
                return Results.Conflict(result.FirstError.Code);
            }

            return Results.Ok(result.Value);
        }

        public static IResult HandleNoContent<T>(ErrorOr.ErrorOr<T> result)
        {
            if (result.IsError && result.FirstError.Type == ErrorType.Validation)
            {
                return Results.BadRequest(result.FirstError.Code);
            }

            if (result.IsError && result.FirstError.Type == ErrorType.Conflict)
            {
                return Results.Conflict(result.FirstError.Code);
            }

            return Results.NoContent();
        }
    }
}
