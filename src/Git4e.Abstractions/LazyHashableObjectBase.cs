using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    /// <summary>
    /// Provides a base class of all lazy hashable objects. It provides a hashable object
    /// instance in a later time i.e. when the actual value is requestes.
    /// This is an abstract class.
    /// </summary>
    public abstract class LazyHashableObjectBase : Lazy<Task<IHashableObject>>, IHashableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Git4e.LazyHashableObjectBase"/> class
        /// using a function that synchronously provides the final <see cref="Git4e.IHashableObject"/>
        /// instance value.
        /// </summary>
        /// <param name="valueFactory">The function that synchronously provides the hashable instance value.</param>
        public LazyHashableObjectBase(Func<IHashableObject> valueFactory) :
            base(() => Task.Factory.StartNew(valueFactory))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Git4e.LazyHashableObjectBase"/> class
        /// using a function that asynchronously provides the final <see cref="Git4e.IHashableObject"/>
        /// instance value.
        /// </summary>
        /// <param name="taskFactory">The function that asynchronously provides the hashable instance value.</param>
        public LazyHashableObjectBase(Func<Task<IHashableObject>> taskFactory) :
            base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap())
        {
        }

        /// <inheritdoc/>
        public abstract string ContentTypeName { get; protected set; }

        /// <inheritdoc/>
        public abstract string Hash { get; protected set; }

        /// <inheritdoc/>
        public abstract string UniqueId { get; protected set; }

        /// <inheritdoc/>
        public abstract string FullHash { get; }

        /// <summary>
        /// Gets an awaiter used to await the value task.
        /// </summary>
        /// <returns>The value awaiter instance.</returns>
        public TaskAwaiter<IHashableObject> GetAwaiter() => Value.GetAwaiter();

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<IHashableObject> GetChildObjects();

        /// <summary>
        /// Loads the actual hashable instance and casts the value to the specified
        /// <see cref="Git4e.IHashableObject"/> implementation.
        /// </summary>
        /// <typeparam name="TReal">The actual <see cref="System.Type"/> of the hashable instance value.</typeparam>
        /// <returns>The loaded hashable object intance.</returns>
        public TReal LoadValue<TReal>()
            where TReal: IHashableObject
        {
            return (TReal)this.LoadValue();
        }

        /// <summary>
        /// Loads the actual hashable instance.
        /// </summary>
        /// <returns>The loaded hashable object intance.</returns>
        public IHashableObject LoadValue()
        {
            return this.Value.Result;
        }

        /// <inheritdoc/>
        public abstract void MarkAsDirty();

        /// <inheritdoc/>
        public abstract Task SerializeContentAsync(Stream stream, CancellationToken cancellationToken = default);
    }
}
