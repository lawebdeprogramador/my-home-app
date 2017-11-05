using System;

namespace MyHomeApp.Models
{
    public class FunctionResponse
    {
        public string Id { get; set; }
        public string LastApp { get; set; }
        public bool Connected { get; set; }
        public int ReturnValue { get; set; }
    }
}
