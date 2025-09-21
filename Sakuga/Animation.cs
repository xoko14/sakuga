namespace Sakuga;

public abstract class Animation<TModel>(TModel model)
    where TModel : class
{
    protected const double FPS = 30;

    private bool _isProcessFinished = false;

    protected abstract Task Process(TModel model);
    protected abstract Element View(ModelAccessor<TModel> model);

    public async Task Run()
    {
        _isProcessFinished = false;
        var processTask = ProcessInner();

        var modelAccessor = new ModelAccessor<TModel>(model);

        var isLast = false;
        do
        {
            isLast = _isProcessFinished;
            await ViewInner(modelAccessor, isLast);
        }while(!isLast);

        await processTask;

    }

    private async Task ProcessInner()
    {
        await Process(model);
        _isProcessFinished = true;
    }

    private async Task ViewInner(ModelAccessor<TModel> model, bool isLast)
    {
        var t1 = DateTime.Now;

        var buffer = "\u001b[2K"+View(model).Draw().Replace(Environment.NewLine, Environment.NewLine+"\u001b[2K");
        var lines = buffer.Split(Environment.NewLine).Length;
        Console.WriteLine(buffer);
        if(!isLast)
            Console.Write($"\u001b[{lines}A");

        var targetDrawTime = 1000 / FPS;
        var t2 = DateTime.Now;
        var takenTime = (t2 - t1).TotalMilliseconds;
        var diffTime = (int)(targetDrawTime - takenTime);
        await Task.Delay(diffTime >= 0 ? diffTime : 0);
    }

}
