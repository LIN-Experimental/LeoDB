using System.Text;

namespace LeoDB;

internal class ByteReader
{
    private readonly byte[] _buffer;
    private readonly int _length;
    private int _pos;

    public int Position { get { return _pos; } set { _pos = value; } }

    public ByteReader(byte[] buffer)
    {
        _buffer = buffer;
        _length = buffer.Length;
        _pos = 0;
    }

    public void Skip(int length)
    {
        _pos += length;
    }

    #region Native data types

    public Byte ReadByte()
    {
        var value = _buffer[_pos];

        _pos++;

        return value;
    }

    public Boolean ReadBoolean()
    {
        var value = _buffer[_pos];

        _pos++;

        return value == 0 ? false : true;
    }

    public UInt16 ReadUInt16()
    {
        _pos += 2;
        return BitConverter.ToUInt16(_buffer, _pos - 2);
    }

    public UInt32 ReadUInt32()
    {
        _pos += 4;
        return BitConverter.ToUInt32(_buffer, _pos - 4);
    }

    public UInt64 ReadUInt64()
    {
        _pos += 8;
        return BitConverter.ToUInt64(_buffer, _pos - 8);
    }

    public Int16 ReadInt16()
    {
        _pos += 2;
        return BitConverter.ToInt16(_buffer, _pos - 2);
    }

    public Int32 ReadInt32()
    {
        _pos += 4;
        return BitConverter.ToInt32(_buffer, _pos - 4);
    }

    public Int64 ReadInt64()
    {
        _pos += 8;
        return BitConverter.ToInt64(_buffer, _pos - 8);
    }

    public Single ReadSingle()
    {
        _pos += 4;
        return BitConverter.ToSingle(_buffer, _pos - 4);
    }

    public Double ReadDouble()
    {
        _pos += 8;
        return BitConverter.ToDouble(_buffer, _pos - 8);
    }

    public Decimal ReadDecimal()
    {
        _pos += 16;
        var a = BitConverter.ToInt32(_buffer, _pos - 16);
        var b = BitConverter.ToInt32(_buffer, _pos - 12);
        var c = BitConverter.ToInt32(_buffer, _pos - 8);
        var d = BitConverter.ToInt32(_buffer, _pos - 4);
        return new Decimal(new int[] { a, b, c, d });
    }

    public byte[] ReadBytes(int count)
    {
        var buffer = new byte[count];

        System.Buffer.BlockCopy(_buffer, _pos, buffer, 0, count);

        _pos += count;

        return buffer;
    }

    #endregion

    #region Extended types

    public string ReadString()
    {
        var length = this.ReadInt32();
        var str = Encoding.UTF8.GetString(_buffer, _pos, length);
        _pos += length;

        return str;
    }

    public string ReadString(int length)
    {
        var str = Encoding.UTF8.GetString(_buffer, _pos, length);
        _pos += length;

        return str;
    }

    /// <summary>
    /// Read BSON string add \0x00 at and of string and add this char in length before
    /// </summary>
    public string ReadBsonString()
    {
        var length = this.ReadInt32();
        var str = Encoding.UTF8.GetString(_buffer, _pos, length - 1);
        _pos += length;

        return str;
    }

    public string ReadCString()
    {
        var pos = _pos;
        var length = 0;

        while (true)
        {
            if (_buffer[pos] == 0x00)
            {
                var str = Encoding.UTF8.GetString(_buffer, _pos, length);
                _pos += length + 1; // read last 0x00
                return str;
            }
            else if (pos > _length)
            {
                return "_";
            }

            pos++;
            length++;
        }
    }

    public DateTime ReadDateTime()
    {
        // fix #921 converting index key into LocalTime
        // this is not best solution because uctDate must be a global parameter
        // this will be review in v5
        var date = new DateTime(this.ReadInt64(), DateTimeKind.Utc);

        return date.ToLocalTime();
    }

    public Guid ReadGuid()
    {
        return new Guid(this.ReadBytes(16));
    }

    public ObjectId ReadObjectId()
    {
        return new ObjectId(this.ReadBytes(12));
    }

    // Legacy PageAddress structure: [uint, ushort]
    // public PageAddress ReadPageAddress()
    // {
    //     return new PageAddress(this.ReadUInt32(), this.ReadUInt16());
    // }

    public BsonValue ReadBsonValue(ushort length)
    {
        var type = (BsonType)this.ReadByte();

        return type switch
        {
            BsonType.Null => BsonValue.Null,
            BsonType.Int32 => (BsonValue)this.ReadInt32(),
            BsonType.Int64 => (BsonValue)this.ReadInt64(),
            BsonType.Double => (BsonValue)this.ReadDouble(),
            BsonType.Decimal => (BsonValue)this.ReadDecimal(),
            BsonType.String => (BsonValue)this.ReadString(length),
            BsonType.Document => new BsonReader(false).ReadDocument(this),
            BsonType.Array => new BsonReader(false).ReadArray(this),
            BsonType.Binary => (BsonValue)this.ReadBytes(length),
            BsonType.ObjectId => (BsonValue)this.ReadObjectId(),
            BsonType.Guid => (BsonValue)this.ReadGuid(),
            BsonType.Boolean => (BsonValue)this.ReadBoolean(),
            BsonType.DateTime => (BsonValue)this.ReadDateTime(),
            BsonType.MinValue => BsonValue.MinValue,
            BsonType.MaxValue => BsonValue.MaxValue,
            _ => throw new NotImplementedException(),
        };
    }

    #endregion
}