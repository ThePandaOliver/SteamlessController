using System.Numerics;
using SteamlessControllerDriver.utils;

namespace SteamlessControllerDriver;

public record ControllerInput(
	bool A,
	bool B,
	bool X,
	bool Y,
	bool DpadUp,
	bool DpadRight,
	bool DpadDown,
	bool DpadLeft,
	bool R1,
	float R2,
	bool R2Full,
	bool R3,
	bool R4,
	bool R5,
	bool L1,
	float L2,
	bool L2Full,
	bool L3,
	bool L4,
	bool L5,
	bool Options,
	bool Share,
	bool Meta1,
	bool Meta2,
	Vector2 StickR,
	bool StickRTouch,
	Vector2 StickL,
	bool StickLTouch,
	Vector2 TrackpadR,
	bool TrackpadRTouch,
	float TrackpadRPressure,
	bool TrackpadRFull,
	Vector2 TrackpadL,
	bool TrackpadLTouch,
	float TrackpadLPressure,
	bool TrackpadLFull,
	bool GripRTouch,
	bool GripLTouch,
	double GyroX,
	double GyroY,
	double GyroZ
) {
	public static ControllerInput Decode(BitDecoder decoder) {
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

		return new ControllerInput(
			a, b, x, y,
			dpadUp, dpadRight, dpadDown, dpadLeft,
			r1, r2, r2Full, r3, r4, r5,
			l1, l2, l2Full, l3, l4, l5,
			options, share, meta1, meta2,
			stickR, stickRTouch,
			stickL, stickLTouch,
			trackpadR, trackpadRTouch, trackpadRPressure, trackpadRFull,
			trackpadL, trackpadLTouch, trackpadLPressure, trackpadLFull,
			gripRTouch, gripLTouch,
			gyroX, gyroY, gyroZ
		);
	}
}