using System;
using System.Threading.Tasks;

namespace RoundTheCode.Blash.BlazorWasm.Services.Loader
{
    /// <summary>
    /// A service used for the loader.
    /// </summary>
    public class LoaderService : ILoaderService
    {
        /// <summary>
        /// Invoked when the loader is shown on the screen.
        /// </summary>
        public event Func<Task> OnShowAsync;

        /// <summary>
        /// Invoked when the loader is hidden on the screen.
        /// </summary>
        public event Func<Task> OnHideAsync;

        /// <summary>
        /// Creates a new instance of the loader.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task CreateAsync()
        {
            await OnShowAsync.Invoke(); // Invokes the event.
        }

        /// <summary>
        /// Disposes the instance of the loader.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task DisposeAsync()
        {
            await OnHideAsync.Invoke(); // Invokes the event.
        }
    }
}
