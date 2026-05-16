using HidSharp;

namespace SteamlessController.Driver;

public static class ControllerManager {
	public static readonly List<ControllerDevice> ActiveDevices = [];
	public static readonly Dictionary<string, Task> ActiveTasksByDevicePath = new();

	private static uint VendorId => SteamlessDriver.VendorId;
	private static int[] ProductIds => SteamlessDriver.ProductIds;

	public delegate void DeviceUpdateLoop(ControllerDevice device);
	
	public static void ScanForDevices(CancellationToken ctsToken, DeviceUpdateLoop deviceUpdateLoop) {
		Console.WriteLine("Scanning for devices...");

		// Get all compatible hid devices
		var devices = DeviceList.Local.GetHidDevices()
			.Where(d => d.VendorID == VendorId && ProductIds.Contains(d.ProductID))
			.ToList();

		// Create new ControllerDevices
		foreach (var device in devices) {
			Console.WriteLine($"Found device: {device.DevicePath}");
			Console.WriteLine($"    VendorID: {device.VendorID}, ProductID: {device.ProductID}");
			
			Console.WriteLine($"Testing device connection: {device.DevicePath}");
			if (device.TryOpen(out var hidStream)) {
				hidStream.Close();
			} else {
				Console.WriteLine($"Failed to open device: {device.DevicePath}");
				Console.WriteLine($"    Device will be ignored");
				continue;
			}
			
			// Log successful connection
			Console.Write($"Device connected: ");
			var controllerDevice = new ControllerDevice(device);
			controllerDevice.LogDeviceInfo();
			ActiveDevices.Add(controllerDevice);

			// Start a new task to handle the device's input reports
			var devicePath = controllerDevice.DevicePath;
			var task = Task.Run(() => deviceUpdateLoop(controllerDevice), ctsToken);
			ActiveTasksByDevicePath[devicePath] = task;
		}
	}
}