﻿/******************************************************************************************************

  TamaniChess is a chess computer
  Copyright (C) 2013  Tamani UG

  Redistribution and use of the TamaniChess source code, TamaniChess constructions plans or any
  derivative works are permitted provided that the following conditions are met:

  * Redistributions may not be sold, nor may they be used in a commercial product or activity.

  * Redistributions that are modified from the original source must include the complete source code,
    including the source code for all components used by a binary built from the modified sources.
    However, as a special exception, the source code distributed need not include anything that is
    normally distributed (in either source or binary form) with the major components (compiler,
    kernel, and so on) of the operating system on which the executable runs, unless that component
    itself accompanies the executable.

  * Redistributions must reproduce the above copyright notice, this list of conditions and the
    following disclaimer in the documentation and/or other materials provided with the distribution.

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
  FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
  DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
  IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
  OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 
******************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Plossum.CommandLine;

namespace chess.shared
{

	public static class Config
	{
		public static string rootPath = "/cc/";
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

		public static void StringSaveToFileSecure(string File, string Text) {
			StringSaveToFileSecure(File, Text, CVert.DefaultEncoding);
		}

		public static void StringSaveToFileSecure(string File, string Text, System.Text.Encoding Enc) {
			string d = Path.GetDirectoryName(File) + "\\";
			string BaseName = Path.GetFileNameWithoutExtension(File);
			string Ext = Path.GetExtension(BaseName);

			string DnlFile = File;
			string BakFile = File + ".bak";
			string TmpFile = File + ".tmp";

			if (!Directory.Exists(d)) {
				Directory.CreateDirectory(d);
			}
			if (System.IO.File.Exists(TmpFile)) {
				System.IO.File.Delete(TmpFile);
			}

			FileStream fs = System.IO.File.Open(TmpFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
			try {
				byte[] buf = CVert.StringToBuffer(Text, Enc);
				fs.Write(buf, 0, buf.Length);
			}
			finally {
				fs.Flush();
				fs.Close();
			}

			if (System.IO.File.Exists(BakFile)) {
				if (System.IO.File.Exists(File)) {
					System.IO.File.Delete(BakFile); //Backup nur löschen, wenn original existiert
				}
				else {
					System.IO.File.Move(BakFile, File); //Wenn original nicht existiert, dann altes Backup als Original speichern
				}
			}

			if (System.IO.File.Exists(DnlFile)) {
				System.IO.File.Move(DnlFile, BakFile);
			}
			System.IO.File.Move(TmpFile, DnlFile);
			System.IO.File.Delete(BakFile);
		}

		public static string StringLoadFromFile(string File, bool IgnoreIfFileNotExists = false) {
			return StringLoadFromFile(File, CVert.DefaultEncoding, IgnoreIfFileNotExists);
		}

		public static string StringLoadFromFile(string File, System.Text.Encoding Enc, bool IgnoreIfFileNotExists = false) {
			if (File == "" || !System.IO.File.Exists(File)) {
				if (File != "" && System.IO.File.Exists(File + ".bak")) {
					File = File + ".bak";
				}
				else {
					if (IgnoreIfFileNotExists) {
						return "";
					}
					else {
						throw new Exception("file " + File + " doesn't exists!");
					}
				}
			}
			using (FileStream fs = System.IO.File.Open(File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				byte[] buf = new byte[(int)fs.Length];
				fs.Read(buf, 0, Convert.ToInt32(fs.Length));
				fs.Close();
				return CVert.BufferToString(buf, Enc);
			}
		}

		public static void StringSaveToFile(string File, string Text) {
			StringSaveToFile(File, Text, CVert.DefaultEncoding);
		}

		public static void StringSaveToFile(string File, string Text, System.Text.Encoding Enc) {
			if (System.IO.File.Exists(File)) {
				System.IO.File.Delete(File);
			}
			using (FileStream fs = System.IO.File.Open(File, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)) {
				try {
					byte[] buf = CVert.StringToBuffer(Text, Enc);
					fs.Write(buf, 0, buf.Length);
				}
				finally {
					fs.Flush();
				}
			}
		}

	}

	public static class CVert
	{

		public static byte[] StringToBuffer(string Str) {
			if (Str == null || Str == "") {
				return new byte[0];
			}
			else {
				return DefaultEncoding.GetBytes(Str);
			}
		}

		public static byte[] StringToBuffer(string Str, System.Text.Encoding Encoding) {
			if (Str == null || Str == "") {
				return new byte[0];
			}
			else {
				return Encoding.GetBytes(Str);
			}
		}

		private static System.Text.Encoding _DefaultEncoding = System.Text.Encoding.UTF8;
		public static System.Text.Encoding DefaultEncoding {
			get {
				return _DefaultEncoding;
			}
			set {
				_DefaultEncoding = value;
			}
		}

		public static string BufferToString(byte[] Buffer) {
			return BufferToString(Buffer, DefaultEncoding);
		}

		public static string BufferToString(byte[] Buffer, System.Text.Encoding Encoding) {
			return Encoding.GetString(Buffer);
		}


	}

	[CommandLineManager(ApplicationName = "Tamani Chess", Copyright = "Copyright (c) Tamani UG")]
	public class TProgrammOptions
	{

		[CommandLineOption(Description = "Displays this help text")]
		public bool help;

		[CommandLineOption(Description = "Starts test routine (for internal use only)")]
		public bool test;

		[CommandLineOption(Description = "Starts a temporary method (for internal use only)")]
		public bool temp;

		[CommandLineOption(Description = "Running as service")]
		public bool service;

		[CommandLineOption(Description = "Debugging")]
		public bool debug;

	}

}
