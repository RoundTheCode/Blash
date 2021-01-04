using Microsoft.AspNetCore.Components;
using System;

namespace RoundTheCode.Blash.BlazorWasm.Models
{
    /// <summary>
    /// A model used to store modal properties.
    /// </summary>
    public class ModalModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="ModalModel"/>.
        /// </summary>
        /// <param name="title">The modal's title.</param>
        /// <param name="content">The modal's content.</param>
        /// <param name="submitButtonTitle">The modal's submit button title.</param>
        public ModalModel(string title, RenderFragment content, string submitButtonTitle)
        {
            Title = title;
            Content = content;
            SubmitButtonTitle = submitButtonTitle;
            ShowButtons = true;
            AllowClose = true;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ModalModel"/>, with the ability to show buttons and allow the closing of the modal.
        /// </summary>
        /// <param name="title">The modal's title.</param>
        /// <param name="content">The modal's content.</param>
        /// <param name="submitButtonTitle">The modal's submit button title.</param>
        /// <param name="showButtons">Whether to show the buttons at the bottom of the modal.</param>
        /// <param name="allowClose">Whether to allow the modal to be closed by the end user.</param>
        public ModalModel(string title, RenderFragment content, string submitButtonTitle, bool showButtons, bool allowClose) : this(title, content, submitButtonTitle)
        {
            ShowButtons = showButtons;
            AllowClose = allowClose;
        }

        /// <summary>
        /// The modal's title.
        /// </summary>
        public string Title { get;  }

        /// <summary>
        /// The modal's content.
        /// </summary>
        public RenderFragment Content { get; }

        /// <summary>
        /// The modal's submit button title.
        /// </summary>
        public string SubmitButtonTitle { get; }

        /// <summary>
        /// Whether to show the buttons at the bottom of the modal.
        /// </summary>
        public bool ShowButtons { get; }

        /// <summary>
        /// Whether to allow the modal to be closed by the end user.
        /// </summary>
        public bool AllowClose { get; }
    }
}
