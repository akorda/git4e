﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface IObjectStore
    {
        Task SaveObjectAsync(IHashableObject content, CancellationToken cancellationToken = default);
        Task SaveObjectsAsync(IEnumerable<IHashableObject> contents, CancellationToken cancellationToken = default);
        Task<string> GetObjectTypeAsync(byte[] hash, CancellationToken cancellationToken = default);
        Task<object> GetObjectContentAsync(byte[] hash, Type contentType, CancellationToken cancellationToken = default);
        Task<byte[]> AddCommit(Commit commit, CancellationToken cancellationToken = default);
    }
}
