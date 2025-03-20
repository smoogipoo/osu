// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osuTK.Graphics;

namespace osu.Game.Users
{
    /// <summary>
    /// Base class for all structures describing the user's current activity.
    /// </summary>
    /// <remarks>
    /// Warning: keep <see cref="UnionAttribute"/> specs consistent with
    /// <see cref="SignalRWorkaroundTypes.BASE_TYPE_MAPPING"/>.
    /// </remarks>
    [Serializable]
    [MessagePackObject]
    [Union(11, typeof(ChoosingBeatmap))]
    [Union(12, typeof(InSoloGame))]
    [Union(13, typeof(WatchingReplay))]
    [Union(14, typeof(SpectatingUser))]
    [Union(21, typeof(SearchingForLobby))]
    [Union(22, typeof(InLobby))]
    [Union(23, typeof(InMultiplayerGame))]
    [Union(24, typeof(SpectatingMultiplayerGame))]
    [Union(31, typeof(InPlaylistGame))]
    [Union(41, typeof(EditingBeatmap))]
    [Union(42, typeof(ModdingBeatmap))]
    [Union(43, typeof(TestingBeatmap))]
    public abstract class UserActivity : IEquatable<UserActivity>
    {
        public abstract string GetStatus(bool hideIdentifiableInformation = false);
        public virtual string? GetDetails(bool hideIdentifiableInformation = false) => null;

        public virtual Color4 GetAppropriateColour(OsuColour colours) => colours.GreenDarker;

        /// <summary>
        /// Returns the ID of the beatmap involved in this activity, if applicable and/or available.
        /// </summary>
        /// <param name="hideIdentifiableInformation"></param>
        public virtual int? GetBeatmapID(bool hideIdentifiableInformation = false) => null;

        public abstract bool Equals(UserActivity? other);

        [MessagePackObject]
        public class ChoosingBeatmap : UserActivity
        {
            public override string GetStatus(bool hideIdentifiableInformation = false) => "Choosing a beatmap";

            public override bool Equals(UserActivity? other)
                => other is ChoosingBeatmap;
        }

        [MessagePackObject]
        [Union(12, typeof(InSoloGame))]
        [Union(23, typeof(InMultiplayerGame))]
        [Union(24, typeof(SpectatingMultiplayerGame))]
        [Union(31, typeof(InPlaylistGame))]
        public abstract class InGame : UserActivity
        {
            [Key(0)]
            public int BeatmapID { get; set; }

            [Key(1)]
            public string BeatmapDisplayTitle { get; set; } = string.Empty;

            [Key(2)]
            public int RulesetID { get; set; }

            [Key(3)]
            public string RulesetPlayingVerb { get; set; } = string.Empty; // TODO: i'm going with this for now, but this is wasteful

            protected InGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
            {
                BeatmapID = beatmapInfo.OnlineID;
                BeatmapDisplayTitle = beatmapInfo.GetDisplayTitle();

                RulesetID = ruleset.OnlineID;
                RulesetPlayingVerb = ruleset.CreateInstance().PlayingVerb;
            }

            [SerializationConstructor]
            protected InGame() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => RulesetPlayingVerb;
            public override string GetDetails(bool hideIdentifiableInformation = false) => BeatmapDisplayTitle;
            public override int? GetBeatmapID(bool hideIdentifiableInformation = false) => BeatmapID;

            public override bool Equals(UserActivity? other)
                => other is InGame otherInGame
                   && BeatmapID == otherInGame.BeatmapID
                   && string.Equals(BeatmapDisplayTitle, otherInGame.BeatmapDisplayTitle, StringComparison.Ordinal)
                   && RulesetID == otherInGame.RulesetID
                   && string.Equals(RulesetPlayingVerb, otherInGame.RulesetPlayingVerb, StringComparison.Ordinal);
        }

        [MessagePackObject]
        public class InSoloGame : InGame
        {
            public InSoloGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }

            [SerializationConstructor]
            public InSoloGame() { }

            public override bool Equals(UserActivity? other)
                => other is InSoloGame
                   && base.Equals(other);
        }

        [MessagePackObject]
        public class InMultiplayerGame : InGame
        {
            public InMultiplayerGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }

            [SerializationConstructor]
            public InMultiplayerGame()
            {
            }

            public override string GetStatus(bool hideIdentifiableInformation = false) => $@"{base.GetStatus(hideIdentifiableInformation)} with others";

