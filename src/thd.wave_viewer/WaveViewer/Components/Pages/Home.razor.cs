
using System.IO.Ports;

namespace WaveViewer.Components.Pages
{
    partial class Home
    {
        private string[]? _serialPorts;

        protected override async Task OnInitializedAsync()
        {
            RefreshSerialPorts();
            await base.OnInitializedAsync();
        }

        private void RefreshSerialPorts()
        {
#if WINDOWS
            _serialPorts = SerialPort.GetPortNames();
#endif
        }
    }
}
