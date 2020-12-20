using KBCommunication.Interfaces;
using System.Collections.Generic;

namespace KBCommunication.Routed.Interfaces
{
    /// <inheritdoc/>
    /// <remarks>Communication with simple routing.</remarks>
    public interface ICommunicationRouter<F> : ICommunication<F, F>
    {
        /// <summary>
        /// Adds new validator.
        /// </summary>
        /// <param name="validator">Validator.</param>
        void AddValidator(IFrameValidator<F> validator);

        /// <summary>
        /// Checks if given <paramref name="validator"/> is added.
        /// </summary>
        /// <param name="validator">Validator.</param>
        /// <returns><c>true</c> if <paramref name="validator"/> has been added, <c>false</c> otherwise.</returns>
        bool CheckValidator(IFrameValidator<F> validator);

        /// <summary>
        /// Removes all validators.
        /// </summary>
        void ClearValidators();

        /// <summary>
        /// Removes validator.
        /// </summary>
        /// <param name="validator">Validator.</param>
        void RemoveValidator(IFrameValidator<F> validator);
    }
}