            public override bool Equals(UserActivity? other)
                => other is InMultiplayerGame
                   && base.Equals(other);
        }

        [MessagePackObject]
        public class InPlaylistGame : InGame
        {
            public InPlaylistGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }

            [SerializationConstructor]
            public InPlaylistGame() { }

            public override bool Equals(UserActivity? other)
                => other is InPlaylistGame
                   && base.Equals(other);
        }

        [MessagePackObject]
        public class TestingBeatmap : EditingBeatmap
        {
            public TestingBeatmap(IBeatmapInfo beatmapInfo)
                : base(beatmapInfo)
            {
            }

            [SerializationConstructor]
            public TestingBeatmap() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => "Testing a beatmap";

            public override bool Equals(UserActivity? other)
                => other is TestingBeatmap
                   && base.Equals(other);
        }

        [MessagePackObject]
        public class EditingBeatmap : UserActivity
        {
            [Key(0)]
            public int BeatmapID { get; set; }

            [Key(1)]
            public string BeatmapDisplayTitle { get; set; } = string.Empty;

            public EditingBeatmap(IBeatmapInfo info)
            {
                BeatmapID = info.OnlineID;
                BeatmapDisplayTitle = info.GetDisplayTitle();
            }

            [SerializationConstructor]
            public EditingBeatmap() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => @"Editing a beatmap";

            public override string GetDetails(bool hideIdentifiableInformation = false) => hideIdentifiableInformation
                // For now let's assume that showing the beatmap a user is editing could reveal unwanted information.
                ? string.Empty
                : BeatmapDisplayTitle;

            public override int? GetBeatmapID(bool hideIdentifiableInformation = false) => hideIdentifiableInformation
                // For now let's assume that showing the beatmap a user is editing could reveal unwanted information.
                ? null
                : BeatmapID;

            public override bool Equals(UserActivity? other)
                => other is EditingBeatmap otherEditingBeatmap
                   && BeatmapID == otherEditingBeatmap.BeatmapID
                   && string.Equals(BeatmapDisplayTitle, otherEditingBeatmap.BeatmapDisplayTitle, StringComparison.Ordinal);
        }

        [MessagePackObject]
        public class ModdingBeatmap : EditingBeatmap
        {
            public ModdingBeatmap(IBeatmapInfo info)
                : base(info)
            {
            }

            [SerializationConstructor]
            public ModdingBeatmap() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => "Modding a beatmap";
            public override Color4 GetAppropriateColour(OsuColour colours) => colours.PurpleDark;

            public override bool Equals(UserActivity? other)
                => other is ModdingBeatmap
                   && base.Equals(other);
        }

        [MessagePackObject]
        public class WatchingReplay : UserActivity
        {
            [Key(0)]
            public long ScoreID { get; set; }

            [Key(1)]
            public string PlayerName { get; set; } = string.Empty;

            [Key(2)]
            public int BeatmapID { get; set; }

            [Key(3)]
            public string? BeatmapDisplayTitle { get; set; }

            public WatchingReplay(ScoreInfo score)
            {
                ScoreID = score.OnlineID;
                PlayerName = score.User.Username;
                BeatmapID = score.BeatmapInfo?.OnlineID ?? -1;
                BeatmapDisplayTitle = score.BeatmapInfo?.GetDisplayTitle();
            }

            [SerializationConstructor]
            public WatchingReplay() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => hideIdentifiableInformation ? @"Watching a replay" : $@"Watching {PlayerName}'s replay";
            public override string? GetDetails(bool hideIdentifiableInformation = false) => BeatmapDisplayTitle;

            public override bool Equals(UserActivity? other)
                => other is WatchingReplay otherWatchingReplay
                   && ScoreID == otherWatchingReplay.ScoreID;
        }

        [MessagePackObject]
        public class SpectatingUser : WatchingReplay
        {
            public SpectatingUser(ScoreInfo score)
                : base(score)
            {
            }

            [SerializationConstructor]
            public SpectatingUser() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => hideIdentifiableInformation ? @"Spectating a user" : $@"Spectating {PlayerName}";

            public override bool Equals(UserActivity? other)
                => other is SpectatingUser
                   && base.Equals(other);
        }

        [MessagePackObject]
        public class SpectatingMultiplayerGame : InGame
        {
            public SpectatingMultiplayerGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }

            [SerializationConstructor]
            public SpectatingMultiplayerGame() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => $"Watching others {base.GetStatus(hideIdentifiableInformation).ToLowerInvariant()}";

            public override bool Equals(UserActivity? other)
                => other is SpectatingMultiplayerGame
                   && base.Equals(other);
        }

        [MessagePackObject]
        public class SearchingForLobby : UserActivity
        {
            public override string GetStatus(bool hideIdentifiableInformation = false) => @"Looking for a lobby";

            public override bool Equals(UserActivity? other)
                => other is SearchingForLobby;
        }

        [MessagePackObject]
        public class InLobby : UserActivity
        {
            [Key(0)]
            public long RoomID { get; set; }

            [Key(1)]
            public string RoomName { get; set; } = string.Empty;

            public InLobby(Room room)
            {
                RoomID = room.RoomID ?? -1;
                RoomName = room.Name;
            }

            [SerializationConstructor]
            public InLobby() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => @"In a lobby";

            public override string? GetDetails(bool hideIdentifiableInformation = false) => hideIdentifiableInformation
                ? null
                : RoomName;

            public override bool Equals(UserActivity? other)
                => other is InLobby otherInLobby
                   && RoomID == otherInLobby.RoomID
                   && RoomName == otherInLobby.RoomName;
        }
    }
}
