using System.Diagnostics.Metrics;

namespace UmbObservability.Demo.OTel;

public static class DiagnosticsConfig
{
    //Resource name for Aspire Dashboard
    public const string ServiceName = "UmbObservability.Counts";

    public static Meter Meter = new(ServiceName);

    //Metric to track the number of page visits 
    public static Counter<int> PageCounter = Meter.CreateCounter<int>("page.count");

}