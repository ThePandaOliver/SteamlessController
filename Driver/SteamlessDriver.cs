using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SteamlessController.Driver.Reports;
using SteamlessController.Driver.utils;

namespace SteamlessController.Driver;

public static class SteamlessDriver {
	public const uint VendorId = 0x28DE;
	public static readonly int[] ProductIds = [0x1302, 0x1304];
	public static readonly object OutputLock = new();

	public static int Main(string[] args) {
		using var host = Host.CreateDefaultBuilder(args)
			.UseWindowsService()
			.Build();

		var appStopping = host.Services
			.GetRequiredService<IHostApplicationLifetime>()
			.ApplicationStopping;

		using var cts = CancellationTokenSource.CreateLinkedTokenSource(appStopping);

		ControllerManager.StartMonitoring(cts.Token, DeviceLoop);

		try {
			host.Run();
		} finally {
			cts.Cancel(); // request device loops to stop
			ControllerManager.StopMonitoring(); // cancel tasks, close streams, clear state
		}

		return 0;
	}

	private static void DeviceLoop(ControllerDevice device, CancellationToken token) {
		var hidDevice = device.HidDevice;
		try {
			// Open device and register device closing for when the cancellation token is triggered
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

			// Devices update loop
			while (!token.IsCancellationRequested) {
				// Read the next hid report
				int read;
				try {
					read = stream.Read(buffer, 0, buffer.Length);
				} catch (Exception ex) when
					(ex is IOException or ObjectDisposedException or InvalidOperationException) {
					if (token.IsCancellationRequested) {
						break;
					}

					Console.WriteLine($"Read stopped: {ex.Message}");
					break;
				}

				// Skip if hid data is empty
				if (read <= 0) continue;

				// Decoding
				var decoder = new BitDecoder(buffer, read);
				var reportId = decoder.ReadByte();

				if (reportId == 0x42) {
					device.currentInputState = ControllerInput.Decode(decoder);
				} else {
					Console.WriteLine($"Received unknown report: 0x{reportId:x2}");
				}
			}
		} catch (Exception e) {
			Console.WriteLine($"Failed to open/read HID device: {e.Message}");
		}
	}
}