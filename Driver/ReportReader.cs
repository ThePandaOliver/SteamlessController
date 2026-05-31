using System.Buffers.Binary;
using System.Numerics;
using SteamlessController.Driver.Reports;

namespace SteamlessController.Driver;

public static class ReportReader {
	public static RawControllerInputReport ReadInputReport(ReadOnlySpan<byte> bytes) {
		if (bytes.Length < 54) {
			throw new ArgumentException("Input packet is too short.", nameof(bytes));
		}

		var reader = new SpanReader(bytes);

		var report = new RawControllerInputReport {
			reportId = reader.ReadByte(),
			PacketSequence = reader.ReadByte(),
			Buttons = (SteamButtons)reader.ReadUInt32(),

			L2Raw = reader.ReadUInt16(),
			R2Raw = reader.ReadUInt16(),

			StickLX = reader.ReadInt16(),
			StickLY = reader.ReadInt16(),
			StickRX = reader.ReadInt16(),
			StickRY = reader.ReadInt16(),

			TrackpadLX = reader.ReadInt16(),
			TrackpadLY = reader.ReadInt16(),
			TrackpadLPressure = reader.ReadUInt16(),

			TrackpadRX = reader.ReadInt16(),
			TrackpadRY = reader.ReadInt16(),
			TrackpadRPressure = reader.ReadUInt16(),
		};
		return report;
	}
	
	private ref struct SpanReader(ReadOnlySpan<byte> bytes) {
		private readonly ReadOnlySpan<byte> _bytes = bytes;
		private int _offset = 0;

		public byte ReadByte() {
			return _bytes[_offset++];
		}

		public short ReadInt16() {
			var value = BinaryPrimitives.ReadInt16LittleEndian(_bytes.Slice(_offset, 2));
			_offset += 2;
			return value;
		}
		
		public ushort ReadUInt16() {
			var value = BinaryPrimitives.ReadUInt16LittleEndian(_bytes.Slice(_offset, 2));
			_offset += 2;
			return value;
		}

		public int ReadInt32() {
			var value = BinaryPrimitives.ReadInt32LittleEndian(_bytes.Slice(_offset, 4));
			_offset += 4;
			return value;
		}

		public uint ReadUInt32() {
			var value = BinaryPrimitives.ReadUInt32LittleEndian(_bytes.Slice(_offset, 4));
			_offset += 4;
			return value;
		}

		public float ReadSingle() {
			var value = BitConverter.Int32BitsToSingle(
				BinaryPrimitives.ReadInt32LittleEndian(_bytes.Slice(_offset, 4)));
			_offset += 4;
			return value;
		}

		public double ReadDouble() {
			var value = BitConverter.Int64BitsToDouble(
				BinaryPrimitives.ReadInt64LittleEndian(_bytes.Slice(_offset, 8)));
			_offset += 8;
			return value;
		}
		
		public int BytesLeft => _bytes.Length - _offset;
	}
}