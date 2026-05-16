using HidSharp;

namespace SteamlessControllerDriver;

public class ControllerDevice(HidDevice hidDevice) {
	public string DevicePath => hidDevice.DevicePath;
	public HidDevice HidDevice => hidDevice;

	public void LogDeviceInfo() {
		lock (SteamlessDriver.OutputLock) {
			Console.WriteLine($"{hidDevice.GetProductName() ?? hidDevice.DevicePath}");
			Console.WriteLine($"    Manufacturer: {hidDevice.GetManufacturer() ?? "<unknown>"}");
			Console.WriteLine($"    DevicePath: {hidDevice.DevicePath}");
			Console.WriteLine($"    MaxInputReportLength: {hidDevice.GetMaxInputReportLength()}");
		}
	}
}