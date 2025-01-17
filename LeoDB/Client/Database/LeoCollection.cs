﻿using LeoDB.Engine;

namespace LeoDB;

public sealed partial class LeoCollection<T> : ILeoCollection<T>
{
    private readonly string _collection;
    private readonly ILeoEngine _engine;
    private readonly List<BsonExpression> _includes;
    private readonly BsonMapper _mapper;
    private readonly EntityMapper _entity;
    private readonly MemberMapper _id;
    private readonly BsonAutoId _autoId;

    /// <summary>
    /// Get collection name
    /// </summary>
    public string Name => _collection;

    /// <summary>
    /// Get collection auto id type
    /// </summary>
    public BsonAutoId AutoId => _autoId;

    /// <summary>
    /// Getting entity mapper from current collection. Returns null if collection are BsonDocument type
    /// </summary>
    public EntityMapper EntityMapper => _entity;

    internal LeoCollection(string name, BsonAutoId autoId, ILeoEngine engine, BsonMapper mapper)
    {
        _collection = name ?? mapper.ResolveCollectionName(typeof(T));
        _engine = engine;
        _mapper = mapper;
        _includes = [];

        // if strong typed collection, get _id member mapped (if exists)
        if (typeof(T) == typeof(BsonDocument))
        {
            _entity = null;
            _id = null;
            _autoId = autoId;
        }
        else
        {
            _entity = mapper.GetEntityMapper(typeof(T));
            _entity.WaitForInitialization();

            _id = _entity.Id;

            if (_id != null && _id.AutoId)
            {
                _autoId =
                    _id.DataType == typeof(int) || _id.DataType == typeof(int?) ? BsonAutoId.Int32 :
                    _id.DataType == typeof(long) || _id.DataType == typeof(long?) ? BsonAutoId.Int64 :
                    _id.DataType == typeof(Guid) || _id.DataType == typeof(Guid?) ? BsonAutoId.Guid :
                    BsonAutoId.ObjectId;
            }
            else
            {
                _autoId = autoId;
            }
        }
    }
}