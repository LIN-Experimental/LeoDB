namespace LeoDB;

/// <summary>
/// Represents a file inside storage collection
/// </summary>
public class LeoFileInfo<TFileId>
{
    public TFileId Id { get; internal set; }

    [CollectionField("filename")]
    public string Filename { get; internal set; }

    [CollectionField("mimeType")]
    public string MimeType { get; internal set; }

    [CollectionField("length")]
    public long Length { get; internal set; } = 0;

    [CollectionField("chunks")]
    public int Chunks { get; internal set; } = 0;

    [CollectionField("uploadDate")]
    public DateTime UploadDate { get; internal set; } = DateTime.Now;

    [CollectionField("metadata")]
    public BsonDocument Metadata { get; set; } = new BsonDocument();

    // database instances references
    private BsonValue _fileId;
    private ILeoCollection<LeoFileInfo<TFileId>> _files;
    private ILeoCollection<BsonDocument> _chunks;

    internal void SetReference(BsonValue fileId, ILeoCollection<LeoFileInfo<TFileId>> files, ILeoCollection<BsonDocument> chunks)
    {
        _fileId = fileId;
        _files = files;
        _chunks = chunks;
    }

    /// <summary>
    /// Open file stream to read from database
    /// </summary>
    public LeoFileStream<TFileId> OpenRead()
    {
        return new LeoFileStream<TFileId>(_files, _chunks, this, _fileId, FileAccess.Read);
    }

    /// <summary>
    /// Open file stream to write to database
    /// </summary>
    public LeoFileStream<TFileId> OpenWrite()
    {
        return new LeoFileStream<TFileId>(_files, _chunks, this, _fileId, FileAccess.Write);
    }

    /// <summary>
    /// Copy file content to another stream
    /// </summary>
    public void CopyTo(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        using (var reader = this.OpenRead())
        {
            reader.CopyTo(stream);
        }
    }

    /// <summary>
    /// Save file content to a external file
    /// </summary>
    public void SaveAs(string filename, bool overwritten = true)
    {
        if (filename.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(filename));

        using (var file = File.Open(filename, overwritten ? FileMode.Create : FileMode.CreateNew))
        {
            using (var stream = this.OpenRead())
            {
                stream.CopyTo(file);
            }
        }
    }
}