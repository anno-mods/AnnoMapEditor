using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.UI.Controls.Progress
{
    public interface IProgress
    {
        int Value { get; set; }

        int Maximum { get; set; }

        string? Message { get; set; }

        bool IsInProgress { get; }

        bool IsDone { get; }


        public void Advance(string message, int value = 1)
        {
            Message = message;
            Value += value;
        }

        public async Task Run(string message, Task task, int value = 1)
        {
            Message = message;
            await task;
            Value += value;
        }

        public async Task Run(Action action, string message, int value = 1)
            => await Run(message, Task.Run(action), value);
    }
}
