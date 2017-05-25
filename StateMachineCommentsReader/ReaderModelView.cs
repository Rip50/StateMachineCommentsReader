using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WinForms = System.Windows.Forms;

namespace StateMachineCommentsReader
{
    internal class GenerateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private IReaderModelView _model;
        private bool ExecutionInProgress {get; set; }

        public void ChangeExecutionState(bool state)
        {
            ExecutionInProgress = state;
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        private void UpdateCanExecute(object o, EventArgs args)
        {
            CanExecuteChanged?.Invoke(this, args);
        }

        public bool CanExecute(object parameter)
        {
            return _model.Directory?.Length != 0 && _model.OutputFile?.Length != 0 && _model.CommentMark.Length!=0 && !ExecutionInProgress;
        }

        public void Execute(object parameter)
        {
            _model.Generate();
        }

        public GenerateCommand(IReaderModelView model)
        {
            _model = model;
            if (_model != null)
            {
                _model.ExecutionInProgressChanged += ChangeExecutionState;
                _model.Updated += UpdateCanExecute;
            }
        }
    }

    internal class ChangeDirCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private IReaderModelView _model;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            using (var dlg = new WinForms.FolderBrowserDialog() { SelectedPath = _model.Directory })
            {
                if (dlg.ShowDialog() == WinForms.DialogResult.OK && _model != null)
                {
                    _model.Directory = dlg.SelectedPath;
                }
            }
            
        }

        public ChangeDirCommand(IReaderModelView model)
        {
            _model = model;
        }
    }

    internal class ChangeOutputFileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private IReaderModelView _model;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var dlg = new SaveFileDialog() { AddExtension=true, Filter="Текстовый файл (.txt)|*.txt", FileName=_model.OutputFile };
            if(dlg.ShowDialog() ?? false)
            {
                _model.OutputFile = dlg.SafeFileName;
            }
        }

        public ChangeOutputFileCommand(IReaderModelView model)
        {
            _model = model;
        }
    }

    internal interface IReaderModelView
    {
        event Action<bool> ExecutionInProgressChanged;
        event EventHandler Updated;

        ICommand ChangeDirCommand { get; }
        ICommand ChangeOutputFileCommand { get; }

        string OutputFile { get; set; }
        string Directory { get; set; }
        string CommentMark { get; set; }

        void Generate();
    }

    internal class ReaderModelView : IReaderModelView, INotifyPropertyChanged
    {
        public ICommand ChangeDirCommand { get; }
        public ICommand ChangeOutputFileCommand { get; }
        public ICommand GenerateCommand { get; }

        public string OutputFile
        {
            get
            {
                return _model.OutputFile;
            }

            set
            {
                _model.OutputFile = value;
                OnPropertyChanged("OutputFile");
                OnUpdate();
            }
        }

        public string Directory
        {
            get
            {
                return _model.Directory;
            }

            set
            {
                _model.Directory = value;
                OnPropertyChanged("Directory");
                OnUpdate();
            }
        }

        public string CommentMark
        {
            get
            {
                return _model.CommentMark;
            }

            set
            {
                _model.CommentMark = value;
                OnPropertyChanged("CommentMark");
                OnUpdate();
            }
        }

        private IReaderModel _model;

        public event PropertyChangedEventHandler PropertyChanged;

        public event Action<bool> ExecutionInProgressChanged;
        public event EventHandler Updated;

        private void OnExecutionInProgressChanged(bool state)
        {
            ExecutionInProgressChanged?.Invoke(state);
        }

        private void OnUpdate()
        {
            Updated?.Invoke(this, new EventArgs());
        }

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public async void Generate()
        {
            OnExecutionInProgressChanged(true);
            await _model.Generate();
            OnExecutionInProgressChanged(false);
        }

        public ReaderModelView(IReaderModel model)
        {
            _model = model;
            ChangeDirCommand = new ChangeDirCommand(this);
            ChangeOutputFileCommand = new ChangeOutputFileCommand(this);
            GenerateCommand = new GenerateCommand(this);
        }
    }
}
