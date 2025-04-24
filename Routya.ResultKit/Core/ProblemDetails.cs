using System.Collections.Generic;

namespace Routya.ResultKit
{
    public class ProblemDetails
    {
        public string? Title { get; set; }
        public int? Status { get; set; }
        public Dictionary<string, object?> Extensions { get; set; } = new Dictionary<string, object?>();
    }
}
