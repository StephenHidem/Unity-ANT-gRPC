using AntRadioGrpcService;
using Cysharp.Net.Http;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SmallEarthTech.AntRadioInterface;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class AntRadioService : IAntRadio
{
    private readonly IPAddress grpAddress = IPAddress.Parse("239.55.43.6");
    private const int multicastPort = 55437;        // multicast port
    private const int gRPCPort = 5073;              // gRPC port

    private gRPCAntRadio.gRPCAntRadioClient _client;
    private readonly ILogger<AntRadioService> _logger;
    private readonly CancellationTokenSource _cts;
    private GrpcChannel _channel;

    public IPAddress ServerIPAddress { get; private set; }

    public int NumChannels { get; private set; }

    public string ProductDescription { get; private set; }

    public uint SerialNumber { get; private set; }

    public string Version { get; private set; }

    public event EventHandler<AntResponse> RadioResponse;

    public AntRadioService(ILogger<AntRadioService> logger, CancellationTokenSource cancellationTokenSource)
    {
        _logger = logger;
        _cts = cancellationTokenSource;
    }

    public async Task FindAntRadioServerAsync()
    {
        IPEndPoint multicastEndPoint = new(grpAddress, multicastPort);
        byte[] req = Encoding.ASCII.GetBytes("AntRadioServer discovery request");
        UdpReceiveResult result;

        // initiate receive
        using UdpClient udpClient = new(AddressFamily.InterNetwork);
        var receiveTask = udpClient.ReceiveAsync();

        // loop every 2 seconds sending a message to the any listening servers
        while (!_cts.IsCancellationRequested)
        {
            // send request for ANT radio server
            _ = udpClient.Send(req, req.Length, multicastEndPoint);

            // get response from server, or timeout, or cancelled
            if (receiveTask.Wait(2000, _cts.Token))
            {
                result = receiveTask.Result;
                ServerIPAddress = result.RemoteEndPoint.Address;
                string msg = Encoding.ASCII.GetString(result.Buffer);
                _logger.LogInformation("ANT radio endpoint {ServerAddress}, message {Msg}", ServerIPAddress, msg);

                UriBuilder uriBuilder = new("http", ServerIPAddress.ToString(), gRPCPort);
                _channel = GrpcChannel.ForAddress(
                    uriBuilder.Uri,
                    new GrpcChannelOptions
                    {
                        HttpHandler = new YetAnotherHttpHandler { Http2Only = true },
                        DisposeHttpClient = true
                    });
                _client = new gRPCAntRadio.gRPCAntRadioClient(_channel);
                PropertiesReply reply = await _client.GetPropertiesAsync(new Empty());
                ProductDescription = reply.ProductDescription;
                SerialNumber = reply.SerialNumber;
                Version = reply.Version;
                break;
            }
            else
            {
                _logger.LogInformation("FindAntRadioServerAsync: Timeout. Retry.");
            }
        }
    }

    public async Task<IAntChannel[]> InitializeContinuousScanMode()
    {
        if (_channel == null)
        {
            _logger.LogError("_channel is null!");
            return new IAntChannel[0];
        }
        InitScanModeReply reply = await _client!.InitializeContinuousScanModeAsync(new Empty());
        AntChannelService[] channels = new AntChannelService[reply.NumChannels];
        for (byte i = 0; i < reply.NumChannels; i++)
        {
            channels[i] = new AntChannelService(_logger, i, _channel);
        }
        channels[0].HandleChannelResponseEvents(_cts.Token);
        return channels;
    }

    public void CancelTransfers(int cancelWaitTime)
    {
        throw new NotImplementedException();
    }

    public IAntChannel GetChannel(int num)
    {
        throw new NotImplementedException();
    }

    public Task<DeviceCapabilities> GetDeviceCapabilities(bool forceNewCopy = false, uint responseWaitTime = 1500)
    {
        throw new NotImplementedException();
    }

    public AntResponse ReadUserNvm(ushort address, byte size, uint responseWaitTime = 500)
    {
        throw new NotImplementedException();
    }

    public AntResponse RequestMessageAndResponse(SmallEarthTech.AntRadioInterface.RequestMessageID messageID, uint responseWaitTime, byte channelNum = 0)
    {
        throw new NotImplementedException();
    }

    public bool WriteRawMessageToDevice(byte msgID, byte[] msgData)
    {
        throw new NotImplementedException();
    }
}
