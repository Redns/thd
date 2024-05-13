#ifndef __FFT_H__
#define __FFT_H__

#define FFT_N 256                                      //定义傅里叶变换的点数
#define PI 3.14159265358979323846264338327950288419717  //定义圆周率值

struct compx { double real, imag; };                    //定义一个复数结构

extern struct compx Compx[];							//FFT输入和输出：从Compx[0]开始存放，根据大小自己定义
extern double SIN_TAB[];								//正弦信号表

void Refresh_Data(struct compx *xin, int id, double wave_data);
void create_sin_tab(double *sin_t);
void FFT(struct compx *xin);
void Get_Result(struct compx *xin, double sample_frequency);

#endif

