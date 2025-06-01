using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using UntitledJBTClone.Game.Models;

namespace UntitledJBTClone.Game.Services
{
    public class MemonParser
    {
        public static MemonFile ParseMemonFile(string filePath)
        {
            var jsonText = File.ReadAllText(filePath);
            var jsonNode = JsonNode.Parse(jsonText);

            if (jsonNode == null)
                throw new InvalidOperationException("Invalid JSON file");

            var memonFile = new MemonFile();

            // Parse version
            if (jsonNode["version"] != null)
                memonFile.Version = jsonNode["version"]!.GetValue<string>();

            // Parse metadata
            if (jsonNode["metadata"] != null)
                memonFile.Metadata = ParseMetadata(jsonNode["metadata"]!);

            // Parse timing
            if (jsonNode["timing"] != null)
                memonFile.Timing = ParseTiming(jsonNode["timing"]!);
            else
            {
                // Handle v0.2.0 format where BPM and offset are in metadata
                var timing = new MemonTiming();
                if (jsonNode["metadata"] != null)
                {
                    var metadata = jsonNode["metadata"]!;
                    if (metadata["BPM"] != null)
                    {
                        var bpm = ParseDecimal(metadata["BPM"]!);
                        timing.Bpms = new List<MemonBpm> { new MemonBpm { Beat = 0, Bpm = bpm } };
                    }
                    if (metadata["offset"] != null)
                    {
                        timing.Offset = ParseDecimal(metadata["offset"]!);
                    }
                }
                memonFile.Timing = timing;
            }

            // Parse charts data
            if (jsonNode["data"] != null)
                memonFile.Data = ParseCharts(jsonNode["data"]!, memonFile.Timing);

            return memonFile;
        }

        private static MemonMetadata ParseMetadata(JsonNode metadataNode)
        {
            var metadata = new MemonMetadata();

            // Handle both v1.0.0 and v0.2.0 formats
            if (metadataNode["title"] != null)
                metadata.Title = metadataNode["title"]!.GetValue<string>();
            else if (metadataNode["song title"] != null)
                metadata.Title = metadataNode["song title"]!.GetValue<string>();

            if (metadataNode["artist"] != null)
                metadata.Artist = metadataNode["artist"]!.GetValue<string>();

            if (metadataNode["audio"] != null)
                metadata.Audio = metadataNode["audio"]!.GetValue<string>();
            else if (metadataNode["music path"] != null)
                metadata.Audio = metadataNode["music path"]!.GetValue<string>();

            if (metadataNode["jacket"] != null)
                metadata.Jacket = metadataNode["jacket"]!.GetValue<string>();
            else if (metadataNode["album cover path"] != null)
                metadata.Jacket = metadataNode["album cover path"]!.GetValue<string>();

            if (metadataNode["preview"] != null)
                metadata.Preview = ParsePreview(metadataNode["preview"]!);

            return metadata;
        }

        private static MemonPreview ParsePreview(JsonNode previewNode)
        {
            var preview = new MemonPreview();

            if (previewNode.GetValueKind() == JsonValueKind.String)
            {
                // Preview is a file path
                preview.File = previewNode.GetValue<string>();
            }
            else if (previewNode.GetValueKind() == JsonValueKind.Object)
            {
                // Preview is a time range
                if (previewNode["start"] != null)
                    preview.Start = ParseDecimal(previewNode["start"]!);

                if (previewNode["duration"] != null)
                    preview.Duration = ParseDecimal(previewNode["duration"]!);
            }

            return preview;
        }

        private static MemonTiming ParseTiming(JsonNode timingNode)
        {
            var timing = new MemonTiming();

            if (timingNode["offset"] != null)
                timing.Offset = ParseDecimal(timingNode["offset"]!);

            if (timingNode["resolution"] != null)
                timing.Resolution = timingNode["resolution"]!.GetValue<int>();

            if (timingNode["bpms"] != null)
                timing.Bpms = ParseBpms(timingNode["bpms"]!);

            if (timingNode["hakus"] != null)
                timing.Hakus = ParseHakus(timingNode["hakus"]!);

            return timing;
        }

