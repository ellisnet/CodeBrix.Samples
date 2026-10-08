using Xunit.Sdk;
using Xunit.v3;

//The settings store, the log sink and the PDF font resolver are process-global, so the collections run one at a time
[assembly: Parallelization(Mode = ParallelMode.None)]
