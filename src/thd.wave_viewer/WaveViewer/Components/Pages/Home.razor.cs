
using MudBlazor;
using System.IO.Ports;
using Color = MudBlazor.Color;

namespace WaveViewer.Components.Pages
{
    partial class Home
    {
        /// <summary>
        /// 最小波特率
        /// </summary>
        public readonly int BAUDRATE_MIN = 1200;

        /// <summary>
        /// FFT 最小计算点数
        /// </summary>
        public readonly int FFT_CALCULATE_MIN_SIZE = 32;

        /// <summary>
        /// 最小采样频率
        /// </summary>
        public readonly int SAMPLE_FREQUENCY_MIN = 1000;

        /// <summary>
        /// 本地串口列表
        /// </summary>
        public string[] SerialPorts
        {
            get
            {
#if WINDOWS
                return System.IO.Ports.SerialPort.GetPortNames() ?? [];
#else
                return [];
#endif
            }
        }

        /// <summary>
        /// 串口号
        /// </summary>
        public string? SerialPortName {  get; set; }

        /// <summary>
        /// 串口
        /// </summary>
        public SerialPort? SerialPort { get; set; }

		/// <summary>
		/// 波特率
		/// </summary>
		private int _baudrate = 115200;
		public int Baudrate
		{
			get
			{
				return _baudrate;
			}

			set
			{
				if (value <= BAUDRATE_MIN)
				{
					_baudrate = BAUDRATE_MIN;
					_snackbar.Add($"波特率不能小于 {BAUDRATE_MIN} bps !", Severity.Error);
					return;
				}
				_baudrate = value;
			}
		}

        /// <summary>
        /// 校验位
        /// </summary>
        public Parity Parity { get; set; } = Parity.None;

        /// <summary>
        /// 数据位数
        /// </summary>
        public int DataBits { get; set; } = 8;

        /// <summary>
        /// 停止位数
        /// </summary>
        public StopBits StopBits { get; set; } = StopBits.One;

        /// <summary>
        /// FFT 计算点数
        /// </summary>
        private int _fftCalculateSize = 512;
        public int FFTCalculateSize
        {
            get
            {
                return _fftCalculateSize;
            }

            set
            {
                if(value < FFT_CALCULATE_MIN_SIZE)
                {
                    _fftCalculateSize = FFT_CALCULATE_MIN_SIZE;
                    _snackbar.Add($"FFT 计算点数不能小于 {FFT_CALCULATE_MIN_SIZE} !", Severity.Error);
                    return;
                }

                if(!Is2Power(value))
                {
                    _fftCalculateSize = 1 << (int)Math.Ceiling(Math.Log2(value));
                    _snackbar.Add("FFT 计算点数必须为 2 的整数次方!", Severity.Warning);
                    return;
                }

                _fftCalculateSize = value;
                _waveData = new double[_fftCalculateSize];
                _specData = new double[_fftCalculateSize >> 1];

                // 更新坐标轴
                _waveXLabels.Clear();
                _specXLabels.Clear();
                _waveChartSeries.Clear();
                _specChartSeries.Clear();

                StateHasChanged();
            }
        }

        /// <summary>
        /// 波形数据
        /// </summary>
        private double[]? _waveData;
        public double[] WaveData
        {
            get
            {
                return _waveData ??= new double[FFTCalculateSize];
            }
        }

        /// <summary>
        /// 频谱数据
        /// </summary>
        private double[]? _specData;
        public double[] SpecData
        {
            get
            {
                return _specData ??= new double[FFTCalculateSize >> 1];
            }
        }

		/// <summary>
		/// 波形图
		/// </summary>
		private readonly List<ChartSeries> _waveChartSeries = [];
        public List<ChartSeries> WaveCharSeries
        {
            get
            {
                if((_waveChartSeries.Count == 0))
                {
                    _waveChartSeries.Add(new ChartSeries
                    {
                        Name = "Wave",
                        Data = WaveData
                    });

                }

                return _waveChartSeries;
            }
        }

        /// <summary>
        /// 频谱图
        /// </summary>
        private readonly List<ChartSeries> _specChartSeries = [];
        public List<ChartSeries> SpecCharSeries
        {
            get
            {
                if(_specChartSeries.Count == 0)
                {
					_specChartSeries.Add(new ChartSeries
					{
						Name = "Spectrum",
						Data = SpecData
					});
				}

                return _specChartSeries;
            }
        }

        /// <summary>
        /// 波形图横坐标
        /// </summary>
        private readonly List<string> _waveXLabels = [];
        public string[] WaveXLabels
        {
            get
            {
                if(_waveXLabels.Count <= 0)
                {
                    for(int i = 0; i < FFTCalculateSize; i++)
                    {
                        _waveXLabels.Add(i % (FFTCalculateSize >> 2) == 0 ? i.ToString() : string.Empty);
                    }
                }
                return [.. _waveXLabels];
            }
        }

