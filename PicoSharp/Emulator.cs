using NLua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static PicoSharp.Emulator;

namespace PicoSharp
{
    public partial class Emulator
    {
        public const int P8_WIDTH = 128;
        public const int P8_HEIGHT = 128;

        public const int MEMORY_SPRITES = 0x0000;
        public const int MEMORY_SPRITES_SIZE = 0x1000;
        public const int MEMORY_SPRITES_MAP = 0x1000;
        public const int MEMORY_SPRITES_MAP_SIZE = 0x1000;
        public const int MEMORY_MAP = 0x2000;
        public const int MEMORY_MAP_SIZE = 0x1000;
        public const int MEMORY_SPRITEFLAGS = 0x3000;
        public const int MEMORY_SPRITEFLAGS_SIZE = 0x100;
        public const int MEMORY_MUSIC = 0x3100;
        public const int MEMORY_MUSIC_SIZE = 0x100;
        public const int MEMORY_SFX = 0x3200;
        public const int MEMORY_SFX_SIZE = 0x1100;
        public const int MEMORY_WORKRAM = 0x4300;
        public const int MEMORY_WORKRAM_SIZE = 0x1b00;
        public const int MEMORY_FONT = 0x5600;
        public const int MEMORY_FONT_SIZE = 0x2000;
        public const int MEMORY_CARTDATA = 0x5e00;
        public const int MEMORY_CARTDATA_SIZE = 0x100;
        public const int MEMORY_DRAWSTATE = 0x5f00;
        public const int MEMORY_DRAWSTATE_SIZE = 0x40;
        public const int MEMORY_HARDWARESTATE = 0x5f40;
        public const int MEMORY_HARDWARESTATE_SIZE = 0x40;
        public const int MEMORY_GPIO = 0x5f80;
        public const int MEMORY_GPIO_SIZE = 0x80;
        public const int MEMORY_SCREEN = 0x6000;
        public const int MEMORY_SCREEN_SIZE = 0x2000;

        public const int MEMORY_SIZE = 1024 * 64;
        public const int CART_MEMORY_SIZE = 0x4300;

        public const int MEMORY_PALETTES = 0x5f00;
        public const int MEMORY_CLIPRECT = 0x5f20;
        public const int MEMORY_LEFT_MARGIN = 0x5f24;
        public const int MEMORY_PENCOLOR = 0x5f25;
        public const int MEMORY_CURSOR = 0x5f26;
        public const int MEMORY_CAMERA = 0x5f28;
        public const int MEMORY_DEVKIT_MODE = 0x5f2d;
        public const int MEMORY_FILLP = 0x5f31;
        public const int MEMORY_FILLP_ATTR = 0x5f33;
        public const int MEMORY_COLOR_FILLP = 0x5f34;
        public const int MEMORY_LINE_VALID = 0x5f35;
        public const int MEMORY_MISCFLAGS = 0x5f36;
        public const int MEMORY_TLINE_MASK_X = 0x5f38;
        public const int MEMORY_TLINE_MASK_Y = 0x5f39;
        public const int MEMORY_TLINE_OFFSET_X = 0x5f3a;
        public const int MEMORY_TLINE_OFFSET_Y = 0x5f3b;
        public const int MEMORY_LINE_X = 0x5f3c;
        public const int MEMORY_LINE_Y = 0x5f3e;
        public const int MEMORY_RNG_STATE = 0x5f44;
        public const int MEMORY_BUTTON_STATE = 0x5f4c;
        public const int MEMORY_SPRITE_PHYS = 0x5f54;
        public const int MEMORY_SCREEN_PHYS = 0x5f55;
        public const int MEMORY_MAP_START = 0x5f56;
        public const int MEMORY_MAP_WIDTH = 0x5f57;
        public const int MEMORY_TEXT_ATTRS = 0x5f58;
        public const int MEMORY_SGET_DEFAULT = 0x5f59;
        public const int MEMORY_MGET_DEFAULT = 0x5f5a;
        public const int MEMORY_PGET_DEFAULT = 0x5f5b;
        public const int MEMORY_AUTO_REPEAT_DELAY = 0x5f5c;
        public const int MEMORY_AUTO_REPEAT_INTERVAL = 0x5f5d;
        public const int MEMORY_PALETTE_SECONDARY = 0x5f60;

        public const int SCREEN_SIZE = 128 * 128 * 4;
        public const int GLYPH_WIDTH = 4;
        public const int GLYPH_HEIGHT = 6;
        public const int SPRITE_WIDTH = 8;
        public const int SPRITE_HEIGHT = 8;
        public const int BUTTON_COUNT = 6;
        public const int BUTTON_INTERNAL_COUNT = 13;
        public const int PLAYER_COUNT = 2;

        public const int DEFAULT_AUTO_REPEAT_DELAY = 15;
        public const int DEFAULT_AUTO_REPEAT_INTERVAL = 4;

        public const int STAT_MEM_USAGE = 0;
        public const int STAT_CPU_USAGE = 1;
        public const int STAT_SYSTEM_CPU_USAGE = 2;
        public const int STAT_PARAM = 6;
        public const int STAT_FRAMERATE = 7;
        public const int STAT_TARGET_FRAMERATE = 8;
        public const int STAT_KEY_PRESSED = 30;
        public const int STAT_KEY_NAME = 31;
        public const int STAT_MOUSE_X = 32;
        public const int STAT_MOUSE_Y = 33;
        public const int STAT_MOUSE_BUTTONS = 34;
        public const int STAT_MOUSE_WHEEL = 36;
        public const int STAT_YEAR_UTC = 80;
        public const int STAT_MONTH_UTC = 81;
        public const int STAT_DAY_UTC = 82;
        public const int STAT_HOUR_UTC = 83;
        public const int STAT_MINUTE_UTC = 84;
        public const int STAT_SECOND_UTC = 85;
        public const int STAT_YEAR = 90;
        public const int STAT_MONTH = 91;
        public const int STAT_DAY = 92;
        public const int STAT_HOUR = 93;
        public const int STAT_MINUTE = 94;
        public const int STAT_SECOND = 95;

        public const int SAMPLE_RATE = 44100;
        public const int MAX_VOLUME = 4096;
        public const int CHANNEL_COUNT = 4;
        public const int SOUND_BUFFER_SIZE = 2048;
        public const int SOUND_COUNT = 64;
        public const int MUSIC_COUNT = 64;
        public const int TICKS_PER_SECOND = 128;

        // ARGB
        private uint[] m_colors = new uint[]
        {
            0x00000000, 0x001d2b53, 0x007e2553, 0x00008751, 0x00ab5236, 0x005f574f, 0x00c2c3c7, 0x00fff1e8,
            0x00ff004d, 0x00ffa300, 0x00ffec27, 0x0000e436, 0x0029adff, 0x0083769c, 0x00ff77a8, 0x00ffccaa
        };

        private struct P8Symbol
        {
            public byte[] encoding; // up to 7 bytes
            public byte length;     // 1..7
            public byte index;      // glyph index
        }

