namespace dong_backend.Models
{
    public class Log
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string Level { get; set; }

        public string Message { get; set; }

        public string Exception { get; set; }

        public string StackTrace { get; set; }

        public string RequestPath { get; set; }

        public string RequestMethod { get; set; }

        public string RequestBody { get; set; }
    }
}
