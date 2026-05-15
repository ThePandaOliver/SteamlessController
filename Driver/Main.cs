using System.Text;
using HidSharp;
using SteamlessControllerDriver;
using SteamlessControllerDriver.utils;

const uint vendorId = 0x28DE;
int[] productIds = [0x1302, 0x1304];

var devices = DeviceList.Local.GetHidDevices()
	.Where(d => d.VendorID == vendorId && productIds.Contains(d.ProductID))
	.OrderBy(d => d.DevicePath, StringComparer.OrdinalIgnoreCase)
	.ToList();

if (devices.Count == 0) {
	Console.WriteLine($"No HID devices matched VID_0x{vendorId:X4} with specified product IDs.");
	return 1;
}

Console.WriteLine($"Found {devices.Count} HID device(s) for VID_0x{vendorId:X4}:");
for (var i = 0; i < devices.Count; i++) {
	var device = devices[i];
	Console.WriteLine($"[{i}] {device.GetProductName() ?? device.DevicePath}");
	Console.WriteLine($"    Manufacturer: {device.GetManufacturer() ?? "<unknown>"}");
	Console.WriteLine($"    DevicePath: {device.DevicePath}");
	Console.WriteLine($"    MaxInputReportLength: {device.GetMaxInputReportLength()}");
}

using var cts = new CancellationTokenSource();
ConsoleCancelEventHandler cancelHandler = (_, e) => {
	e.Cancel = true;
	cts.Cancel();
};
Console.CancelKeyPress += cancelHandler;

try {
	var outputLock = new object();
	var tasks = new List<Task>();
	for (var i = 0; i < devices.Count; i++) {
		var captureIndex = i;
		var captureDevice = devices[i];
		tasks.Add(Task.Run(() => DumpDeviceReports(captureIndex, captureDevice, outputLock, cts.Token),
			cts.Token));
	}

	Console.WriteLine("Listening for HID input reports. Press Ctrl+C to stop.");
	Task.WaitAll(tasks.ToArray());
	return 0;
} catch (AggregateException ex) when (cts.IsCancellationRequested) {
	foreach (var inner in ex.Flatten().InnerExceptions) {
		if (inner is OperationCanceledException) {
			continue;
		}

		Console.WriteLine(inner.Message);
	}

	return 0;
} finally {
	Console.CancelKeyPress -= cancelHandler;
}

void DumpDeviceReports(int deviceIndex, HidDevice device, object outputLock, CancellationToken token) {
	try {
		using var stream = device.Open();
		using var registration = token.Register(() => {
			try {
				stream.Close();
			} catch {
				// ignored
			}
		});

		var reportLength = Math.Max(1, device.GetMaxInputReportLength());
		var buffer = new byte[reportLength];
		Console.WriteLine($"[{deviceIndex}] opened {device.DevicePath}");

		while (!token.IsCancellationRequested) {
			int read;
			try {
				read = stream.Read(buffer, 0, buffer.Length);
			} catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidOperationException) {
				if (token.IsCancellationRequested) {
					break;
				}

				lock (outputLock) {
					Console.WriteLine($"[{deviceIndex}] read stopped: {ex.Message}");
				}

				break;
			}

			if (read <= 0) {
				continue;
			}

			var bitString = ToBitString(buffer, read);
			var timestamp = DateTimeOffset.UtcNow.ToString("O");
			var decoder = new BitDecoder(buffer, read);
			var report = ControllerReport.Decode(decoder);

			lock (outputLock) {
				// Console.WriteLine($"{timestamp} [{deviceIndex}] {device.DevicePath} {hex}");
				Console.WriteLine($"{timestamp} [{deviceIndex}] {device.DevicePath} {bitString}");
				// Console.WriteLine(report.ToString());
				Console.WriteLine(report.inputs.ToString());
			}
		}
	} catch (Exception ex) {
		lock (outputLock) {
			Console.WriteLine($"[{deviceIndex}] failed to open/read HID device: {ex.Message}");
		}
	}
}

string ToBitString(byte[] buffer, int length) {
	var sb = new StringBuilder(length * 8);
	for (var i = 0; i < length; i++) {
		sb.Append(Convert.ToString(buffer[i], 2).PadLeft(8, '0'));
	}

	return sb.ToString();
}