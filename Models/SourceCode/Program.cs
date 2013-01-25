﻿using Reign.Core;
using System;

namespace Demo
{
	static class Program
	{
		#if WIN32
		[STAThread]
		#elif WINRT
		[MTAThread]
		#endif
		static void Main(string[] args)// NOTE: NaCl requires args
		{
			OS.Run(new MainApp(), 0);
		}
	}
}
