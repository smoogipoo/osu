// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.RealtimeMultiplayer;

namespace osu.Game.Screens.Multi.Realtime
{
    public abstract class MatchComposite : CompositeDrawable
    {
        [CanBeNull]
        protected MultiplayerRoom Room => roomManager?.Client.Room;

        [Resolved]
        private RealtimeRoomManager roomManager { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            roomManager.Client.RoomChanged += OnRoomChanged;
            OnRoomChanged();
        }

        protected virtual void OnRoomChanged()
        {
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (roomManager != null)
                roomManager.Client.RoomChanged -= OnRoomChanged;
        }
    }
}