        private static List<MemonBpm> ParseBpms(JsonNode bpmsNode)
        {
            var bpms = new List<MemonBpm>();

            if (bpmsNode.AsArray() != null)
            {
                foreach (var bpmNode in bpmsNode.AsArray())
                {
                    if (bpmNode != null)
                    {
                        var bpm = new MemonBpm();

                        if (bpmNode["beat"] != null)
                            bpm.Beat = ParseTimeInBeats(bpmNode["beat"]!);

                        if (bpmNode["bpm"] != null)
                            bpm.Bpm = ParseDecimal(bpmNode["bpm"]!);

                        bpms.Add(bpm);
                    }
                }
            }

            return bpms;
        }

        private static List<double> ParseHakus(JsonNode hakusNode)
        {
            var hakus = new List<double>();

            if (hakusNode.AsArray() != null)
            {
                foreach (var hakuNode in hakusNode.AsArray())
                {
                    if (hakuNode != null)
                        hakus.Add(ParseTimeInBeats(hakuNode));
                }
            }

            return hakus;
        }

        private static Dictionary<string, MemonChart> ParseCharts(JsonNode dataNode, MemonTiming defaultTiming)
        {
            var charts = new Dictionary<string, MemonChart>();

            if (dataNode.AsObject() != null)
            {
                foreach (var kvp in dataNode.AsObject())
                {
                    if (kvp.Value != null)
                    {
                        var chart = ParseChart(kvp.Value, defaultTiming);
                        charts[kvp.Key] = chart;
                    }
                }
            }

            return charts;
        }

        private static MemonChart ParseChart(JsonNode chartNode, MemonTiming defaultTiming)
        {
            var chart = new MemonChart();

            if (chartNode["level"] != null)
                chart.Level = ParseDecimal(chartNode["level"]!);

            if (chartNode["resolution"] != null)
                chart.Resolution = chartNode["resolution"]!.GetValue<int>();

            // Use chart-specific timing if available, otherwise use default
            if (chartNode["timing"] != null)
                chart.Timing = ParseTiming(chartNode["timing"]!);
            else
                chart.Timing = defaultTiming;

            if (chartNode["notes"] != null)
                chart.Notes = ParseNotes(chartNode["notes"]!);

            return chart;
        }

        private static List<MemonNote> ParseNotes(JsonNode notesNode)
        {
            var notes = new List<MemonNote>();

            if (notesNode.AsArray() != null)
            {
                foreach (var noteNode in notesNode.AsArray())
                {
                    if (noteNode != null)
                    {
                        var note = new MemonNote();

                        if (noteNode["n"] != null)
                            note.N = noteNode["n"]!.GetValue<int>();

                        if (noteNode["t"] != null)
                            note.T = ParseTimeInBeats(noteNode["t"]!);

                        if (noteNode["l"] != null)
                            note.L = ParseTimeInBeats(noteNode["l"]!);

                        if (noteNode["p"] != null)
                            note.P = noteNode["p"]!.GetValue<int>();

                        notes.Add(note);
                    }
                }
            }

            return notes;
        }

        private static double ParseTimeInBeats(JsonNode timeNode)
        {
            if (timeNode.GetValueKind() == JsonValueKind.Number)
            {
                // Time as integer ticks
                return timeNode.GetValue<double>();
            }
            else if (timeNode.GetValueKind() == JsonValueKind.Array && timeNode.AsArray()?.Count == 3)
            {
                // Time as fraction [integral, numerator, denominator]
                var array = timeNode.AsArray();
                var integral = array![0]!.GetValue<double>();
                var numerator = array[1]!.GetValue<double>();
                var denominator = array[2]!.GetValue<double>();

                return integral + (numerator / denominator);
            }

            return 0;
        }

        private static double ParseDecimal(JsonNode decimalNode)
        {
            if (decimalNode.GetValueKind() == JsonValueKind.Number)
            {
                return decimalNode.GetValue<double>();
            }
            else if (decimalNode.GetValueKind() == JsonValueKind.String)
            {
                if (double.TryParse(decimalNode.GetValue<string>(), out double result))
                    return result;
            }

            return 0;
        }
    }
}