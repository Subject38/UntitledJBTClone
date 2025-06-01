using System;
using System.Collections.Generic;

namespace UntitledJBTClone.Game.Models
{
    public class MemonFile
    {
        public string Version { get; set; } = "1.0.0";
        public MemonMetadata Metadata { get; set; } = new();
        public MemonTiming Timing { get; set; } = new();
        public Dictionary<string, MemonChart> Data { get; set; } = new();
    }

    public class MemonMetadata
    {
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Audio { get; set; } = "";
        public string Jacket { get; set; } = "";
        public MemonPreview Preview { get; set; } = new();
    }

    public class MemonPreview
    {
        public double Start { get; set; } = 0;
        public double Duration { get; set; } = 10;
        public string File { get; set; } = "";
    }

    public class MemonTiming
    {
        public double Offset { get; set; } = 0;
        public int Resolution { get; set; } = 240;
        public List<MemonBpm> Bpms { get; set; } = new() { new MemonBpm { Beat = 0, Bpm = 120 } };
        public List<double> Hakus { get; set; } = new();
    }

    public class MemonBpm
    {
        public double Beat { get; set; }
        public double Bpm { get; set; }
    }

    public class MemonChart
    {
        public double Level { get; set; } = 1;
        public int Resolution { get; set; } = 240;
        public MemonTiming Timing { get; set; }
        public List<MemonNote> Notes { get; set; } = new();
    }

    public class MemonNote
    {
        public int N { get; set; } // Position (0-15)
        public double T { get; set; } // Time in beats
        public double L { get; set; } // Long note duration
        public int P { get; set; } // Tail position (0-5)

        public bool IsLongNote => L > 0;
    }

    public class SongInfo
    {
        public string FolderPath { get; set; } = "";
        public string MemonPath { get; set; } = "";
        public string AudioPath { get; set; } = "";
        public string JacketPath { get; set; } = "";
        public MemonFile MemonData { get; set; } = new();
    }
}