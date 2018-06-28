using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace Bnaya.Samples
{
    public class HelloJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            await Console.Out.WriteLineAsync($@"#{context.RefireCount}) Greetings from HelloJob! 
    {context.Trigger.Key}
    see you next time at {context.NextFireTimeUtc: HH:mm:ss}");
            Console.ResetColor();
        }
    }
}
