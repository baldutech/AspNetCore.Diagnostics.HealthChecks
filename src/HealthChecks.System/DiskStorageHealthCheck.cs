using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.System
{
    public class DiskStorageHealthCheck : IHealthCheck
    {
        private readonly DiskStorageOptions _options;

        public DiskStorageHealthCheck(DiskStorageOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var configuredDrives = _options.ConfiguredDrives.Values;

                foreach (var (DriveName, MinimumFreeMegabytes) in configuredDrives)
                {
                    var (Exists, ActualFreeMegabytes) = GetSystemDriveInfo(DriveName);

                    if (Exists)
                    {
                        if (ActualFreeMegabytes < MinimumFreeMegabytes)
                        {
                            return Task.FromResult(
                                new HealthCheckResult(context.Registration.FailureStatus, description: $"Minimum configured megabytes for disk {DriveName} is {MinimumFreeMegabytes} but actual free space are {ActualFreeMegabytes} megabytes"));
                        }
                    }
                    else
                    {
                        return Task.FromResult(
                            new HealthCheckResult(context.Registration.FailureStatus, description: $"Configured drive {DriveName} is not present on system"));
                    }
                }
                return HealthCheckResultTask.Healthy;
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                     new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }

        private static (bool Exists, long ActualFreeMegabytes) GetSystemDriveInfo(string driveName)
        {
            var driveInfo = DriveInfo.GetDrives()
                .FirstOrDefault(drive => string.Equals(drive.Name, driveName, StringComparison.InvariantCultureIgnoreCase));

            return driveInfo == null
                ? (false, 0)
                : (true, driveInfo.AvailableFreeSpace / 1024 / 1024);
        }
    }
}
