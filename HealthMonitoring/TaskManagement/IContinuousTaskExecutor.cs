using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.TaskManagement
{
    public interface IContinuousTaskExecutor<T> : IDisposable
    {
        bool TryRegisterTaskFor(T item, Func<T, CancellationToken, Task> continuousTaskFactory);
    }
}