        /// <summary>
        /// 频谱图横坐标
        /// </summary>
        private readonly List<string> _specXLabels = [];
        public string[] SpecXLabels
        {
            get
            {
                if (_specXLabels.Count <= 0)
                {
					for (int i = 0; i < FFTCalculateSize; i++)
					{
						_specXLabels.Add(i.ToString());
					}
                }
                return [.. _specXLabels];
            }
        }

        /// <summary>
        /// 显示模式
        /// </summary>
        public DisplayMode DisplayMode { get; set; }

        /// <summary>
        /// 接收数据包数量
        /// </summary>
        public int PacketCount { get; set; } = 0;

        /// <summary>
        /// 峰峰值
        /// </summary>
        public int Vpp {  get; set; } = 0;

        /// <summary>
        /// 信号频率
        /// </summary>
        public int SignalFrequency {  get; set; }

        /// <summary>
        /// 数据包有效标志
        /// </summary>
        public bool IsPackageValid { get; set; }

        /// <summary>
        /// 串口是否连接
        /// </summary>
        public bool IsSerialPortConnected { get; set; }

        /// <summary>
        /// 串口启动按钮颜色
        /// </summary>
        public Color SerialPortLaunchButtonColor
        {
            get
            {
                return IsSerialPortConnected ? Color.Error : Color.Info;
            }
        }

        /// <summary>
        /// 串口启动按钮文字
        /// </summary>
        public string SerialPortLaunchButtonText
        {
            get
            {
                return IsSerialPortConnected ? "关 闭" : "连 接";
            }
        }

        /// <summary>
        /// 串口启动按钮图标
        /// </summary>
        public string SerialPortLaunchButtonIcon
        {
            get
            {
                return IsSerialPortConnected ? Icons.Material.Outlined.LinkOff : Icons.Material.Outlined.Link;
            }
        }

        /// <summary>
        /// 串口启动
        /// </summary>
        public void OnLaunchSerialPort()
        {
            // 关闭串口
            if(IsSerialPortConnected)
            {
#if WINDOWS
                SerialPort?.Close();
#else
                _snackbar.Add("该系统不支持串口操作！", Severity.Warning);
                return;
#endif
                IsSerialPortConnected = false;
                _snackbar.Add("串口已关闭！", Severity.Success);
                return;
            }

            // 打开串口
            if (SerialPortName == null)
            {
                _snackbar.Add("请选择串口！", Severity.Error);
                return;
            }

#if WINDOWS
            SerialPort = new SerialPort
            {
                PortName = SerialPortName,
                BaudRate = Baudrate,
                Parity = Parity,
                DataBits = DataBits,
                StopBits = StopBits
            };
            SerialPort.DataReceived += SerialPort_DataReceived;
            SerialPort.Open();
#else
            _snackbar.Add("该系统暂不支持打开串口！", Severity.Warning);
            return;
#endif
            IsSerialPortConnected = true;
            _snackbar.Add("串口已打开！", Severity.Success);
        }

        /// <summary>
        /// 串口接收中断回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // 接收串口数据
                var packet = SerialPort?.ReadLine();
                if (!IsPackageValid && (packet?.Split(':').First() == "FREQ"))
                {
                    PacketCount = 0;
                    IsPackageValid = true;
                    SignalFrequency = int.Parse(packet.Split(':')[1]);

                    // 清除波形
                    Array.Fill(WaveData, 0);
                    Array.Fill(SpecData, 0);

                    if(DisplayMode == DisplayMode.Increase)
                    {
                        await InvokeAsync(() =>
                        {
                            StateHasChanged();
                        });
                    }
                    return;
                }

                // 解析数据包
                var data = packet?.Split(',');
                if(data?.Length == 2)
                {
                    WaveData[PacketCount] = double.Parse(data[0]);
                    if (PacketCount < SpecData.Length)
                    {
                        SpecData[PacketCount] = double.Parse(data[1]);
                    }
                    PacketCount++;

                    if(DisplayMode == DisplayMode.Increase)
                    {
                        // 更新波形
                        await InvokeAsync(() =>
                        {
                            StateHasChanged();
                        });
                    }
                }

                if (PacketCount == FFTCalculateSize)
                {
                    IsPackageValid = false;
                    Vpp = (int)(WaveData.Max() - WaveData.Min());

                    await InvokeAsync(() => 
                    {
                        StateHasChanged();
                    });
                }
            }
            catch
            {
                IsPackageValid = false;
            }
        }

        /// <summary>
        /// 判断是否为 2 的整数次方
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private bool Is2Power(int x)
        {
            return (x - (1 << (int)Math.Log2(x))) == 0;
        }
    }

    public enum DisplayMode
    {
        None = 0,
        Increase
    }
}
