using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoundTheCode.Blash.BlazorWasm.Models;
using RoundTheCode.Blash.BlazorWasm.Services.Loader;

namespace RoundTheCode.Blash.BlazorWasm.Services.Modal
{
    /// <summary>
    ///  A service used for the modal.
    /// </summary>
    public class ModalService : IModalService
    {
        /// <summary>
        /// The modal's properties.
        /// </summary>
        public ModalModel ModalModel { get; protected set; }

        /// <summary>
        /// Invoked when a modal is opened.
        /// </summary>
        public event Func<Task> OnOpenAsync;

        /// <summary>
        /// Invoked when a modal is closed.
        /// </summary>
        public event Func<Task> OnCloseAsync;

        /// <summary>
        /// Invoked when a modal is submitted.
        /// </summary>
        public event Func<Task> OnSubmitAsync;

        /// <summary>
        /// Invoked when a method inside a modal has an error.
        /// </summary>
        public event Func<Exception, Task> OnErrorAsync;

        protected readonly ILoaderService _loaderService;

        /// <summary>
        /// A new instance of <see cref="ModalService"/>.
        /// </summary>
        /// <param name="loaderService">An instance of <see cref="ILoaderService"/>.</param>
        public ModalService([NotNull] ILoaderService loaderService)
        {
            _loaderService = loaderService;
        }

        /// <summary>
        /// A method to create a modal.
        /// </summary>
        /// <param name="modalModel">A instance of <see cref="ModalModel"/>.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task CreateAsync(ModalModel modalModel)
        {
            ModalModel = modalModel;
            await OnOpenAsync.Invoke();
        }

        /// <summary>
        /// A method to submitting a form in a modal.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task SubmitAsync()
        {
            try
            {
                await _loaderService.CreateAsync(); // Show the loader
                await OnSubmitAsync.Invoke(); // Submit the form.

                await DisposeAsync(); // Remove the modal.
                await _loaderService.DisposeAsync(); // Remove the loader.
            }
            catch (Exception exception)
            {
                await DisposeAsync(); // Remove the modal.
                await _loaderService.DisposeAsync(); // Remove the loader.

                await OnErrorAsync.Invoke(exception); // Invoke the fact that there is an error.
            }
        }

        /// <summary>
        /// A method for disposing the modal.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task DisposeAsync()
        {
            ModalModel = null; // Remove the instance of the model.
            await OnCloseAsync.Invoke(); // Invoke the on close method.

            OnSubmitAsync = null; // Remove the instance when submitting a modal.
        }
    }
}
