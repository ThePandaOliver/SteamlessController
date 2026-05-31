using System.Runtime.InteropServices;
using System.Text;
using HidSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SteamlessController.Driver.Reports;

namespace SteamlessController.Driver;

public static class DriverMain {
	public const uint VendorId = 0x28DE;

	public static readonly int[] ProductIds = [
		0x1302, // Wired 
		0x1304 // Puck
	];

	public static readonly List<string> ActivePaths = [];
	public static readonly Dictionary<string, Task> ActiveTasks = [];
	public static readonly Lock OutputLock = new();
	public static CancellationToken AppToken;

	public static int Main(string[] args) {
		using var host = Host.CreateDefaultBuilder(args)
			.UseWindowsService()
			.Build();

		var appStopping = host.Services
			.GetRequiredService<IHostApplicationLifetime>()
			.ApplicationStopping;

		using var cts = CancellationTokenSource.CreateLinkedTokenSource(appStopping);
		AppToken = cts.Token;

		DeviceList.Local.Changed += (_, _) => ScanForDevices();

		// initial scan
		ScanForDevices();

		try {
			host.Run();
		} finally {
			cts.Cancel(); // request device loops to stop
			ActivePaths.ForEach(CleanupDevice); // Cleanup all active devices
		}

		return 0;
	}

	public static void ScanForDevices() {
		Console.WriteLine("Scanning for devices...");

		// Get all compatible hid devices
		var devices = DeviceList.Local.GetHidDevices()
			.Where(d => d.VendorID == VendorId && ProductIds.Contains(d.ProductID))
			.ToList();

		var currentPaths = devices
			.Select(d => d.DevicePath)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		lock (OutputLock) {
			// disconnects
			for (var i = ActivePaths.Count - 1; i >= 0; i--) {
				var activePath = ActivePaths[i];
				if (currentPaths.Contains(activePath)) {
					continue;
				}

				CleanupDevice(activePath);
				Console.WriteLine($"Device disconnected: {activePath}");
			}

			// connects
			foreach (var device in devices.Where(device => ActivePaths.All(path => path != device.DevicePath))) {
				var newDevicePath = device.DevicePath;
				Console.WriteLine($"Device connected: {newDevicePath}");

				// Test if Device can be opened
				if (!device.TryOpen(out var hidStream)) {
					Console.WriteLine($"Failed to open device: {newDevicePath}");
					continue;
				}

				hidStream.Close();

				ActivePaths.Add(newDevicePath);

				var task = Task.Run(() => DeviceUpdateLoop(device), AppToken);
				ActiveTasks[newDevicePath] = task;
			}
		}
	}

	public static void DeviceUpdateLoop(HidDevice device) {
		try {
			// Open device and register device closing for when the cancellation token is triggered
			using var stream = device.Open();
			using var registration = AppToken.Register(() => {
				try {
					stream.Close();
				} catch {
					// ignored
				}
			});

			var reportLength = Math.Max(1, device.GetMaxInputReportLength());
			var buffer = new byte[reportLength];
			Console.WriteLine($"Opened {device.DevicePath}");

			// Devices update loop
			while (!AppToken.IsCancellationRequested) {
				// Read the next hid report
				int read;
				try {
					read = stream.Read(buffer, 0, buffer.Length);
				} catch (Exception ex) when
					(ex is IOException or ObjectDisposedException or InvalidOperationException) {
					if (AppToken.IsCancellationRequested) {
						break;
					}

					Console.WriteLine($"Read stopped: {ex.Message}");
					break;
				}

				// Skip if hid data is empty
				if (read <= 0) continue;

				// Handle reports
				try {
					var reportType = buffer[0];
					if (reportType == 0x45) {
						var rawInput = ReportReader.ReadInputReport(buffer);
						// Console.WriteLine($"Received input report: {rawInput}");
					} else {
						lock (OutputLock) {
							var sb = new StringBuilder();
							sb.AppendLine("Received unknown report:");
							sb.AppendLine($"    ID: 0x{reportType}");
							sb.AppendLine($"    Length: {read}");
							sb.AppendLine($"    Data: {Convert.ToHexString(buffer)}");
							throw new InvalidOperationException(sb.ToString());
						}
					}
				} catch (Exception e) {
					Console.WriteLine($"Failed to parse report: {e.Message}");
				}
			}
		} catch (Exception e) {
			Console.WriteLine($"Failed to open/read HID device: {e.Message}");
		}
	}

	public static void CleanupDevice(string devicePath) {
		ActivePaths.Remove(devicePath);
		ActiveTasks.Remove(devicePath);
	}
}