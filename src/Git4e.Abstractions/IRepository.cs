﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface IRepository
    {
        IServiceProvider ServiceProvider { get; }

        IContentSerializer ContentSerializer { get; }

        IHashCalculator HashCalculator { get; }

        IObjectStore ObjectStore { get; }

        IContentTypeResolver ContentTypeResolver { get; }

        IRootFromHashCreator RootFromHashCreator { get; }

        string HeadCommitHash { get; }

        Task<ICommit> CheckoutAsync(string branch, CancellationToken cancellationToken = default);

        Task<string> CommitAsync(string author, DateTime when, string message, LazyHashableObjectBase root, CancellationToken cancellationToken = default);

        Task CreateBranchAsync(string branch, bool checkout, CancellationToken cancellationToken = default);

        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///
        /// </summary>
        /// <param name="commit"></param>
        /// <param name="parentIndex">1 based index of the parent commit to retrieve</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ICommit> GetParentCommitAsync(ICommit commit, int parentIndex = 1, CancellationToken cancellationToken = default);

        IAsyncEnumerable<ICommit> GetCommitHistoryAsync(CancellationToken cancellationToken = default);
    }
}
