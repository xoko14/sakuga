using Sakuga.Builtin;

await RunTaskForEachUser();


async Task RunTaskForEachUser()
{
    var userIds = new int[100];
    var animation = new IteratedProcessAnimationBuilder<int>()
        .SetLoopHandler(userIds, async (i, userId, logger) =>
        {
            // Here we would do some async work that might take some time
            // Instead, lets just fake some delay
            var randomDelay = Random.Shared.Next(100, 500);
            await Task.Delay(randomDelay);

            // The logger Action can be used to log stuff
            if (i % 10 == 0)
            {
                logger($"Oops! Something went wrong at index {i}");
            }
        })
        .SetProgressBarLength(80)
        .Build();

    await animation.Run();
}