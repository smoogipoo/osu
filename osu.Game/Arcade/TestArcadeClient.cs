// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;

namespace osu.Game.Arcade
{
    public partial class TestArcadeClient : ArcadeClient
    {
        public override IBindable<bool> IsConnected { get; } = new Bindable<bool>(true);

        public Func<string, ArcadeIdentity> GetUserWithCodeFunc { get; set; } = _ => throw new NotImplementedException();

        public Func<ArcadeUserStats[]> FetchLeaderboardFunc { get; set; } = () => [];

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public override Task<ArcadeIdentity> GetUserWithCode(string code)
            => Task.FromResult(GetUserWithCodeFunc(code));

        public override Task<ArcadeUserStats[]> FetchLeaderboard()
            => Task.FromResult(FetchLeaderboardFunc());

        public override Task<MultiplayerRoom[]> GetActiveRooms()
            => Task.FromResult(new[]
            {
                new MultiplayerRoom(0)
                {
                    Settings =
                    {
                        PlaylistItemId = 1
                    },
                    Playlist =
                    [
                        new MultiplayerPlaylistItem(new PlaylistItem(new APIBeatmap
                        {
                            BeatmapSet = new APIBeatmapSet
                            {
                                Artist = "Some Artist",
                                Title = "Some Title",
                                AuthorString = "Some Author"
                            },
                            DifficultyName = "Some Difficulty"
                        }))
                        {
                            ID = 1
                        }
                    ],
                    Users =
                    [
                        new MultiplayerRoomUser(1),
                        new MultiplayerRoomUser(2)
                    ]
                }
            });

        public override Task Connect(ArcadeIdentity identity)
            => Connect(api.LocalUser.Value.OnlineID, identity);

        public Task Connect(int clientId, ArcadeIdentity identity)
            => ((IArcadeClient)this).UserConnected(clientId, identity);

        public override Task Disconnect()
            => Disconnect(api.LocalUser.Value.OnlineID);

        public Task Disconnect(int clientId)
            => ((IArcadeClient)this).UserDisconnected(clientId);

        public static ArcadeUserStats CreateUserStats(int userId, int victories) => new ArcadeUserStats
        {
            UserId = userId,
            Username = $"User {userId}",
            Victories = victories
        };
    }
}
