
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
        /// 采样频率
        /// </summary>
        private int _sampleFrequency = 1000;
        public int SampleFrequency
        {
            get
            {
                return _sampleFrequency;
            }

            set
            {
                if(value < SAMPLE_FREQUENCY_MIN)
                {
                    _sampleFrequency = SAMPLE_FREQUENCY_MIN;
                    _snackbar.Add($"采样频率不能低于 {SAMPLE_FREQUENCY_MIN} Hz !", Severity.Error);
                    return;
                }
                _sampleFrequency = value;
            }
        }

        /// <summary>
        /// FFT 计算点数
        /// </summary>
        private int _fftCalculateSize = 32;
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

                // 更新坐标轴
                _waveData.Clear();
                _specData.Clear();
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
        private readonly List<double> _waveData = [];
        public double[] WaveData
        {
            get
            {
                if(_waveData.Count == 0)
                {
					for (int i = 0; i < FFTCalculateSize; i++)
					{
						_waveData.Add(10 * Math.Sin(2 * Math.PI * i / FFT_CALCULATE_MIN_SIZE));
					}
                }
                return [.. _waveData];
            }
        }

        /// <summary>
        /// 频谱数据
        /// </summary>
        private readonly List<double> _specData = [];
        public double[] SpecData
        {
            get
            {
                if(_specData.Count == 0)
                {
					for (int i = 0; i < FFTCalculateSize; i++)
					{
						_specData.Add((i % (FFTCalculateSize >> 3) == 0) ? Math.Abs(i - FFTCalculateSize / 2) : 0);
					}
				}
                return [.. _specData];
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
                if(_waveChartSeries.Count == 0)
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
        private List<string> _specXLabels = [];
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

        public string SerialPortLaunchButtonIcon
        {
            get
            {
                return IsSerialPortConnected ? Icons.Material.Outlined.LinkOff : Icons.Material.Outlined.Link;
            }
        }

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
            SerialPort.Open();
#else
            _snackbar.Add("该系统暂不支持打开串口！", Severity.Warning);
            return;
#endif
            IsSerialPortConnected = true;
            _snackbar.Add("串口已打开！", Severity.Success);
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
}
