using System.Net;
using HealthChecks.Network.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Network
{
    public class DnsResolveHealthCheck : IHealthCheck
    {
        private readonly DnsResolveOptions _options;
        public DnsResolveHealthCheck(DnsResolveOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var item in _options.ConfigureHosts.Values)
                {
                    var ipAddresses = await Dns.GetHostAddressesAsync(item.Host).WithCancellationTokenAsync(cancellationToken);

                    foreach (var ipAddress in ipAddresses)
                    {
                        if (item.Resolutions == null || !item.Resolutions.Contains(ipAddress.ToString()))
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Ip Address {ipAddress} was not resolved from host {item.Host}");
                        }
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
