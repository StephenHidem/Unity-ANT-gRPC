using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmallEarthTech.AntPlus.Extensions.Hosting;
using SmallEarthTech.AntRadioInterface;
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

    // Start is called once before the first execution of Update after the MonoBehavior is created
    async void Start()
    {
        Debug.Log("Starting");

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
            }).
            Build();

        // search for an ANT radio server on the local network
        AntRadioService _antRadioService = _host.Services.GetRequiredService<IAntRadio>() as AntRadioService;
        await _antRadioService.FindAntRadioServerAsync();

        // populate ANT radio server info
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Label>("product-description").text = _antRadioService.ProductDescription;
        root.Q<Label>("server-ip").text = _antRadioService.ServerIPAddress.ToString();
        root.Q<Label>("serial-number").text = _antRadioService.SerialNumber.ToString();
        root.Q<Label>("version").text = _antRadioService.Version;

        AntCollection _antDevices = _host.Services.GetRequiredService<AntCollection>();
        _deviceListController = new AntDeviceListController();
        _deviceListController.InitializeAntDeviceList(root, _antDevices);

        // IMPORTANT: Initiate scanning on a background thread.
        _ = Task.Run(() => { _ = _antDevices.StartScanning(); });

        Debug.Log("Running");
    }

    // Update is called once per frame
    void Update()
    {
        _deviceListController?.Update();
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        // cancel the subscription to the ANT gRPC service
        _cancellationTokenSource.Cancel();
    }
}
