using System;
using System.Threading.Tasks;

namespace RoundTheCode.Blash.BlazorWasm.Services.Loader
{
    /// <summary>
    /// A service used for the loader.
    /// </summary>
    public interface ILoaderService
    {
        /// <summary>
        /// Invoked when the loader is shown on the screen.
        /// </summary>
        event Func<Task> OnShowAsync;

        /// <summary>
        /// Invoked when the loader is hidden on the screen.
        /// </summary>
        event Func<Task> OnHideAsync;

        /// <summary>
        /// Creates a new instance of the loader.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task CreateAsync();

        /// <summary>
        /// Disposes the instance of the loader.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task DisposeAsync();
    }
}
