// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Game;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Play;
using Velopack;
using Velopack.Sources;

namespace osu.Desktop.Updater
{
    public partial class VelopackUpdateManager : Game.Updater.UpdateManager
    {
        private readonly UpdateManager updateManager;
        private INotificationOverlay notificationOverlay = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        private ILocalUserPlayInfo? localUserInfo { get; set; }

        private UpdateInfo? pendingUpdate;

        public VelopackUpdateManager()
        {
            updateManager = new UpdateManager(new GithubSource(@"https://github.com/smoogipoo/osu", null, false), new UpdateOptions
            {
                AllowVersionDowngrade = true,
            });
        }

        [BackgroundDependencyLoader]
        private void load(INotificationOverlay notifications)
        {
            notificationOverlay = notifications;
        }

        protected override async Task<bool> PerformUpdateCheck() => await checkForUpdateAsync().ConfigureAwait(false);

        private async Task<bool> checkForUpdateAsync(UpdateProgressNotification? notification = null)
        {
            // whether to check again in 30 minutes. generally only if there's an error or no update was found (yet).
            bool scheduleRecheck = false;

            try
            {
                // Avoid any kind of update checking while gameplay is running.
                if (localUserInfo?.IsPlaying.Value == true)
                {
                    scheduleRecheck = true;
                    return false;
                }

                // TODO: we should probably be checking if there's a more recent update, rather than shortcutting here.
                // Velopack does support this scenario (see https://github.com/ppy/osu/pull/28743#discussion_r1743495975).
                if (pendingUpdate != null)
                {
                    // If there is an update pending restart, show the notification to restart again.
                    notificationOverlay.Post(new UpdateApplicationCompleteNotification
                    {
                        Activated = () =>
                        {
                            restartToApplyUpdate();
                            return true;
                        }
                    });

                    return true;
                }

                pendingUpdate = await updateManager.CheckForUpdatesAsync().ConfigureAwait(false);

                // No update is available. We'll check again later.
                if (pendingUpdate == null)
                {
                    scheduleRecheck = true;
                    return false;
                }

                // An update is found, let's notify the user and start downloading it.
                if (notification == null)
                {
                    notification = new UpdateProgressNotification
                    {
                        CompletionClickAction = restartToApplyUpdate,
                    };

                    Schedule(() => notificationOverlay.Post(notification));
                }

                notification.StartDownload();

                try
                {
                    await updateManager.DownloadUpdatesAsync(pendingUpdate, p => notification.Progress = p / 100f).ConfigureAwait(false);

                    notification.State = ProgressNotificationState.Completed;
                }
                catch (Exception e)
                {
                    // In the case of an error, a separate notification will be displayed.
                    scheduleRecheck = true;
                    notification.FailDownload();
                    Logger.Error(e, @"update failed!");
                }
            }
            catch (Exception e)
            {
                // we'll ignore this and retry later. can be triggered by no internet connection or thread abortion.
                scheduleRecheck = true;
                Logger.Log($@"update check failed ({e.Message})");
            }
            finally
            {
                if (scheduleRecheck)
                {
                    Scheduler.AddDelayed(() => Task.Run(async () => await checkForUpdateAsync().ConfigureAwait(false)), 60000 * 30);
                }
            }

            return true;
        }

        private bool restartToApplyUpdate()
        {
            // TODO: Migrate this to async flow whenever available (see https://github.com/ppy/osu/pull/28743#discussion_r1740505665).
            // Currently there's an internal Thread.Sleep(300) which will cause a stutter when the user clicks to restart.
            updateManager.WaitExitThenApplyUpdates(pendingUpdate?.TargetFullRelease);
            Schedule(() => game.AttemptExit());
            return true;
        }
    }
}
