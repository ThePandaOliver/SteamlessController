namespace SteamlessControllerDriver.utils;

/// <summary>
/// A utility for reading values at the bit level from a byte buffer.
/// Tracks position internally so you can read sequentially.
/// </summary>
public class BitDecoder(byte[] buffer, int length) {
	private int _bitPosition;

	public int Length { get; } = length;

	public int Position => _bitPosition;

	/// <summary>
	/// Read the next N bits as an unsigned long.
	/// </summary>
	public ulong ReadBits(int count) {
		if (count < 0 || count > 64)
			throw new ArgumentOutOfRangeException(nameof(count), "Count must be 0-64.");

		if (_bitPosition + count > Length * 8)
			throw new IndexOutOfRangeException("Not enough bits remaining in buffer.");

		ulong result = 0;

		for (int i = 0; i < count; i++) {
			int byteIndex = (_bitPosition + i) / 8;
			int bitIndex = (_bitPosition + i) % 8;
			byte bit = (byte)((buffer[byteIndex] >> (7 - bitIndex)) & 1);
			result = (result << 1) | bit;
		}

		_bitPosition += count;
		return result;
	}

	/// <summary>
	/// Read the next 8 bits as a byte.
	/// </summary>
	public byte ReadByte() {
		return (byte)ReadBits(8);
	}

	/// <summary>
	/// Read the next 16 bits as a signed 16-bit integer (little-endian).
	/// </summary>
	public short ReadInt16() {
		return (short)ReadBits(16);
	}

	/// <summary>
	/// Read the next 16 bits as an unsigned 16-bit integer.
	/// </summary>
	public ushort ReadUInt16() {
		return (ushort)ReadBits(16);
	}

	/// <summary>
	/// Read the next 32 bits as a signed 32-bit integer.
	/// </summary>
	public int ReadInt32() {
		return (int)ReadBits(32);
	}

	/// <summary>
	/// Read the next 32 bits as an unsigned 32-bit integer.
	/// </summary>
	public uint ReadUInt32() {
		return (uint)ReadBits(32);
	}

	/// <summary>
	/// Read the next 32 bits as a float (IEEE 754).
	/// </summary>
	public float ReadFloat() {
		var bits = ReadUInt32();
		return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
	}

	/// <summary>
	/// Read the next 64 bits as a double (IEEE 754).
	/// </summary>
	public double ReadDouble() {
		var bits = ReadBits(64);
		return BitConverter.Int64BitsToDouble((long)bits);
	}

	/// <summary>
	/// Read the next 1 bit as a boolean.
	/// </summary>
	public bool ReadBool() {
		return ReadBits(1) != 0;
	}

	/// <summary>
	/// Skip the next N bits.
	/// </summary>
	public void Skip(int count) {
		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count));

		if (_bitPosition + count > Length * 8)
			throw new IndexOutOfRangeException("Not enough bits remaining to skip.");

		_bitPosition += count;
	}

	/// <summary>
	/// Align to the next byte boundary.
	/// </summary>
	public void AlignToByte() {
		if (_bitPosition % 8 != 0) {
			_bitPosition += 8 - (_bitPosition % 8);
		}
	}
}
