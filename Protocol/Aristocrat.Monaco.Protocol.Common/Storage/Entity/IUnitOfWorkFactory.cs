namespace Aristocrat.Monaco.Protocol.Common.Storage.Entity
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    ///     Creates a <see cref="IUnitOfWork"/> instances.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IUnitOfWorkFactory
    {
        /// <summary>
        ///     Creates an <see cref="IUnitOfWork"/> instance.
        /// </summary>
        /// <returns><see cref="IUnitOfWork"/>.</returns>
        /// <remarks>
        ///     The instance returned from this function should be wrapped in a using clause.
        ///     <code language="cs"><![CDATA[
        ///     using (var unitOfWork = _unitOfWorkFactory.Create())
        ///     {
        ///         // Database operations go here
        ///         var host = unitOfWork.Repository<Host>.Add(...);
        ///         await unitOfWork.SaveChanges();
        ///     }
        ///     ]]>
        ///     </code>
        /// </remarks>
        IUnitOfWork Create();

        /// <summary>
        ///     Wraps using a unit of work instance.
        /// </summary>
        /// <typeparam name="T">Return value type.</typeparam>
        /// <param name="action">The action to perform database operations.</param>
        /// <returns>Instance or value of <typeparamref name="T"/></returns>
        T Invoke<T>(Func<IUnitOfWork, T> action);

        /// <summary>
        ///     Wraps using a unit of work instance.
        /// </summary>
        /// <param name="action">The action to perform database operations.</param>
        void Invoke(Action<IUnitOfWork> action);

        /// <summary>
        ///     Wraps using a unit of work instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action to perform database operations.</param>
        /// <returns><see cref="Task{T}"/>, instance or value of <typeparamref name="T"/>.</returns>
        Task<T> Invoke<T>(Func<IUnitOfWork, Task<T>> action);

        /// <summary>
        ///     Wraps using a unit of work instance.
        /// </summary>
        /// <param name="action">The action to perform database operations.</param>
        /// <returns><see cref="Task"/>.</returns>
        Task Invoke(Func<IUnitOfWork, Task> action);
    }
}