        private static readonly P8Symbol[] m_p8Symbols = new[]
        {
            new P8Symbol { encoding = new byte[]{0x5c,0x30}, length=2, index=0   },
            new P8Symbol { encoding = new byte[]{0x5c,0x2a}, length=2, index=1   },
            new P8Symbol { encoding = new byte[]{0x5c,0x23}, length=2, index=2   },
            new P8Symbol { encoding = new byte[]{0x5c,0x2d}, length=2, index=3   },
            new P8Symbol { encoding = new byte[]{0x5c,0x7c}, length=2, index=4   },
            new P8Symbol { encoding = new byte[]{0x5c,0x2b}, length=2, index=5   },
            new P8Symbol { encoding = new byte[]{0x5c,0x5e}, length=2, index=6   },
            new P8Symbol { encoding = new byte[]{0x5c,0x61}, length=2, index=7   },
            new P8Symbol { encoding = new byte[]{0x5c,0x62}, length=2, index=8   },
            new P8Symbol { encoding = new byte[]{0x5c,0x74}, length=2, index=9   },
            new P8Symbol { encoding = new byte[]{0x5c,0x6e}, length=2, index=10  },
            new P8Symbol { encoding = new byte[]{0x5c,0x76}, length=2, index=11  },
            new P8Symbol { encoding = new byte[]{0x5c,0x66}, length=2, index=12  },
            new P8Symbol { encoding = new byte[]{0x5c,0x72}, length=2, index=13  },
            new P8Symbol { encoding = new byte[]{0x5c,0x30,0x31,0x34}, length=4, index=14 },
            new P8Symbol { encoding = new byte[]{0x5c,0x30,0x31,0x35}, length=4, index=15 },
            new P8Symbol { encoding = new byte[]{0xe2,0x96,0xae}, length=3, index=16  },
            new P8Symbol { encoding = new byte[]{0xe2,0x96,0xa0}, length=3, index=17  },
            new P8Symbol { encoding = new byte[]{0xe2,0x96,0xa1}, length=3, index=18  },
            new P8Symbol { encoding = new byte[]{0xe2,0x81,0x99}, length=3, index=19  },
            new P8Symbol { encoding = new byte[]{0xe2,0x81,0x98}, length=3, index=20  },
            new P8Symbol { encoding = new byte[]{0xe2,0x80,0x96}, length=3, index=21  },
            new P8Symbol { encoding = new byte[]{0xe2,0x97,0x80}, length=3, index=22  },
            new P8Symbol { encoding = new byte[]{0xe2,0x96,0xb6}, length=3, index=23  },
            new P8Symbol { encoding = new byte[]{0xe3,0x80,0x8c}, length=3, index=24  },
            new P8Symbol { encoding = new byte[]{0xe3,0x80,0x8d}, length=3, index=25  },
            new P8Symbol { encoding = new byte[]{0xc2,0xa5},       length=2, index=26  },
            new P8Symbol { encoding = new byte[]{0xe2,0x80,0xa2}, length=3, index=27  },
            new P8Symbol { encoding = new byte[]{0xe3,0x80,0x81}, length=3, index=28  },
            new P8Symbol { encoding = new byte[]{0xe3,0x80,0x82}, length=3, index=29  },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x9b}, length=3, index=30  },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x9c}, length=3, index=31  },
            new P8Symbol { encoding = new byte[]{0xe2,0x97,0x8b}, length=3, index=127 },
            new P8Symbol { encoding = new byte[]{0xe2,0x96,0x88}, length=3, index=128 },
            new P8Symbol { encoding = new byte[]{0xe2,0x96,0x92}, length=3, index=129 },
            new P8Symbol { encoding = new byte[]{0xf0,0x9f,0x90,0xb1}, length=4, index=130 },
            new P8Symbol { encoding = new byte[]{0xe2,0xac,0x87,0xef,0xb8,0x8f}, length=6, index=131 },
            new P8Symbol { encoding = new byte[]{0xe2,0x96,0x91}, length=3, index=132 },
            new P8Symbol { encoding = new byte[]{0xe2,0x9c,0xbd}, length=3, index=133 },
            new P8Symbol { encoding = new byte[]{0xe2,0x97,0x8f}, length=3, index=134 },
            new P8Symbol { encoding = new byte[]{0xe2,0x99,0xa5}, length=3, index=135 },
            new P8Symbol { encoding = new byte[]{0xe2,0x98,0x89}, length=3, index=136 },
            new P8Symbol { encoding = new byte[]{0xec,0x9b,0x83}, length=3, index=137 },
            new P8Symbol { encoding = new byte[]{0xe2,0x8c,0x82}, length=3, index=138 },
            new P8Symbol { encoding = new byte[]{0xe2,0xac,0x85,0xef,0xb8,0x8f}, length=6, index=139 },
            new P8Symbol { encoding = new byte[]{0xf0,0x9f,0x98,0x90}, length=4, index=140 },
            new P8Symbol { encoding = new byte[]{0xe2,0x99,0xaa}, length=3, index=141 },
            new P8Symbol { encoding = new byte[]{0xf0,0x9f,0x85,0xbe,0xef,0xb8,0x8f}, length=7, index=142 },
            new P8Symbol { encoding = new byte[]{0xe2,0x97,0x86}, length=3, index=143 },
            new P8Symbol { encoding = new byte[]{0xe2,0x80,0xa6}, length=3, index=144 },
            new P8Symbol { encoding = new byte[]{0xe2,0x9e,0xa1,0xef,0xb8,0x8f}, length=6, index=145 },
            new P8Symbol { encoding = new byte[]{0xe2,0x98,0x85}, length=3, index=146 },
            new P8Symbol { encoding = new byte[]{0xe2,0xa7,0x97}, length=3, index=147 },
            new P8Symbol { encoding = new byte[]{0xe2,0xac,0x86,0xef,0xb8,0x8f}, length=6, index=148 },
            new P8Symbol { encoding = new byte[]{0xcb,0x87},       length=2, index=149 },
            new P8Symbol { encoding = new byte[]{0xe2,0x88,0xa7}, length=3, index=150 },
            new P8Symbol { encoding = new byte[]{0xe2,0x9d,0x8e}, length=3, index=151 },
            new P8Symbol { encoding = new byte[]{0xe2,0x96,0xa4}, length=3, index=152 },
            new P8Symbol { encoding = new byte[]{0xe2,0x96,0xa5}, length=3, index=153 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x82}, length=3, index=154 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x84}, length=3, index=155 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x86}, length=3, index=156 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x88}, length=3, index=157 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x8a}, length=3, index=158 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x8b}, length=3, index=159 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x8d}, length=3, index=160 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x8f}, length=3, index=161 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x91}, length=3, index=162 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x93}, length=3, index=163 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x95}, length=3, index=164 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x97}, length=3, index=165 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x99}, length=3, index=166 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x9b}, length=3, index=167 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x9d}, length=3, index=168 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0x9f}, length=3, index=169 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xa1}, length=3, index=170 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xa4}, length=3, index=171 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xa6}, length=3, index=172 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xa8}, length=3, index=173 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xaa}, length=3, index=174 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xab}, length=3, index=175 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xac}, length=3, index=176 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xad}, length=3, index=177 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xae}, length=3, index=178 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xaf}, length=3, index=179 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xb2}, length=3, index=180 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xb5}, length=3, index=181 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xb8}, length=3, index=182 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xbb}, length=3, index=183 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xbe}, length=3, index=184 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xbf}, length=3, index=185 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x80}, length=3, index=186 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x81}, length=3, index=187 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x82}, length=3, index=188 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x84}, length=3, index=189 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x86}, length=3, index=190 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x88}, length=3, index=191 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x89}, length=3, index=192 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x8a}, length=3, index=193 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x8b}, length=3, index=194 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x8c}, length=3, index=195 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x8d}, length=3, index=196 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x8f}, length=3, index=197 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x92}, length=3, index=198 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x93}, length=3, index=199 },
            new P8Symbol { encoding = new byte[]{0xe3,0x81,0xa3}, length=3, index=200 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x83}, length=3, index=201 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x85}, length=3, index=202 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0x87}, length=3, index=203 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xa2}, length=3, index=204 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xa4}, length=3, index=205 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xa6}, length=3, index=206 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xa8}, length=3, index=207 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xaa}, length=3, index=208 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xab}, length=3, index=209 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xad}, length=3, index=210 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xaf}, length=3, index=211 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xb1}, length=3, index=212 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xb3}, length=3, index=213 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xb5}, length=3, index=214 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xb7}, length=3, index=215 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xb9}, length=3, index=216 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xbb}, length=3, index=217 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xbd}, length=3, index=218 },
            new P8Symbol { encoding = new byte[]{0xe3,0x82,0xbf}, length=3, index=219 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x81}, length=3, index=220 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x84}, length=3, index=221 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x86}, length=3, index=222 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x88}, length=3, index=223 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x8a}, length=3, index=224 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x8b}, length=3, index=225 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x8c}, length=3, index=226 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x8d}, length=3, index=227 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x8e}, length=3, index=228 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x8f}, length=3, index=229 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x92}, length=3, index=230 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x95}, length=3, index=231 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x98}, length=3, index=232 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x9b}, length=3, index=233 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x9e}, length=3, index=234 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x9f}, length=3, index=235 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xa0}, length=3, index=236 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xa1}, length=3, index=237 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xa2}, length=3, index=238 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xa4}, length=3, index=239 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xa6}, length=3, index=240 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xa8}, length=3, index=241 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xa9}, length=3, index=242 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xaa}, length=3, index=243 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xab}, length=3, index=244 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xac}, length=3, index=245 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xad}, length=3, index=246 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xaf}, length=3, index=247 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xb2}, length=3, index=248 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xb3}, length=3, index=249 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0x83}, length=3, index=250 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xa3}, length=3, index=251 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xa5}, length=3, index=252 },
            new P8Symbol { encoding = new byte[]{0xe3,0x83,0xa7}, length=3, index=253 },
            new P8Symbol { encoding = new byte[]{0xe2,0x97,0x9c}, length=3, index=254 },
            new P8Symbol { encoding = new byte[]{0xe2,0x97,0x9d}, length=3, index=255 },
        };

        public enum PaletteType
        {
            Draw,
            Screen,
            Secondary
        }

        public enum DrawType
        {
            Default,
            Graphic,
            Sprite
        }

        public enum ButtonMask
        {
            Left = 0x0001,
            Right = 0x0002,
            Up = 0x0004,
            Down = 0x0008,
            Action1 = 0x0010,
            Action2 = 0x0020,
        };

        private enum Tone
        {
            C, CS, D, DS, E, F, FS, G, GS, A, AS, B
        }

        private enum Waveform
        {
            Triangle, TiltedSaw, Saw, Square, Pulse, Organ, Noise, Phaser
        }

        private enum Effect
        {
            None, Slide, Vibrato, Drop, FadeIn, FadeOut, ArpeggioFast, ArpeggioSlow
        }

        private string[] m_toneMap =
        {
            "C ", "C#", "D ", "D#", "E ", "F ", "F#", "G ", "G#", "A ", "A#", "B "
        };

        private float[] m_toneFrequencies =
        {
            130.81f, 138.59f, 146.83f, 155.56f, 164.81f, 174.61f, 185.00f, 196.00f, 207.65f, 220.0f, 233.08f, 246.94f
        };

        private string[] m_functionName =
        {
            // ****************************************************************
            // *** Graphics ***
            // ****************************************************************
            "camera",
            "circ",
            "circfill",
            "clip",
            "cls",
            "color",
            "cursor",
            "fget",
            "fillp",
            "flip",
            "fset",
            "line",
            "oval",
            "ovalfill",
            "pal",
            "palt",
            "pget",
            "print",
            "pset",
            "rect",
            "rectfill",
            "rrect",
            "rrectfill",
            "sget",
            "spr",
            "sset",
            "sspr",
            "tline",
            // ****************************************************************
            // *** Tables ***
            // ****************************************************************
            //"add",
            //"all",
            //"count",
            //"del",
            //"_foreach",
            // "pairs",
            // ****************************************************************
            // *** Input ***
            // ****************************************************************
            "btn",
            "btnp",
            // ****************************************************************
            // *** Sound ***
            // ****************************************************************
            "music",
            "sfx",
            // ****************************************************************
            // *** Map ***
            // ****************************************************************
            "map",
            "mget",
            "mset",
            // ****************************************************************
            // *** Memory ***
            // ****************************************************************
            // "cstore",
            "memcpy",
            "memset",
            "peek",
            "peek2",
            "peek4",
            "poke",
            "poke2",
            "poke4",
            "reload",
            // ****************************************************************
            // *** Math ***
            // ****************************************************************
            "abs",
            "atan2",
            "band",
            "bnot",
            "bor",
            "bxor",
            "ceil",
            "cos",
            "flr",
            "lshr",
            "max",
            "mid",
            "min",
            "rnd",
            "rotl",
            "rotr",
            "sgn",
            "shl",
            "shr",
            "sin",
            "sqrt",
            "srand",
            // ****************************************************************
            // *** Cartridge data ***
            // ****************************************************************
            "cartdata",
            "dget",
            "dset",
            // ****************************************************************
            // *** Coroutines ***
            // ****************************************************************
            // "cocreate",
            // "coresume",
            // "costatus",
            // "yield",
            // ****************************************************************
            // *** Values and objects ***
            // ****************************************************************
            "chr",
            // "setmetatable",
            // "getmetatable",
            // "type",
            "sub",
            // "tonum",
            "tostr",
            // ****************************************************************
            // *** Time ***
            // ****************************************************************
            "time",
            // ****************************************************************
            // *** System ***
            // ****************************************************************
            "menuitem",
            "extcmd",
            // ****************************************************************
            // *** Debugging ***
            // ****************************************************************
            // "assert",
            "printh",
            "stat",
            // "stop",
            // "trace,
            // ****************************************************************
            // *** Misc ***
            // ****************************************************************
        };

        private string m_fontMap = "▮■□⁙⁘‖◀▶「」¥•、。゛゜ !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~○█▒🐱⬇️░✽●♥☉웃⌂⬅️😐♪🅾️◆…➡️★⧗⬆️ˇ∧❎▤▥あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをんっゃゅょアイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲンッャュョ◜◝";
        public byte[] m_memory;
        private byte[] m_cart_memory;
        private byte[] m_font;

        private double m_fps = 30.0;
        private Point m_mouse = Point.Empty;

        private Lua m_lua = null;
        private LuaBridge m_luaBridge = null;

        private Random m_random = null;

        private double m_accumulator = 0.0;
        private double m_time = 0.0;
        private uint m_frames = 0;
        private uint m_actual_fps = 0;

        private int m_tline_precision = 13;

        private bool m_musicEnabled = true;
        private bool m_soundEnabled = true;
        private short[] m_soundBuffer = null;
        private Queue<SoundCommand> m_soundQueue = null;

        private SoundState[] m_channels = null;
        private MusicState m_musicState = null;

        private CartData m_cartData = null;

        private int[] m_pixelArray = null;

        private const double m_twoPI = 2.0 * Math.PI;

        private byte[] m_buttons = new byte[2];
        private byte[] m_buttonsp = new byte[2];
        private byte[] m_buttonsPrev = new byte[2];
        private byte[] m_button_first_repeat = new byte[2];
        private uint[,] m_button_down_time = new uint[2, 6];

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int nVirtKey);

        public event EventHandler<AudioEventArgs> Audio;

        public Emulator(string fileName)
        {
            m_random = new Random();
            m_pixelArray = new int[P8_WIDTH * P8_HEIGHT];
            m_memory = new byte[MEMORY_SIZE];
            m_channels = new SoundState[CHANNEL_COUNT];
            for (int i = 0; i < CHANNEL_COUNT; i++)
                m_channels[i] = new SoundState();
            m_musicState = new MusicState(CHANNEL_COUNT);
            m_soundBuffer = new short[SOUND_BUFFER_SIZE];
            m_soundQueue = new Queue<SoundCommand>();
            m_luaBridge = new LuaBridge(this);
            m_cartData = new CartData(m_memory);

            string fontString = Tools.GetEmbeddedResourceAsString("P8Font.txt");

            m_font = P8Parser.HexStringToByteArray(fontString);

            //for (int i = 0; i < m_font.Length; i++)
            //    m_font[i] = (byte)((m_font[i] << 4) | (m_font[i] >> 4));

            string luaString = null;

            P8Parser.Parse(fileName, m_memory, out luaString);

            // Save a copy of cart memory for reload()
            m_cart_memory = new byte[CART_MEMORY_SIZE];
            Buffer.BlockCopy(m_memory, 0, m_cart_memory, 0, CART_MEMORY_SIZE);

            // Initialize physical address registers so addr_remap/gfx_addr_remap
            // map to the correct default locations (sprites=0x00, screen=0x60)
            m_memory[MEMORY_SPRITE_PHYS] = 0x00;
            m_memory[MEMORY_SCREEN_PHYS] = 0x60;
            m_memory[MEMORY_MAP_START] = 0x20;

            reset_color();
            clear_screen(0);
            srand(m_random.Next());

            if (luaString != null)
            {
                string luaApiString = Tools.GetEmbeddedResourceAsString("LuaApi.txt");
                string luaCleanString = P8Parser.CleanLua(luaApiString + luaString, true);

                // Convert remaining UTF-8 emoji sequences (in strings) to single P8SCII bytes,
                // exactly like femto8's convert_utf8_to_p8scii.
                var p8symbols = new P8Parser.P8Symbol[m_p8Symbols.Length];
                for (int i = 0; i < m_p8Symbols.Length; i++)
                    p8symbols[i] = new P8Parser.P8Symbol { encoding = m_p8Symbols[i].encoding, length = m_p8Symbols[i].length, index = m_p8Symbols[i].index };
                byte[] luaBytes = P8Parser.ConvertUtf8ToP8SCII(luaCleanString, p8symbols);

                m_lua = new Lua();
                // Use Latin-1 so bytes 0x80-0xFF survive the C# ↔ Lua round trip.
                // KeraLua defaults to ASCII which replaces anything >0x7F with '?'.
                m_lua.State.Encoding = Encoding.GetEncoding(28591); // ISO-8859-1
                m_lua.DebugHook += OnDebugHook;
                m_lua.HookException += OnHookException;

                foreach (string name in m_functionName)
                    RegisterFunction(name);

                // Register P8SCII button glyphs as Lua globals (matches femto8)
                // so that e.g. btnp(⬅️) → btnp(\x8b) → btnp(0)
                m_lua.DoString("_ENV[string.char(0x8b)]=0");  // ⬅️ left
                m_lua.DoString("_ENV[string.char(0x91)]=1");  // ➡️ right
                m_lua.DoString("_ENV[string.char(0x94)]=2");  // ⬆️ up
                m_lua.DoString("_ENV[string.char(0x83)]=3");  // ⬇️ down
                m_lua.DoString("_ENV[string.char(0x8e)]=4");  // 🅾️ o
                m_lua.DoString("_ENV[string.char(0x97)]=5");  // ❎ x

                System.Threading.ThreadPool.QueueUserWorkItem(delegate
                {
                    // Load the P8SCII byte array directly via KeraLua's LoadBuffer
                    // to bypass NLua's string encoding for the source code.
                    var state = m_lua.State;
                    var loadStatus = state.LoadBuffer(luaBytes, "cart");
                    if (loadStatus == KeraLua.LuaStatus.OK)
                    {
                        int pcallResult = (int)state.PCall(0, -1, 0);
                        if (pcallResult != 0)
                        {
                            string err = state.ToString(-1);
                            Debug.WriteLine("Lua pcall error: " + err);
                            state.Pop(1);
                        }
                    }
                    else
                    {
                        string err = state.ToString(-1);
                        Debug.WriteLine("Lua load error: " + err);
                        state.Pop(1);
                    }

                    var initFunction = m_lua["_init"] as LuaFunction;

                    if (initFunction != null)
                        initFunction.Call();
                }, null);
            }
        }

        private void OnDebugHook(object sender, NLua.Event.DebugHookEventArgs e)
        {
            Debug.WriteLine(e.LuaDebug.ToString());
        }

        private void OnHookException(object sender, NLua.Event.HookExceptionEventArgs e)
        {
            Debug.WriteLine(e.Exception.ToString());
        }

        private void RegisterFunction(string name)
        {
            Type type = m_luaBridge.GetType();
            MethodInfo methodInfo = type.GetMethod(name);
            m_lua.RegisterFunction(name, m_luaBridge, methodInfo);
        }

        private int addr_remap(int address)
        {
            if (address >= 0x0000 && address < 0x2000)
                address = (m_memory[MEMORY_SPRITE_PHYS] << 8) | (address & 0x1fff);
            else if (address >= 0x6000 && address < 0x8000)
                address = (m_memory[MEMORY_SCREEN_PHYS] << 8) | (address & 0x1fff);
            return address;
        }

        private float width = 0;

        public void Update(double elapsedTime)
        {
            m_accumulator += elapsedTime;
            m_time += elapsedTime;

            if (m_accumulator < 1.0 / m_fps)
                return;

            //RenderSounds();
            UpdateInput();

            m_cartData.Flush();

            m_accumulator = 0.0;
            m_frames++;

            if (m_lua != null)
            {
                var updateFunction60 = m_lua["_update60"] as LuaFunction;
                var updateFunction = m_lua["_update"] as LuaFunction;
                var drawFunction = m_lua["_draw"] as LuaFunction;

                if (updateFunction60 != null)
                    updateFunction60.Call();
                else if (updateFunction != null)
                    updateFunction.Call();

                if (drawFunction != null)
                    drawFunction.Call();
            }

            //rectfill((int)Math.Round(width), (int)Math.Round(width), (int)Math.Round(128 - width), (int)Math.Round(128 - width), 1);
            //rect((int)Math.Round(width), (int)Math.Round(width), (int)Math.Round(128 - width), (int)Math.Round(128 - width), 2);
            //circfill(64, 64, (int)Math.Round(64 - width), 3);
            //circ(64, 64, (int)Math.Round(64 - width), 4);
            //line(0, 0, (int)Math.Round(128 - width), 128, 5);
            //print(String.Format("{0:f2}", m_time), 0, 0, 7);

            if (++width == 128)
                width = 0;

            flip();
        }

        // ****************************************************************
        // *** Graphics ***
        // ****************************************************************

        // camera([x,] [y])
        public int camera(int x = 0, int y = 0)
        {
            camera_set(x, y);

            return 0;
        }

        // circ(x, y, [r,] [col])
        public int circ(int x, int y, int r = 4, int color = -1)
        {
            int col = color == -1 ? (pencolor_get() & 0xF) : color;
            int fillp = col == -1 ? 0 : col & 0xFFFF;

            draw_circ(x, y, r, col, fillp);

            return 0;
        }

        // circfill(x, y, [r,] [col])
        public int circfill(int x, int y, int r = 4, int col = -1)
        {
            int color = col == -1 ? (pencolor_get() & 0xF) : col;
            int fillp = col == -1 ? 0 : col & 0xFFFF;

            draw_circfill(x, y, r, color, fillp);

            return 0;
        }

        // clip(x, y, w, h)

        // cls([color])
        public int cls(int color = 0)
        {
            clear_screen(color);

            return 0;
        }

        // color(col)
        public int color(int col = 6)
        {
            pencolor_set((byte)col);

            return 0;
        }

        // cursor([x,] [y,] [col])
        public void cursor(int x = 0, int y = 0, int col = -1)
        {
            cursor_set(x, y, (col == -1) ? -1 : ((pencolor_get() & 0xf0) | col));
            left_margin_set(x);
        }

        // fget(n, [f])
        public object fget(int n, int f = -1)
        {
            byte flags = m_memory[MEMORY_SPRITEFLAGS + n];

            if (f == -1)
                return flags;

            return ((flags & (1 << f)) != 0);
        }

        // fillp([pat])
        public int fillp(int pat = -1)
        {
            if (pat == -1)
            {
                m_memory[MEMORY_FILLP] = 0;
                m_memory[MEMORY_FILLP + 1] = 0;
                m_memory[MEMORY_FILLP_ATTR] = 0;
            }
            else
            {
                m_memory[MEMORY_FILLP] = (byte)((pat >> 16) & 0xff);
                m_memory[MEMORY_FILLP + 1] = (byte)((pat >> 24) & 0xff);
                m_memory[MEMORY_FILLP_ATTR] = (byte)(((pat & 0x8000) != 0 ? 1 : 0) | ((pat & 0x4000) != 0 ? 2 : 0) | ((pat & 0x2000) != 0 ? 4 : 0));

            }

            return 0;
        }

        // flip()
        public void flip()
        {
            for (int y = 0; y < P8_HEIGHT; y++)
            {
                for (int x = 0; x < P8_WIDTH; x++)
                {
                    int screenOffset = MEMORY_SCREEN + (x >> 1) + y * 64;
                    byte value = m_memory[screenOffset];
                    byte index = color_get(PaletteType.Screen, ((x + 1) % 2 == 0 ? value >> 4 : value & 0xF));
                    uint color = m_colors[index & 0xF];

                    m_pixelArray[x + (y * 128)] = (int)color;
                }
            }
        }

        // fset(n, [f,] v)
        public void fset(int n, int f, object v = null)
        {
            if (v == null)
            {
                m_memory[MEMORY_SPRITEFLAGS + n] = (byte)f;

                return;
            }

            if ((bool)v)
                m_memory[MEMORY_SPRITEFLAGS + n] |= (byte)(1 << f);
            else
                m_memory[MEMORY_SPRITEFLAGS + n] &= (byte)~(1 << f);
        }

        // line([x0,] [y0,] x1, y1, [col])
        public void line()
        {
            m_memory[MEMORY_LINE_VALID] = 1;
        }

        public void line(int x1, int y1, int col = -1)
        {
            int x0 = m_memory[MEMORY_LINE_X] | (m_memory[MEMORY_LINE_X + 1] << 8);
            int y0 = m_memory[MEMORY_LINE_Y] | (m_memory[MEMORY_LINE_Y + 1] << 8);

            bool valid = m_memory[MEMORY_LINE_VALID] == 0;
            int color = (col == -1) ? (pencolor_get() & 0xF) : col;
            const int fillp = 0;

            if (valid)
                draw_line(x0, y0, x1, y1, color, fillp);

            m_memory[MEMORY_LINE_X] = (byte)(x1 & 0xff);
            m_memory[MEMORY_LINE_X + 1] = (byte)((x1 >> 8) & 0xff);
            m_memory[MEMORY_LINE_Y] = (byte)(y1 & 0xff);
            m_memory[MEMORY_LINE_Y + 1] = (byte)((y1 >> 8) & 0xff);
            m_memory[MEMORY_LINE_VALID] = 0;
        }

        public void line(int x0, int y0, int x1, int y1, int col = -1)
        {
            bool valid = true;
            int color = (col == -1) ? (pencolor_get() & 0xF) : col;
            const int fillp = 0;

            if (valid)
                draw_line(x0, y0, x1, y1, color, fillp);

            m_memory[MEMORY_LINE_X] = (byte)(x1 & 0xff);
            m_memory[MEMORY_LINE_X + 1] = (byte)((x1 >> 8) & 0xff);
            m_memory[MEMORY_LINE_Y] = (byte)(y1 & 0xff);
            m_memory[MEMORY_LINE_Y + 1] = (byte)((y1 >> 8) & 0xff);
            m_memory[MEMORY_LINE_VALID] = 0;
        }


        // oval(x0, y0, x1, y1, [col])
        public int oval(int x0, int y0, int x1, int y1, int col = -1)
        {
            int color = col != -1 ? col : pencolor_get() & 0xF;
            int fillp = col != -1 ? col & 0xffff : 0;

            draw_oval(x0, y0, x1, y1, col, fillp);

            return 0;
        }

        // ovalfill(x0, y0, x1, y1, [col])
        public int ovalfill(int x0, int y0, int x1, int y1, int col = -1)
        {
            int color = col != -1 ? col : pencolor_get() & 0xF;
            int fillp = col != -1 ? col & 0xffff : 0;

            draw_ovalfill(x0, y0, x1, y1, col, fillp);

            return 0;
        }

        // pal(c0, c1, [p])
        public void pal(int c0 = -1, int c1 = -1, int p = -1)
        {
            if (c0 == -1 && c1 == -1 && p == -1)
            {
                reset_color();
                return;
            }

            if (c0 != -1 && c1 != -1)
            {
                if (p == -1)
                    p = 0;

                byte old_val = color_get((PaletteType)p, c0);
                byte new_val = (byte)((c1 & 0x8f) | (old_val & 0x10));
                color_set((PaletteType)p, c0, new_val);
                return;
            }
        }

        // palt([col,] [t])
        public void palt(int col, bool? t = null)
        {
            if (col == -1 && t == null)
            {
                reset_color();
                return;
            }

            if (t == null)
            {
                int mask = col;
                for (int i = 0; i < 16; i++)
                {
                    int bit = (mask >> (15 - i)) & 1;
                    byte c = color_get(PaletteType.Draw, i);
                    color_set(PaletteType.Draw, i, bit != 0 ? (byte)(c | 0x10) : (byte)(c & 0x0F));
                }
                return;
            }

            if (col < 0 || col > 15)
                return;
            
            byte cur = color_get(PaletteType.Draw, col);

            color_set(PaletteType.Draw, col, t.Value ? (byte)(cur | 0x10) : (byte)(cur & 0x0F));
        }

        // pget(x, y)
        public byte pget(int x, int y)
        {
            int x0, y0, x1, y1;
            clip_get(out x0, out y0, out x1, out y1);

            if (x >= x0 && y >= y0 && x < x1 && y < y1)
                return gfx_get(x, y, MEMORY_SCREEN, MEMORY_SCREEN_SIZE);

            int default_val = (m_memory[MEMORY_MISCFLAGS] & 0x10) != 0 ? m_memory[MEMORY_PGET_DEFAULT] : 0;
            return (byte)default_val;
        }

        // print(str, [x,] [y,] [col])

        // pset(x, y, [c])
        public void pset(int x, int y, int c = -1)
        {
            int cameraX, cameraY;
            camera_get(out cameraX, out cameraY);
            x -= cameraX;
            y -= cameraY;
            int x0 = m_memory[MEMORY_CLIPRECT];
            int y0 = m_memory[MEMORY_CLIPRECT + 1];
            int x1 = m_memory[MEMORY_CLIPRECT + 2];
            int y1 = m_memory[MEMORY_CLIPRECT + 3];

            if (x >= x0 && x < x1 && y >= y0 && y < y1)
            {
                c = color_get(PaletteType.Draw, c);
                gfx_set(x, y, MEMORY_SCREEN, MEMORY_SCREEN_SIZE, c);
            }
        }

        // rect(x0, y0, x1, y1, [col])
        public void rect(int x0, int y0, int x1, int y1, int col = -1)
        {
            int color = col != -1 ? col : pencolor_get() & 0xF;
            int fillp = col != -1 ? col & 0xffff : 0;

            int left = Math.Min(x0, x1);
            int top = Math.Min(y0, y1);
            int right = Math.Max(x0, x1);
            int bottom = Math.Max(y0, y1);

            draw_rect(left, top, right, bottom, color, fillp);
        }

        // rectfill(x0, y0, x1, y1, [col])
        public void rectfill(int x0, int y0, int x1, int y1, int col = -1)
        {
            int color = col != -1 ? col : pencolor_get() & 0xF;
            int fillp = col != -1 ? col & 0xffff : 0;

            int left = Math.Min(x0, x1);
            int top = Math.Min(y0, y1);
            int right = Math.Max(x0, x1);
            int bottom = Math.Max(y0, y1);

            draw_rectfill(left, top, right, bottom, color, fillp);
        }

        // rrect(x, y, w, h, r, [col])
        public int rrect(int x, int y, int w, int h, int r, int col = -1)
        {
            int color = col != -1 ? col : pencolor_get() & 0xF;
            int fillp = col != -1 ? col & 0xffff : 0;

            int left = x;
            int top = y;

            int right = left + w - 1;
            int bottom = top + h - 1;
            r = Math.Max(0, Math.Min(r, Math.Min(w, h) / 2));

            draw_hline(left + r, top, right - r, col, fillp);
            draw_hline(left + r, bottom, right - r, col, fillp);
            draw_vline(left, top + r, bottom - r, col, fillp);
            draw_vline(right, top + r, bottom - r, col, fillp);
            draw_circ_mask(left + r, top + r, r, col, fillp, 1 << 3 | 1 << 7);
            draw_circ_mask(right - r, top + r, r, col, fillp, 1 << 2 | 1 << 6);
            draw_circ_mask(left + r, bottom - r, r, col, fillp, 1 << 1 | 1 << 5);
            draw_circ_mask(right - r, bottom - r, r, col, fillp, 1 << 0 | 1 << 4);

            return 0;
        }

        // rrectfill(x, y, w, h, r, [col])
        public int rrectfill(int x, int y, int w, int h, int r, int col = -1)
        {
            int color = col != -1 ? col : pencolor_get() & 0xF;
            int fillp = col != -1 ? col & 0xffff : 0;

            int left = x;
            int top = y;

            int right = left + w;
            int bottom = top + h;
            r = Math.Max(0, Math.Min(r, Math.Min(w, h) / 2));

            draw_rectfill(left, top + r, right, bottom - r, col, fillp);
            draw_rectfill(left + r, top, right - r, top + r, col, fillp);
            draw_rectfill(left + r, bottom - r, right - r, bottom, col, fillp);
            draw_circfill_mask(left + r, top + r, r, col, fillp, 1 << 3 | 1 << 7);
            draw_circfill_mask(right - r, top + r, r, col, fillp, 1 << 2 | 1 << 6);
            draw_circfill_mask(left + r, bottom - r, r, col, fillp, 1 << 1 | 1 << 5);
            draw_circfill_mask(right - r, bottom - r, r, col, fillp, 1 << 0 | 1 << 4);

            return 0;
        }

        // sget(x, y)
        public byte sget(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < P8_WIDTH && y < P8_HEIGHT)
                return gfx_get(x, y, MEMORY_SPRITES, MEMORY_SPRITES_SIZE);

            int default_val = (m_memory[MEMORY_MISCFLAGS] & 0x10) != 0 ? m_memory[MEMORY_SGET_DEFAULT] : 0;
            return (byte)default_val;
        }

        static float val = 0.0f;

        // spr(n, x, y, [w,] [h,] [flip_x,] [flip_y])

        // sset(x, y, [c])
        public int sset(int x, int y, int c = -1)
        {
            int col = c != -1 ? c : pencolor_get() & 0xF;

            if (x >= 0 && y >= 0 && x < P8_WIDTH && y < P8_HEIGHT)
                gfx_set(x, y, MEMORY_SPRITES, MEMORY_SPRITES_SIZE, col);

            return 0;
        }

        // sspr(sx, sy, sw, sh, dx, dy, [dw,] [dh,] [flip_x,] [flip_y])

        // tline( x0, y0, x1, y1, mx, my, [mdx,] [mdy])
        public int tline(int x0, int y0, int x1, int y1, int mx, int my, int mdx = -1, int mdy = -1)
        {
            // Defaults: mdx = 1/8, mdy = 0 in fixed-point
            if (mdx == -1)
            {
                // (1 << m_tline_precision) / 8 == 1 << (m_tline_precision - 3)
                mdx = 1 << (m_tline_precision - 3);
            }

            if (mdy == -1)
            {
                mdy = 0;
            }

            // Bresenham setup
            int dx = Math.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            // Fixed-point masks & offsets (match original)
            int maskXFixed = ((int)m_memory[MEMORY_TLINE_MASK_X] << m_tline_precision) - 1;
            int maskYFixed = ((int)m_memory[MEMORY_TLINE_MASK_Y] << m_tline_precision) - 1;
            int offX = m_memory[MEMORY_TLINE_OFFSET_X] * 8;
            int offY = m_memory[MEMORY_TLINE_OFFSET_Y] * 8;

            const int layer = 0; // original default

            while (true)
            {
                // Wrap texture coordinates with masks (fixed-point)
                int mxWrapped = mx & maskXFixed;
                int myWrapped = my & maskYFixed;

                int tx = (mxWrapped >> m_tline_precision) + offX;
                int ty = (myWrapped >> m_tline_precision) + offY;

                int index = map_get(tx / 8, ty / 8);
                byte sprite_flags = m_memory[MEMORY_SPRITEFLAGS + index];

                if (index != 0 && (layer == 0 || ((layer & sprite_flags) == layer)))
                {
                    int sxTile = (index & 0xF) * SPRITE_WIDTH;
                    int syTile = (index >> 4) * SPRITE_HEIGHT;

                    int col = gfx_get(sxTile + (tx % 8), syTile + (ty % 8), MEMORY_SPRITES, MEMORY_SPRITES_SIZE);
                    pixel_set(x0, y0, col, 0, DrawType.Default);
                }

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }

                mx += mdx;
                my += mdy;
            }

            return 0;
        }

        // ****************************************************************
        // *** Tables ***
        // ****************************************************************

        // add(tbl, v)
        public object add(LuaTable tbl, object v)
        {
            if (tbl == null)
                return v;

            tbl[tbl.Keys.Count + 1] = v;

            return v;
        }

        // all(tbl)
        public object all(LuaTable tbl)
        {
            return null;
        }

        // count(tbl)
        public int count(LuaTable tbl)
        {
            if (tbl == null)
                return 0;

            return tbl.Keys.Count;
        }

        // del(tbl, v)
        public object del(LuaTable tbl, object dv)
        {
            if (tbl != null)
            {
                bool found = false;

                for (int i = 1; i <= tbl.Keys.Count; i++)
                {
                    if (tbl[i].Equals(dv))
                    {
                        tbl[i] = null;
                        found = true;
                    }

                    if (found)
                        tbl[i] = tbl[i + 1];
                }

                if (found)
                    return dv;
            }

            return null;
        }

        // foreach(tbl, func)
        public void _foreach(LuaTable tbl, LuaFunction func)
        {
            if (tbl == null)
                return;

            foreach (KeyValuePair<object, object> kv in tbl)
            {
                if (kv.Value == null)
                    break;

                func.Call(kv.Value);
            }
        }

        // pairs(tbl) 

        // ****************************************************************
        // *** Input ***
        // ****************************************************************

        // btn([i,] [p]) -> bool when indexed, int mask when no args
        public object btn(object _i = null, object _p = null)
        {
            if (_i == null)
                return m_buttons[0] | (m_buttons[1] << 8); // int mask

            int i = Convert.ToInt32(_i);
            int p = (_p != null) ? Convert.ToInt32(_p) : 0;
            if (p >= PLAYER_COUNT) p = 0;
            if (i < 0 || i >= BUTTON_COUNT) return false;

            return ((m_buttons[p] >> i) & 1) != 0; // bool
        }

        // btnp([i,] [p]) -> bool when indexed, int mask when no args
        public object btnp(object _i = null, object _p = null)
        {
            if (_i == null)
                return m_buttonsp[0] | (m_buttonsp[1] << 8); // int mask

            int i = Convert.ToInt32(_i);
            int p = (_p != null) ? Convert.ToInt32(_p) : 0;
            if (p >= PLAYER_COUNT) p = 0;
            if (i < 0 || i >= BUTTON_COUNT) return false;

            return ((m_buttonsp[p] >> i) & 1) != 0; // bool
        }

        // ****************************************************************
        // *** Sound ***
        // ****************************************************************

        // music(n, [fadems,] [channelmask])
        public void music(int n, int fadems = 0, int channelmask = 0)
        {
            lock (m_soundQueue)
            {
                m_soundQueue.Enqueue(new SoundCommand(n, fadems, channelmask));
            }
        }

        // sfx(n, [channel,] [offset,] [length])
        public void sfx(int n, int channel = -1, int offset = 0, int length = 32)
        {
            lock (m_soundQueue)
            {
                m_soundQueue.Enqueue(new SoundCommand(n, channel, offset, length));
            }
        }

        // ****************************************************************
        // *** Map ***
        // ****************************************************************

        // map(celx, cely, sx, sy, celw, celh, [layer])
        public int map(int celx, int cely, int sx, int sy, int celw = -1, int celh = -1, int layer = 0)
        {
            int default_celw = m_memory[MEMORY_MAP_WIDTH];
            if (default_celw == 0) default_celw = 128;
            int map_start = m_memory[MEMORY_MAP_START];
            int max_map_cells = (map_start >= 0x80) ? (0x10000 - (map_start << 8)) : 0x2000;
            int default_celh = (default_celw > 0) ? (max_map_cells / default_celw) : 64;

            celw = celw != -1 ? celw : default_celw;
            celh = celh != -1 ? celh : default_celh;

            // Visible-cell optimization: only draw cells that overlap the clip rect
            int cam_x, cam_y;
            camera_get(out cam_x, out cam_y);
            int clip_x0, clip_y0, clip_x1, clip_y1;
            clip_get(out clip_x0, out clip_y0, out clip_x1, out clip_y1);

            int screen_x0 = cam_x + clip_x0;
            int screen_y0 = cam_y + clip_y0;
            int screen_x1 = cam_x + clip_x1;
            int screen_y1 = cam_y + clip_y1;

            int start_x = Math.Max(0, (screen_x0 - sx) / SPRITE_WIDTH);
            int start_y = Math.Max(0, (screen_y0 - sy) / SPRITE_HEIGHT);
            int end_x = Math.Min(celw, (screen_x1 - sx + SPRITE_WIDTH - 1) / SPRITE_WIDTH);
            int end_y = Math.Min(celh, (screen_y1 - sy + SPRITE_HEIGHT - 1) / SPRITE_HEIGHT);

            bool sprite_0_opaque = (m_memory[MEMORY_MISCFLAGS] & 0x8) != 0;

            for (int y = start_y; y < end_y; y++)
            {
                for (int x = start_x; x < end_x; x++)
                {
                    byte index = map_get(celx + x, cely + y);
                    byte sprite_flags = m_memory[MEMORY_SPRITEFLAGS + index];
                    bool should_draw = (index != 0 || sprite_0_opaque) && (layer == 0 || ((layer & sprite_flags) == layer));

                    if (should_draw)
                    {
                        int left = sx + x * SPRITE_WIDTH;
                        int top = sy + y * SPRITE_HEIGHT;
                        draw_sprite(index, left, top, false, false);
                    }
                }
            }

            return 0;
        }

        // mget(celx, cely)
        public byte mget(int celx, int cely)
        {
            int address = map_cell_addr(celx, cely);
            if (address == 0)
            {
                int default_val = (m_memory[MEMORY_MISCFLAGS] & 0x10) != 0 ? m_memory[MEMORY_MGET_DEFAULT] : 0;
                return (byte)default_val;
            }
            return m_memory[address];
        }

        // mset(celx, cely, snum)
        public void mset(int celx, int cely, int snum)
        {
            map_set(celx, cely, snum);
        }

        // ****************************************************************
        // *** Memory ***
        // ****************************************************************

        // cstore(destaddr, sourceaddr, len, [filename])
        // memcpy(destaddr, sourceaddr, len)
        public int memcpy(int destaddr, int sourceaddr, int len)
        {
            if (len < 1 || destaddr == sourceaddr)
                return 0;

            if (sourceaddr < 0 || (sourceaddr + len) > (1 << 16))
                return 0;

            if (destaddr < 0 || (destaddr + len) > (1 << 16))
                return 0;

            while (len > 0)
            {
                int chunk = Math.Min(Math.Min(len, 0x2000 - (sourceaddr & 0x1fff)), 0x2000 - (destaddr & 0x1fff));
                int destaddr1 = addr_remap(destaddr);
                int sourceaddr1 = addr_remap(sourceaddr);
                Buffer.BlockCopy(m_memory, sourceaddr1, m_memory, destaddr1, chunk);
                destaddr += chunk;
                sourceaddr += chunk;
                len -= chunk;
            }

            return 0;
        }

        // memset(destaddr, val, len)
        public void memset(int destaddr, int val, int len)
        {
            while (len > 0)
            {
                int chunk = Math.Min(len, 0x2000 - (destaddr & 0x1fff));
                int destaddr1 = addr_remap(destaddr);
                for (int i = 0; i < chunk; i++)
                    m_memory[destaddr1 + i] = (byte)val;
                destaddr += chunk;
                len -= chunk;
            }
        }

        // peek(addr, [n])
        public byte peek(int addr, int n = 1)
        {
            addr = addr_remap(addr);
            return m_memory[addr];
        }

        // peek2(addr, [n])
        public short peek2(int addr, int n = 1)
        {
            addr = addr_remap(addr);
            return (short)(m_memory[addr + 1] << 8 | m_memory[addr]);
        }

        // peek4(addr, [n])
        public int peek4(int addr, int n = 1)
        {
            addr = addr_remap(addr);
            return (int)(m_memory[addr + 3] << 24 | m_memory[addr + 2] << 16 | m_memory[addr + 1] << 8 | m_memory[addr]);
        }

        // poke(addr, val1, ...)
        public void poke(int addr, byte[] values)
        {
            if (values == null || values.Length == 0)
                return;

            addr = addr_remap(addr);

            for (int i = 0; i < values.Length; i++)
                m_memory[addr + i] = values[i];

            if (addr >= MEMORY_CARTDATA && addr + values.Length <= MEMORY_CARTDATA + MEMORY_CARTDATA_SIZE)
                m_cartData.SetDelayedFlush();
        }

        // poke2(addr, val1, ...)
        public void poke2(int addr, short[] values)
        {
            if (values == null || values.Length == 0)
                return;

            addr = addr_remap(addr);

            for (int i = 0; i < values.Length; i++)
            {
                int o = addr + i * 2;
                short v = values[i];
                m_memory[o] = (byte)(v & 0xFF);
                m_memory[o + 1] = (byte)(v >> 8);
            }

            if (addr >= MEMORY_CARTDATA && addr + values.Length * 2 <= MEMORY_CARTDATA + MEMORY_CARTDATA_SIZE)
                m_cartData.SetDelayedFlush();
        }

        // poke4(addr, val1, ...)
        public void poke4(int addr, int[] values)
        {
            if (values == null || values.Length == 0)
                return;

            addr = addr_remap(addr);

            for (int i = 0; i < values.Length; i++)
            {
                int o = addr + i * 4;
                int v = values[i];
                m_memory[o] = (byte)(v & 0xFF);
                m_memory[o + 1] = (byte)((v >> 8) & 0xFF);
                m_memory[o + 2] = (byte)((v >> 16) & 0xFF);
                m_memory[o + 3] = (byte)((v >> 24) & 0xFF);
            }

            if (addr >= MEMORY_CARTDATA && addr + values.Length * 4 <= MEMORY_CARTDATA + MEMORY_CARTDATA_SIZE)
                m_cartData.SetDelayedFlush();
        }

        // reload(destaddr, sourceaddr, len, [filename])
        public void reload(int destaddr, int sourceaddr, int len)
        {
            destaddr = addr_remap(destaddr);
            if (m_cart_memory != null && destaddr >= 0 && destaddr + len <= 0x10000 && sourceaddr >= 0 && sourceaddr + len <= CART_MEMORY_SIZE)
                Buffer.BlockCopy(m_cart_memory, sourceaddr, m_memory, destaddr, len);
        }

        // ****************************************************************
        // *** Math ***
        // ****************************************************************

        // abs(num)
        public int abs(float num)
        {
            return (int)Math.Abs(num);
        }

        // atan2(dx, dy)
        public float atan2(float dx, float dy)
        {
            float value = (float)(Math.Atan2(dx, dy) / (2 * Math.PI) - 0.25);

            if (value < 0.0f)
                value += 1.0f;

            return value;
        }

        // band(first, second)
        public int band(int first, int second)
        {
            return (first & second);
        }

        // bnot(num)
        public int bnot(int num)
        {
            return ~num;
        }

        // bor(first, second)
        public int bor(int first, int second)
        {
            return (first | second);
        }

        // bxor(first, second)
        public int bxor(int first, int second)
        {
            return (first ^ second);
        }

        // ceil(num)
        public float ceil(float num)
        {
            return (float)Math.Ceiling(num);
        }

        // cos(angle)
        public float cos(float angle)
        {
            return (float)Math.Cos((1f - angle) * m_twoPI);
        }

        // flr(num)
        public int flr(float num)
        {
            return (int)Math.Floor(num);
        }

        // lshr(num, bits)
        public int lshr(int num, int bits)
        {
            return (num >> bits ^ num & 0xFFFFFF);
        }

        // max(first, [second])
        public float max(float first, float second)
        {
            return Math.Max(first, second);
        }

        // mid(first, second, third)
        public float mid(float first, float second, float third)
        {
            if ((first < second && second < third) || (third < second && second < first))
                return second;
            else if ((second < first && first < third) || (third < first && first < second))
                return first;
            else
                return third;
        }

        // min(first, [second])
        public float min(float first, float second)
        {
            return Math.Min(first, second);
        }

        // rnd(max)
        public float rnd(float max)
        {
            uint hi = (uint)(m_memory[MEMORY_RNG_STATE] | (m_memory[MEMORY_RNG_STATE + 1] << 8) |
                      (m_memory[MEMORY_RNG_STATE + 2] << 16) | (m_memory[MEMORY_RNG_STATE + 3] << 24));
            uint lo = (uint)(m_memory[MEMORY_RNG_STATE + 4] | (m_memory[MEMORY_RNG_STATE + 5] << 8) |
                      (m_memory[MEMORY_RNG_STATE + 6] << 16) | (m_memory[MEMORY_RNG_STATE + 7] << 24));

            hi = (hi << 16) | (hi >> 16);
            hi += lo;
            lo += hi;

            m_memory[MEMORY_RNG_STATE] = (byte)(hi & 0xFF);
            m_memory[MEMORY_RNG_STATE + 1] = (byte)((hi >> 8) & 0xFF);
            m_memory[MEMORY_RNG_STATE + 2] = (byte)((hi >> 16) & 0xFF);
            m_memory[MEMORY_RNG_STATE + 3] = (byte)((hi >> 24) & 0xFF);
            m_memory[MEMORY_RNG_STATE + 4] = (byte)(lo & 0xFF);
            m_memory[MEMORY_RNG_STATE + 5] = (byte)((lo >> 8) & 0xFF);
            m_memory[MEMORY_RNG_STATE + 6] = (byte)((lo >> 16) & 0xFF);
            m_memory[MEMORY_RNG_STATE + 7] = (byte)((lo >> 24) & 0xFF);

            return (hi >> 16) * max / 65536.0f;
        }

        // rotl(num, bits)
        public int rotl(int num, int bits)
        {
            return (num << bits) | (num >> (32 - bits));
        }

        // rotr(num, bits)
        public int rotr(int num, int bits)
        {
            return (num >> bits) | (num << (32 - bits));
        }

        // sgn([number])
        public float sgn(float number)
        {
            return (number > 0 ? 1.0f : -1.0f);
        }

        // shl(num, bits)
        public int shl(int num, int bits)
        {
            return (num << bits);
        }

        // shr(num, bits)
        public int shr(int num, int bits)
        {
            return (num >> bits);
        }

        // sin(angle)
        public float sin(float angle)
        {
            return (float)Math.Sin((1f - angle) * m_twoPI);
        }

        // sqrt(num)
        public float sqrt(float num)
        {
            return (float)Math.Sqrt(num);
        }

        // srand(val)
        public void srand(int val)
        {
            uint seed = (uint)val;
            m_memory[MEMORY_RNG_STATE] = (byte)(seed & 0xFF);
            m_memory[MEMORY_RNG_STATE + 1] = (byte)((seed >> 8) & 0xFF);
            m_memory[MEMORY_RNG_STATE + 2] = (byte)((seed >> 16) & 0xFF);
            m_memory[MEMORY_RNG_STATE + 3] = (byte)((seed >> 24) & 0xFF);
            m_memory[MEMORY_RNG_STATE + 4] = 0;
            m_memory[MEMORY_RNG_STATE + 5] = 0;
            m_memory[MEMORY_RNG_STATE + 6] = 0;
            m_memory[MEMORY_RNG_STATE + 7] = 0;
        }

        // ****************************************************************
        // *** Cartridge data ***
        // ****************************************************************

        // cartdata(id)
        public void cartdata(string id)
        {
            m_cartData.Open(id);
        }

        // dget(index)
        public int dget(int index)
        {
            int byte1 = m_memory[MEMORY_CARTDATA + index * 4 + 3] << 24;
            int byte2 = m_memory[MEMORY_CARTDATA + index * 4 + 2] << 16;
            int byte3 = m_memory[MEMORY_CARTDATA + index * 4 + 1] << 8;
            int byte4 = m_memory[MEMORY_CARTDATA + index * 4];

            return byte1 | byte2 | byte3 | byte4;
        }

        // dset(index, value)
        public void dset(int index, int value)
        {
            m_memory[MEMORY_CARTDATA + index * 4] = (byte)value;
            m_memory[MEMORY_CARTDATA + index * 4 + 1] = (byte)(value >> 8);
            m_memory[MEMORY_CARTDATA + index * 4 + 2] = (byte)(value >> 16);
            m_memory[MEMORY_CARTDATA + index * 4 + 3] = (byte)(value >> 24);

            m_cartData.SetDelayedFlush();
        }

        // ****************************************************************
        // *** Coroutines ***
        // ****************************************************************

        // cocreate(func)
        // coresume(cor)
        // costatus(cor)
        // yield() 

        // ****************************************************************
        // *** Values and objects ***
        // ****************************************************************

        //chr(ord)
        public int chr(int ord)
        {
            return (int)ord;
        }

        // setmetatable(tbl, metatbl)
        // getmetatable(tbl)
        // type(v)
        // sub(str, from, [to])
        public string sub(string str, int start, int end = -1)
        {
            if (str == null) return "";
            int str_len = str.Length;

            if (start < 1) start = 1;
            if (start > str_len + 1) start = str_len + 1;

            if (end == -1)
            {
                int len = str_len - start + 1;
                if (len < 0) len = 0;
                return str.Substring(start - 1, len);
            }

            if (end < 0) end = str_len + end + 1;
            if (end < 1) end = 1;
            if (end > str_len) end = str_len;

            int length = end - start + 1;
            if (length < 0) length = 0;
            return str.Substring(start - 1, length);
        }

        // tonum(str)
        // tostr(val, [usehex])
        public string tostr(object val, bool usehex = false)
        {
            if (usehex)
                return String.Format("0x{0:X8}", val.ToString());

            return val.ToString();
        }

        // ****************************************************************
        // *** Time ***
        // ****************************************************************

        // time()
        public float time()
        {
            return (float)m_frames / (float)m_fps;
        }

        // ****************************************************************
        // *** System ***
        // ****************************************************************

        // menuitem(index, [label, callback])
        public void menuitem(int index = 0, string label = null, object callback = null)
        {
        }

        // extcmd(cmd)
        public void extcmd(string cmd)
        {
            // stub - pause/reset/shutdown not supported in this environment
        }

        // ****************************************************************
        // *** Debugging ***
        // ****************************************************************

        // assert(cond, [message])
        // printh(str, [filename], [overwrite])
        public void printh(string str = null, string filename = null, bool overwrite = false)
        {
            Debug.WriteLine(str ?? "");
        }

        // stat(n)
        public object stat(int n)
        {
            switch (n)
            {
                case STAT_FRAMERATE:
                    return (int)m_actual_fps;
                case STAT_TARGET_FRAMERATE:
                    return (int)m_fps;
                case STAT_MOUSE_X:
                    return m_mouse.X;
                case STAT_MOUSE_Y:
                    return m_mouse.Y;
                case STAT_MOUSE_BUTTONS:
                    return 0;
                case STAT_MOUSE_WHEEL:
                    return 0;
                case STAT_YEAR:
                    return DateTime.Now.Year;
                case STAT_MONTH:
                    return DateTime.Now.Month;
                case STAT_DAY:
                    return DateTime.Now.Day;
                case STAT_HOUR:
                    return DateTime.Now.Hour;
                case STAT_MINUTE:
                    return DateTime.Now.Minute;
                case STAT_SECOND:
                    return DateTime.Now.Second;
                case STAT_YEAR_UTC:
                    return DateTime.UtcNow.Year;
                case STAT_MONTH_UTC:
                    return DateTime.UtcNow.Month;
                case STAT_DAY_UTC:
                    return DateTime.UtcNow.Day;
                case STAT_HOUR_UTC:
                    return DateTime.UtcNow.Hour;
                case STAT_MINUTE_UTC:
                    return DateTime.UtcNow.Minute;
                case STAT_SECOND_UTC:
                    return DateTime.UtcNow.Second;
                default:
                    return 0;
            }
        }

        // stop() (undocumented)
        // trace() (undocumented)


        // ****************************************************************
        // *** Misc ***
        // ****************************************************************


        // ****************************************************************
        // ****************************************************************

        public void warning(string msg)
        {
            Debug.WriteLine("WARNING: {0}", msg);
        }
        public void setfps(int fps)
        {
            m_fps = Math.Max(0, fps);
        }

        public int getmousex()
        {
            return m_mouse.X;
        }

        public int getmousey()
        {
            return m_mouse.Y;
        }

        public int note_to_hz(int note)
        {
            return 440 * 2 ^ ((note - 33) / 12);
        }

        private void UpdateMusic()
        {
        }

        private static byte GetKeyboardMaskP0()
        {
            byte mask = 0;
            if ((GetAsyncKeyState((int)Keys.Left) & 0x8000) != 0) mask |= 1 << 0;  // Left
            if ((GetAsyncKeyState((int)Keys.Right) & 0x8000) != 0) mask |= 1 << 1;  // Right
            if ((GetAsyncKeyState((int)Keys.Up) & 0x8000) != 0) mask |= 1 << 2;  // Up
            if ((GetAsyncKeyState((int)Keys.Down) & 0x8000) != 0) mask |= 1 << 3;  // Down
            if ((GetAsyncKeyState((int)Keys.Z) & 0x8000) != 0) mask |= 1 << 4;  // O
            if ((GetAsyncKeyState((int)Keys.X) & 0x8000) != 0) mask |= 1 << 5;  // X
            return mask;
        }

        public void UpdateInput()
        {
            byte curr0 = GetKeyboardMaskP0();
            m_buttonsp[0] = (byte)(curr0 & ~m_buttonsPrev[0]);
            m_buttons[0] = curr0;

            byte curr1 = 0;
            m_buttonsp[1] = (byte)(curr1 & ~m_buttonsPrev[1]);
            m_buttons[1] = curr1;

            m_buttonsPrev[0] = curr0;
            m_buttonsPrev[1] = curr1;
        }

        private void UpdateChannel(SoundState channel)
        {
            if (channel.SoundMode == SoundMode.None)
                return;

            if (channel.SoundMode == SoundMode.Music)
            {
                // Sound has ended, behavior depends on flag for music
                if (channel.Sample >= channel.End)
                {
                    bool isLoopBegin = (m_memory[MEMORY_MUSIC + 4 * m_musicState.Pattern] & (1 << 7)) != 0;
                    bool isLoopEnd = (m_memory[MEMORY_MUSIC + 4 * m_musicState.Pattern + 1] & (1 << 7)) != 0;
                    bool isStop = (m_memory[MEMORY_MUSIC + 4 * m_musicState.Pattern + 2] & (1 << 7)) != 0;

                    if (isStop)
                    {
                        //for (int i = 0; i < m_musicState.Channels.Length; i++)
                        //    m_musicState.Channels[i].SoundMode = SoundMode.None;

                        return;
                    }

                    m_musicState.Pattern++;

                    if (isLoopEnd || m_musicState.Pattern == MUSIC_COUNT)
                    {
                        int i = m_musicState.Pattern - 1;

                        while (i >= 0)
                        {
                            isLoopBegin = (m_memory[MEMORY_MUSIC + 4 * i] & (1 << 7)) != 0;

                            if (isLoopBegin || i == 0)
                                break;

                            i--;
                        }

                        m_musicState.Pattern = i;
                    }


                    for (int i = 0; i < m_channels.Length; i++)
                    {
                        byte channelData = m_memory[MEMORY_MUSIC + 4 * m_musicState.Pattern + i];
                        bool enabled = (channelData & (1 << 6)) == 0;

                        if (enabled)
                        {
                            m_channels[i].SoundMode = SoundMode.Music;
                            m_channels[i].SoundIndex = channelData & 0x7F;
                            m_channels[i].Sample = 0;
                            m_channels[i].Position = 0;
                            m_channels[i].End = 31;
                        }
                        else
                            m_channels[i].SoundMode = SoundMode.None;

                        //Debug.WriteLine("UpdateChannel enabled: {0} sound: {1}", enabled, m_channels[i].SoundIndex);
                    }

                    //Debug.WriteLine("m_musicState.Pattern: {0}", m_musicState.Pattern);
                }
            }
            else if (channel.SoundMode == SoundMode.Sound)
            {
                if (channel.Sample >= channel.End)
                    channel.SoundMode = SoundMode.None;
            }
        }

        private void UpdateSoundQueue()
        {
            lock (m_soundQueue)
            {
                foreach (SoundCommand soundCommand in m_soundQueue)
                {
                    //Debug.WriteLine("SoundMode: {0} Index: {1} Start: {2} End: {3} FadeMs: {4} Mask: {5}", soundCommand.SoundMode, soundCommand.Index, soundCommand.Start, soundCommand.End, soundCommand.FadeMs, soundCommand.Mask);

                    if (soundCommand.SoundMode == SoundMode.Sound)
                    {
                        // Stop sound on channel
                        if (soundCommand.Index == -1)
                        {
                            if (soundCommand.Channel >= 0 && soundCommand.Channel <= m_channels.Length)
                                m_channels[soundCommand.Channel].SoundMode = SoundMode.None;

                            continue;
                        }
                        // Stop sound from looping
                        else if (soundCommand.Index == -2)
                            continue;
                        // Stop sound on all channels that are playing it
                        else if (soundCommand.Channel == -2)
                        {
                            foreach (SoundState channel in m_channels)
                            {
                                if (channel.SoundIndex == soundCommand.Index)
                                    channel.SoundMode = SoundMode.None;
                            }

                            continue;
                        }
                        // Find first available channel
                        else if (soundCommand.Channel == -1)
                        {
                            for (int i = 0; i < m_channels.Length; i++)
                            {
                                if (m_channels[i].SoundMode == SoundMode.None)
                                {
                                    soundCommand.Channel = i;

                                    break;
                                }
                            }
                        }

                        if (soundCommand.Channel >= 0 && soundCommand.Channel < m_channels.Length && soundCommand.Index >= 0 && soundCommand.Index <= SOUND_COUNT)
                        {
                            // Overtaking channel
                            SoundState channel = m_channels[soundCommand.Channel];
                            byte speed = m_memory[MEMORY_SFX + 68 * soundCommand.Index + 64 + 1];
                            int samplePerTick = (SAMPLE_RATE / 128) * (speed + 1);

                            channel.SoundMode = SoundMode.Sound;
                            channel.SoundIndex = soundCommand.Index;
                            channel.End = soundCommand.End;
                            channel.Sample = soundCommand.Start;
                            channel.Position = soundCommand.Start * samplePerTick;
                        }
                    }
                    else if (soundCommand.SoundMode == SoundMode.Music)
                    {
                        if (soundCommand.Index == -1)
                        {
                            for (int i = 0; i < m_channels.Length; i++)
                                m_channels[i].SoundMode = SoundMode.None;
                        }
                        else
                        {
                            m_musicState.Pattern = soundCommand.Index;
                            m_musicState.ChannelMask = (byte)soundCommand.Mask;

                            for (int i = 0; i < m_channels.Length; i++)
                            {

                                byte channelData = m_memory[MEMORY_MUSIC + 4 * m_musicState.Pattern + i];
                                bool enabled = (channelData & (1 << 6)) == 0;

                                if (enabled)
                                {
                                    m_channels[i].SoundMode = SoundMode.Music;
                                    m_channels[i].SoundIndex = channelData & 0x7F;
                                    m_channels[i].Sample = 0;
                                    m_channels[i].Position = 0;
                                    m_channels[i].End = 31;
                                }
                                else
                                    m_channels[i].SoundMode = SoundMode.None;
                            }
                        }
                    }
                }

                m_soundQueue.Clear();
            }
        }

        private float GetFrequency(int pitch)
        {
            return m_toneFrequencies[pitch % 12] / 2 * (1 << (pitch / 12));
        }

        private void RenderSound(Waveform waveform, int pitch, int volume, int position, int offset, int length, ref short[] buffer)
        {
            short amplitude = (short)((MAX_VOLUME / 8) * volume);
            uint frequency = (uint)GetFrequency(pitch);

            switch (waveform)
            {
                case Waveform.Triangle:
                    DSP.TriangleWave(frequency, amplitude, 0, position, offset, length, ref buffer);
                    break;
                case Waveform.TiltedSaw:
                    DSP.TiltedSawtoothWave(frequency, amplitude, 0, 0.85f, position, offset, length, ref buffer);
                    break;
                case Waveform.Saw:
                    DSP.SawtoothWave(frequency, amplitude, 0, position, offset, length, ref buffer);
                    break;
                case Waveform.Square:
                    DSP.SquareWave(frequency, amplitude, 0, position, offset, length, ref buffer);
                    break;
                case Waveform.Pulse:
                    DSP.PulseWave(frequency, amplitude, 0, 1.0f / 3.0f, position, offset, length, ref buffer);
                    break;
                case Waveform.Organ:
                    DSP.OrganWave(frequency, amplitude, 0, 0.5f, position, offset, length, ref buffer);
                    break;
                case Waveform.Noise:
                    DSP.Noise(frequency, amplitude, position, offset, length, ref buffer);
                    break;
                case Waveform.Phaser:
                    break;
            }
        }

        public void RenderSounds()
        {
            UpdateSoundQueue();

            for (int i = 0; i < m_soundBuffer.Length; i++)
                m_soundBuffer[i] = 0;

            for (int i = 0; i < CHANNEL_COUNT; i++)
            {
                int index = 0;
                //SoundMode soundMode = m_channels[i].SoundMode;
                SoundState channel = m_channels[i]; // (soundMode == SoundMode.Sound ? m_channels[i] : m_musicState.Channels[i]);
                //channel.SoundMode = SoundMode.Music;

                if ((channel.SoundMode == SoundMode.Music && m_musicEnabled) || (channel.SoundMode == SoundMode.Sound && m_soundEnabled))
                {
                    byte editorMode = m_memory[MEMORY_SFX + 68 * channel.SoundIndex + 64];
                    byte speed = m_memory[MEMORY_SFX + 68 * channel.SoundIndex + 64 + 1];
                    byte loopStart = m_memory[MEMORY_SFX + 68 * channel.SoundIndex + 64 + 2];
                    byte loopEnd = m_memory[MEMORY_SFX + 68 * channel.SoundIndex + 64 + 3];

                    int samplePerTick = (SAMPLE_RATE / 128) * (speed + 1);

                    //Debug.WriteLine("editorMode: {0} speed: {1} loopStart: {2} loopEnd: {3} samplePerTick: {4}", editorMode, speed, loopStart, loopEnd, samplePerTick);

                    while (index < m_soundBuffer.Length)
                    {
                        byte dataLo = m_memory[MEMORY_SFX + 68 * channel.SoundIndex + channel.Sample * 2];
                        byte dataHi = m_memory[MEMORY_SFX + 68 * channel.SoundIndex + channel.Sample * 2 + 1];
                        short data = (short)((dataHi << 8) | dataLo);

                        bool useSfx = ((data & 0x8000) != 0);
                        byte effect = (byte)((data & 0x7000) >> 12);
                        byte volume = (byte)((data & 0x0E00) >> 9);
                        byte waveform = (byte)((data & 0x01C0) >> 6);
                        byte pitch = (byte)(data & 0x003F);

                        int length = Math.Min(m_soundBuffer.Length - index, samplePerTick - (channel.Position % samplePerTick));

                        //Debug.WriteLine("useSfx: {0} effect: {1} volume: {2} waveform: {3} pitch: {4}", useSfx, effect, volume, waveform, pitch);
                        //Debug.WriteLine(String.Format("{0}{1:X1}{2:X1}{3:X1}{4:X1} ({5})", m_toneMap[pitch % m_toneMap.Length], pitch / m_toneMap.Length, waveform, volume, effect, channel.SoundIndex));

                        RenderSound((Waveform)waveform, pitch, volume, channel.Position, index, length, ref m_soundBuffer);

                        //if (Audio != null)
                        //    Audio(this, new AudioEventArgs(index, length, m_soundBuffer));

                        index += length;
                        channel.Position += length;
                        channel.Sample = channel.Position / samplePerTick;

                        UpdateChannel(channel);
                    }
                }
            }

            for (int i = 0; i < m_soundBuffer.Length; i++)
            {
                //Debug.WriteLine("{0:X4}", m_soundBuffer[i]);

                //if (i == 30)
                //    break;
            }

            //Debug.WriteLine("");

            if (Audio != null)
                Audio(this, new AudioEventArgs(m_soundBuffer));
            //if (Audio != null)
            //    Audio(this, new AudioEventArgs(0, m_soundBuffer.Length, m_soundBuffer));
        }

        public int[] PixelArray
        {
            get { return m_pixelArray; }
        }

        public double FPS
        {
            get { return m_fps; }
        }
    }

    public class AudioEventArgs : EventArgs
    {
        //public int Index;
        //public int Length;
        public short[] Samples;

        //public AudioEventArgs(int index, int length, short[] samples)
        public AudioEventArgs(short[] samples)
        {
            //Index = index;
            // Length = length;
            Samples = samples;
        }
    }

    public enum SoundMode
    {
        None,
        Sound,
        Music
    }

    public class SoundCommand
    {
        public SoundMode SoundMode;
        public int Index;
        public int Channel;
        public int Start;
        public int End;
        public int FadeMs;
        public int Mask;

        public SoundCommand(int index, int channel, int start, int end)
        {
            SoundMode = SoundMode.Sound;
            Index = index;
            Channel = channel;
            Start = start;
            End = end;
        }

        public SoundCommand(int index, int fadeMs, int mask)
        {
            SoundMode = SoundMode.Music;
            Index = index;
            FadeMs = fadeMs;
            Mask = mask;
        }
    }

    public class SoundState
    {
        public SoundMode SoundMode;
        public int SoundIndex;
        //public int MemoryOffset;
        public int Sample;
        public int Position;
        public int End;

        public SoundState()
        {
        }
    }

    public class MusicState
    {
        //public SoundState[] Channels;
        public int Pattern;
        public byte ChannelMask;

        public MusicState(int channelCount)
        {
            //Channels = new SoundState[channelCount];

            //for (int i = 0; i < channelCount; i++)
            //    Channels[i] = new SoundState();
        }
    }
}
