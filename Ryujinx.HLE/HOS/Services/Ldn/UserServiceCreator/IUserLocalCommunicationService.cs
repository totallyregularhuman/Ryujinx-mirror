﻿using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Services.Ldn.Types;
using System;
using System.Net;

namespace Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator
{
    class IUserLocalCommunicationService : IpcService
    {
        // TODO: Determine what the hardcoded unknown value is.
        private const int UnknownValue = 90;

        private NetworkInterface _networkInterface;

        private int _stateChangeEventHandle = 0;

        public IUserLocalCommunicationService(ServiceCtx context)
        {
            _networkInterface = new NetworkInterface(context.Device.System);
        }

        [Command(0)]
        // GetState() -> s32 state
        public ResultCode GetState(ServiceCtx context)
        {
            if (_networkInterface.NetworkInterfaceState != ResultCode.Success)
            {
                context.ResponseData.Write((int)NetworkState.Error);

                return ResultCode.Success;
            }

            ResultCode result = _networkInterface.GetState(out NetworkState state);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write((int)state);
            }

            return result;
        }

        [Command(100)]
        // AttachStateChangeEvent() -> handle<copy>
        public ResultCode AttachStateChangeEvent(ServiceCtx context)
        {
            if (_stateChangeEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_networkInterface.StateChangeEvent.ReadableEvent, out _stateChangeEventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_stateChangeEventHandle);

            // Return ResultCode.InvalidArgument if handle is null, doesn't occur in our case.

            return ResultCode.Success;
        }

        [Command(400)]
        // InitializeOld(u64, pid)
        public ResultCode InitializeOld(ServiceCtx context)
        {
            return _networkInterface.Initialize(UnknownValue);
        }

        [Command(401)]
        // Finalize()
        public ResultCode Finalize(ServiceCtx context)
        {
            return _networkInterface.Finalize();
        }

        [Command(402)] // 7.0.0+
        // Initialize(u64 ip_addresses, u64, pid)
        public ResultCode Initialize(ServiceCtx context)
        {
            // TODO: Determine what addresses are.

            ulong ipAddresses = context.RequestData.ReadUInt64();

            IPAddress unknownAddress1 = new IPAddress((uint)(ipAddresses & uint.MaxValue));
            IPAddress unknownAddress2 = new IPAddress((uint)(ipAddresses >> 32));

            return _networkInterface.Initialize(UnknownValue, version: 1, unknownAddress1, unknownAddress2);
        }
    }
}