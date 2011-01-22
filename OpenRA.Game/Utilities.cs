#region Copyright & License Information
/*
 * Copyright 2007-2010 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see LICENSE.
 */
#endregion

using System;
using System.Diagnostics;
using System.Threading;

namespace OpenRA
{
	public class Utilities
	{
		readonly string NativeUtility;
		readonly string Utility = "OpenRA.Utility.exe";
		
		public Utilities(string nativeUtility)
		{
			NativeUtility = nativeUtility;
		}
	
		public void ExtractZip(string zipFile, string path, Action<string> parseOutput, Action onComplete)
		{
			Process p = new Process();
			p.StartInfo.FileName = Utility;
			p.StartInfo.Arguments = "\"--extract-zip={0},{1}\"".F(zipFile, path);
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.Start();
			var t = new Thread( _ =>
			{
				using (var reader = p.StandardOutput)
				{
					// This is wrong, chrisf knows why
					while (!p.HasExited)
					{
						string s = reader.ReadLine();
						if (string.IsNullOrEmpty(s)) continue;
						parseOutput(s);
					}
				}
				onComplete();
			}) { IsBackground = true };
			t.Start();
		}
		
		public void CopyRAFiles(string cdPath, Action<string> parseOutput, Action onComplete)
		{
			Process p = new Process();
			p.StartInfo.FileName = Utility;
			p.StartInfo.Arguments = "\"--install-ra-packages={0}\"".F(cdPath);
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.Start();
			
			var t = new Thread( _ =>
			{
				using (var reader = p.StandardOutput)
				{
					// This is wrong, chrisf knows why
					while (!p.HasExited)
					{
						string s = reader.ReadLine();
						if (string.IsNullOrEmpty(s)) continue;
						parseOutput(s);
					}
				}
				onComplete();
			}) { IsBackground = true };
			t.Start();	
		}
		
		public void PromptFilepathAsync(string title, string message, bool directory, Action<string> withPath)
		{
			Process p = new Process();
			p.StartInfo.FileName = NativeUtility;
			p.StartInfo.Arguments = "--filepicker --title \"{0}\" --message \"{1}\" {2} --button-text \"Select\"".F(title, message, directory ? "--require-directory" : "");
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.EnableRaisingEvents = true;
			p.Exited += (_,e) =>
			{
				withPath(p.StandardOutput.ReadToEnd().Trim());
			};
			p.Start();
		}
	}
}