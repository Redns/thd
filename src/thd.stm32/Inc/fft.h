#ifndef __FFT_H__
#define __FFT_H__

#include "math.h"

#define FFT_N                       512       
#define FFT_SIN_TABLE_LENGTH        (FFT_N / 4 + 1)        

#define PI                          3.1415926535

typedef struct 
{
    double real;
    double imag;
} complex_t;

void fft_init();
double fft_calculate(complex_t *signal, double sample_freq);

#endif

