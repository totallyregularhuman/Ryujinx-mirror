﻿using ARMeilleure.Memory;
using Ryujinx.HLE.HOS.Services.Sdb.Pdm.QueryService.Types;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Sdb.Pdm.QueryService
{
    static class QueryPlayStatisticsManager
    {
        private static Dictionary<UInt128, ApplicationPlayStatistics> applicationPlayStatistics = new Dictionary<UInt128, ApplicationPlayStatistics>();

        internal static ResultCode GetPlayStatistics(ServiceCtx context, bool byUserId = false)
        {
            long inputPosition = context.Request.SendBuff[0].Position;
            long inputSize     = context.Request.SendBuff[0].Size;

            long outputPosition = context.Request.ReceiveBuff[0].Position;
            long outputSize     = context.Request.ReceiveBuff[0].Size;

            UInt128 userId = byUserId ? new UInt128(context.RequestData.ReadBytes(0x10)) : new UInt128();

            if (byUserId)
            {
                if (!context.Device.System.State.Account.TryGetUser(userId, out _))
                {
                    return ResultCode.UserNotFound;
                }
            }

            PlayLogQueryCapability queryCapability = (PlayLogQueryCapability)context.Device.System.ControlData.PlayLogQueryCapability;

            List<ulong> titleIds = new List<ulong>();

            for (int i = 0; i < inputSize / sizeof(ulong); i++)
            {
                titleIds.Add(BitConverter.ToUInt64(context.Memory.ReadBytes(inputPosition, inputSize), 0));
            }

            if (queryCapability == PlayLogQueryCapability.WhiteList)
            {
                // Check if input titleids are in the whitelist
                foreach (ulong titleId in titleIds)
                {
                    if (!context.Device.System.ControlData.PlayLogQueryableApplicationId.Contains(titleId))
                    {
                        return (ResultCode)Am.ResultCode.ObjectInvalid;
                    }
                }
            }

            MemoryHelper.FillWithZeros(context.Memory, outputPosition, (int)outputSize);

            // return ResultCode.ServiceUnavailable if data is locked by another process.
            IEnumerable<KeyValuePair<UInt128, ApplicationPlayStatistics>> filteredApplicationPlayStatistics;

            if (queryCapability == PlayLogQueryCapability.None)
            {
                filteredApplicationPlayStatistics = applicationPlayStatistics.Where(kv => kv.Value.TitleId == context.Process.TitleId);
            }
            else // PlayLogQueryCapability.All
            {
                filteredApplicationPlayStatistics = applicationPlayStatistics.Where(kv => titleIds.Contains(kv.Value.TitleId));
            }

            if (byUserId)
            {
                filteredApplicationPlayStatistics = filteredApplicationPlayStatistics.Where(kv => kv.Key == userId);
            }

            for (int i = 0; i < filteredApplicationPlayStatistics.Count(); i++)
            {
                MemoryHelper.Write(context.Memory, outputPosition + (i * Marshal.SizeOf<ApplicationPlayStatistics>()), filteredApplicationPlayStatistics.ElementAt(i).Value);
            }

            context.ResponseData.Write(filteredApplicationPlayStatistics.Count());

            return ResultCode.Success;
        }
    }
}