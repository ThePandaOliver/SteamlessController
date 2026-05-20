using HidSharp;

namespace SteamlessController.Driver;

public static class ControllerManager {
	public static readonly List<ControllerDevice> ActiveDevices = [];
	private static readonly Lock _lock = new();

	private static uint VendorId => SteamlessDriver.VendorId;
	private static int[] ProductIds => SteamlessDriver.ProductIds;
	
	public static event Action<ControllerDevice>? DeviceConnected;
	public static event Action<ControllerDevice>? DeviceDisconnected;
	
	public delegate void DeviceUpdateLoop(ControllerDevice device, CancellationToken token);
	
	public static void StartMonitoring(
		CancellationToken appToken,
		DeviceUpdateLoop deviceUpdateLoop
	) {

		DeviceList.Local.Changed += (_, _) => ScanForDevices(appToken, deviceUpdateLoop);
		
		// initial scan
		ScanForDevices(appToken, deviceUpdateLoop);
	}
	
	public static void ScanForDevices(
		CancellationToken appToken,
		DeviceUpdateLoop deviceUpdateLoop
	) {
		Console.WriteLine("Scanning for devices...");

		// Get all compatible hid devices
		var devices = DeviceList.Local.GetHidDevices()
			.Where(d => d.VendorID == VendorId && ProductIds.Contains(d.ProductID))
			.ToList();
		
		var currentPaths = devices
			.Select(d => d.DevicePath)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		lock (_lock) {
			// disconnects
			for (var i = ActiveDevices.Count - 1; i >= 0; i--) {
				var active = ActiveDevices[i];
				if (currentPaths.Contains(active.DevicePath)) {
					continue;
				}
				
				DeviceDisconnected?.Invoke(active);
				active.Cleanup();
				
				Console.WriteLine($"Device disconnected: {active.DevicePath}");
				ActiveDevices.RemoveAt(i);
			}

			// connects
			foreach (var device in devices.Where(device => ActiveDevices.All(d => d.DevicePath != device.DevicePath))) {
				Console.WriteLine($"Device connected: {device.DevicePath}");

				if (!device.TryOpen(out var hidStream)) {
					Console.WriteLine($"Failed to open device: {device.DevicePath}");
					continue;
				}

				hidStream.Close();

				var controllerDevice = new ControllerDevice(device);
				controllerDevice.LogDeviceInfo();
				ActiveDevices.Add(controllerDevice);

				var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(appToken);
				controllerDevice.Cts = linkedCts;
				
				var task = Task.Run(() => deviceUpdateLoop(controllerDevice, linkedCts.Token), linkedCts.Token);
				controllerDevice.UpdateTask = task;
				DeviceConnected?.Invoke(controllerDevice);
			}
		}
	}
	
	public static void StopMonitoring() {
		lock (_lock) {
			foreach (var device in ActiveDevices) {
				device.Cleanup();
			}

			ActiveDevices.Clear();
		}
	}
}