// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using Foundation;
using osu.Framework.iOS;
using osu.Game.Tests;
using UIKit;

namespace osu.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : GameAppDelegate
    {
        protected override Framework.Game CreateGame() => new OsuTestBrowser();
    }
}
