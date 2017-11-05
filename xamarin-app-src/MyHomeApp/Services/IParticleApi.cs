using System.Collections.Generic;
using System.Threading.Tasks;
using MyHomeApp.Models;
using Refit;

namespace MyHomeApp.Services
{
    public interface IParticleApi
    {
        [Post("/v1/devices/{deviceId}/{function}?access_token={accessToken}")]
        Task<FunctionResponse> CallFunction(string function, [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data, string deviceId, string accessToken);

        [Get("/v1/devices/{deviceId}/{variable}?access_token={accessToken}")]
        Task<VariableResponse> GetVariable(string variable, string deviceId, string accessToken);
    }
}
