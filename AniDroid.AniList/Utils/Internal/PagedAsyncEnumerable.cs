﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AniDroid.AniList.Interfaces;
using AniDroid.AniList.Models;
using OneOf;

namespace AniDroid.AniList.Utils.Internal 
{
    // TODO: Use IAsyncEnumerator+IAsyncEnumnerable from C# 8.0 ASAP
    internal class PagedAsyncEnumerable<T> : IAsyncEnumerable<AniListObject.PagedData<T>>
    {
        private readonly Func<PagingInfo, CancellationToken, Task<OneOf<AniListObject.PagedData<T>, IAniListError>>> _getPage;
        private readonly Func<PagingInfo, AniListObject.PagedData<T>, bool> _nextPage;

        public int PageSize { get; }

        public PagedAsyncEnumerable(int pageSize,
            Func<PagingInfo, CancellationToken, Task<OneOf<AniListObject.PagedData<T>, IAniListError>>> getPage,
            Func<PagingInfo, AniListObject.PagedData<T>, bool> nextPage)
        {
            if (pageSize <= 0) throw new ArgumentException($"Value cannot be less than or equal to zero (0)", nameof(pageSize));
            this.PageSize = pageSize;
            this._getPage = getPage ?? throw new ArgumentNullException(nameof(getPage));
            this._nextPage = nextPage ?? throw new ArgumentNullException(nameof(nextPage));
        }

        public IAsyncEnumerator<AniListObject.PagedData<T>> GetEnumerator()
            => new Enumerator(this);

        public class Enumerator : IAsyncEnumerator<AniListObject.PagedData<T>>
        {
            private readonly PagedAsyncEnumerable<T> _source;
            private readonly PagingInfo _info;

            public AniListObject.PagedData<T> Current { get; private set; }

            public Enumerator(PagedAsyncEnumerable<T> source)
            {
                this._source = source;
                this._info = new PagingInfo(source.PageSize);
            }

            public async Task<bool> MoveNextAsync(CancellationToken ct = default)
            {
                if (this._info.Remaining == false)
                    return false;

                var pageResult = await this._source._getPage(this._info, ct).ConfigureAwait(false);

                pageResult.Switch(data => this.Current = data)
                    .Switch(error => { });

                if (this.Current == null)
                    return false;

                this._info.Page++;
                this._info.Remaining = this._source._nextPage(this._info, this.Current);

                return true;
            }

            public void Dispose()
            {
                this.Current = null;
            }
        }
    }
}