using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Antlr4.Runtime;
using P8LuaGrammar;

namespace PicoSharp
{
    public static class P8Parser
    {
        private const int RAW_DATA_LENGTH = 0x4300;

        private const int IMAGE_WIDTH = 160;
        private const int IMAGE_HEIGHT = 205;

        public enum P8Type
        {
            Header,
            Lua,
            Gfx_4Bit,
            Gfx_8Bit,
            Gff,
            Label,
            Map,
            Sfx,
            Music,
            Count
        }

        private static string[] P8Name =
        {
            null,
            "__lua__",
            "__gfx__",
            "__gfx8__",
            "__gff__",
            "__label__",
            "__map__",
            "__sfx__",
            "__music__",
        };

        public static void Parse(string fileName, byte[] memory, out string luaString)
        {
            string extension = Path.GetExtension(fileName);

            if (extension.Equals(".png"))
                ParsePng(fileName, memory, out luaString);
            else
                ParseP8(fileName, memory, out luaString);

            luaString = CleanLuaCharacters(luaString);
        }

        private static string CleanLuaCharacters(string source)
        {
            if (String.IsNullOrEmpty(source))
                return null;

            // https://www.freeformatter.com/java-dotnet-escape.html
            // https://github.com/juanitogan/p8-programming-fonts/wiki/P8SCII

            source = source.Replace("\u2B05\uFE0F", "0");       // left (⬅️)
            source = source.Replace("\u27A1\uFE0F", "1");       // right (➡️)
            source = source.Replace("\u2B06\uFE0F", "2");       // up (⬆️)
            source = source.Replace("\u2B07\uFE0F", "3");       // down (⬇️)
            source = source.Replace("\uD83C\uDD7E\uFE0F", "4"); // o (🅾️)
            source = source.Replace("\u274E", "5");             // x (❎)

            source = source.Replace("\u008b", "0");             // left (⬅️)
            source = source.Replace("\u0091", "1");             // right (➡️)
            source = source.Replace("\u0094", "2");             // up (⬆️)
            source = source.Replace("\u0083", "3");             // down (⬇️)
            source = source.Replace("\u008e", "4");             // o (🅾️)
            source = source.Replace("\u0097", "5");             // x (❎)

            byte[] win1252 = { 0xC7, 0xFC, 0xE9, 0xE2, 0xE4, 0xE0, 0xE5, 0xE7, 0xEA, 0xEB, 0xE8, 0xEF, 0xEE, 0xEC, 0xC4, 0xC5, 0xC9, 0xE6, 0xC6, 0xF4, 0xF6, 0xF2, 0xFB, 0xF9, 0xFF, 0xD6 };

            for (int i = 0; i < win1252.Length; i++)
                source = source.Replace(Convert.ToString((char)(0x80 + i)), Convert.ToString((char)win1252[i]));

            //source = source.Replace("\u0084", "");
            //source = source.Replace("\u0001a", "");

            return source;
        }

        public static byte[] HexStringToByteArray(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var hex = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if ((c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'f') ||
                    (c >= 'A' && c <= 'F'))
                    hex.Append(c);
            }

            int n = hex.Length / 2;
            var data = new byte[n];
            for (int i = 0; i < n; i++)
                data[i] = Convert.ToByte(hex.ToString(i * 2, 2), 16);
            return data;
        }

        private static byte[] BitmapToBytes(Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            IntPtr ptr = data.Scan0;

            int numBytes = Math.Abs(data.Stride) * bitmap.Height;
            byte[] bytes = new byte[numBytes];

            Marshal.Copy(ptr, bytes, 0, numBytes);

            bitmap.UnlockBits(data);

            return bytes;
        }

