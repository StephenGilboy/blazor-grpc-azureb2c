using Gilboy.Messages;

namespace Gilboy.Web.Data;

public class GreeterWebService
{
    private readonly Greeter.GreeterClient _greeterClient;

    public GreeterWebService(Greeter.GreeterClient greeterClient)
    {
        _greeterClient = greeterClient ?? throw new ArgumentNullException(nameof(greeterClient));
    }
    
    public async Task<string> SayHello(string name)
    {
        var request = new HelloRequest
        {
            Name = name
        };
        var response = await _greeterClient.SayHelloAsync(request);
        return response?.Message ?? "No response";
    }
}