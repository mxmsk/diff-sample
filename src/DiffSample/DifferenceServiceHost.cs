using System.Threading;
using System.Threading.Tasks;
using DiffSample.Abstractions;
using Microsoft.Extensions.Hosting;

namespace DiffSample
{
    /// <summary>
    /// Hosts the difference pipeline inside ASP.NET.
    /// </summary>
    /// <remarks>
    /// Based on https://github.com/stevejgordon/IHostedServiceSample/blob/master/IHostedServiceSample/HostedService.cs
    /// </remarks>
    public class DifferenceServiceHost : IHostedService
    {
        private readonly IDifferenceService _differenceService;
        private Task _serviceTask;
        private CancellationTokenSource _cts;

        /// <summary>
        /// Initializes a new instance of the <see cref="DifferenceServiceHost"/> class.
        /// </summary>
        /// <param name="differenceService">The service that implements difference.</param>
        public DifferenceServiceHost(IDifferenceService differenceService)
        {
            _differenceService = differenceService;
        }

        /// Triggered when the application host is ready to start the service.
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _serviceTask = _differenceService.RunAsync(cancellationToken);

            return _serviceTask;
        }

        /// Triggered when the application host is performing a graceful shutdown.
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_serviceTask == null)
            {
                return;
            }

            _cts.Cancel();
            await Task.WhenAny(_serviceTask, Task.Delay(-1, cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
