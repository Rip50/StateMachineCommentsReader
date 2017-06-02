using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineCommentsReader
{
    internal interface IStateMachineReader
    {
        event EventHandler<string> CurrentFileChanged;
        Task Process(string directory, string output, string commentMark);
    }


    internal interface IState
    {
        IState Succeeded { get; set; }
        IState Failed { get; set; }
        IState Next(char i);
        void Write(char i);
    }

    internal delegate void WriteInFileFunc(char i);

    internal class StandardState : IState
    {
        private char _m;
        private WriteInFileFunc _write;

        public IState Succeeded { get; set; }

        public IState Failed { get; set; }

        public IState Next(char i)
        {
            if (i.CompareTo(_m) == 0)
            {
                return Succeeded;
            }
            Write(i);
            return Failed;
        }

        public void Write(char i)
        {
            _write(i);
        }

        public StandardState(char expected, IState succeeded, IState failure, WriteInFileFunc writeFunc)
        {
            _m = expected;
            Succeeded = succeeded == null ? this : succeeded;
            Failed = failure == null ? this : failure;
            _write = writeFunc;
        }
    }

    internal abstract class StatementsFactory
    {
        protected TextWriter _writer;
        protected string _mark;

        public abstract IState MakeBeginSequence();
        public abstract IState MakeBodySequence();
        public abstract IState MakeWholeSequence();
        
        protected virtual IState FromSequence(string sequence)
        {
            if (sequence == null || sequence.Length < 1)
                return new StandardState(char.MinValue, null, null, WriteNothing);
            var first = new StandardState(sequence[0], null, null, WriteNothing);
            var last = first;
            for (int i = 1; i < sequence.Length; i++)
            {
                var current = new StandardState(sequence[i], null, first, WriteNothing);
                last.Succeeded = current;
                last.Failed = first;
                last = current;
            }

            return first;
        }

        protected virtual IState FindLastSucceeded(IState state)
        {
            if (state.Succeeded == state || state.Succeeded == null)
                return state;
            return FindLastSucceeded(state.Succeeded);
        }

        public StatementsFactory(TextWriter writer, string mark)
        {
            _writer = writer;
            _mark = mark;
        }

        protected void WriteNothing(char i)
        { }


        protected void WriteChar(char i)
        {
            _writer?.Write(i);
        }
    }

    internal class SingleLineCommentFactory : StatementsFactory
    {
        public SingleLineCommentFactory(TextWriter writer, string mark) : base(writer, mark)
        {
        }

        public override IState MakeBeginSequence()
        {
            return FromSequence($"//{_mark}");
        }

        public override IState MakeBodySequence()
        {
            var begin = MakeBeginSequence();
            var last = FindLastSucceeded(begin);
            last.Failed = begin;
            last.Succeeded = new StandardState('\n', begin, null, WriteChar);
       
            return begin;
        }

        public override IState MakeWholeSequence()
        {
            return MakeBodySequence();
        }
    }

    internal class MultiLineCommentFactory : StatementsFactory
    {
        public MultiLineCommentFactory(TextWriter writer, string mark) : base(writer, mark)
        {
        }

        public override IState MakeBeginSequence()
        {
            return FromSequence($"/*{_mark}");
        }

        public override IState MakeBodySequence()
        {
            var begin = MakeBeginSequence();
            var last = FindLastSucceeded(begin);
            last.Failed = begin;
            last.Succeeded = new StandardState('*', null, null, WriteChar);
            last.Succeeded.Succeeded = new StandardState('/', begin, last.Succeeded, WriteNothing);
            return begin;
        }

        public override IState MakeWholeSequence()
        {
            return MakeBodySequence();
        }
    }

    [TestClass]
    public class StandardStateMachineReader_Test
    {
        [TestMethod]
        public void test_Process()
        {
            using (var writer = new StringWriter())
            {
                var factory = new MultiLineCommentFactory(writer, "FIX");
                var inst = factory.MakeWholeSequence();

                var str = "asdfasdf adsfasduiw awe aew /*FIX 12345\nasddasd\n1234*/\n1234";//"A //FIX asdas\n";//
                foreach (var ch in str)
                {
                    var hash = inst.GetHashCode();
                    inst = inst.Next(ch);
                }

                Assert.AreEqual(" 12345\nasddasd\n1234", writer.GetStringBuilder().ToString());

            }
        }
    }

    internal class StandardStateMachineReader : IStateMachineReader
    {
        public event EventHandler<string> CurrentFileChanged;

        private void OnCurrentFileChanged(string file)
        {
            CurrentFileChanged?.Invoke(this, file);
        }

        public async Task Process(string directory, string output, string commentMark)
        {

            using (var writer = new StreamWriter(output, false))
            {
                var factory = new SingleLineCommentFactory(writer, commentMark);
                var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    OnCurrentFileChanged(file);
                    var inst = factory.MakeWholeSequence();
                    using (var reader = new StreamReader(file))
                    {
                        foreach(var ch in reader.ReadToEnd())
                        {
                            inst = inst.Next(ch);
                        }

                    }
                }
            }
        }
    }
}
