using KBCommunication.Interfaces;
using System;

namespace KBCommunication
{
    /// <inheritdoc cref="IFrameValidator{F}"/>
    public class FrameValidator<F> : IFrameValidator<F>
    {
        private readonly Func<F, bool> validateFunc;

        /// <summary>
        /// Creates frame validator.
        /// </summary>
        /// <param name="validateFunc">Validating function.</param>
        public FrameValidator(Func<F, bool> validateFunc)
        {
            this.validateFunc = validateFunc ?? throw new ArgumentNullException();
        }

        /// <inheritdoc/>
        public event EventHandler<F> FrameValidated;

        /// <inheritdoc/>
        public event EventHandler<F> ValidationFailed;

        /// <inheritdoc/>
        public void RegisterData(F data)
        {
            if (Validate(data))
            {
                FrameValidated?.Invoke(this, data);
            }
            else
            {
                ValidationFailed?.Invoke(this, data);
            }
        }

        /// <inheritdoc/>
        public F FromFrame(F frame) => frame;

        /// <inheritdoc/>
        public bool Validate(F frame) => validateFunc(frame);
    }
}
