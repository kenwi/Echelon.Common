using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace Echelon.Bot.Services
{
    public class TestJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("YOLO!");
            return Task.CompletedTask;
        }
    }

    public class ScheduledServiceBase<T> : IHostedService
    {
        private readonly StdSchedulerFactory factory = new();
        private IScheduler scheduler;
        private IScheduleBuilder scheduleBuilder;

        public ScheduledServiceBase(
            SimpleScheduleBuilder scheduleBuilder,
            IMessageWriter messageWriter)
        {
            this.scheduleBuilder = scheduleBuilder;
            this.scheduler = null!;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var jobDetail = JobBuilder.Create(typeof(T))
                .WithIdentity("job1", "group1")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSchedule(scheduleBuilder)
                .Build();

            scheduler = await factory.GetScheduler(cancellationToken);
            await scheduler.ScheduleJob(jobDetail, trigger);
            await scheduler.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await scheduler.Shutdown();
        }
    }
}



