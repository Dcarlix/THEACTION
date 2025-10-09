using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionIA.Interfaces
{
    public interface ISpeechToText
    {
		Task<string> RecognizeSpeechAsync(string locale = "es-ES");
	}
}
