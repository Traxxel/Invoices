using System.Diagnostics;
using Serilog;

namespace Invoice.Infrastructure.Logging;

public class PerformanceLogger
{
    private readonly ILogger _logger;

    public PerformanceLogger(ILogger logger)
    {
        _logger = logger;
    }

    public IDisposable LogOperation(string operationName)
    {
        return new PerformanceScope(_logger, operationName);
    }

    private class PerformanceScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;

        public PerformanceScope(ILogger logger, string operationName)
        {
            _logger = logger;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();

            _logger.Debug("Starting operation: {OperationName}", _operationName);
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.Debug("Completed operation: {OperationName} in {ElapsedMs}ms",
                _operationName, _stopwatch.ElapsedMilliseconds);
        }
    }
}

