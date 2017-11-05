using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;

namespace MyHomeApp.Services
{
    public static class ConnectivityService
    {
        private static IConnectivity _connectivity;

        public static IConnectivity Instance
        {
            get => _connectivity ?? (_connectivity = CrossConnectivity.Current);
            set => _connectivity = value;
        }
    }
}