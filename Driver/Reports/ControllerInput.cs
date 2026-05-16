using System.Numerics;
using SteamlessController.Driver.utils;

namespace SteamlessController.Driver.Reports;

public class ControllerInput {
	public required byte UpdateId;
	
	public required bool A;
	public required bool B;
	public required bool X;
	public required bool Y;

	public required bool DpadUp;
	public required bool DpadRight;
	public required bool DpadDown;
	public required bool DpadLeft;

	public required bool R1;
	public required float R2;
	public required bool R2Full;
	public required bool R3;
	public required bool R4;
	public required bool R5;

	public required bool L1;
	public required float L2;
	public required bool L2Full;
	public required bool L3;
	public required bool L4;
	public required bool L5;

	public required bool Options;
	public required bool Share;
	public required bool Meta1;
	public required bool Meta2;

	public required Vector2 StickR;
	public required bool StickRTouch;
	public required Vector2 StickL;
	public required bool StickLTouch;

	public required Vector2 TrackpadR;
	public required bool TrackpadRTouch;
	public required float TrackpadRPressure;
	public required bool TrackpadRFull;
	public required Vector2 TrackpadL;
	public required bool TrackpadLTouch;
	public required float TrackpadLPressure;
	public required bool TrackpadLFull;

	public required bool GripRTouch;
	public required bool GripLTouch;
	public required double GyroX;
	public required double GyroY;
	public required double GyroZ;

	public static ControllerInput Decode(BitDecoder decoder) {
		var updateId = decoder.ReadByte();
		var r4 = decoder.ReadBool();
		var options = decoder.ReadBool();
		var r3 = decoder.ReadBool();
		var meta2 = decoder.ReadBool();
		var y = decoder.ReadBool();
		var x = decoder.ReadBool();
		var b = decoder.ReadBool();
		var a = decoder.ReadBool();
		var l3 = decoder.ReadBool();
		var share = decoder.ReadBool();
		var dpadUp = decoder.ReadBool();
		var dpadLeft = decoder.ReadBool();
		var dpadRight = decoder.ReadBool();
		var dpadDown = decoder.ReadBool();
		var r1 = decoder.ReadBool();
		var r5 = decoder.ReadBool();
		var r2Full = decoder.ReadBool();
		var trackpadRFull = decoder.ReadBool();
		var trackpadRTouch = decoder.ReadBool();
		var stickRTouch = decoder.ReadBool();
		var l1 = decoder.ReadBool();
		var l5 = decoder.ReadBool();
		var l4 = decoder.ReadBool();
		var meta1 = decoder.ReadBool();
		decoder.Skip(2); // Unknown 2 bits
		var gripLTouch = decoder.ReadBool();
		var gripRTouch = decoder.ReadBool();
		var l2Full = decoder.ReadBool();
		var trackpadLFull = decoder.ReadBool();
		var trackpadLTouch = decoder.ReadBool();
		var stickLTouch = decoder.ReadBool();
		var l2 = FloatConverter.DecodeUnsignedNormalized(decoder, 16);
		var r2 = FloatConverter.DecodeUnsignedNormalized(decoder, 16);
		var stickL = new Vector2(
			FloatConverter.DecodeSignedNormalized(decoder, 16),
			FloatConverter.DecodeSignedNormalized(decoder, 16)
		);
		var stickR = new Vector2(
			FloatConverter.DecodeSignedNormalized(decoder, 16),
			FloatConverter.DecodeSignedNormalized(decoder, 16)
		);
		var trackpadL = new Vector2(
			FloatConverter.DecodeSignedNormalized(decoder, 16),
			FloatConverter.DecodeSignedNormalized(decoder, 16)
		);
		var trackpadLPressure = FloatConverter.DecodeSignedNormalized(decoder, 16);
		var trackpadR = new Vector2(
			FloatConverter.DecodeSignedNormalized(decoder, 16),
			FloatConverter.DecodeSignedNormalized(decoder, 16)
		);
		var trackpadRPressure = FloatConverter.DecodeSignedNormalized(decoder, 16);
		var gyroX = decoder.ReadDouble();
		var gyroY = decoder.ReadDouble();
		var gyroZ = decoder.ReadDouble();

		return new ControllerInput {
			UpdateId = updateId,
			A = a,
			B = b,
			X = x,
			Y = y,
			DpadUp = dpadUp,
			DpadRight = dpadRight,
			DpadDown = dpadDown,
			DpadLeft = dpadLeft,
			R1 = r1,
			R2 = r2,
			R2Full = r2Full,
			R3 = r3,
			R4 = r4,
			R5 = r5,
			L1 = l1,
			L2 = l2,
			L2Full = l2Full,
			L3 = l3,
			L4 = l4,
			L5 = l5,
			Options = options,
			Share = share,
			Meta1 = meta1,
			Meta2 = meta2,
			StickR = stickR,
			StickRTouch = stickRTouch,
			StickL = stickL,
			StickLTouch = stickLTouch,
			TrackpadR = trackpadR,
			TrackpadRTouch = trackpadRTouch,
			TrackpadRPressure = trackpadRPressure,
			TrackpadRFull = trackpadRFull,
			TrackpadL = trackpadL,
			TrackpadLTouch = trackpadLTouch,
			TrackpadLPressure = trackpadLPressure,
			TrackpadLFull = trackpadLFull,
			GripRTouch = gripRTouch,
			GripLTouch = gripLTouch,
			GyroX = gyroX,
			GyroY = gyroY,
			GyroZ = gyroZ
		};
	}
}