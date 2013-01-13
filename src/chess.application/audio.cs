using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading;
using Mono.Audio;
using System.IO;

namespace chess.application
{

	public class TAudioPlayer
	{

		private AudioDevice adev;
		private Dictionary<string, AudioData> files = new Dictionary<string, AudioData>();

		public TAudioPlayer() {
			adev = Mono.Audio.AudioDevice.CreateDevice(null);
		}

		private bool setupDone = false;

		public void load(string name, string file) {
			using (var s = new System.IO.FileStream(file, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read)) {
				var ms = LoadFromStream(s);
				AudioData data;
				if (System.IO.Path.GetExtension(file).ToLower() == ".au")
					data = new AuData(LoadFromStream(ms));
				else
					data = new WavData(LoadFromStream(ms));
				if (!setupDone) {
					setupDone = true;
					data.Setup(adev); //TODO!!!
				}
				files.Add(name, data);
				Console.WriteLine("Audio file loaded: " + file);
			}
		}

		private MemoryStream LoadFromStream(Stream s) {
			var mstream = new MemoryStream();
			byte[] buf = new byte[4096];
			int count;
			while ((count = s.Read(buf, 0, 4096)) > 0) {
				mstream.Write(buf, 0, count);
			}
			mstream.Position = 0;
			return mstream;
		}

		public void play(string name) {
			var data = files[name];
			//data.Setup(adev);
			try {
				data.Play(adev);
			}
			catch {
				//Console.WriteLine(ex.ToString());
			} //TODO
		}

	}

}

