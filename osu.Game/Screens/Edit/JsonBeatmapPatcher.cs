// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using JsonDiffPatch;
using Newtonsoft.Json.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.Skinning;
using Decoder = osu.Game.Beatmaps.Formats.Decoder;

namespace osu.Game.Screens.Edit
{
    public class JsonBeatmapPatcher
    {
        private readonly EditorBeatmap editorBeatmap;

        public JsonBeatmapPatcher(EditorBeatmap editorBeatmap)
        {
            this.editorBeatmap = editorBeatmap;
        }

        public void Patch(string currentState, string newState)
        {
            var differ = new JsonDiffer();
            var patchDocument = differ.Diff(JToken.Parse(currentState), JToken.Parse(newState), false);

            IBeatmap finalBeatmap = readBeatmap(newState);

            editorBeatmap.BeginChange();

            var replacedObjects = new HashSet<int>();

            foreach (var op in patchDocument.Operations)
            {
                if (tryHandleHitObjectList(finalBeatmap, op))
                    continue;

                tryHandleIndividualHitObject(finalBeatmap, op, replacedObjects);
            }

            editorBeatmap.EndChange();
        }

        private bool tryHandleIndividualHitObject(IBeatmap finalBeatmap, Operation operation, HashSet<int> replacedObjects)
        {
            string pathString = operation.Path.ToString();

            var match = Regex.Match(pathString, @"^/hit_objects/\$items/(\d+)(.*)$");
            if (!match.Success)
                return false;

            int objectIndex = int.Parse(match.Groups[1].Value);

            if (!string.IsNullOrEmpty(match.Groups[2].Value))
            {
                // We don't support changing individual operations of hitobjects - just replace the hitobject in entirety.
                if (!replacedObjects.Contains(objectIndex))
                {
                    editorBeatmap.RemoveAt(objectIndex);
                    editorBeatmap.Insert(objectIndex, finalBeatmap.HitObjects[objectIndex]);

                    replacedObjects.Add(objectIndex);
                }
            }
            else
            {
                switch (operation)
                {
                    case AddOperation _:
                        editorBeatmap.Insert(objectIndex, finalBeatmap.HitObjects[objectIndex]);
                        break;

                    case RemoveOperation _:
                        editorBeatmap.RemoveAt(objectIndex);
                        break;

                    case ReplaceOperation _:
                        editorBeatmap.RemoveAt(objectIndex);
                        editorBeatmap.Insert(objectIndex, finalBeatmap.HitObjects[objectIndex]);
                        break;
                }
            }

            return true;
        }

        private bool tryHandleHitObjectList(IBeatmap finalBeatmap, Operation operation)
        {
            string pathString = operation.Path.ToString();

            var match = Regex.Match(pathString, @"^/hit_objects/\$items$");
            if (!match.Success)
                return false;

            switch (operation)
            {
                case ReplaceOperation _:
                    editorBeatmap.Clear();
                    editorBeatmap.AddRange(finalBeatmap.HitObjects);
                    break;
            }

            return true;
        }

        private IBeatmap readBeatmap(string state)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(state)))
            using (var reader = new LineBufferedReader(stream, true))
            {
                var decoded = Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
                decoded.BeatmapInfo.Ruleset = editorBeatmap.BeatmapInfo.Ruleset;
                return new PassThroughWorkingBeatmap(decoded).GetPlayableBeatmap(editorBeatmap.BeatmapInfo.Ruleset);
            }
        }

        private class PassThroughWorkingBeatmap : WorkingBeatmap
        {
            private readonly IBeatmap beatmap;

            public PassThroughWorkingBeatmap(IBeatmap beatmap)
                : base(beatmap.BeatmapInfo, null)
            {
                this.beatmap = beatmap;
            }

            protected override IBeatmap GetBeatmap() => beatmap;

            protected override Texture GetBackground() => throw new NotImplementedException();

            protected override Track GetBeatmapTrack() => throw new NotImplementedException();

            protected internal override ISkin GetSkin() => throw new NotImplementedException();

            public override Stream GetStream(string storagePath) => throw new NotImplementedException();
        }
    }
}
