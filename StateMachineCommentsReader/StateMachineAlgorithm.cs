using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineCommentsReader
{
    internal interface IStateMachineReader
    {
        Task Process(string directory, string output, string commentMark);
    }


    class StandardStateMachineReader : IStateMachineReader
    {
        public Task Process(string directory, string output, string commentMark)
        {
            throw new NotImplementedException();
        }
    }
}
