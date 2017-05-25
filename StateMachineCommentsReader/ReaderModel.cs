using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineCommentsReader
{
    internal interface IReaderModel
    {
        string Directory { get; set; }
        string OutputFile { get; set; }
        string CommentMark { get; set; }

        Task Generate();
    }

    internal class ReaderModel : IReaderModel
    {
        private IStateMachineReader _reader;

        public ReaderModel(IStateMachineReader reader)
        {
            _reader = reader;     
        }

        public string CommentMark
        {
            get; set;
        } = "";

        public string Directory
        {
            get; set;
        } = "";

        public string OutputFile
        {
            get; set;
        } = "";

        public async Task Generate()
        {
            await _reader?.Process(Directory, OutputFile, CommentMark);
        }
    }
}