        public static void ParseP8(string fileName, byte[] memory, out string luaString)
        {
            string source = File.ReadAllText(fileName, Encoding.UTF8);
            string[] stringArray = new string[(int)P8Type.Count];
            string[] lineArray = source.Split(new string[] { "\n", }, StringSplitOptions.None);
            P8Type p8Type = P8Type.Header;

            foreach (var raw in lineArray)
            {
                var line = raw.TrimEnd('\r');
                bool tokenFound = false;

                for (int i = 0; i < P8Name.Length; i++)
                {
                    var tag = P8Name[i];
                    if (tag != null && line.StartsWith(tag, StringComparison.Ordinal))
                    {
                        p8Type = (P8Type)i;
                        tokenFound = true;
                        break;
                    }
                }
                if (tokenFound) continue;

                stringArray[(int)p8Type] += raw + "\n";
            }

            byte[] spriteData = HexStringToByteArray(stringArray[(int)P8Parser.P8Type.Gfx_4Bit]);
            byte[] spriteFlagsData = HexStringToByteArray(stringArray[(int)P8Parser.P8Type.Gff]);
            byte[] mapData = HexStringToByteArray(stringArray[(int)P8Parser.P8Type.Map]);
            byte[] sfxData = HexStringToByteArray(stringArray[(int)P8Parser.P8Type.Sfx]);
            byte[] musicData = HexStringToByteArray(stringArray[(int)P8Parser.P8Type.Music]);

            if (spriteData != null)
                Array.Copy(spriteData, 0, memory, Emulator.MEMORY_SPRITES, spriteData.Length);

            if (spriteFlagsData != null)
                Array.Copy(spriteFlagsData, 0, memory, Emulator.MEMORY_SPRITEFLAGS, spriteFlagsData.Length);

            if (mapData != null)
                Array.Copy(mapData, 0, memory, Emulator.MEMORY_MAP, mapData.Length);

            for (int i = Emulator.MEMORY_SPRITES; i < Emulator.MEMORY_SPRITES + Emulator.MEMORY_SPRITES_SIZE; i++)
                memory[i] = (byte)((memory[i] << 4) | (memory[i] >> 4));

            for (int i = Emulator.MEMORY_SPRITES_MAP; i < Emulator.MEMORY_SPRITES_MAP + Emulator.MEMORY_SPRITES_MAP_SIZE; i++)
                memory[i] = (byte)((memory[i] << 4) | (memory[i] >> 4));

            if (sfxData != null)
            {
                int readOffset = 0, writeOffset = 0;

                while (readOffset < sfxData.Length)
                {
                    byte byte0 = sfxData[readOffset++];
                    byte byte1 = sfxData[readOffset++];
                    byte byte2 = sfxData[readOffset++];
                    byte byte3 = sfxData[readOffset++];

                    for (int i = 0; i < 16; i++)
                    {
                        byte byteA = sfxData[readOffset++];
                        byte byteB = sfxData[readOffset++];
                        byte byteC = sfxData[readOffset++];
                        byte byteD = sfxData[readOffset++];
                        byte byteE = sfxData[readOffset++];

                        byte note1_pitch = byteA;
                        byte note1_waveform = (byte)(byteB >> 4);
                        byte note1_volume = (byte)(byteB & 0xF);
                        byte note1_effect = (byte)(byteC >> 4);

                        byte note2_pitch = (byte)((byteC << 4) | (byteD >> 4));
                        byte note2_waveform = (byte)(byteD & 0xF);
                        byte note2_volume = (byte)(byteE >> 4);
                        byte note2_effect = (byte)(byteE & 0xF);

                        //Debug.WriteLine(String.Format("{0}{1:X1}{2:X1}{3:X1}{4:X1}", m_toneMap[note1_pitch % m_toneMap.Length], note1_pitch / m_toneMap.Length, note1_waveform, note1_volume, note1_effect));
                        //Debug.WriteLine(String.Format("{0}{1:X1}{2:X1}{3:X1}{4:X1}", m_toneMap[note2_pitch % m_toneMap.Length], note2_pitch / m_toneMap.Length, note2_waveform, note2_volume, note2_effect));

                        short note1 = (short)((note1_waveform > 7 ? 1 << 15 : 0) | (note1_effect << 12) | (note1_volume << 9) | ((note1_waveform > 7 ? note1_waveform - 8 : note1_waveform) << 6) | note1_pitch);
                        short note2 = (short)((note2_waveform > 7 ? 1 << 15 : 0) | (note2_effect << 12) | (note2_volume << 9) | ((note2_waveform > 7 ? note2_waveform - 8 : note2_waveform) << 6) | note2_pitch);

                        memory[Emulator.MEMORY_SFX + writeOffset++] = (byte)(note1 & 0xFF);
                        memory[Emulator.MEMORY_SFX + writeOffset++] = (byte)(note1 >> 8);
                        memory[Emulator.MEMORY_SFX + writeOffset++] = (byte)(note2 & 0xFF);
                        memory[Emulator.MEMORY_SFX + writeOffset++] = (byte)(note2 >> 8);
                    }

                    memory[Emulator.MEMORY_SFX + writeOffset++] = byte0; // Editor Mode
                    memory[Emulator.MEMORY_SFX + writeOffset++] = byte1; // Speed
                    memory[Emulator.MEMORY_SFX + writeOffset++] = byte2; // Loop Start
                    memory[Emulator.MEMORY_SFX + writeOffset++] = byte3; // Loop End
                }
            }

            if (musicData != null)
            {
                int readOffset = 0, writeOffset = 0;

                while (readOffset < musicData.Length)
                {
                    byte flagsByte = musicData[readOffset++];
                    byte effectID1 = musicData[readOffset++];
                    byte effectID2 = musicData[readOffset++];
                    byte effectID3 = musicData[readOffset++];
                    byte effectID4 = musicData[readOffset++];

                    bool beginPatternLoop = ((flagsByte & (1 << 0)) != 0);
                    bool endPatternLoop = ((flagsByte & (1 << 1)) != 0);
                    bool stopAtEndOfPattern = ((flagsByte & (1 << 2)) != 0);

                    bool channel1Silence = (effectID1 == 0x41) || (effectID2 == 0x41) || (effectID3 == 0x41) || (effectID4 == 0x41);
                    bool channel2Silence = (effectID1 == 0x42) || (effectID2 == 0x42) || (effectID3 == 0x42) || (effectID4 == 0x42);
                    bool channel3Silence = (effectID1 == 0x43) || (effectID2 == 0x43) || (effectID3 == 0x43) || (effectID4 == 0x43);
                    bool channel4Silence = (effectID1 == 0x44) || (effectID2 == 0x44) || (effectID3 == 0x44) || (effectID4 == 0x44);

                    memory[Emulator.MEMORY_MUSIC + writeOffset++] = (byte)((channel1Silence ? 1 << 6 : effectID1 & 0x7F) | (beginPatternLoop ? 1 << 7 : 0));
                    memory[Emulator.MEMORY_MUSIC + writeOffset++] = (byte)((channel2Silence ? 1 << 6 : effectID2 & 0x7F) | (endPatternLoop ? 1 << 7 : 0));
                    memory[Emulator.MEMORY_MUSIC + writeOffset++] = (byte)((channel3Silence ? 1 << 6 : effectID3 & 0x7F) | (stopAtEndOfPattern ? 1 << 7 : 0));
                    memory[Emulator.MEMORY_MUSIC + writeOffset++] = (byte)(channel4Silence ? 1 << 6 : effectID4 & 0x7F);
                }
            }

            luaString = stringArray[(int)P8Parser.P8Type.Lua];
        }

