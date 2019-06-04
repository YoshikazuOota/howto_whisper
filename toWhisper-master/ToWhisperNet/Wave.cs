﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToWhisperNet
{
    /// <summary>
    /// Waveファイル操作
    /// </summary>
    public class Wave
    {
        /// <summary>
        /// 波形データ
        /// </summary>
        public double[] Data { get; set; }
        public double[] EData { get; set; }
        public WaveFormat Format { get; private set; }
        /// <summary>
        /// 音声データを読み込みます
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>読み込んだデータをdouble[](-1.0～1.0)に変換したもの</returns>
        public double[] Read(string filename)
        {
            Data = null;
            using (WaveFileReader reader = new WaveFileReader(filename))
            {
                Format = reader.WaveFormat;
                if (Format.Channels != 1)
                {
                    throw new FormatException("モノラル音声のみ対応しています");
                }
                if (Format.BitsPerSample != 16)
                {
                    throw new FormatException("16bit音声のみ対応しています");
                }
                byte[] src = new byte[reader.Length];
                reader.Read(src, 0, src.Length);
                Data = ConvertToDouble(src);
            }
            return Data;
        }
        /// <summary>
        /// 音声データをファイルに出力します
        /// </summary>
        /// <param name="filename">出力ファイル名</param>
        /// <param name="data">音声データ</param>
        public void Write(string filename, double[] data)
        {
            using (WaveFileWriter writer = new WaveFileWriter(filename, Format))
            {
                writer.Write(ConvertToByte(data), 0, data.Length*2);
            }
        }

        short nonzero = 1;

        private double[] ConvertToDouble(byte[] data)
        {
            double[] result = new double[data.Length / 2];
            for(int i=0; i<data.Length; i+=2)
            {
                short d = (short)(data[i] | (data[i + 1] << 8));
                if(d == 0)
                {
                    d = nonzero; // 信号レベルが0の状態が続くと以降が無音になってしまうための対応
                }
                result[i / 2] = d / 32767.0;
            }
            return result;
        }
        private byte[] ConvertToByte(double[] data)
        {
            byte[] result = new byte[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
            {
                short d = (short)(data[i] * 32767.0);
                if(d == nonzero)
                {
                    d = 0;
                }
                result[i * 2] = (byte)(d & 255);
                result[i * 2 + 1] = (byte)((d >> 8) & 255);
            }
            return result;
        }
    }
}
