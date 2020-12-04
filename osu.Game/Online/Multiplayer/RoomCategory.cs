// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.Serialization;

namespace osu.Game.Online.Multiplayer
{
    public enum RoomCategory
    {
        [EnumMember(Value = "realtime")]
        Realtime,

        [EnumMember(Value = "normal")]
        Normal,

        [EnumMember(Value = "spotlight")]
        Spotlight
    }
}
