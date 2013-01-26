/******************************************************************************************************

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

		private Thread th;

		public TAudioPlayer() {
			adev = Mono.Audio.AudioDevice.CreateDevice(null);
			th = new Thread(mainLoop);
			th.Start();
		}

		private AutoResetEvent waiter = new AutoResetEvent(false);

		private Action action;

		private void mainLoop() {
			while (true) {
				try {
					waiter.WaitOne();
					if (action != null) {
						var act = action;
						action = null;
						act();
					}
				}
				catch (Exception ex) {
					Console.WriteLine(ex.ToString());
				}
			}
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
			action = () => {
				playInternal(name);
				playInternal("silence500ms");
			};
			waiter.Set();
		}

		private void playInternal(string name) {
			var data = files[name];
			//data.Setup(adev);
			try {
				data.Play(adev);
			}
			catch(Exception ex) {
				Console.WriteLine(ex.ToString());
				//adev.Wait();
			} //TODO
		}


	}

}

