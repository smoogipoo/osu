// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using osu.Game.Online.API;

namespace osu.Game.Online.Rooms
{
    [Serializable]
    [MessagePackObject]
    public class MultiplayerPlaylistItem
    {
        [Key(0)]
        public long ID { get; set; }

        [Key(1)]
        public int OwnerID { get; set; }

        [Key(2)]
        public int BeatmapID { get; set; }

        [Key(3)]
        public string BeatmapChecksum { get; set; } = string.Empty;

        [Key(4)]
        public int RulesetID { get; set; }

        [Key(5)]
        public IEnumerable<APIMod> RequiredMods { get; set; } = Enumerable.Empty<APIMod>();

        [Key(6)]
        public IEnumerable<APIMod> AllowedMods { get; set; } = Enumerable.Empty<APIMod>();

        [Key(7)]
        public bool Expired { get; set; }

        public MultiplayerPlaylistItem()
        {
        }

        public MultiplayerPlaylistItem(PlaylistItem item)
        {
            ID = item.ID;
            BeatmapID = item.BeatmapID;
            BeatmapChecksum = item.Beatmap.Value?.MD5Hash ?? string.Empty;
            RulesetID = item.RulesetID;
            RequiredMods = item.RequiredMods.Select(m => new APIMod(m)).ToArray();
            AllowedMods = item.AllowedMods.Select(m => new APIMod(m)).ToArray();
            Expired = item.Expired;
        }
    }
}
