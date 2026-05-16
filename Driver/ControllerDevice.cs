using HidSharp;
using SteamlessController.Driver.Reports;

namespace SteamlessController.Driver;

public class ControllerDevice(HidDevice hidDevice) {
	public string DevicePath => hidDevice.DevicePath;
	public HidDevice HidDevice => hidDevice;
	public Task? UpdateTask;

	public ControllerInput? currentInputState;

	public void LogDeviceInfo() {
		lock (SteamlessDriver.OutputLock) {
			Console.WriteLine($"{hidDevice.GetProductName() ?? hidDevice.DevicePath}");
			Console.WriteLine($"    Manufacturer: {hidDevice.GetManufacturer() ?? "<unknown>"}");
			Console.WriteLine($"    DevicePath: {hidDevice.DevicePath}");
			Console.WriteLine($"    MaxInputReportLength: {hidDevice.GetMaxInputReportLength()}");
		}
	}
}