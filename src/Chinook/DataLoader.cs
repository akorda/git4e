using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Git4e;
using Microsoft.Data.Sqlite;

namespace Chinook
{
    public class DataLoader
    {
        public IEnumerable<Artist> Artists { get; private set; }
        public IEnumerable<Album> Albums { get; private set; }
        public IEnumerable<Track> Tracks { get; private set; }
        public IEnumerable<Genre> Genres { get; private set; }
        public IEnumerable<MediaType> MediaTypes { get; private set; }
        public Library Library { get; private set; }

        public async Task LoadDataAsync(
            string connectionString,
            CancellationToken cancellationToken = default)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                var sql = $@"SELECT ArtistId, Name FROM artists";
                var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
                this.Artists = (await connection.QueryAsync<Artist>(command)).ToArray();

                sql = $@"SELECT AlbumId, Title, ArtistId FROM albums";
                command = new CommandDefinition(sql, cancellationToken: cancellationToken);
                this.Albums = (await connection.QueryAsync<Album>(command)).ToArray();

                sql = $@"SELECT TrackId, Name, AlbumId, MediaTypeId, GenreId, Composer, Milliseconds, Bytes, UnitPrice FROM tracks";
                command = new CommandDefinition(sql, cancellationToken: cancellationToken);
                this.Tracks = (await connection.QueryAsync<Track>(command)).ToArray();

                sql = $@"SELECT GenreId, Name FROM genres";
                command = new CommandDefinition(sql, cancellationToken: cancellationToken);
                this.Genres = (await connection.QueryAsync<Genre>(command)).ToArray();

                sql = $@"SELECT MediaTypeId, Name FROM media_types";
                command = new CommandDefinition(sql, cancellationToken: cancellationToken);
                this.MediaTypes = (await connection.QueryAsync<MediaType>(command)).ToArray();

                var artistsMap = this.Artists.ToDictionary(a => a.ArtistId);
                var albumsMap = this.Albums.ToDictionary(a => a.AlbumId);
                var genresMap = this.Genres.ToDictionary(a => a.GenreId);
                var mediaTypesMap = this.MediaTypes.ToDictionary(a => a.MediaTypeId);

                foreach (var track in this.Tracks)
                {
                    if (genresMap.TryGetValue(track.GenreId, out var genre))
                        track.Genre = new LazyHashableObject(genre);
                    if (mediaTypesMap.TryGetValue(track.MediaTypeId, out var mediaType))
                        track.MediaType = new LazyHashableObject(mediaType);
                }

                foreach (var albumTracks in this.Tracks.GroupBy(a => a.AlbumId))
                {
                    var albumId = albumTracks.Key;
                    if (albumsMap.TryGetValue(albumId, out var album))
                    {
                        album.Tracks = albumTracks.Select(track => new LazyTrack(track)).ToList();
                    }
                }

                foreach (var artistAlbums in this.Albums.GroupBy(a => a.ArtistId))
                {
                    var artistId = artistAlbums.Key;
                    if (artistsMap.TryGetValue(artistId, out var artist))
                    {
                        artist.Albums = artistAlbums.Select(album => new LazyAlbum(album)).ToList();
                    }
                }

                this.Library = new Library
                {
                    Artists = this.Artists.Select(artist => new LazyArtist(artist)).ToList()
                };
            }
        }
    }
}
