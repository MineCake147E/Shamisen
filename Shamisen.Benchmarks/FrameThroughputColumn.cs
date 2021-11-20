using System;
using System.Linq;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Shamisen.Benchmarks
{
    public class FrameThroughputColumn : IColumn
    {
        #region Properties
        public FrameThroughputColumn(Func<BenchmarkCase, int> frameSelector)
        {
            FrameSelector = frameSelector ?? throw new ArgumentNullException(nameof(frameSelector));
        }

        public string Id => "FrameThroughput";
        public string ColumnName { get; init; } = "Frame Throughput [Frames/s]";
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Custom;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType => UnitType.Dimensionless;
        public string Legend { get; init; } = $"Frames processed per second";

        public Func<BenchmarkCase, int> FrameSelector { get; }
        public bool IsAvailable(Summary summary) => true;
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase) => GetValue(summary, benchmarkCase, summary.Style);
        #endregion
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
        {
            try
            {
                var nf = FrameSelector?.Invoke(benchmarkCase);
                var frames = (double)nf * 1.0E9f;
                var mean = summary[benchmarkCase].ResultStatistics.Mean;
                var throughput = frames / mean;
                return $"{throughput:#,#.00000000}";
            }
            catch (Exception ex)
            {
                foreach (var item in benchmarkCase.Parameters.Items.Select(a => $"{a}").ToArray())
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine(ex.ToString());
                return "N/A";
            }
        }
    }
}
