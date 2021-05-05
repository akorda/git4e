﻿using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface IHashableObject
    {
        string Type { get; }
        string Hash { get; }

        /// <summary>
        /// Hash encoded with any included properties
        /// </summary>
        string FullHash { get; }

        Task SerializeContentAsync(Stream stream, CancellationToken cancellationToken = default);
        IAsyncEnumerable<IHashableObject> GetChildObjects();
        void MarkAsDirty();
    }
}
