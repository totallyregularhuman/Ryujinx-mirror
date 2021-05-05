﻿using FFmpeg.AutoGen;
using Ryujinx.Common.Logging;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Nvdec.H264
{
    unsafe class FFmpegContext : IDisposable
    {
        private delegate av_log_set_callback_callback_func LoggingDelegate(void* p0, int level, string format, byte* vl);

        private readonly av_log_set_callback_callback _logFunc;
        private readonly AVCodec* _codec;
        private AVPacket* _packet;
        private AVCodecContext* _context;

        public FFmpegContext()
        {
            _logFunc = Logging;

            // Redirect log output
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_MAX_OFFSET);
            ffmpeg.av_log_set_callback(_logFunc);

            _codec = ffmpeg.avcodec_find_decoder(AVCodecID.AV_CODEC_ID_H264);
            _context = ffmpeg.avcodec_alloc_context3(_codec);

            ffmpeg.avcodec_open2(_context, _codec, null);

            _packet = ffmpeg.av_packet_alloc();
        }

        private void Logging(void* p0, int level, string format, byte* vl)
        {
            if (level > ffmpeg.av_log_get_level())
            {
                return;
            }

            int lineSize = 1024;
            byte* lineBuffer = stackalloc byte[lineSize];
            int printPrefix = 1;

            ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);

            string line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer).Trim();

            switch (level)
            {
                case ffmpeg.AV_LOG_PANIC:
                case ffmpeg.AV_LOG_FATAL:
                case ffmpeg.AV_LOG_ERROR:
                    Logger.Error?.Print(LogClass.FFmpeg, line);
                    break;
                case ffmpeg.AV_LOG_WARNING:
                    Logger.Warning?.Print(LogClass.FFmpeg, line);
                    break;
                case ffmpeg.AV_LOG_INFO:
                    Logger.Info?.Print(LogClass.FFmpeg, line);
                    break;
                case ffmpeg.AV_LOG_VERBOSE:
                case ffmpeg.AV_LOG_DEBUG:
                case ffmpeg.AV_LOG_TRACE:
                    Logger.Debug?.Print(LogClass.FFmpeg, line);
                    break;
            }
        }

        public int DecodeFrame(Surface output, ReadOnlySpan<byte> bitstream)
        {
            // Ensure the packet is clean before proceeding
            ffmpeg.av_packet_unref(_packet);

            fixed (byte* ptr = bitstream)
            {
                _packet->data = ptr;
                _packet->size = bitstream.Length;

                int rc = ffmpeg.avcodec_send_packet(_context, _packet);

                if (rc != 0)
                {
                    return rc;
                }
            }

            return ffmpeg.avcodec_receive_frame(_context, output.Frame);
        }

        public void Dispose()
        {
            fixed (AVPacket** ppPacket = &_packet)
            {
                ffmpeg.av_packet_free(ppPacket);
            }

            ffmpeg.avcodec_close(_context);

            fixed (AVCodecContext** ppContext = &_context)
            {
                ffmpeg.avcodec_free_context(ppContext);
            }
        }
    }
}