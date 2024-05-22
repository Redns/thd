using System;
using Avalonia.Controls;
using ScottPlot.Avalonia;

namespace WaveViewer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        double[] siginal = new double[102400];
        for(int i = 0; i < 102400; i++)
        {
            siginal[i] = Math.Sin(i * 2 * Math.PI / 10240.0) + 0.5 * Math.Sin(i * 4 * Math.PI / 10240.0) + 0.25 * Math.Sin(i * 6 * Math.PI / 10240.0);
        }

        var avaPlot1 = this.Find<AvaPlot>("AvaPlot1");
        if(avaPlot1 == null)
        {
            return;    
        }
        
        avaPlot1.Plot.Add.Signal(siginal);
        avaPlot1.Plot.Benchmark.IsVisible = true;
        avaPlot1.Plot.Title("波形", 30);
        avaPlot1.Plot.Axes.Title.IsVisible = true;
        avaPlot1.Refresh();
    }
}