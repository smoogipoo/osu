// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.Backgrounds;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class ButtonWithTrianglesExposed : ReadyButton
    {
        public new Triangles Triangles => base.Triangles;
    }
}
