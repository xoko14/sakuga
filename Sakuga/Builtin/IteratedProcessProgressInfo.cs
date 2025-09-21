using System.Drawing;
using Pastel;

namespace Sakuga.Builtin;

public class IteratedProcessProgressInfoModel
{
    public int BarLength { get; set; }
    public int SweepSpeed { get; set; }

    internal int TotalCount;
    internal int Index;
    internal double TimeAccumulator;
    internal TimeSpan TimeTaken;
    internal TimeSpan AvgTime;
    internal TimeSpan EstimatedTime;
    internal List<string> Logs = [];
  
}

public class IteratedProcessProgressInfo<T>(IteratedProcessProgressInfoModel model, IEnumerable<T> iterable, Func<int, T, Action<string>, Task> task) : Animation<IteratedProcessProgressInfoModel>(model)
{
private decimal _sweepLocation = 0;
    protected override async Task Process(IteratedProcessProgressInfoModel model)
    {
        var logger = GetLogger(model);
        model.TotalCount = iterable.Count();
        for (var i = 0; i < model.TotalCount; i++)
        {
            var startTime = DateTime.Now;
            var item = iterable.ElementAt(i);
            await task(i, item, logger);
            var endTime = DateTime.Now;
            model.TimeTaken = endTime - startTime;
            model.Index = i + 1;
            model.TimeAccumulator+= model.TimeTaken.TotalMilliseconds;
            model.AvgTime = TimeSpan.FromMilliseconds(model.TimeAccumulator / model.Index);
            model.EstimatedTime = (model.TotalCount - model.Index) * model.AvgTime;

        }
    }

    private static Action<string> GetLogger(IteratedProcessProgressInfoModel model)
    {
        return (s) =>
        {
            model.Logs.Add(s);
        };
    }

    protected override Element View(ModelAccessor<IteratedProcessProgressInfoModel> model)
    {
        var totalCount = model.Get(m => m.TotalCount);
        var index = model.Get(m => m.Index);
        var barLength = model.Get(m => m.BarLength);
        var sweepSpeed = model.Get(m => m.SweepSpeed);
        var timeTaken = model.Get(m => m.TimeTaken);
        var estimatedTime = model.Get(m => m.EstimatedTime);
        var avgTime = model.Get(m => m.AvgTime);
        var logs = model.Get(m => m.Logs);

        var logList = new LineStack();
        logList.AddRange(logs.Select(l => (Element)l));
        var info = TimeStats(totalCount, index, timeTaken, estimatedTime, avgTime);
        var progressBar = ProgressBar(totalCount, index, barLength, sweepSpeed);
        return new LineStack
        {
            "Logs:",
            logList,
            info,
            progressBar,
        };
    }

    private Element ProgressBar(int totalCount, int index, int barLength, int speed)
    {
        var percentage = index / (decimal)totalCount;
        var percentageDisplay = $"{percentage * 100:0.00}%";
        var progressLength = Math.Round(barLength * percentage);
        var percentageStart = (barLength / 2) - (percentageDisplay.Length / 2);
        var progressBar = "";

        _sweepLocation += speed;
        if (_sweepLocation >= progressLength)
            _sweepLocation = 0;
        var highlight = (int)Math.Round(_sweepLocation);

        for (var i = 0; i < barLength; i++)
        {

            var pChar = " ";
            if (i < progressLength)
            {
                pChar = i == highlight ? "█".Pastel(Color.GreenYellow) : "█".Pastel(Color.LimeGreen);
                pChar.PastelBg(Color.LimeGreen);
            }
            if (i >= percentageStart && i < percentageStart + percentageDisplay.Length)
            {
                var pIdx = i - percentageStart;
                pChar = percentageDisplay[pIdx].ToString().Pastel(ConsoleColor.White);
                if(i < progressLength)
                    pChar.PastelBg(i == highlight ? Color.GreenYellow : Color.LimeGreen);
            }

            progressBar += pChar;
        }

        return $"[{progressBar}]";
    }

    private Element TimeStats(int totalCount, int index, TimeSpan timeTaken, TimeSpan estimatedTime, TimeSpan averageTime)
    {
        return new LineStack
        {
            $"({index}/{totalCount})",
            $"- Taken {timeTaken.TotalMilliseconds:F}ms",
            $"- Avg {averageTime.TotalMilliseconds:F}ms",
            $"- Estimated {estimatedTime.TotalMinutes:F}min left",
        };
    }
}

public class IteratedProcessAnimationBuilder<T>
{
    private Func<int, T, Action<string>, Task>? _loopHandler;
    private IEnumerable<T>? _enumerable;
    private int _barLength;

    public IteratedProcessAnimationBuilder<T> SetLoopHandler(IEnumerable<T> enumerable, Func<int, T, Action<string>, Task> handler)
    {
        _loopHandler = handler;
        _enumerable = enumerable;
        return this;
    }

    public IteratedProcessAnimationBuilder<T> SetProgressBarLength(int barLength)
    {
        _barLength = barLength;
        return this;
    }

    public IteratedProcessProgressInfo<T> Build()
    {
        var model = new IteratedProcessProgressInfoModel
        {
            BarLength = _barLength,
            SweepSpeed = 1,
        };
        return new IteratedProcessProgressInfo<T>(
            model,
            _enumerable?? new List<T>(),
            _loopHandler?? ((_, _, _) => Task.CompletedTask));
    }
}
