using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Git4e;

namespace Chinook
{
    public class CommitsComparer : ICommitsComparer
    {
        public async Task CompareCommits(ICommit commit, ICommit prevCommit, ICommitsComparerVisitor visitor, CancellationToken cancellationToken = default)
        {
            if (commit.Root.Hash == prevCommit.Root.Hash)
            {
                Console.WriteLine($"Commit '{commit.Hash}' and commit '{prevCommit.Hash}' have the same Root");
                return;
            }

            var lib1 = commit.Root.GetValue<Library>();
            var lib2 = prevCommit.Root.GetValue<Library>();

            //first compare collections hashes
            if (lib1.Artists.Hash != lib2.Artists.Hash)
            {
                if (visitor != null)
                    await visitor.OnCollectionsDifferAsync(commit, lib1.Artists, lib2.Artists, cancellationToken);

                foreach (var la1 in lib1.Artists)
                {
                    //find artists in lib1 but not in lib2 and updated artists
                    var la2 = lib2.Artists.FirstOrDefault(a => a.ArtistId == la1.ArtistId);
                    if (la2 == null)
                    {
                        if (visitor != null)
                            await visitor.OnItemCreatedAsync(commit, la1, cancellationToken);

                        continue;
                    }

                    if (la1.Hash != la2.Hash)
                    {
                        if (visitor != null)
                            await visitor.OnItemUpdatedAsync(commit, la1, la2, cancellationToken);

                        await CompareArtists(commit, la1.GetValue(), la2.GetValue(), visitor, cancellationToken);
                        continue;
                    }
                }

                foreach (var la2 in lib2.Artists)
                {
                    var la1 = lib1.Artists.FirstOrDefault(a => a.ArtistId == la2.ArtistId);
                    if (la1 == null)
                    {
                        if (visitor != null)
                            await visitor.OnItemDeletedAsync(commit, la2, cancellationToken);

                        continue;
                    }
                }
            }
        }

        private async Task CompareArtists(ICommit commit, Artist artist1, Artist artist2, ICommitsComparerVisitor visitor, CancellationToken cancellationToken)
        {
            //compare simple properties

            if (!string.Equals(artist1.Name, artist2.Name))
            {
                if (visitor != null)
                    await visitor.OnItemPropertyUpdatedAsync(commit, nameof(Artist.Name), artist1, artist2, artist1.Name, artist2.Name, cancellationToken);
            }

            //first compare collections hashes
            if (artist1.Albums.Hash != artist2.Albums.Hash)
            {
                if (visitor != null)
                    await visitor.OnCollectionsDifferAsync(commit, artist1.Albums, artist2.Albums, cancellationToken);

                //find albums in lib1 but not in lib2 and updated albums
                foreach (var al1 in artist1.Albums)
                {
                    var al2 = artist2.Albums.FirstOrDefault(a => a.AlbumId == al1.AlbumId);
                    if (al2 == null)
                    {
                        if (visitor != null)
                            await visitor.OnItemCreatedAsync(commit, al1, cancellationToken);

                        continue;
                    }

                    if (al1.Hash != al2.Hash)
                    {
                        if (visitor != null)
                            await visitor.OnItemUpdatedAsync(commit, al1, al2, cancellationToken);

                        await CompareAlbums(commit, al1.GetValue(), al2.GetValue(), visitor, cancellationToken);
                        continue;
                    }
                }

                foreach (var al2 in artist2.Albums)
                {
                    var al1 = artist1.Albums.FirstOrDefault(a => a.AlbumId == al2.AlbumId);
                    if (al1 == null)
                    {
                        if (visitor != null)
                            await visitor.OnItemDeletedAsync(commit, al2, cancellationToken);

                        continue;
                    }
                }
            }
        }

