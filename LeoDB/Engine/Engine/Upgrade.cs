using System;
using System.Buffers;
using System.IO;

namespace LeoDB.Engine;

public partial class LeoEngine
{

    private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

    /// <summary>
    /// If Upgrade=true, run this before open Disk service
    /// </summary>
    private void TryUpgrade()
    {
        var filename = _settings.Filename;

        // if file not exists, just exit
        if (!File.Exists(filename)) return;

        const int bufferSize = 1024;
        var buffer = _bufferPool.Rent(bufferSize);

        using (var stream = new FileStream(
            _settings.Filename,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read, bufferSize))
        {
            stream.Position = 0;
            stream.Read(buffer, 0, bufferSize);

            if (FileReaderV7.IsVersion(buffer) == false) return;
        }
        _bufferPool.Return(buffer, true);
        // run rebuild process
        this.Recovery(_settings.Collation);
    }
}