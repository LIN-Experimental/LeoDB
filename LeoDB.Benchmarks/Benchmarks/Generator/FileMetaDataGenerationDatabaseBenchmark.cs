using BenchmarkDotNet.Attributes;
using LeoDB.Benchmarks.Models;
using LeoDB.Benchmarks.Models.Generators;
using System.Collections.Generic;

namespace LeoDB.Benchmarks.Benchmarks.Generator
{
    /// <summary>
    /// This benchmark is used purely for the sake of providing information
    /// about how long and how many resources it takes to generate the test data.
    /// </summary>
    [BenchmarkCategory(Constants.Categories.DATA_GEN)]
    public class FileMetaDataGenerationDatabaseBenchmark
    {
        // Benchmark params
        [Params(10, 50, 100, 500, 1000, 5000, 10000)]
        public int N;

        [Benchmark]
        public List<FileMetaBase> DataGeneration()
        {
            return FileMetaGenerator<FileMetaBase>.GenerateList(N);
        }

        [Benchmark]
        public List<FileMetaWithExclusion> DataWithExclusionsGeneration()
        {
            return FileMetaGenerator<FileMetaWithExclusion>.GenerateList(N);
        }
    }
}