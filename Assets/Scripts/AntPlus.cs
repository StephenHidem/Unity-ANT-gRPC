using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmallEarthTech.AntPlus.Extensions.Hosting;
using SmallEarthTech.AntRadioInterface;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class AntPlus : MonoBehaviour
{
    private IHost _host;
    private readonly string[] _options = {
        "--Logging:LogLevel:Default=Debug",
        "--TimeoutOptions:MissedMessages=10"
    };
    private AntRadioService _antRadioService;
    private AntDeviceListController _deviceListController;
    private CancellationTokenSource _cancellationTokenSource;
    private AntCollection _antDevices;

    // Start is called once before the first execution of Update after the MonoBehavior is created
    async void Start()
    {
        Debug.Log("Starting");

        _host = Host.CreateDefaultBuilder(_options).
            ConfigureLogging(logging =>
            {
                logging.ClearProviders().AddUnityLogger();
            }).
            UseAntPlus().
            ConfigureServices(services =>
            {
                services.AddSingleton<IAntRadio, AntRadioService>();
                services.AddSingleton<CancellationTokenSource>();
            }).
            Build();

        _cancellationTokenSource = _host.Services.GetService<CancellationTokenSource>();
        _antRadioService = _host.Services.GetRequiredService<IAntRadio>() as AntRadioService;
        await _antRadioService.FindAntRadioServerAsync();

        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Label>("product-description").text = _antRadioService.ProductDescription;
        root.Q<Label>("server-ip").text = _antRadioService.ServerIPAddress.ToString();
        root.Q<Label>("serial-number").text = _antRadioService.SerialNumber.ToString();
        root.Q<Label>("version").text = _antRadioService.Version;

        _antDevices = _host.Services.GetRequiredService<AntCollection>();

        _deviceListController = new AntDeviceListController();
        _deviceListController.InitializeAntDeviceList(root, _antDevices);

        await _antDevices.StartScanning();

        Debug.Log("Running");
    }

    // Update is called once per frame
    void Update()
    {
        _deviceListController?.Update();
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        //_cancellationTokenSource = new CancellationTokenSource();

    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        _cancellationTokenSource.Cancel();

    }
}
