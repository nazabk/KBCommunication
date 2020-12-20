using System;

namespace KBCommunication.Interfaces
{
    /// <summary>
    /// Looks up for frames in received data and validates them.
    /// </summary>
    /// <typeparam name="F">Type of frames.</typeparam>
    /// <typeparam name="T">Type of received data.</typeparam>
    public interface IFrameFactory<F, T>
    {
        /// <summary>
        /// On new frame detection.
        /// </summary>
        event EventHandler<F> FrameValidated;

        /// <summary>
        /// On data skipping.
        /// </summary>
        event EventHandler<T> ValidationFailed;

        /// <summary>
        /// Registers new data for purposes of frame validation.
        /// </summary>
        /// <param name="data">Derived data.</param>        
        void RegisterData(T data);

        /// <summary>
        /// Creates <typeparamref name="T"/> reprezentation of frame <typeparamref name="F"/>.
        /// </summary>
        /// <param name="frame">Frame.</param>
        /// <returns><typeparamref name="T"/> data.</returns>
        T FromFrame(F frame);
    }

    /// <inheritdoc/>
    /// <typeparam name="F">Type of frames.</typeparam>
    public interface IFrameValidator<F> : IFrameFactory<F, F>
    {
        /// <summary>
        /// Validates frame.
        /// </summary>
        /// <param name="frame">Frame to validate.</param>
        /// <returns><c>true</c> if frame is valid, <c>false</c> otherwise.</returns>
        bool Validate(F frame);
    }
}
