﻿using LibHac;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.FsSrv.Impl;
using LibHac.FsSrv.Sf;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Spl;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Path = System.IO.Path;

namespace Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy
{
    static class FileSystemProxyHelper
    {
        public static ResultCode OpenNsp(ServiceCtx context, string pfsPath, out IFileSystem openedFileSystem)
        {
            openedFileSystem = null;

            try
            {
                LocalStorage storage = new LocalStorage(pfsPath, FileAccess.Read, FileMode.Open);
                ReferenceCountedDisposable<LibHac.Fs.Fsa.IFileSystem> nsp = new(new PartitionFileSystem(storage));

                ImportTitleKeysFromNsp(nsp.Target, context.Device.System.KeySet);

                openedFileSystem = new IFileSystem(FileSystemInterfaceAdapter.CreateShared(ref nsp));
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        public static ResultCode OpenNcaFs(ServiceCtx context, string ncaPath, LibHac.Fs.IStorage ncaStorage, out IFileSystem openedFileSystem)
        {
            openedFileSystem = null;

            try
            {
                Nca nca = new Nca(context.Device.System.KeySet, ncaStorage);

                if (!nca.SectionExists(NcaSectionType.Data))
                {
                    return ResultCode.PartitionNotFound;
                }

                LibHac.Fs.Fsa.IFileSystem fileSystem = nca.OpenFileSystem(NcaSectionType.Data, context.Device.System.FsIntegrityCheckLevel);
                var sharedFs = new ReferenceCountedDisposable<LibHac.Fs.Fsa.IFileSystem>(fileSystem);

                openedFileSystem = new IFileSystem(FileSystemInterfaceAdapter.CreateShared(ref sharedFs));
            }
            catch (HorizonResultException ex)
            {
                return (ResultCode)ex.ResultValue.Value;
            }

            return ResultCode.Success;
        }

        public static ResultCode OpenFileSystemFromInternalFile(ServiceCtx context, string fullPath, out IFileSystem openedFileSystem)
        {
            openedFileSystem = null;

            DirectoryInfo archivePath = new DirectoryInfo(fullPath).Parent;

            while (string.IsNullOrWhiteSpace(archivePath.Extension))
            {
                archivePath = archivePath.Parent;
            }

            if (archivePath.Extension == ".nsp" && File.Exists(archivePath.FullName))
            {
                FileStream pfsFile = new FileStream(
                    archivePath.FullName.TrimEnd(Path.DirectorySeparatorChar),
                    FileMode.Open,
                    FileAccess.Read);

                try
                {
                    PartitionFileSystem nsp = new PartitionFileSystem(pfsFile.AsStorage());

                    ImportTitleKeysFromNsp(nsp, context.Device.System.KeySet);

                    string filename = fullPath.Replace(archivePath.FullName, string.Empty).TrimStart('\\');

                    Result result = nsp.OpenFile(out LibHac.Fs.Fsa.IFile ncaFile, filename.ToU8Span(), OpenMode.Read);
                    if (result.IsFailure())
                    {
                        return (ResultCode)result.Value;
                    }

                    return OpenNcaFs(context, fullPath, ncaFile.AsStorage(), out openedFileSystem);
                }
                catch (HorizonResultException ex)
                {
                    return (ResultCode)ex.ResultValue.Value;
                }
            }

            return ResultCode.PathDoesNotExist;
        }

        public static void ImportTitleKeysFromNsp(LibHac.Fs.Fsa.IFileSystem nsp, KeySet keySet)
        {
            foreach (DirectoryEntryEx ticketEntry in nsp.EnumerateEntries("/", "*.tik"))
            {
                Result result = nsp.OpenFile(out LibHac.Fs.Fsa.IFile ticketFile, ticketEntry.FullPath.ToU8Span(), OpenMode.Read);

                if (result.IsSuccess())
                {
                    Ticket ticket = new Ticket(ticketFile.AsStream());

                    keySet.ExternalKeySet.Add(new RightsId(ticket.RightsId), new AccessKey(ticket.GetTitleKey(keySet)));
                }
            }
        }

        public static Result ReadFsPath(out FsPath path, ServiceCtx context, int index = 0)
        {
            ulong position = context.Request.PtrBuff[index].Position;
            ulong size     = context.Request.PtrBuff[index].Size;

            byte[] pathBytes = new byte[size];

            context.Memory.Read(position, pathBytes);

            return FsPath.FromSpan(out path, pathBytes);
        }

        public static ref readonly FspPath GetFspPath(ServiceCtx context, int index = 0)
        {
            ulong position = (ulong)context.Request.PtrBuff[index].Position;
            ulong size = (ulong)context.Request.PtrBuff[index].Size;

            ReadOnlySpan<byte> buffer = context.Memory.GetSpan(position, (int)size);
            ReadOnlySpan<FspPath> fspBuffer = MemoryMarshal.Cast<byte, FspPath>(buffer);

            return ref fspBuffer[0];
        }

        public static ref readonly LibHac.FsSrv.Sf.Path GetSfPath(ServiceCtx context, int index = 0)
        {
            ulong position = (ulong)context.Request.PtrBuff[index].Position;
            ulong size = (ulong)context.Request.PtrBuff[index].Size;

            ReadOnlySpan<byte> buffer = context.Memory.GetSpan(position, (int)size);
            ReadOnlySpan<LibHac.FsSrv.Sf.Path> pathBuffer = MemoryMarshal.Cast<byte, LibHac.FsSrv.Sf.Path>(buffer);

            return ref pathBuffer[0];
        }
    }
}
