// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerRoomSounds : MultiplayerRoomComposite
    {
        [Resolved]
        private MultiplayerMatchSubScreen screen { get; set; }

        private Sample hostChangedSample;
        private Sample userJoinedSample;
        private Sample userLeftSample;
        private Sample userKickedSample;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            hostChangedSample = audio.Samples.Get(@"Multiplayer/host-changed");
            userJoinedSample = audio.Samples.Get(@"Multiplayer/player-joined");
            userLeftSample = audio.Samples.Get(@"Multiplayer/player-left");
            userKickedSample = audio.Samples.Get(@"Multiplayer/player-kicked");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Host.BindValueChanged(hostChanged);
        }

        protected override void UserJoined(MultiplayerRoomUser user)
        {
            base.UserJoined(user);

            if (screen.IsCurrentScreen())
                userJoinedSample?.Play();
        }

        protected override void UserLeft(MultiplayerRoomUser user)
        {
            base.UserLeft(user);

            if (screen.IsCurrentScreen())
                userLeftSample?.Play();
        }

        protected override void UserKicked(MultiplayerRoomUser user)
        {
            base.UserKicked(user);

            if (screen.IsCurrentScreen())
                userKickedSample?.Play();
        }

        private void hostChanged(ValueChangedEvent<APIUser> value)
        {
            // only play sound when the host changes from an already-existing host.
            if (value.OldValue == null) return;

            if (screen.IsCurrentScreen())
                hostChangedSample?.Play();
        }
    }
}
