using HIDMaestro;
using HidSharp;
using SteamlessController.Driver.Reports;

namespace SteamlessController.Driver;

public class ControllerDevice(HidDevice hidDevice) {
	public string DevicePath => hidDevice.DevicePath;
	public HidDevice HidDevice => hidDevice;

	public Task? UpdateTask;
	public CancellationTokenSource? Cts;
	public HMController? HidCtrl;

	public ControllerInput? CurrentInputState;
	
	public event Action<ControllerDevice>? InputReceived;

	public void LogDeviceInfo() {
		lock (SteamlessDriver.OutputLock) {
			Console.WriteLine($"{hidDevice.GetProductName() ?? hidDevice.DevicePath}");
			Console.WriteLine($"    Manufacturer: {hidDevice.GetManufacturer() ?? "<unknown>"}");
			Console.WriteLine($"    DevicePath: {hidDevice.DevicePath}");
			Console.WriteLine($"    MaxInputReportLength: {hidDevice.GetMaxInputReportLength()}");
		}
	}

	public virtual void OnInputReceived() {
		InputReceived?.Invoke(this);
	}
	
	public void Cleanup() {
		Cts?.Cancel();
		Cts?.Dispose();
		Cts = null;
		UpdateTask?.Dispose();
		UpdateTask = null;
		HidCtrl?.Dispose();
		HidCtrl = null;
	}
}