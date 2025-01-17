using BenchmarkDotNet.Attributes;
using LeoDB.Benchmarks.Models;
using LeoDB.Benchmarks.Models.Generators;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeoDB.Benchmarks.Benchmarks.Queries
{
    [BenchmarkCategory(Constants.Categories.QUERIES)]
    public class QueryAllBenchmark : BenchmarkBase
    {
        private ILeoCollection<FileMetaBase> _fileMetaCollection;

        [GlobalSetup]
        public void GlobalSetup()
        {
            File.Delete(DatabasePath);

            DatabaseInstance = new LeoDatabase(ConnectionString());
            _fileMetaCollection = DatabaseInstance.GetCollection<FileMetaBase>();

            _fileMetaCollection.Insert(FileMetaGenerator<FileMetaBase>.GenerateList(DatasetSize)); // executed once per each N value
            DatabaseInstance.Checkpoint();
        }

        [Benchmark(Baseline = true)]
        public List<FileMetaBase> FindAll()
        {
            return _fileMetaCollection.FindAll().ToList();
        }

        [Benchmark]
        public List<FileMetaBase> FindAllWithExpression()
        {
            return _fileMetaCollection.Find(_ => true).ToList();
        }

        [Benchmark]
        public List<FileMetaBase> FindAllWithQuery()
        {
            return _fileMetaCollection.Find(Query.All()).ToList();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            // Disposing logic
            DatabaseInstance?.Checkpoint();
            DatabaseInstance?.Dispose();
            DatabaseInstance = null;

            File.Delete(DatabasePath);
        }
    }
}