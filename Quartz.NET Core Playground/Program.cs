using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using CronExpressionDescriptor;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;


// Cron: http://www.quartz-scheduler.org/documentation/quartz-2.x/tutorials/crontrigger.html
// Create Cron: https://www.freeformatter.com/cron-expression-generator-quartz.html
// Describe Cron: https://cronexpressiondescriptor.azurewebsites.net/?expression=0+0%2F2+8-17+*+*+%3F&locale=en

// Quartz: https://github.com/quartznet/quartznet
// Quartz: https://www.quartz-scheduler.net/documentation/quartz-3.x/quick-start.html

// Describe Cron (lib): https://github.com/bradymholt/cron-expression-descriptor

namespace Bnaya.Samples
{
    class Program
    {
        private const string JOB_NAME = "Hello 1";
        private const string GROUP_NAME = "Group A";
        private const string CRON = "*/7 * * ? * * *"; // Every 7 seconds

        static async Task Main(string[] args)
        {
            try
            {
                LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());

                // Grab the Scheduler instance from the Factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();
                IJobDetail job = CreateJob(JOB_NAME, GROUP_NAME);

                // and start it off
                await scheduler.Start();

                ITrigger triggerA = CreateTrigger("trigger A", GROUP_NAME);
                await scheduler.ScheduleJob(job, triggerA);
                ITrigger triggerB = CreateCronTrigger(job, "trigger B", GROUP_NAME, CRON);
                await scheduler.ScheduleJob(triggerB);

                // some sleep to show what's happening
                await Task.Delay(TimeSpan.FromSeconds(60));

                // and last shut down the scheduler when you are ready to close your program
                await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                await Console.Error.WriteLineAsync(se.ToString());
            }

            Console.ReadKey();
        }

        private static IJobDetail CreateJob(string jobName, string groupName)
        {
            // define the job and tie it to our HelloJob class
            return JobBuilder.Create<HelloJob>()
                .WithIdentity(jobName, groupName)
                .Build();
        }

        private static ITrigger CreateTrigger(
            string triggerName,
            string groupName)
        {
            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(triggerName, groupName)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(10)
                    .WithRepeatCount(3))
                .Build();
            return trigger;
        }

        private static ITrigger CreateCronTrigger(
            IJobDetail job,
            string triggerName,
            string groupName,
            string cron)
        {
            string desc = ExpressionDescriptor.GetDescription(cron);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{cron}: {desc}");
            Console.ResetColor();

            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(triggerName, groupName)
                .WithCronSchedule(cron)
                .ForJob(job)
                .EndAt(DateTimeOffset.Now.AddSeconds(30))
                .Build();


            return trigger;
        }
    }
}
