using AntGrpcShared.ClientServices;
using Cysharp.Net.Http;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmallEarthTech.AntPlus.Extensions.Hosting;
using SmallEarthTech.AntRadioInterface;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class AntPlus : MonoBehaviour
{
    private IHost _host;
    private readonly string[] _options = {
        "--Logging:LogLevel:Default=Debug",
        "--TimeoutOptions:MissedMessages=10"
    };
    private AntDeviceListController _deviceListController;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _updateAntRadioInfo;
    private VisualElement _root;
    private AntRadioService _antRadioService;

    // Start is called once before the first execution of Update after the MonoBehavior is created
    void Start()
    {
        Debug.Log("Starting");
        _root = GetComponent<UIDocument>().rootVisualElement;
        _deviceListController = new AntDeviceListController(_root);

        _host = Host.CreateDefaultBuilder(_options).
            ConfigureLogging(logging =>
            {
                // Unity doesn't support event logging, so clear logging providers and add custom logger
                logging.ClearProviders().AddUnityLogger();
            }).
            UseAntPlus().   // add ANT libraries and hosting extensions to services
            ConfigureServices(services =>
            {
                // add the ANT radio service and cancellation token to signal app termination
                services.AddSingleton<IAntRadio, AntRadioService>();
                services.AddSingleton(_cancellationTokenSource);
                services.AddSingleton(new GrpcChannelOptions { HttpHandler = new YetAnotherHttpHandler { Http2Only = true }, DisposeHttpClient = true });
            }).
            Build();

        // search for an ANT radio server on the local network
        _antRadioService = _host.Services.GetRequiredService<IAntRadio>() as AntRadioService;
        _ = Task.Run(async () =>
        {
            try
            {
                await _antRadioService.FindAntRadioServerAsync();

                // populate ANT radio server info
                _updateAntRadioInfo = true;

                AntCollection _antDevices = _host.Services.GetRequiredService<AntCollection>();
                _deviceListController.InitializeAntDeviceList(_antDevices);

                // IMPORTANT: Initiate scanning on a background thread.
                await _antDevices.StartScanning();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("FindAntRadioServerAsync OperationCanceledException");
            }
        });

        Debug.Log("Running");
    }

    // Update is called once per frame
    void Update()
    {
        _deviceListController?.Update();
        if (_updateAntRadioInfo)
        {
            _updateAntRadioInfo = false;
            _root.Q<Label>("product-description").text = _antRadioService.ProductDescription;
            _root.Q<Label>("server-ip").text = _antRadioService.ServerIPAddress.ToString();
            _root.Q<Label>("serial-number").text = _antRadioService.SerialNumber.ToString();
            _root.Q<Label>("version").text = _antRadioService.Version;
        }
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        // cancel the subscription to the ANT gRPC service
        _cancellationTokenSource.Cancel();
    }
}
