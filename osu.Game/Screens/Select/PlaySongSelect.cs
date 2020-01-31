// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : SongSelect
    {
        private bool removeAutoModOnResume;
        private OsuScreen player;

        public override bool AllowExternalScreenChange => true;

        protected override UserActivity InitialActivity => new UserActivity.ChoosingBeatmap();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BeatmapOptions.AddButton(@"Edit", @"beatmap", FontAwesome.Solid.PencilAlt, colours.Yellow, () =>
            {
                ValidForResume = false;
                Edit();
            }, Key.Number4);

            BeatmapOptions.AddButton(@"Clear", @"local scores", FontAwesome.Solid.Eraser, colours.Purple, () => clearScores(Beatmap.Value.BeatmapInfo), Key.Number2);

            ((PlayBeatmapDetailArea)BeatmapDetails).Leaderboard.ScoreSelected += score => this.Push(new SoloResults(score));
        }

        public override void OnResuming(IScreen last)
        {
            player = null;

            if (removeAutoModOnResume)
            {
                var autoType = Ruleset.Value.CreateInstance().GetAutoplayMod().GetType();
                ModSelect.DeselectTypes(new[] { autoType }, true);
                removeAutoModOnResume = false;
            }

            ((PlayBeatmapDetailArea)BeatmapDetails).Leaderboard.RefreshScores();

            base.OnResuming(last);
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new PlayBeatmapDetailArea();

        protected override bool OnStart()
        {
            if (player != null) return false;

            // Ctrl+Enter should start map with autoplay enabled.
            if (GetContainingInputManager().CurrentState?.Keyboard.ControlPressed == true)
            {
                var auto = Ruleset.Value.CreateInstance().GetAutoplayMod();
                var autoType = auto.GetType();

                var mods = Mods.Value;

                if (mods.All(m => m.GetType() != autoType))
                {
                    Mods.Value = mods.Append(auto).ToArray();
                    removeAutoModOnResume = true;
                }
            }

            Beatmap.Value.Track.Looping = false;

            SampleConfirm?.Play();

            LoadComponentAsync(player = new PlayerLoader(() => new Player()), l =>
            {
                if (this.IsCurrentScreen()) this.Push(player);
            });

            return true;
        }

        private void clearScores(BeatmapInfo beatmap)
        {
            if (beatmap == null || beatmap.ID <= 0) return;

            DialogOverlay?.Push(new BeatmapClearScoresDialog(beatmap, () =>
                // schedule done here rather than inside the dialog as the dialog may fade out and never callback.
                Schedule(() => ((PlayBeatmapDetailArea)BeatmapDetails).Leaderboard.RefreshScores())));
        }
    }
}
