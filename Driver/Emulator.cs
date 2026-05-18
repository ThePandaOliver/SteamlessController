using HIDMaestro;

namespace SteamlessController.Driver;

public static class Emulator {
	private static HMContext? _ctx;
	
	public static void SetupEmulator() {
		_ctx = new HMContext();
		int loaded = _ctx.LoadDefaultProfiles();
		Console.WriteLine($"  Loaded {loaded} embedded profiles");
		
		Console.Write("  Installing driver... ");
		_ctx.InstallDriver();
		Console.WriteLine("OK");
		
		var dsProfile = _ctx.GetProfile("dualsense")
		                ?? throw new InvalidOperationException("Profile 'dualsense' not found");
		
		Console.Write($"  Creating controller 0 ({dsProfile.Name})... ");
		using var ctrl0 = _ctx.CreateController(dsProfile);
		Console.WriteLine("OK");
	}
}