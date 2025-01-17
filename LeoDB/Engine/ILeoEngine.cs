﻿namespace LeoDB.Engine;

public interface ILeoEngine : IDisposable
{
    int Checkpoint();
    long Rebuild(RebuildOptions options);
    bool BeginTrans();
    bool Commit();
    bool Rollback();
    IBsonDataReader Query(string collection, Query query);
    int Insert(string collection, IEnumerable<BsonDocument> docs, BsonAutoId autoId);
    int Update(string collection, IEnumerable<BsonDocument> docs);
    int UpdateMany(string collection, BsonExpression transform, BsonExpression predicate);
    int InsertOrUpdate(string collection, IEnumerable<BsonDocument> docs, BsonAutoId autoId);
    int Delete(string collection, IEnumerable<BsonValue> ids);
    int DeleteMany(string collection, BsonExpression predicate);
    bool DropCollection(string name);
    bool RenameCollection(string name, string newName);
    bool EnsureIndex(string collection, string name, BsonExpression expression, bool unique, bool save = true);
    bool DropIndex(string collection, string name);
    BsonValue Pragma(string name);
    bool Pragma(string name, BsonValue value);
}