        static byte AssembleByte(uint v)
        {
            byte A = (byte)(v >> 24);
            byte R = (byte)(v >> 16);
            byte G = (byte)(v >> 8);
            byte B = (byte)(v >> 0);
            return (byte)(((A & 3) << 6) | ((R & 3) << 4) | ((G & 3) << 2) | (B & 3));
        }

        public static void ParsePng(string fileName, byte[] memory, out string luaString)
        {
            luaString = null;

            using var bmp = (Bitmap)Bitmap.FromFile(fileName);

            if (bmp.Width != IMAGE_WIDTH || bmp.Height != IMAGE_HEIGHT)
                return;

            var pixels = BitmapToBytes(bmp);
            var packed = new byte[IMAGE_WIDTH * IMAGE_HEIGHT];

            using (var ms = new MemoryStream(pixels))
            using (var br = new BinaryReader(ms))
                for (int i = 0; i < packed.Length; i++)
                    packed[i] = AssembleByte(br.ReadUInt32());

            Buffer.BlockCopy(packed, 0, memory, 0, RAW_DATA_LENGTH);

            int off = RAW_DATA_LENGTH;

            static bool IsCompressedHeader(byte[] buf, int idx) =>
                idx + 3 < buf.Length &&
                buf[idx + 0] == (byte)':' &&
                buf[idx + 1] == (byte)'c' &&
                buf[idx + 2] == (byte)':' &&
                buf[idx + 3] == 0x00;

            static string DecodeCompressed(byte[] buf, int idx)
            {
                idx += 4;

                if (idx + 3 >= buf.Length) return string.Empty;
                int compressedLen = (buf[idx] << 8) | buf[idx + 1];
                idx += 2;

                idx += 2;

                int maxLen = Math.Min(32769 - RAW_DATA_LENGTH, Math.Max(0, buf.Length - idx));
                compressedLen = Math.Min(compressedLen, maxLen);

                const string lookup = "\n 0123456789abcdefghijklmnopqrstuvwxyz!#%(){}[]<>+=/*:;.,~_";
                var sb = new StringBuilder();
                int end = idx + compressedLen;

                while (idx < end)
                {
                    byte b = buf[idx++];

                    if (b == 0x00)
                    {
                        if (idx >= end) break;
                        sb.Append((char)buf[idx++]);
                    }
                    else if (b <= 0x3B)
                    {
                        sb.Append(lookup[b - 1]);
                    }
                    else
                    {
                        if (idx >= end) break;
                        byte bn = buf[idx++];
                        int back = ((b - 0x3C) << 4) + (bn & 0x0F);
                        int len = (bn >> 4) + 2;

                        int start = sb.Length - back;
                        for (int j = 0; j < len; j++)
                            sb.Append(sb[start + j]);
                    }
                }

                return sb.ToString();
            }

            static string DecodeRaw(byte[] buf, int idx)
            {
                int end = Array.IndexOf(buf, (byte)0x00, idx);
                if (end < 0) end = buf.Length;
                return Encoding.UTF8.GetString(buf, idx, end - idx);
            }

            string code = IsCompressedHeader(packed, off)
                ? DecodeCompressed(packed, off)
                : DecodeRaw(packed, off);

            luaString = code.Replace("\u0005", "").Replace("\u0000", "");
        }

        public static string CleanLua(string source, bool strictMode)
        {
            var processed = Preprocessor.Process(source, strictMode);

            var input = new AntlrInputStream(processed);
            var lexer = strictMode ? (Lexer)new P8LuaLexer(input) : new CombinedLuaLexer(input);
            lexer.RemoveErrorListeners();

            var tokens = new CommonTokenStream(lexer);
            var parser = strictMode ? (ILuaParser)new P8LuaParser(tokens) : new CombinedLuaParser(tokens);
            parser.RemoveErrorListeners();

            var context = parser.Chunk();
            var listener = new LuaListener(strictMode ? (ILuaListener)new ConcreteP8LuaListener() : (ILuaListener)new ConcreteCombinedLuaListener(), input, processed);
            var cleaned = listener.ReplaceAll(context);

            return cleaned;
        }
    }
}
