using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoundTheCode.Blash.Api.Background.Tasks.BaseObjects;

namespace RoundTheCode.Blash.Api.Background.Jobs
{
    public class TwitterIntegrationJob
    {
        public Guid Id { get; private set; }

        public DateTimeOffset Created { get; }

        public Func<Guid, Task> JobActionAsync { get; }

        public bool Async { get; }

        public DateTimeOffset? Completed { get; private set; }

        public TwitterIntegrationJob(Func<Guid, Task> jobActionAsync)
        {
            Id = Guid.NewGuid();
            Created = DateTimeOffset.Now;
            JobActionAsync = jobActionAsync;
        }

        public TwitterIntegrationJob(Func<Guid, Task> jobActionAsync, bool async) : this(jobActionAsync)
        {
            Async = async;
        }

        public async Task RunJobAsync()
        {
            try
            {
                await JobActionAsync?.Invoke(Id);
            }
            catch (Exception)
            {

            }
        }

        public void MarkAsComplete()
        {
            Completed = DateTimeOffset.Now;
        }
    }
}
