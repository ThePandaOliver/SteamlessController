using HIDMaestro;

namespace SteamlessController.Driver;

public static class Emulator {
	private static HMContext? _ctx;

	public static void StartEmulator() {
		_ctx = new HMContext();
		var loaded = _ctx.LoadDefaultProfiles();
		Console.WriteLine($"  Loaded {loaded} embedded profiles");

		Console.Write("  Installing driver... ");
		_ctx.InstallDriver();
		Console.WriteLine("OK");

		ControllerManager.DeviceConnected += device => { SetupEmulatorForDevice(device); };
	}

	private static void SetupEmulatorForDevice(ControllerDevice device) {
		if (_ctx == null) {
			Console.WriteLine("  Emulator not started");
			return;
		}

		var profile = _ctx.GetProfile("xbox-elite-v2")
		              ?? throw new InvalidOperationException("Profile 'xbox-elite-v2' not found");

		Console.Write($"  Creating controller ({profile.Name})... ");
		var ctrl = _ctx.CreateController(profile);
		Console.WriteLine("OK");

		var vid = profile.VendorId;
		var pid = profile.ProductId;
		HMOemNameOverride.Set(vid, pid, "SdkDemo Custom Label");
		Console.WriteLine($"  Overrode joy.cpl label for VID_{vid:X4}&PID_{pid:X4} " +
		                  $"-> \"SdkDemo Custom Label\" (open joy.cpl in another window to verify)");

		device.HidCtrl = ctrl;

		ctrl.OutputReceived += (controller, packet) => {
			Console.WriteLine($"  [output] ctrl source={packet.Source} " +
			                  $"reportId=0x{packet.ReportId:X2} len={packet.Data.Length}");
		};

		device.InputReceived += device => {
			var input = device.CurrentInputState;
			if (input == null) return;

			ushort hat = 0;
			if (input.DpadUp) hat |= 0x01;
			if (input.DpadDown) hat |= 0x02;
			if (input.DpadLeft) hat |= 0x04;
			if (input.DpadRight) hat |= 0x08;
			
			HMButton buttons = HMButton.None;
			if (input.A) buttons |= HMButton.A;
			if (input.B) buttons |= HMButton.B;
			if (input.X) buttons |= HMButton.X;
			if (input.Y) buttons |= HMButton.Y;
			if (input.Options) buttons |= HMButton.Start;
			if (input.Share) buttons |= HMButton.Share;
			if (input.L1) buttons |= HMButton.LeftBumper;
			if (input.R1) buttons |= HMButton.RightBumper;
			if (input.L3) buttons |= HMButton.LeftStick;
			if (input.R3) buttons |= HMButton.RightStick;
			
			var state = new HMGamepadState {
				Axes = HMGamepadStateHelpers.StandardAxes(ctrl.Profile,
					leftStickX: input.StickL.X,
					leftStickY: input.StickL.Y,
					rightStickX: input.StickR.X,
					rightStickY: input.StickR.Y,
					leftTrigger: input.L2,
					rightTrigger: input.R2
				),
				HatRaw = hat,
				Buttons = buttons,
			};
			
			ctrl.SubmitState(state);
		};
	}
}