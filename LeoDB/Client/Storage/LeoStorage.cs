﻿using System.Linq.Expressions;

namespace LeoDB;

/// <summary>
/// Storage is a special collection to store files and streams.
/// </summary>
public class LeoStorage<TFileId> : ILeoStorage<TFileId>
{
    private readonly ILeoDatabase _db;
    private readonly ILeoCollection<LeoFileInfo<TFileId>> _files;
    private readonly ILeoCollection<BsonDocument> _chunks;

    public LeoStorage(ILeoDatabase db, string filesCollection, string chunksCollection)
    {
        _db = db;
        _files = db.GetCollection<LeoFileInfo<TFileId>>(filesCollection);
        _chunks = db.GetCollection(chunksCollection);
    }

    #region Find Files

    /// <summary>
    /// Find a file inside datafile and returns LeoFileInfo instance. Returns null if not found
    /// </summary>
    public LeoFileInfo<TFileId> FindById(TFileId id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));

        var fileId = _db.Mapper.Serialize(typeof(TFileId), id);

        var file = _files.FindById(fileId);

        if (file == null) return null;

        file.SetReference(fileId, _files, _chunks);

        return file;
    }

    /// <summary>
    /// Find all files that match with predicate expression.
    /// </summary>
    public IEnumerable<LeoFileInfo<TFileId>> Find(BsonExpression predicate)
    {
        var query = _files.Query();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        foreach (var file in query.ToEnumerable())
        {
            var fileId = _db.Mapper.Serialize(typeof(TFileId), file.Id);

            file.SetReference(fileId, _files, _chunks);

            yield return file;
        }
    }

    /// <summary>
    /// Find all files that match with predicate expression.
    /// </summary>
    public IEnumerable<LeoFileInfo<TFileId>> Find(string predicate, BsonDocument parameters) => this.Find(BsonExpression.Create(predicate, parameters));

    /// <summary>
    /// Find all files that match with predicate expression.
    /// </summary>
    public IEnumerable<LeoFileInfo<TFileId>> Find(string predicate, params BsonValue[] args) => this.Find(BsonExpression.Create(predicate, args));

    /// <summary>
    /// Find all files that match with predicate expression.
    /// </summary>
    public IEnumerable<LeoFileInfo<TFileId>> Find(Expression<Func<LeoFileInfo<TFileId>, bool>> predicate) => this.Find(_db.Mapper.GetExpression(predicate));

    /// <summary>
    /// Find all files inside file collections
    /// </summary>
    public IEnumerable<LeoFileInfo<TFileId>> FindAll() => this.Find((BsonExpression)null);

    /// <summary>
    /// Returns if a file exisits in database
    /// </summary>
    public bool Exists(TFileId id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));

        var fileId = _db.Mapper.Serialize(typeof(TFileId), id);

        return _files.Exists("_id = @0", fileId);
    }

    #endregion

    #region Upload

    /// <summary>
    /// Open/Create new file storage and returns linked Stream to write operations.
    /// </summary>
    public LeoFileStream<TFileId> OpenWrite(TFileId id, string filename, BsonDocument metadata = null)
    {
        // get _id as BsonValue
        var fileId = _db.Mapper.Serialize(typeof(TFileId), id);

        // checks if file exists
        var file = this.FindById(id);

        if (file == null)
        {
            file = new LeoFileInfo<TFileId>
            {
                Id = id,
                Filename = Path.GetFileName(filename),
                MimeType = MimeTypeConverter.GetMimeType(filename),
                Metadata = metadata ?? new BsonDocument()
            };

            // set files/chunks instances
            file.SetReference(fileId, _files, _chunks);
        }
        else
        {
            // if filename/metada was changed
            file.Filename = Path.GetFileName(filename);
            file.MimeType = MimeTypeConverter.GetMimeType(filename);
            file.Metadata = metadata ?? file.Metadata;
        }

        return file.OpenWrite();
    }

    /// <summary>
    /// Upload a file based on stream data
    /// </summary>
    public LeoFileInfo<TFileId> Upload(TFileId id, string filename, Stream stream, BsonDocument metadata = null)
    {
        using (var writer = this.OpenWrite(id, filename, metadata))
        {
            stream.CopyTo(writer);

            return writer.FileInfo;
        }
    }

    /// <summary>
    /// Upload a file based on file system data
    /// </summary>
    public LeoFileInfo<TFileId> Upload(TFileId id, string filename)
    {
        if (filename.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(filename));

        using (var stream = File.OpenRead(filename))
        {
            return this.Upload(id, Path.GetFileName(filename), stream);
        }
    }

    /// <summary>
    /// Update metadata on a file. File must exist.
    /// </summary>
    public bool SetMetadata(TFileId id, BsonDocument metadata)
    {
        var file = this.FindById(id);

        if (file == null) return false;

        file.Metadata = metadata ?? new BsonDocument();

        _files.Update(file);

        return true;
    }

    #endregion

    #region Download

    /// <summary>
    /// Load data inside storage and returns as Stream
    /// </summary>
    public LeoFileStream<TFileId> OpenRead(TFileId id)
    {
        var file = this.FindById(id);

        if (file == null) throw LeoException.FileNotFound(id.ToString());

        return file.OpenRead();
    }

    /// <summary>
    /// Copy all file content to a steam
    /// </summary>
    public LeoFileInfo<TFileId> Download(TFileId id, Stream stream)
    {
        var file = this.FindById(id) ?? throw LeoException.FileNotFound(id.ToString());

        file.CopyTo(stream);

        return file;
    }

    /// <summary>
    /// Copy all file content to a file
    /// </summary>
    public LeoFileInfo<TFileId> Download(TFileId id, string filename, bool overwritten)
    {
        var file = this.FindById(id) ?? throw LeoException.FileNotFound(id.ToString());

        file.SaveAs(filename, overwritten);

        return file;
    }

    #endregion

    #region Delete

    /// <summary>
    /// Delete a file inside datafile and all metadata related
    /// </summary>
    public bool Delete(TFileId id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));

        // get Id as BsonValue
        var fileId = _db.Mapper.Serialize(typeof(TFileId), id);

        // remove file reference
        var deleted = _files.Delete(fileId);

        if (deleted)
        {
            // delete all chunks
            _chunks.DeleteMany("_id BETWEEN { f: @0, n: 0} AND {f: @0, n: @1 }", fileId, int.MaxValue);
        }

        return deleted;
    }

    #endregion
}