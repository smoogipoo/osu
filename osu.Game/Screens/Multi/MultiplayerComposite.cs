// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;

namespace osu.Game.Screens.Multi
{
    public class MultiplayerComposite : CompositeDrawable
    {
        protected readonly Bindable<int?> RoomID = new Bindable<int?>();
        protected readonly Bindable<string> Name = new Bindable<string>();
        protected readonly Bindable<User> Host = new Bindable<User>();
        protected readonly Bindable<RoomStatus> Status = new Bindable<RoomStatus>();
        protected readonly Bindable<GameType> Type = new Bindable<GameType>();
        protected readonly BindableList<PlaylistItem> Playlist = new BindableList<PlaylistItem>();
        protected readonly Bindable<IEnumerable<User>> Participants = new Bindable<IEnumerable<User>>();
        protected readonly Bindable<int> ParticipantCount = new Bindable<int>();
        protected readonly Bindable<int?> MaxParticipants = new Bindable<int?>();
        protected readonly Bindable<DateTimeOffset> EndDate = new Bindable<DateTimeOffset>();
        protected readonly Bindable<RoomAvailability> Availability = new Bindable<RoomAvailability>();
        protected readonly Bindable<TimeSpan> Duration = new Bindable<TimeSpan>();

        private readonly Bindable<BeatmapInfo> currentBeatmap = new Bindable<BeatmapInfo>();
        public IBindable<BeatmapInfo> CurrentBeatmap => currentBeatmap;

        private readonly Bindable<IEnumerable<Mod>> currentMods = new Bindable<IEnumerable<Mod>>();
        public IBindable<IEnumerable<Mod>> CurrentMods => currentMods;

        private readonly Bindable<RulesetInfo> currentRuleset = new Bindable<RulesetInfo>();
        public IBindable<RulesetInfo> CurrentRuleset => currentRuleset;

        public MultiplayerComposite()
        {
            Playlist.ItemsAdded += _ => updatePlaylist();
            Playlist.ItemsRemoved += _ => updatePlaylist();
        }

        private void updatePlaylist()
        {
            // Todo: We only ever have one playlist item for now. In the future, this will be user-settable

            var playlistItem = Playlist.FirstOrDefault();

            currentBeatmap.Value = playlistItem?.Beatmap;
            currentMods.Value = playlistItem?.RequiredMods ?? Enumerable.Empty<Mod>();
            currentRuleset.Value = playlistItem?.Ruleset;
        }
    }
}
