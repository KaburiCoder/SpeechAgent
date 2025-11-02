using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.Messaging;
using SocketIOClient;
using SocketIOClient.Transport;
using SpeechAgent.Constants;
using SpeechAgent.Features.Settings;
using SpeechAgent.Messages;
using SpeechAgent.Services.MedicSIO.Args;
using SpeechAgent.Services.MedicSIO.Consts;
using SpeechAgent.Services.MedicSIO.Dto;

namespace SpeechAgent.Services.MedicSIO
{
  public interface IMedicSIOService
  {
    Task Connect();
    Task DisConnect();
    Task<BaseResponseDto> JoinRoom();
    Task<BaseResponseDto> LeaveRoom();
    Task<BaseResponseDto> SendPatientInfo(PatientInfoDto patientInfo);
    Task RequestRecord();
    Task<RequestSummaryResponseDto> RequestSummary(RequestSummaryDto dto);
    bool IsConnected { get; }
    bool IsRoomJoined { get; }
  }

  public class MedicSIOService : IMedicSIOService
  {
    SocketIOClient.SocketIO _sio;
    bool _isRoomJoined = false;
    private readonly ISettingsService _settingsService;

    public MedicSIOService(ISettingsService settingsService)
    {
      _settingsService = settingsService;

      var sioOptions = new SocketIOOptions
      {
        Path = "/api/socket.io",
        //Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
        Reconnection = true,
        ReconnectionAttempts = int.MaxValue,
        ReconnectionDelay = 2000,
        ReconnectionDelayMax = 5000, // 최대 1초까지만 증가
        RandomizationFactor = 0.0, // 랜덤 지연 제거
        AutoUpgrade = false,
        Transport = TransportProtocol.WebSocket,
      };
      _sio = new SocketIOClient.SocketIO(ApiConfig.SocketBaseUrl, sioOptions);

      _sio.OnConnected += async (sender, e) =>
      {
        WeakReferenceMessenger.Default.Send(new MedicSIOConnectionChangedMessage(true));
        await JoinRoom();
      };

      _sio.OnReconnected += async (sender, e) =>
      {
        WeakReferenceMessenger.Default.Send(new MedicSIOConnectionChangedMessage(true));
        await JoinRoom();
      };

      // 연결 끊김 이벤트
      _sio.OnDisconnected += (sender, e) =>
      {
        WeakReferenceMessenger.Default.Send(new MedicSIOConnectionChangedMessage(false));
        IsRoomJoined = false;
      };

      // 에러 이벤트
      _sio.On("error", response => { });

      _sio.On(
        EventNames.PingFromWeb,
        async response =>
        {
          await App.Current.Dispatcher.InvokeAsync(() =>
          {
            WeakReferenceMessenger.Default.Send(new WebPingReceivedMessage(DateTime.Now));
          });
          await response.CallbackAsync(
            new PingFromWebDto { TargetAppName = _settingsService.Settings.TargetAppName }
          );
        }
      );

      _sio.On(
        EventNames.ReceiveAudio,
        response =>
        {
          var data = response.GetValue<ReceiveAudioArgs>();

          // data.OpusBuffer 를 파일로 저장
          var di = new DirectoryInfo($@"c:\VoiceMedic\{data.Chart}");
          if (!di.Exists)
            di.Create();

          var filePath = Path.Join(di.FullName, $"{DateTime.Now:yyyy_MM_dd HH_mm_ss}.webm");

          try
          {
            File.WriteAllBytes(filePath, data.AudioBuffer);
          }
          catch (Exception ex)
          {
            Debug.WriteLine(ex.Message);
          }
        }
      );
    }

    public class ReceiveAudioArgs
    {
      [JsonPropertyName("audioBuffer")]
      public byte[] AudioBuffer { get; set; } = [];

      [JsonPropertyName("chart")]
      public string Chart { get; set; } = string.Empty;
    }

    public bool IsConnected => _sio.Connected;
    public bool IsRoomJoined
    {
      get => _isRoomJoined;
      private set
      {
        if (_isRoomJoined != value)
        {
          WeakReferenceMessenger.Default.Send(new MedicSIOJoinRoomChangedMessage(value));
        }
        _isRoomJoined = value;
      }
    }

    public Task Connect()
    {
      return _sio.ConnectAsync();
    }

    public Task DisConnect()
    {
      return _sio.DisconnectAsync();
    }

    public async Task<BaseResponseDto> JoinRoom()
    {
      var res = await EmitWithAck<BaseResponseDto>(EventNames.JoinRoom, GetRoomDto());
      IsRoomJoined = res.Success;
      return res;
    }

    public async Task<BaseResponseDto> LeaveRoom()
    {
      var res = await EmitWithAck<BaseResponseDto>(EventNames.LeaveRoom, GetRoomDto());

      return res;
    }

    public async Task<BaseResponseDto> SendPatientInfo(PatientInfoDto patientInfo)
    {
      var res = await EmitWithAck<BaseResponseDto>(
        EventNames.SendPatientInfo,
        GetRoomDto(),
        patientInfo
      );
      return res;
    }

    public Task RequestRecord()
    {
      throw new NotImplementedException();
    }

    public async Task<T> EmitWithAck<T>(string eventName, params object[] data)
    {
      var tcs = new TaskCompletionSource<T>();
      try
      {
        await _sio.EmitAsync(
          eventName,
          (res) =>
          {
            tcs.SetResult(res.GetValue<T>());
          },
          data
        );
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      return await tcs.Task;
    }

    private RoomDto GetRoomDto()
    {
      string key = _settingsService.Settings.ConnectKey;

      return new RoomDto { RoomId = $"agent_{key}", To = $"web_{key}" };
    }

    public async Task<RequestSummaryResponseDto> RequestSummary(RequestSummaryDto dto)
    {
      var res = await EmitWithAck<BaseResponseWithDataDto<RequestSummaryResponseDto>>(
        EventNames.RequestSummary,
        GetRoomDto(),
        dto
      );

      return res.Data ?? new RequestSummaryResponseDto();
    }
  }
}
