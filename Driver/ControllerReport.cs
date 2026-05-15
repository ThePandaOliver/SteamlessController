using SteamlessControllerDriver.utils;

namespace SteamlessControllerDriver;

public record ControllerReport(
	byte reportId,
	byte seqNum,
	ControllerInput inputs
) {
	public static ControllerReport Decode(BitDecoder decoder) {
		return new ControllerReport(
			decoder.ReadByte(),
			decoder.ReadByte(),
			ControllerInput.Decode(decoder)
		);
	}
}