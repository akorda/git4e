using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public abstract class LazyHashableObjectBase : Lazy<Task<IHashableObject>>, IHashableObject
    {
        public LazyHashableObjectBase(Func<IHashableObject> valueFactory) :
            base(() => Task.Factory.StartNew(valueFactory))
        {
        }

        public LazyHashableObjectBase(Func<Task<IHashableObject>> taskFactory) :
            base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap())
        {
        }

        public abstract string Type { get; protected set; }
        public abstract string Hash { get; protected set; }
        public abstract string UniqueId { get; protected set; }
        public abstract string FullHash { get; }

        public TaskAwaiter<IHashableObject> GetAwaiter() => Value.GetAwaiter();
        public abstract IAsyncEnumerable<IHashableObject> GetChildObjects();

        public TReal GetValue<TReal>()
            where TReal: IHashableObject
        {
            return (TReal)this.Value.Result;
        }

        public IHashableObject GetValue()
        {
            return this.Value.Result;
        }

        public abstract void MarkAsDirty();
        public abstract Task SerializeContentAsync(Stream stream, CancellationToken cancellationToken = default);
    }
}
