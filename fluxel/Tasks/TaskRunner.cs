using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Midori.Logging;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace fluxel.Tasks;

public class TaskRunner
{
    private static Logger logger { get; } = Logger.GetLogger("Tasks");

    private List<IBasicTask> tasks { get; } = new();
    private List<ICronTask> cron { get; } = new();

    private object @lock { get; } = new { };

    private bool running { get; set; }

    public IReadOnlyList<IBasicTask> Queue => tasks;

    public void Start()
    {
        if (running)
            return;

        logger.Add("Starting Task Runner.");
        running = true;
        Task.Run(loop);
    }

    public void Stop()
    {
        logger.Add("Stopping Task Runner.");
        running = false;
    }

    public void Schedule(IBulkTask task)
        => task.GetTasks().ForEach(Schedule);

    public void Schedule(IBasicTask task)
    {
        lock (@lock)
        {
            if (task is ICronTask ct)
            {
                ct.Valid = true;
                cron.Add(ct);
            }
            else
                tasks.Add(task);
        }
    }

    private async void loop()
    {
        while (running)
        {
            IBasicTask? task = null;

            try
            {
                lock (@lock)
                {
                    var time = DateTimeOffset.Now;

                    foreach (var ct in cron)
                    {
                        if (time.Hour == ct.Hour && time.Minute == ct.Minute)
                        {
                            if (!ct.Valid)
                                continue;

                            task = ct;
                            ct.Valid = false;
                        }
                        else
                            ct.Valid = true;
                    }

                    if (task is null && tasks.Count > 0)
                    {
                        task = tasks[0];
                        tasks.RemoveAt(0);
                    }
                }

                task?.Run();
            }
            catch (Exception e)
            {
                var name = task?.Name ?? task?.GetType().Name ?? "unknown";
                logger.Add($"An error occurred while running task '{name}'.", LogLevel.Error, e);
            }
            finally
            {
                // If there are no tasks, wait 2 seconds before checking again.
                if (tasks.Count == 0)
                    await Task.Delay(2000);
            }
        }
    }
}
