using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LeoDB;

public interface ILeoQueryable<T> : ILeoQueryableResult<T>
{
    ILeoQueryable<T> Include(BsonExpression path);
    ILeoQueryable<T> Include(List<BsonExpression> paths);
    ILeoQueryable<T> Include<K>(Expression<Func<T, K>> path);

    ILeoQueryable<T> Where(BsonExpression predicate);
    ILeoQueryable<T> Where(string predicate, BsonDocument parameters);
    ILeoQueryable<T> Where(string predicate, params BsonValue[] args);
    ILeoQueryable<T> Where(Expression<Func<T, bool>> predicate);

    ILeoQueryable<T> OrderBy(BsonExpression keySelector, int order = 1);
    ILeoQueryable<T> OrderBy<K>(Expression<Func<T, K>> keySelector, int order = 1);
    ILeoQueryable<T> OrderByDescending(BsonExpression keySelector);
    ILeoQueryable<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector);

    ILeoQueryable<T> GroupBy(BsonExpression keySelector);
    ILeoQueryable<T> Having(BsonExpression predicate);

    ILeoQueryableResult<BsonDocument> Select(BsonExpression selector);
    ILeoQueryableResult<K> Select<K>(Expression<Func<T, K>> selector);
}

public interface ILeoQueryableResult<T>
{
    ILeoQueryableResult<T> Limit(int limit);
    ILeoQueryableResult<T> Skip(int offset);
    ILeoQueryableResult<T> Offset(int offset);
    ILeoQueryableResult<T> ForUpdate();

    BsonDocument GetPlan();
    IBsonDataReader ExecuteReader();
    IEnumerable<BsonDocument> ToDocuments();
    IEnumerable<T> ToEnumerable();
    List<T> ToList();
    T[] ToArray();

    int Into(string newCollection, BsonAutoId autoId = BsonAutoId.ObjectId);

    T First();
    T FirstOrDefault();
    T Single();
    T SingleOrDefault();

    int Count();
    long LongCount();
    bool Exists();
}