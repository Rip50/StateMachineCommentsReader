using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineCommentsReader
{
    enum GenerationState
    {
        InProgress, Completed
    }

    internal interface IReaderModel
    {
        string Directory { get; set; }
        string OutputFile { get; set; }
        string CommentMark { get; set; }

        event EventHandler<GenerationState> GenerationStateChanged;
        event EventHandler<string> CurrentFileChanged;


        void Generate();
    }

    internal class ReaderModel : IReaderModel
    {
        private IStateMachineReader _reader;

        public ReaderModel(IStateMachineReader reader)
        {
            _reader = reader;
            _reader.CurrentFileChanged += CurrentFileChanged;
        }

        public string CommentMark
        {
            get; set;
        } = "FIX";

        public string Directory
        {
            get; set;
        } = "D:\\Projects\\geometrix-shell";

        public string OutputFile
        {
            get; set;
        } = "D:\\FIX.txt";

        public event EventHandler<string> CurrentFileChanged;
        public event EventHandler<GenerationState> GenerationStateChanged;

        private void OnGenerationStateChanged(GenerationState state)
        {
            GenerationStateChanged?.Invoke(this, state);
        }

        public async void Generate()
        {
            OnGenerationStateChanged(GenerationState.InProgress);
            await _reader?.Process(Directory, OutputFile, CommentMark);
            OnGenerationStateChanged(GenerationState.Completed);
        }
    }
}
