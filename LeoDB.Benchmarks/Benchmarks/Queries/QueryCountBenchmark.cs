using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using LeoDB.Benchmarks.Models;
using LeoDB.Benchmarks.Models.Generators;

namespace LeoDB.Benchmarks.Benchmarks.Queries
{
	[BenchmarkCategory(Constants.Categories.QUERIES)]
	public class QueryCountBenchmark : BenchmarkBase
	{
		private ILeoCollection<FileMetaBase> _fileMetaCollection;

		[GlobalSetup]
		public void GlobalSetup()
		{
			File.Delete(DatabasePath);

			DatabaseInstance = new LeoDatabase(ConnectionString());
			_fileMetaCollection = DatabaseInstance.GetCollection<FileMetaBase>();
			_fileMetaCollection.EnsureIndex(fileMeta => fileMeta.ShouldBeShown);

			_fileMetaCollection.Insert(FileMetaGenerator<FileMetaBase>.GenerateList(DatasetSize)); // executed once per each N value

			DatabaseInstance.Checkpoint();
		}

		[Benchmark(Baseline = true)]
		public int CountWithLinq()
		{
			return _fileMetaCollection.Find(Query.EQ(nameof(FileMetaBase.ShouldBeShown), true)).Count();
		}

		[Benchmark]
		public int CountWithExpression()
		{
			return _fileMetaCollection.Count(fileMeta => fileMeta.ShouldBeShown);
		}

		[Benchmark]
		public int CountWithQuery()
		{
			return _fileMetaCollection.Count(Query.EQ(nameof(FileMetaBase.ShouldBeShown), true));
		}

		[GlobalCleanup]
		public void GlobalCleanup()
		{
			// Disposing logic
			DatabaseInstance.DropCollection(nameof(FileMetaBase));
			DatabaseInstance?.Checkpoint();
			DatabaseInstance?.Dispose();
			DatabaseInstance = null;

			File.Delete(DatabasePath);
		}
	}
}