using Serilog;


namespace dong_backend.Services
{
    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var requestPath = context.Request.Path;
                var requestMethod = context.Request.Method;
                var requestBody = await GetRequestBody(context.Request);

                Log.Error(ex, "An unhandled exception occurred. RequestPath: {RequestPath}, RequestMethod: {RequestMethod}, RequestBody: {RequestBody}",
                    requestPath, requestMethod, requestBody);

                throw;
            }
        }

        private async Task<string> GetRequestBody(HttpRequest request)
        {
            request.EnableBuffering();

            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
    }

}
