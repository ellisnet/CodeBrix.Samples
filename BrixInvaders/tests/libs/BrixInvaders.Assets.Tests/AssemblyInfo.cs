using Xunit.Sdk;
using Xunit.v3;

//The tests register the Kenney zips with the process-global engine and materialize into its process-global
//  registries, so the collections run one at a time.
[assembly: Parallelization(Mode = ParallelMode.None)]