        private async Task CompareAlbums(ICommit commit, Album album1, Album album2, ICommitsComparerVisitor visitor, CancellationToken cancellationToken)
        {
            //compare simple properties

            if (!string.Equals(album1.Title, album2.Title))
            {
                if (visitor != null)
                    await visitor.OnItemPropertyUpdatedAsync(commit, nameof(Album.Title), album1, album2, album1.Title, album2.Title, cancellationToken);
            }

            //first compare collections hashes
            if (album1.Tracks.Hash != album2.Tracks.Hash)
            {
                if (visitor != null)
                    await visitor.OnCollectionsDifferAsync(commit, album1.Tracks, album1.Tracks, cancellationToken);

                //find tracks in lib1 but not in lib2 and updated tracks
                foreach (var tr1 in album1.Tracks)
                {
                    var tr2 = album2.Tracks.FirstOrDefault(a => a.TrackId == tr1.TrackId);
                    if (tr2 == null)
                    {
                        if (visitor != null)
                            await visitor.OnItemCreatedAsync(commit, tr1, cancellationToken);

                        continue;
                    }

                    if (tr1.Hash != tr2.Hash)
                    {
                        if (visitor != null)
                            await visitor.OnItemUpdatedAsync(commit, tr1, tr2, cancellationToken);

                        await CompareTracks(commit, tr1.GetValue(), tr2.GetValue(), visitor, cancellationToken);
                        continue;
                    }
                }

                foreach (var tr2 in album2.Tracks)
                {
                    var tr1 = album1.Tracks.FirstOrDefault(a => a.TrackId == tr2.TrackId);
                    if (tr1 == null)
                    {
                        if (visitor != null)
                            await visitor.OnItemDeletedAsync(commit, tr2, cancellationToken);

                        continue;
                    }
                }
            }
        }

        private async Task CompareTracks(ICommit commit, Track track1, Track track2, ICommitsComparerVisitor visitor, CancellationToken cancellationToken)
        {
            //compare simple properties

            if (!string.Equals(track1.Name, track2.Name))
            {
                if (visitor != null)
                    await visitor.OnItemPropertyUpdatedAsync(commit, nameof(Track.Name), track1, track2, track1.Name, track2.Name, cancellationToken);
            }
            if (!string.Equals(track1.Composer, track2.Composer))
            {
                if (visitor != null)
                    await visitor.OnItemPropertyUpdatedAsync(commit, nameof(Track.Composer), track1, track2, track1.Composer, track2.Composer, cancellationToken);
            }
            if (track1.Milliseconds != track2.Milliseconds)
            {
                if (visitor != null)
                    await visitor.OnItemPropertyUpdatedAsync(commit, nameof(Track.Milliseconds), track1, track2, track1.Milliseconds, track2.Milliseconds, cancellationToken);
            }
            if (track1.Bytes != track2.Bytes)
            {
                if (visitor != null)
                    await visitor.OnItemPropertyUpdatedAsync(commit, nameof(Track.Bytes), track1, track2, track1.Bytes, track2.Bytes, cancellationToken);
            }
            if (track1.UnitPrice != track2.UnitPrice)
            {
                if (visitor != null)
                    await visitor.OnItemPropertyUpdatedAsync(commit, nameof(Track.UnitPrice), track1, track2, track1.UnitPrice, track2.UnitPrice, cancellationToken);
            }
            if (!string.Equals(track1.Genre.UniqueId, track2.Genre.UniqueId))
            {
                if (visitor != null)
                    await visitor.OnItemPropertyUpdatedAsync(commit, nameof(Genre.GenreId), track1, track2, track1.Genre.UniqueId, track2.Genre.UniqueId, cancellationToken);
            }
            if (!string.Equals(track1.MediaType.UniqueId, track2.MediaType.UniqueId))
            {
                if (visitor != null)
                    await visitor.OnItemPropertyUpdatedAsync(commit, nameof(MediaType.MediaTypeId), track1, track2, track1.MediaType.UniqueId, track2.MediaType.UniqueId, cancellationToken);
            }
        }
    }
}
