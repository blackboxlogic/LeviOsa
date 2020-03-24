using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace JpgToBmp
{
	class Program
	{
		private static Dictionary<string, ImageFormat> ImageFormats =
			new Dictionary<string, ImageFormat>(StringComparer.OrdinalIgnoreCase)
		{
			{"bmp", ImageFormat.Bmp },
			{"emf", ImageFormat.Emf },
			{"exif", ImageFormat.Exif },
			{"gif", ImageFormat.Gif },
			{"icon", ImageFormat.Icon },
			{"jpg", ImageFormat.Jpeg },
			{"png", ImageFormat.Png },
			{"tif", ImageFormat.Tiff },
			{"wmf", ImageFormat.Wmf },
			{"PBM", null },
			{"PGM", null }, // Use this as intermediary between other formats and svg
			{"PPM", null },
			{"SVG", null }
		};

		static void Main(string[] args)
		{
			Console.WriteLine(string.Join("\n", args));

			if (args.Length == 0 || args[0].Length == 0)
			{
				Console.WriteLine("Usage: JpgToBmp.exe <sourceFile> [<format>]");
				Console.WriteLine(string.Join("\n", ImageFormats.Keys));
				Console.WriteLine("default: svg");
				return;
			}

			string fromPath = args[0];
			string toFormat = args.Length > 1 ? args[1] : "svg";

			try
			{
				Image image = Open(fromPath);
				var toPath = Path.ChangeExtension(fromPath, toFormat);
				SaveAs(image, toPath);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Read();
			}
		}

		private static Image Open(string path)
		{
			Console.WriteLine($"Opening {path}");
			var extention = Path.GetExtension(path).ToUpper().Trim('.');

			if (extention == "SVG")
			{
				throw new FormatException("This program cannot open SVG files.");
			}
			else if (Enum.TryParse<ShaniSoft.Drawing.PNMType>(extention, out _))
			{
				return ShaniSoft.Drawing.PNM.ReadPNM(path);
			}
			else
			{
				return Image.FromFile(path);
			}
		}

		private static void SaveAs(Image image, string path)
		{
			Console.WriteLine($"Saving as {path}.");

			var extension = Path.GetExtension(path).ToUpper().Trim('.');

			if (extension == "SVG")
			{
				var tempPath = Path.ChangeExtension(path, "pgm");
				ShaniSoft.Drawing.PNM.WritePNM(tempPath, image);
				RunProcess(@"potrace-1.16.win64\potrace.exe", $"--svg \"{tempPath}\"");
				File.Delete(tempPath);
			}
			else if (Enum.TryParse<ShaniSoft.Drawing.PNMType>(extension, out _))
			{
				ShaniSoft.Drawing.PNM.WritePNM(path, image);
			}
			else
			{
				image.Save(path, ImageFormats[extension]);
			}
		}

		private static void RunProcess(string command, string args)
		{
			Console.WriteLine($"Running: {command} {args}");
			Process process = new Process()
			{
				StartInfo = new ProcessStartInfo
				{
					WindowStyle = ProcessWindowStyle.Hidden,
					FileName = command,
					Arguments = args,
					WorkingDirectory = Environment.CurrentDirectory
				}
			};
			process.Start();
			process.WaitForExit();
		}
	}
}
