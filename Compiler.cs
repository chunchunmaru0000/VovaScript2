using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VovaScript
{
	public class Compiler
	{
		IStatement program;

		public Compiler(IStatement program) => this.program = program;

		public void Compile()
		{

		}
	}
}
