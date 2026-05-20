using HIDMaestro;

namespace SteamlessController.Driver;

public static class Emulator {
	private static HMContext? _ctx;

	public static void StartEmulator() {
		_ctx = new HMContext();
		int loaded = _ctx.LoadDefaultProfiles();
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
		};
	}
}