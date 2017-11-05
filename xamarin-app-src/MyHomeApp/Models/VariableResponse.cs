using System;

namespace MyHomeApp.Models
{
    public class VariableResponse
    {
        public string Cmd { get; set; }
        public string Name { get; set; }
        public string Result { get; set; }
        public CoreInfo CoreInfo { get; set; }
    }

    public class CoreInfo
    {
        public string LastApp { get; set; }
        public DateTime LastHeard { get; set; }
        public bool Connected { get; set; }
        public DateTime LastHandshakeAt { get; set; }
        public string DeviceID { get; set; }
        public int ProductId { get; set; }
    }
}
