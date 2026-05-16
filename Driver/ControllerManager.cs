using HidSharp;

namespace SteamlessControllerDriver;

public static class ControllerManager {
	public static readonly List<ControllerDevice> ActiveDevices = [];
	public static readonly Dictionary<string, Task> ActiveTasksByDevicePath = new();

	private static uint VendorId => SteamlessDriver.VendorId;
	private static int[] ProductIds => SteamlessDriver.ProductIds;

	public delegate void DeviceInputReportsReceivedHandler(ControllerDevice device, HidStream stream);
	public static event DeviceInputReportsReceivedHandler? DeviceInputReportsReceived;

	public static void ScanForDevices(CancellationToken ctsToken) {
		Console.WriteLine("Scanning for devices...");

		var devices = DeviceList.Local.GetHidDevices()
			.Where(d => d.VendorID == VendorId && ProductIds.Contains(d.ProductID))
			.ToList();
		
		// Create new ControllerDevices
		foreach (var device in devices) {
			Console.Write($"Device connected: ");
			var controllerDevice = new ControllerDevice(device);
			controllerDevice.LogDeviceInfo();
			ActiveDevices.Add(controllerDevice);

			// Start a new task to handle the device's input reports
			var devicePath = controllerDevice.DevicePath;
			var task = Task.Run(() => CreateDeviceUpdateLoop(controllerDevice, ctsToken), ctsToken);
			ActiveTasksByDevicePath[devicePath] = task;
		}
	}

	private static void CreateDeviceUpdateLoop(ControllerDevice device, CancellationToken token) {
		var hidDevice = device.HidDevice;
		try { 
			using var stream = hidDevice.Open();
			using var registration = token.Register(() => {
				try {
					stream.Close();
				} catch {
					// ignored
				}
			});

			var reportLength = Math.Max(1, hidDevice.GetMaxInputReportLength());
			var buffer = new byte[reportLength];
			Console.WriteLine($"Opened {device.DevicePath}");
			
			while (!token.IsCancellationRequested) {
				int read;
				try {
					read = stream.Read(buffer, 0, buffer.Length);
				} catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidOperationException) {
					if (token.IsCancellationRequested) {
						break;
					}

					Console.WriteLine($"Read stopped: {ex.Message}");

					break;
				}
				
				if (read <= 0) {
					continue;
				}
				
				DeviceInputReportsReceived?.Invoke(device, stream);
			}
		} catch (Exception e) {
			Console.WriteLine($"Failed to open/read HID device: {e.Message}");
		}
	}
}