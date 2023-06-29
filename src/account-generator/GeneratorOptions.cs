using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace account_generator
{
    public sealed class GeneratorOptions
    {
        public enum RunModeOption
        {
            OneTime,
            Continuous
        }

        /// <summary>
        /// If set to OneTime, the generator will generate the number of transactions specified by the BatchSize value.
        /// The Continuous mode will run until the console application is closed or interrupted with a Ctrl+C command.
        /// </summary>
        public RunModeOption RunMode { get; set; } = RunModeOption.OneTime;

        /// <summary>
        /// Refers to how many account summaries to generate within each series of 5 batches (eg. 5 batches * 200 batch size = 1000 new records) when RunModeOption is set to OneTime.
        /// </summary>
        public int BatchSize { get; set; } = 100;

        public int SleepTime { get; set; } = 10000;

        public bool Verbose { get; set; } = true;
    }
}
