using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VovaScript
{
	public class Compiler
	{
		IStatement Program;
		string OutFileName;

		public Compiler(IStatement program, string filename)
		{
			Program = program;
			OutFileName = filename;
		}

		public void Compile()
		{

		}
	}
}
