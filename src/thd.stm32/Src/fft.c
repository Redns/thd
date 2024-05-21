#include "fft.h"

static double FFT_SIN_TABLE[FFT_SIN_TABLE_LENGTH];

/**
 * @brief 复数相乘
 * @param a 
 * @param b 
 * @return complex_t 
 */
static complex_t fft_complex_multi(complex_t a, complex_t b)
{
	complex_t res;

	res.real = a.real*b.real - a.imag*b.imag;
	res.imag = a.real*b.imag + a.imag*b.real;

	return res;
}

/**
 * @brief 查表法计算正弦值
 * @param dec 弧度
 * @return double 正弦值
 */
static double fft_sin(double dec)
{
	int n = (int)(dec * FFT_N / (2 * PI));

	if((n >= 0) && (n <= FFT_N / 4))
	{
		return FFT_SIN_TABLE[n];
	}
	else if((n > FFT_N / 4) && (n < FFT_N / 2))
	{
		return FFT_SIN_TABLE[FFT_N / 2 - n];
	}
	else if ((n >= FFT_N / 2) && (n < 3 * FFT_N / 4))
	{
		return -FFT_SIN_TABLE[n - FFT_N / 2];
	}
	else if((n >= 3 * FFT_N / 4) && (n < 3 * FFT_N))
	{
		return -FFT_SIN_TABLE[FFT_N - n];
	}

	return 0;
}

/**
 * @brief 查表法计算余弦值
 * 
 * @param dec 
 * @return double 
 */
static double fft_cos(double dec)
{
	dec += PI / 2;
	if(dec > 2 * PI)
	{
		dec -= 2 * PI;
	}
	return fft_sin(dec);
}

/**
 * @brief 初始化 FFT 计算
 */
void fft_init()
{
	/* 初始化正弦表 */
	for(int i = 0; i < FFT_SIN_TABLE_LENGTH; i++)
	{
		FFT_SIN_TABLE[i] = sin(2 * PI * i / FFT_N);
	}
}

/**
 * @brief FFT 计算
 * @param signal 待计算的信号
 * @return 信号频率
 */
double fft_calculate(complex_t *signal, double sample_freq)
{
	register int f, m, nv2, nm1, i, k, l, j = 0;
	complex_t u, w, t;

	nv2 = FFT_N / 2;					//变址运算，即把自然顺序变成倒位序，采用雷德算法
	nm1 = FFT_N - 1;
	for (i = 0; i < nm1; ++i)
	{
		if (i < j)						//如果i<j,即进行变址
		{
			t = signal[j];
			signal[j] = signal[i];
			signal[i] = t;
		}
		k = nv2;						//求j的下一个倒位序
		while (k <= j)					//如果k<=j,表示j的最高位为1
		{
			j = j - k;					//把最高位变成0
			k = k / 2;					//k/2，比较次高位，依次类推，逐个比较，直到某个位为0
		}
		j = j + k;						//把0改为1
	}

	{
		int le, lei, ip;				//FFT运算核，使用蝶形运算完成FFT运算
		f = FFT_N;
		for (l = 1; (f = f / 2) != 1; ++l);				//计算l的值，即计算蝶形级数
		for (m = 1; m <= l; m++)						// 控制蝶形结级数
		{   
			//m表示第m级蝶形，l为蝶形级总数l=log（2）N
			le = 2 << (m - 1);							//le蝶形结距离，即第m级蝶形的蝶形结相距le点
			lei = le / 2;                               //同一蝶形结中参加运算的两点的距离
			u.real = 1.0;								//u为蝶形结运算系数，初始值为1
			u.imag = 0.0;
			w.real = fft_cos(PI / lei);					//w为系数商，即当前系数与前一个系数的商
			w.imag = -fft_sin(PI / lei);
			for (j = 0; j <= lei - 1; j++)				//控制计算不同种蝶形结，即计算系数不同的蝶形结
			{
				for (i = j; i <= FFT_N - 1; i = i + le)	//控制同一蝶形结运算，即计算系数相同蝶形结
				{
					ip = i + lei;						//i，ip分别表示参加蝶形运算的两个节点
					t = fft_complex_multi(signal[ip], u);					//蝶形运算，详见公式
					signal[ip].real = signal[i].real - t.real;
					signal[ip].imag = signal[i].imag - t.imag;
					signal[i].real = signal[i].real + t.real;
					signal[i].imag = signal[i].imag + t.imag;
				}
				u = fft_complex_multi(u, w);							//改变系数，进行下一个蝶形运算
			}
		}
	}

	double max_index = 0, max_value = 0;
	for (int i = 0; i < FFT_N / 2; ++i)
	{          
		signal[i].real = sqrt(signal[i].real*signal[i].real + signal[i].imag*signal[i].imag) / (FFT_N >> (i != 0));
		signal[i].imag = i * sample_freq / FFT_N;

		if(signal[i].real > max_value)
		{
			max_index = i;
			max_value = signal[i].real;
		}
	}

	return max_index * sample_freq / FFT_N;
}