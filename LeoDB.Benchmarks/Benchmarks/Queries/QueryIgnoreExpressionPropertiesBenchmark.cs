using BenchmarkDotNet.Attributes;
using LeoDB.Benchmarks.Models;
using LeoDB.Benchmarks.Models.Generators;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeoDB.Benchmarks.Benchmarks.Queries
{
    [BenchmarkCategory(Constants.Categories.QUERIES)]
    public class QueryIgnoreExpressionPropertiesBenchmark : BenchmarkBase
    {
        private ILeoCollection<FileMetaBase> _fileMetaCollection;
        private ILeoCollection<FileMetaWithExclusion> _fileMetaExclusionCollection;

        [GlobalSetup(Target = nameof(DeserializeBaseline))]
        public void GlobalSetup()
        {
            File.Delete(DatabasePath);

            DatabaseInstance = new LeoDatabase(ConnectionString());
            _fileMetaCollection = DatabaseInstance.GetCollection<FileMetaBase>();
            _fileMetaCollection.EnsureIndex(fileMeta => fileMeta.ShouldBeShown);

            _fileMetaCollection.Insert(FileMetaGenerator<FileMetaBase>.GenerateList(DatasetSize)); // executed once per each N value

            DatabaseInstance.Checkpoint();
        }

        [GlobalSetup(Target = nameof(DeserializeWithIgnore))]
        public void GlobalIndexSetup()
        {
            File.Delete(DatabasePath);

            DatabaseInstance = new LeoDatabase(ConnectionString());
            _fileMetaExclusionCollection = DatabaseInstance.GetCollection<FileMetaWithExclusion>();
            _fileMetaExclusionCollection.EnsureIndex(fileMeta => fileMeta.ShouldBeShown);

            _fileMetaExclusionCollection.Insert(FileMetaGenerator<FileMetaWithExclusion>.GenerateList(DatasetSize)); // executed once per each N value

            DatabaseInstance.Checkpoint();
        }

        [Benchmark(Baseline = true)]
        public List<FileMetaBase> DeserializeBaseline()
        {
            return _fileMetaCollection.Find(fileMeta => fileMeta.ShouldBeShown).ToList();
        }

        [Benchmark]
        public List<FileMetaWithExclusion> DeserializeWithIgnore()
        {
            return _fileMetaExclusionCollection.Find(fileMeta => fileMeta.ShouldBeShown).ToList();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            // Disposing logic
            DatabaseInstance.DropCollection(nameof(FileMetaBase));
            _fileMetaCollection = null;

            DatabaseInstance.DropCollection(nameof(FileMetaWithExclusion));
            _fileMetaExclusionCollection = null;

            DatabaseInstance?.Checkpoint();
            DatabaseInstance?.Dispose();
            DatabaseInstance = null;

            File.Delete(DatabasePath);
        }
    }
}