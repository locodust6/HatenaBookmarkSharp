﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HatenaBookmarkSharp.Models;

namespace HatenaBookmarkSharp
{
    public interface IHatenaBookmarkClient
    {
        Task<Bookmark> GetBookmarkAsync(Uri uri, CancellationToken cancellationToken = default);

        Task<Bookmark> PostBookmarkAsync(PostRequest parameter, CancellationToken cancellationToken = default);

        Task DeleteBookmarkAsync(Uri uri, CancellationToken cancellationToken = default);

        Task<Entry> GetEntryAsync(Uri uri, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Tag>> GetMyTagsAsync(CancellationToken cancellationToken = default);

        Task<User> GetMyAsync(CancellationToken cancellationToken = default);

        Task<RequestToken> GetRequestTokenAsync(CancellationToken cancellationToken = default);

        Task<AccessToken> GetAccessTokenAsync(
            string authenticationCode,
            RequestToken requestToken,
            CancellationToken cancellationToken = default);

        Uri GenerateAuthenticationUri(string requestToken);
    }
}
