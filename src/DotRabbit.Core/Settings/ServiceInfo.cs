using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Settings;

internal class ServiceInfo
    : IServiceInfo
{
    private readonly Service _service;

    public ServiceInfo(string serviceName)
    {
        _service = new Service(serviceName);
    }
    public Service GetInfo()
    {
        return _service;
    }
}
