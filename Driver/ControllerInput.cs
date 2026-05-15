using System.Numerics;
using SteamlessControllerDriver.utils;

namespace SteamlessControllerDriver;

public record ControllerInput(
	bool a,
	bool b,
	bool x,
	bool y,

	bool dpadUp,
	bool dpadRight,
	bool dpadDown,
	bool dpadLeft,

	bool r1,
	float r2,
	bool r2Full,
	bool r3,
	bool r4,
	bool r5,

	bool l1,
	float l2,
	bool l2Full,
	bool l3,
	bool l4,
	bool l5,

	Vector2 stickR,
	bool stickRTouch,
	Vector2 stickL,
	bool stickLTouch,
	
	Vector2 trackpadR,
	bool trackpadRTouch,
	float trackpadRPressure,
	Vector2 trackpadL,
	bool trackpadLTouch,
	float trackpadLPressure,
	
	bool gripRTouch,
	bool gripLTouch,

	bool options,
	bool share,
	bool meta1,
	bool meta2
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
		decoder.Skip(1); // Unknown bit
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
		decoder.Skip(1); // Unknown bit
		var trackpadLTouch = decoder.ReadBool();
		var stickLTouch = decoder.ReadBool();
		var l2 = 0f;
		decoder.Skip(16); // Skips the trigger values
		var r2 = 0;
		decoder.Skip(16); // Skips the trigger values
		var stickL = new Vector2(0f, 0f);
		decoder.Skip(32); // Skips the stick values
		var stickR = new Vector2(0f, 0f);
		decoder.Skip(32); // Skips the stick values
		// TODO: Do trackpad
		var trackpadL = new Vector2(0f, 0f);
		var trackpadLPressure = 0f;
		var trackpadR = new Vector2(0f, 0f);
		var trackpadRPressure = 0f;
		
		return new ControllerInput(
			a,
			b,
			x,
			y,
			
			dpadUp,
			dpadRight,
			dpadDown,
			dpadLeft,
			
			r1,
			r2,
			r2Full,
			r3,
			r4,
			r5,
			
			l1,
			l2,
			l2Full,
			l3,
			l4,
			l5,
			
			stickR,
			stickRTouch,
			stickL,
			stickLTouch,
			
			trackpadR,
			trackpadRTouch,
			trackpadRPressure,
			trackpadL,
			trackpadLTouch,
			trackpadLPressure,
			
			gripRTouch,
			gripLTouch,
			
			options,
			share,
			meta1,
			meta2
		);
	}
}