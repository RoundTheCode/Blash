using System;
using System.Threading.Tasks;
using RoundTheCode.Blash.BlazorWasm.Models;

namespace RoundTheCode.Blash.BlazorWasm.Services.Modal
{
    /// <summary>
    ///  A service used for the modal.
    /// </summary>
    public interface IModalService
    {
        /// <summary>
        /// The modal's properties.
        /// </summary>
        ModalModel ModalModel { get; }

        /// <summary>
        /// Invoked when a modal is opened.
        /// </summary>
        event Func<Task> OnOpenAsync;
        
        /// <summary>
        /// Invoked when a modal is closed.
        /// </summary>
        event Func<Task> OnCloseAsync;

        /// <summary>
        /// Invoked when a modal is submitted.
        /// </summary>
        event Func<Task> OnSubmitAsync;

        /// <summary>
        /// Invoked when a method inside a modal has an error.
        /// </summary>
        event Func<Exception, Task> OnErrorAsync;

        /// <summary>
        /// A method to create a modal.
        /// </summary>
        /// <param name="modalModel">A instance of <see cref="ModalModel"/>.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task CreateAsync(ModalModel modalModel);

        /// <summary>
        /// A method to submitting a form in a modal.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task SubmitAsync();

        /// <summary>
        /// A method to removing a modal.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task DisposeAsync();

    }
}
