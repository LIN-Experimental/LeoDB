using BenchmarkDotNet.Attributes;
using LeoDB.Benchmarks.Models;
using LeoDB.Benchmarks.Models.Generators;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeoDB.Benchmarks.Benchmarks.Queries
{
    [BenchmarkCategory(Constants.Categories.QUERIES)]
    public class QuerySimpleIndexBenchmarks : BenchmarkBase
    {
        private ILeoCollection<FileMetaBase> _fileMetaCollection;

        [GlobalSetup(Targets = new[] { nameof(FindWithExpression), nameof(FindWithQuery) })]
        public void GlobalSetup()
        {
            File.Delete(DatabasePath);

            DatabaseInstance = new LeoDatabase(ConnectionString());
            _fileMetaCollection = DatabaseInstance.GetCollection<FileMetaBase>();

            _fileMetaCollection.Insert(FileMetaGenerator<FileMetaBase>.GenerateList(DatasetSize)); // executed once per each N value
        }

        [GlobalSetup(Targets = new[] { nameof(FindWithIndexExpression), nameof(FindWithIndexQuery) })]
        public void GlobalIndexSetup()
        {
            File.Delete(DatabasePath);

            DatabaseInstance = new LeoDatabase(ConnectionString());
            _fileMetaCollection = DatabaseInstance.GetCollection<FileMetaBase>();
            _fileMetaCollection.EnsureIndex(fileMeta => fileMeta.IsFavorite);

            _fileMetaCollection.Insert(FileMetaGenerator<FileMetaBase>.GenerateList(DatasetSize)); // executed once per each N value

            DatabaseInstance.Checkpoint();
        }

        [Benchmark(Baseline = true)]
        public List<FileMetaBase> FindWithExpression()
        {
            return _fileMetaCollection.Find(fileMeta => fileMeta.IsFavorite).ToList();
        }

        [Benchmark]
        public List<FileMetaBase> FindWithQuery()
        {
            return _fileMetaCollection.Find(Query.EQ(nameof(FileMetaBase.IsFavorite), true)).ToList();
        }

        [Benchmark]
        public List<FileMetaBase> FindWithIndexExpression()
        {
            return _fileMetaCollection.Find(fileMeta => fileMeta.IsFavorite).ToList();
        }

        [Benchmark]
        public List<FileMetaBase> FindWithIndexQuery()
        {
            return _fileMetaCollection.Find(Query.EQ(nameof(FileMetaBase.IsFavorite), true)).ToList();
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