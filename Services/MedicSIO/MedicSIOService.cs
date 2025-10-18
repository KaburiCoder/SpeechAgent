using SocketIOClient;
using SpeechAgent.Features.Settings;
using SpeechAgent.Services.MedicSIO.Consts;
using SpeechAgent.Services.MedicSIO.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

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
    bool IsConnected { get; }
  }

  public class MedicSIOService : IMedicSIOService
  {
    SocketIOClient.SocketIO _sio;

    public MedicSIOService()
    {
      var sioOptions = new SocketIOOptions
      {
        Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
        Reconnection = true,
      };
      _sio = new SocketIOClient.SocketIO("http://localhost:3100/agent", sioOptions);

      _sio.OnConnected += async (sender, e) =>
      {
        await JoinRoom();
      };
      _sio.OnReconnected += async (sender, e) =>
      {
        await JoinRoom();
      };

      // 연결 끊김 이벤트
      _sio.OnDisconnected += (sender, e) =>
      {
      };

      // 에러 이벤트
      _sio.On("error", response =>
      {
      });

      _sio.On(EventNames.PongFromWeb, async response =>
      {
        await response.CallbackAsync();
      });
    }

    public bool IsConnected => throw new NotImplementedException();

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
      var res = await EmitWithAck<BaseResponseDto>(EventNames.JoinRoom, new JoinRoomDto { RoomId = setting.Default.CONNECT_KEY });

      return res;
    }

    public async Task<BaseResponseDto> LeaveRoom()
    {
      var res = await EmitWithAck<BaseResponseDto>(EventNames.LeaveRoom);

      return res;
    }

    public async Task<BaseResponseDto> SendPatientInfo(PatientInfoDto patientInfo)
    {
      var res = await EmitWithAck<BaseResponseDto>(EventNames.SendPatientInfo, patientInfo);
      return res;
    }

    public Task RequestRecord()
    {
      throw new NotImplementedException();
    }

    public async Task<T> EmitWithAck<T>(string eventName, params object[] data)
    {
      var tcs = new TaskCompletionSource<T>();
      await _sio.EmitAsync(eventName, (res) =>
      {
        tcs.SetResult(res.GetValue<T>());
      }, data);

      return await tcs.Task;
    }
  }
}
