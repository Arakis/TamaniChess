using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace chess.shared
{

	public static class Config
	{
		public static string rootPath = "/home/pi/chess/";
		public static string applicationPath = Path.Combine(rootPath, "app") + "/";
		public static string gfxPath = Path.Combine(applicationPath, "gfx") + "/";
		public static string applicationExe = Path.Combine(applicationPath, "chess.application.exe");
		public static string updatePath = Path.Combine(rootPath, "update") + "/";
		public static string soundPath = Path.Combine(applicationPath, "sounds") + "/";
		public static string monoExe = "/usr/bin/";
	}

	//public static class ControlServer
	//{

	//	public static void start() {

	//	}

	//}

	public static class Tools
	{

		public static void copyDirectory(string sourceDirName, string destDirName, bool copySubDirs, bool overwite = false) {
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			if (!dir.Exists) {
				throw new DirectoryNotFoundException(
						"Source directory does not exist or could not be found: "
						+ sourceDirName);
			}

			if (!Directory.Exists(destDirName)) {
				Directory.CreateDirectory(destDirName);
			}

			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files) {
				string temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, overwite);
			}

			if (copySubDirs) {
				foreach (DirectoryInfo subdir in dirs) {
					string temppath = Path.Combine(destDirName, subdir.Name);
					copyDirectory(subdir.FullName, temppath, copySubDirs);
				}
			}
		}

	}

}
