namespace SteamlessController.Driver.Reports;

public struct RawControllerInputReport {
	public byte reportId;
	public byte PacketSequence;
	public SteamButtons Buttons;

	public ushort L2Raw;
	public ushort R2Raw;

	public short StickLX;
	public short StickLY;
	public short StickRX;
	public short StickRY;

	public short TrackpadLX;
	public short TrackpadLY;
	public ushort TrackpadLPressure;
	public short TrackpadRX;
	public short TrackpadRY;
	public ushort TrackpadRPressure;

	public float GyroX;
	public float GyroY;
	public float GyroZ;

	public override string ToString() {
		return $"""
		        ReportId: {reportId}
		        PacketSequence: {PacketSequence}
		        Buttons: {Buttons}

		        L2Raw: {L2Raw}
		        R2Raw: {R2Raw}

		        StickLX: {StickLX}
		        StickLY: {StickLY}
		        StickRX: {StickRX}
		        StickRY: {StickRY}

		        TrackpadLX: {TrackpadLX}
		        TrackpadLY: {TrackpadLY}
		        TrackpadLPressure: {TrackpadLPressure}
		        TrackpadRX: {TrackpadRX}
		        TrackpadRY: {TrackpadRY}
		        TrackpadRPressure: {TrackpadRPressure}

		        GyroX: {GyroX}
		        GyroY: {GyroY}
		        GyroZ: {GyroZ}
		        """;
	}
}