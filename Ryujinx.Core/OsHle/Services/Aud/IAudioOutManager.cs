using ChocolArm64.Memory;
using Ryujinx.Audio;
using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.Core.OsHle.Services.Aud
{
    class IAudioOutManager : IpcService
    {
        private const string DefaultAudioOutput = "DeviceOut";

        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IAudioOutManager()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, ListAudioOuts     },
                { 1, OpenAudioOut      },
                { 2, ListAudioOutsAuto },
                { 3, OpenAudioOutAuto  }
            };
        }

        public long ListAudioOuts(ServiceCtx Context)
        {
            long Position = Context.Request.ReceiveBuff[0].Position;
            long Size     = Context.Request.ReceiveBuff[0].Size;

            int NameCount = 0;

            byte[] DeviceNameBuffer = Encoding.ASCII.GetBytes(DefaultAudioOutput + "\0");

            if ((ulong)DeviceNameBuffer.Length <= (ulong)Size)
            {
                Context.Memory.WriteBytes(Position, DeviceNameBuffer);

                NameCount++;
            }
            else
            {
                Context.Ns.Log.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");
            }

            Context.ResponseData.Write(NameCount);

            return 0;
        }

        public long OpenAudioOut(ServiceCtx Context)
        {
            IAalOutput AudioOut = Context.Ns.AudioOut;

            string DeviceName = AMemoryHelper.ReadAsciiString(
                Context.Memory,
                Context.Request.SendBuff[0].Position,
                Context.Request.SendBuff[0].Size);

            if (DeviceName == string.Empty)
            {
                DeviceName = DefaultAudioOutput;
            }

            long Position = Context.Request.ReceiveBuff[0].Position;
            long Size     = Context.Request.ReceiveBuff[0].Size;

            byte[] DeviceNameBuffer = Encoding.ASCII.GetBytes(DeviceName + "\0");

            if ((ulong)DeviceNameBuffer.Length <= (ulong)Size)
            {
                Context.Memory.WriteBytes(Position, DeviceNameBuffer);
            }
            else
            {
                Context.Ns.Log.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");
            }

            int SampleRate = Context.RequestData.ReadInt32();
            int Channels   = Context.RequestData.ReadInt32();

            Channels = (ushort)(Channels >> 16);

            if (SampleRate == 0)
            {
                SampleRate = 48000;
            }

            if (Channels < 1 || Channels > 2)
            {
                Channels = 2;
            }

            KEvent ReleaseEvent = new KEvent();

            ReleaseCallback Callback = () =>
            {
                ReleaseEvent.WaitEvent.Set();
            };

            int Track = AudioOut.OpenTrack(SampleRate, Channels, Callback, out AudioFormat Format);

            MakeObject(Context, new IAudioOut(AudioOut, ReleaseEvent, Track));

            Context.ResponseData.Write(SampleRate);
            Context.ResponseData.Write(Channels);
            Context.ResponseData.Write((int)Format);
            Context.ResponseData.Write((int)PlaybackState.Stopped);

            return 0;
        }
        
        public long ListAudioOutsAuto(ServiceCtx Context)
        {
            (long Position, long Size) = Context.Request.GetBufferType0x22();

            int NameCount = 0;

            byte[] DeviceNameBuffer = Encoding.ASCII.GetBytes(DefaultAudioOutput + "\0");

            if ((ulong)DeviceNameBuffer.Length <= (ulong)Size)
            {
                Context.Memory.WriteBytes(Position, DeviceNameBuffer);

                NameCount++;
            }
            else
            {
                Context.Ns.Log.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");
            }

            Context.ResponseData.Write(NameCount);

            return 0;
        }
        
        public long OpenAudioOutAuto(ServiceCtx Context)
        {
            IAalOutput AudioOut = Context.Ns.AudioOut;

            (long SendPosition, long SendSize) = Context.Request.GetBufferType0x21();
            
            string DeviceName = AMemoryHelper.ReadAsciiString(
                Context.Memory,
                SendPosition,
                SendSize
            );

            if (DeviceName == string.Empty)
            {
                DeviceName = DefaultAudioOutput;
            }

            (long Position, long Size) = Context.Request.GetBufferType0x22();

            byte[] DeviceNameBuffer = Encoding.ASCII.GetBytes(DeviceName + "\0");

            if ((ulong)DeviceNameBuffer.Length <= (ulong)Size)
            {
                Context.Memory.WriteBytes(Position, DeviceNameBuffer);
            }
            else
            {
                Context.Ns.Log.PrintError(LogClass.ServiceAudio, $"Output buffer size {Size} too small!");
            }
            
            long AudioParams1 = Context.RequestData.ReadInt64();
			long AudioParams2 = Context.RequestData.ReadInt64();

            KEvent ReleaseEvent = new KEvent();

            ReleaseCallback Callback = () =>
            {
                ReleaseEvent.WaitEvent.Set();
            };

            //TODO: Add Makeobject (currently unknown object)
            
            Context.ResponseData.Write(AudioParams1);
			Context.ResponseData.Write(AudioParams2);

            return 0;
        }        
    }
}
