// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Platform;
using osu.Game.Collections;
using osu.Game.IO;
using osu.Game.IO.Legacy;
using osu.Game.Overlays.Notifications;
using osu.Game.Utils;

namespace osu.Game.Database
{
    public class LegacyCollectionExporter
    {
        private readonly Storage exportStorage;
        private readonly RealmAccess realm;

        public Action<Notification>? PostNotification { get; set; }

        public LegacyCollectionExporter(Storage storage, RealmAccess realm)
        {
            this.realm = realm;
            exportStorage = (storage as OsuStorage)?.GetExportStorage() ?? storage.GetStorageForDirectory(@"exports");
        }

        /// <summary>
        /// Exports a model to the default export location.
        /// This will create a notification tracking the progress of the export, visible to the user.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public void Export(CancellationToken cancellationToken = default)
        {
            const string base_filename = "collections";
            const string extension = ".db";

            realm.Run(r =>
            {
                var collections = r.All<BeatmapCollection>().ToList();

                IEnumerable<string> existingExports = exportStorage
                                                      .GetFiles(string.Empty, $"{base_filename}*{extension}")
                                                      .Concat(exportStorage.GetDirectories(string.Empty));

                string filename = NamingUtils.GetNextBestFilename(existingExports, $"{base_filename}{extension}");

                ProgressNotification notification = new ProgressNotification
                {
                    State = ProgressNotificationState.Active,
                    Text = $"Exporting {base_filename}...",
                };

                PostNotification?.Invoke(notification);

                using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, notification.CancellationToken);

                try
                {
                    using (var stream = exportStorage.CreateFileSafely(filename))
                    {
                        using (var sw = new SerializationWriter(stream))
                        {
                            sw.Write(int.Parse(DateTime.Now.ToString(@"yyyyMMdd")));
                            sw.Write(collections.Count);

                            foreach (var collection in collections)
                            {
                                sw.Write(collection.Name);
                                sw.Write(collection.BeatmapMD5Hashes.Count);

                                foreach (string checksum in collection.BeatmapMD5Hashes)
                                    sw.Write(checksum);
                            }
                        }
                    }
                }
                catch
                {
                    notification.State = ProgressNotificationState.Cancelled;

                    // cleanup if export is failed or canceled.
                    exportStorage.Delete(filename);
                    throw;
                }

                notification.CompletionText = $"Exported {base_filename}! Click to view.";
                notification.CompletionClickAction = () => exportStorage.PresentFileExternally(filename);
                notification.State = ProgressNotificationState.Completed;
            });
        }
    }
}
