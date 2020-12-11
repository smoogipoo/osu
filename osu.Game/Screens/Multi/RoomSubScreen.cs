// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.Multi
{
    [Cached(typeof(IPreviewTrackOwner))]
    public abstract class RoomSubScreen : MultiplayerSubScreen, IPreviewTrackOwner
    {
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool OnExiting(IScreen next)
        {
            RoomManager?.PartRoom();
            Mods.Value = Array.Empty<Mod>();

            return base.OnExiting(next);
        }
    }
}
