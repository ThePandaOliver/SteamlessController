namespace SteamlessController.Driver.utils;

/// <summary>
/// Simple utility to convert raw bit values into floats (normalized).
/// </summary>
public static class FloatConverter {
	/// <summary>
	/// Decode N bits from buffer at a bit offset, interpret as unsigned, normalize to 0-1.
	/// </summary>
	public static float DecodeUnsignedNormalized(BitDecoder decoder, int bitCount) {
		ulong raw = decoder.ReadBits(bitCount);

		// Max value for bitCount bits
		ulong maxVal = (1UL << bitCount) - 1;
		return maxVal > 0 ? (float)raw / maxVal : 0f;
	}

	/// <summary>
	/// Decode N bits from buffer at a bit offset, interpret as signed, normalize to -1 to 1.
	/// </summary>
	public static float DecodeSignedNormalized(BitDecoder decoder, int bitCount) {
		ulong raw = decoder.ReadBits(bitCount);

		// Convert to signed using two's complement
		long signed = ToSigned(raw, bitCount);

		// Normalize: find max magnitude
		long maxVal = (1L << (bitCount - 1)) - 1; // e.g., for 16-bit: 32767
		return maxVal > 0 ? (float)signed / maxVal : 0f;
	}

	/// <summary>
	/// Decode N bits as raw unsigned integer value (no normalization).
	/// </summary>
	public static ulong DecodeRawUnsigned(BitDecoder decoder, int bitCount) {
		return decoder.ReadBits(bitCount);
	}

	/// <summary>
	/// Decode N bits as raw signed integer value (no normalization).
	/// </summary>
	public static long DecodeRawSigned(BitDecoder decoder, int bitCount) {
		ulong raw = decoder.ReadBits(bitCount);
		return ToSigned(raw, bitCount);
	}

	private static long ToSigned(ulong raw, int bits) {
		if (bits >= 64) return unchecked((long)raw);

		ulong signBit = 1UL << (bits - 1);
		if ((raw & signBit) != 0) {
			// negative: sign-extend
			ulong mask = (~0UL) << bits;
			ulong extended = raw | mask;
			return unchecked((long)extended);
		}

		return unchecked((long)raw);
	}
}