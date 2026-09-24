using Xunit.Sdk;
using Xunit.v3;

//The settings tests open the process-global AppSettings store and the log sink is process-global too, so the
//  collections run one at a time.
[assembly: Parallelization(Mode = ParallelMode.None)]
