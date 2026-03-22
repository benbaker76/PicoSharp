using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSharp
{
    class LuaBridge
    {
        private Emulator m_emulator = null;

        public LuaBridge(Emulator emulator)
        {
            m_emulator = emulator;
        }

        // ****************************************************************
        // *** Graphics ***
        // ****************************************************************

        // camera([x,] [y])
        public void camera(object _x = null, object _y = null)
        {
            int x = (_x == null ? 0 : Convert.ToInt32(_x));
            int y = (_y == null ? 0 : Convert.ToInt32(_y));

            m_emulator.camera(x, y);
        }

        // circ(x, y, [r,] [col])
        public void circ(object _xc = null, object _yc = null, object _r = null, object _col = null)
        {
            int xc = (_xc == null ? 0 : Convert.ToInt32(_xc));
            int yc = (_yc == null ? 0 : Convert.ToInt32(_yc));
            int r = (_r == null ? 4 : Convert.ToInt32(_r));
            int col = (_col == null ? -1 : Convert.ToInt32(_col));

            m_emulator.circ(xc, yc, r, col);
        }

        // circfill(x, y, [r,] [col])
        public void circfill(object _xc = null, object _yc = null, object _r = null, object _col = null)
        {
            int xc = (_xc == null ? 0 : Convert.ToInt32(_xc));
            int yc = (_yc == null ? 0 : Convert.ToInt32(_yc));
            int r = (_r == null ? 4 : Convert.ToInt32(_r));
            int col = (_col == null ? -1 : Convert.ToInt32(_col));

            m_emulator.circfill(xc, yc, r, col);
        }

        // clip(x, y, w, h)
        public int clip(object _x = null, object _y = null, object _w = null, object _h = null, object _clip_previous = null)
        {
            if (_x == null && _y == null && _w == null && _h == null && _clip_previous == null)
                m_emulator.clip_set(0, 0, Emulator.P8_WIDTH, Emulator.P8_HEIGHT);
            else
            {
                int x = (_x == null ? 0 : Convert.ToInt32(_x));
                int y = (_y == null ? 0 : Convert.ToInt32(_y));
                int w = (_w == null ? 0 : Convert.ToInt32(_w));
                int h = (_h == null ? 0 : Convert.ToInt32(_h));
                bool clip_previous = (_clip_previous == null ? false : Convert.ToBoolean(_clip_previous));

                int prev_x0 = 0, prev_y0 = 0, prev_x1 = Emulator.P8_WIDTH, prev_y1 = Emulator.P8_HEIGHT;
                if (clip_previous)
                    m_emulator.clip_get(out prev_x0, out prev_y0, out prev_x1, out prev_y1);

                int x1 = x + w;
                int y1 = y + h;
                x = Math.Max(x, prev_x0);
                y = Math.Max(y, prev_y0);
                x1 = Math.Min(x1, prev_x1);
                y1 = Math.Min(y1, prev_y1);

                m_emulator.clip_set(x, y, x1-x, y1-y);
            }

            return 0;
        }

        // cls([color])
        public void cls(object _color = null)
        {
            int color = (_color == null ? 0 : Convert.ToInt32(_color));

            m_emulator.cls(color);
        }

        // color(col)
        public void color(object _col = null)
        {
            int col = (_col == null ? 0 : Convert.ToInt32(_col));

            m_emulator.color(col);
        }

        // cursor([x,] [y,] [col])
        public void cursor(object _x = null, object _y = null, object _col = null)
        {
            int x = (_x == null ? 0 : Convert.ToInt32(_x));
            int y = (_y == null ? 0 : Convert.ToInt32(_y));
            int col = (_col == null ? -1 : Convert.ToInt32(_col));

            m_emulator.cursor(x, y, col);
        }

        // fget(n, [f])
        public object fget(object _n = null, object _f = null)
        {
            int n = (_n == null ? 0 : Convert.ToInt32(_n));
            int f = (_f == null ? -1 : Convert.ToInt32(_f));

            return m_emulator.fget(n, f);
        }

        // fillp([pat])
        public void fillp(object _pat = null)
        {
            int pat = (_pat == null ? -1 : Convert.ToInt32(_pat));

            m_emulator.fillp(pat);
        }

        // flip()
        public void flip()
        {
            m_emulator.flip();
        }

        // fset(n, [f,] v)
        public void fset(object _n = null, object _f = null, object _v = null)
        {
            int n = (_n == null ? 0 : Convert.ToInt32(_n));
            int f = (_f == null ? 0 : Convert.ToInt32(_f));
            object v = (_v == null ? null : _v);

            m_emulator.fset(n, f, v);
        }

        // line([x0,] [y0,] x1, y1, [col])
        public void line(object _x0 = null, object _y0 = null, object _x1 = null, object _y1 = null, object _col = null)
        {
            int x0 = (_x0 == null ? 0 : Convert.ToInt32(_x0));
            int y0 = (_y0 == null ? 0 : Convert.ToInt32(_y0));
            int x1 = (_x1 == null ? 1 : Convert.ToInt32(_x1));
            int y1 = (_y1 == null ? 1 : Convert.ToInt32(_y1));
            int col = (_col == null ? -1 : Convert.ToInt32(_col));

            m_emulator.line(x0, y0, x1, y1, col);
        }

        // oval(x0, y0, x1, y1, [col])
        public void oval(object _x0 = null, object _y0 = null, object _x1 = null, object _y1 = null, object _col = null)
        {
            int x0 = (_x0 == null ? 0 : Convert.ToInt32(_x0));
            int y0 = (_y0 == null ? 0 : Convert.ToInt32(_y0));
            int x1 = (_x1 == null ? 0 : Convert.ToInt32(_x1));
            int y1 = (_y1 == null ? 0 : Convert.ToInt32(_y1));
            int col = (_col == null ? -1 : Convert.ToInt32(_col));

            m_emulator.oval(x0, y0, x1, y1, col);
        }

        // ovalfill(x0, y0, x1, y1, [col])
        public void ovalfill(object _x0 = null, object _y0 = null, object _x1 = null, object _y1 = null, object _col = null)
        {
            int x0 = (_x0 == null ? 0 : Convert.ToInt32(_x0));
            int y0 = (_y0 == null ? 0 : Convert.ToInt32(_y0));
            int x1 = (_x1 == null ? 0 : Convert.ToInt32(_x1));
            int y1 = (_y1 == null ? 0 : Convert.ToInt32(_y1));
            int col = (_col == null ? -1 : Convert.ToInt32(_col));

            m_emulator.ovalfill(x0, y0, x1, y1, col);
        }

        // pal(c0, c1, [p])
        public void pal(object _c0 = null, object _c1 = null, object _p = null)
        {
            int c0 = (_c0 == null ? -1 : Convert.ToInt32(_c0));
            int c1 = (_c1 == null ? -1 : Convert.ToInt32(_c1));
            int p = (_p == null ? -1 : Convert.ToInt32(_p));

            m_emulator.pal(c0, c1, p);
        }

        // palt([col,] [t])
        public void palt(object _col = null, object _t = null)
        {
            int col = (_col == null ? -1 : Convert.ToInt32(_col));
            bool? t = (_t == null ? null : (bool?)Convert.ToBoolean(_t));

            m_emulator.palt(col, t);
        }

        // pget(x, y)
        public byte pget(object _x = null, object _y = null)
        {
            int x = (_x == null ? 0 : Convert.ToInt32(_x));
            int y = (_y == null ? 0 : Convert.ToInt32(_y));

            return m_emulator.pget(x, y);
        }

        // print(str, [x,] [y,] [col])
        public int print(object _str, object _x = null, object _y = null, object _col = null)
        {
            if (_str == null)
                return 0;

            string str = Convert.ToString(_str) ?? string.Empty;

            int w;

            // ---- Case 1: print(str [, col])  → 1 or 2 args total
            if (_x == null && _y == null)
            {
                // get cursor
                m_emulator.cursor_get(out int x, out int y);

                // choose color (optional)
                bool colProvided = _col != null;
                int col = colProvided
                    ? Convert.ToInt32(_col)
                    : (m_emulator.pencolor_get() & 0xF);

                if (colProvided)
                    m_emulator.pencolor_set((byte)col);

                // draw
                w = m_emulator.draw_text(str, x, y, col);

                // embedded \0 check (len > strlen in C) OR misc flag 0x04
                bool hasEmbeddedNull = str.IndexOf('\0') >= 0;
                bool forceInline = (m_emulator.m_memory[Emulator.MEMORY_MISCFLAGS] & 0x04) != 0;

                if (hasEmbeddedNull || forceInline)
                {
                    x += w;
                }
                else
                {
                    x = m_emulator.left_margin_get();
                    y += Emulator.GLYPH_HEIGHT;
                }

                // update cursor and maybe scroll
                m_emulator.cursor_set(x, y, -1);

                if ((m_emulator.m_memory[Emulator.MEMORY_MISCFLAGS] & 0x40) == 0)
                    m_emulator.scroll();

                return w;
            }

            // ---- Case 2: print(str, x, y [, col])  → 3 or 4 args total
            if (_x != null && _y != null)
            {
                int x = Convert.ToInt32(_x);
                int y = Convert.ToInt32(_y);

                bool colProvided = _col != null;
                int col = colProvided
                    ? Convert.ToInt32(_col)
                    : (m_emulator.pencolor_get() & 0xF);

                if (colProvided)
                    m_emulator.pencolor_set((byte)col);

                // keep left margin aligned with this x (matches C)
                m_emulator.left_margin_set(x);

                w = m_emulator.draw_text(str, x, y, col);
                return w;
            }

            return 0;
        }

        // pset(x, y, [c])
        public void pset(object _x = null, object _y = null, object _c = null)
        {
            int x = (_x == null ? 0 : Convert.ToInt32(_x));
            int y = (_y == null ? 0 : Convert.ToInt32(_y));
            int c = (_c == null ? -1 : Convert.ToInt32(_c));

            m_emulator.pset(x, y, c);
        }

        // rect(x0, y0, x1, y1, [col])
        public void rect(object _x0 = null, object _y0 = null, object _x1 = null, object _y1 = null, object _col = null)
        {
            int x0 = (_x0 == null ? 0 : Convert.ToInt32(_x0));
            int y0 = (_y0 == null ? 0 : Convert.ToInt32(_y0));
            int x1 = (_x1 == null ? 0 : Convert.ToInt32(_x1));
            int y1 = (_y1 == null ? 0 : Convert.ToInt32(_y1));
            int col = (_col == null ? -1 : Convert.ToInt32(_col));

            m_emulator.rect(x0, y0, x1, y1, col);
        }

        // rectfill(x0, y0, x1, y1, [col])
        public void rectfill(object _x0 = null, object _y0 = null, object _x1 = null, object _y1 = null, object _col = null)
        {
            int x0 = (_x0 == null ? 0 : Convert.ToInt32(_x0));
            int y0 = (_y0 == null ? 0 : Convert.ToInt32(_y0));
            int x1 = (_x1 == null ? 0 : Convert.ToInt32(_x1));
            int y1 = (_y1 == null ? 0 : Convert.ToInt32(_y1));
            int col = (_col == null ? -1 : Convert.ToInt32(_col));

            m_emulator.rectfill(x0, y0, x1, y1, col);
        }

        // rrect(x, y, w, h, r, [col])
        public void rrect(object _x = null, object _y = null, object _w = null, object _h = null, object _r = null, object _col = null)
        {
            int x = (_x == null ? 0 : Convert.ToInt32(_x));
            int y = (_y == null ? 0 : Convert.ToInt32(_y));
            int w = (_w == null ? 0 : Convert.ToInt32(_w));
            int h = (_h == null ? 0 : Convert.ToInt32(_h));
            int r = (_r == null ? 0 : Convert.ToInt32(_r));
            int col = (_col == null ? -1 : Convert.ToInt32(_col));
            m_emulator.rrect(x, y, w, h, r, col);
        }

        // rrectfill(x, y, w, h, r, [col])
        public void rrectfill(object _x = null, object _y = null, object _w = null, object _h = null, object _r = null, object _col = null)
        {
            int x = (_x == null ? 0 : Convert.ToInt32(_x));
            int y = (_y == null ? 0 : Convert.ToInt32(_y));
            int w = (_w == null ? 0 : Convert.ToInt32(_w));
            int h = (_h == null ? 0 : Convert.ToInt32(_h));
            int r = (_r == null ? 0 : Convert.ToInt32(_r));
            int col = (_col == null ? -1 : Convert.ToInt32(_col));
            m_emulator.rrectfill(x, y, w, h, r, col);
        }

        // sget(x, y)
        public byte sget(object _x = null, object _y = null)
        {
            int x = (_x == null ? 0 : Convert.ToInt32(_x));
            int y = (_y == null ? 0 : Convert.ToInt32(_y));

            return m_emulator.sget(x, y);
        }

        // spr(n, x, y, [w], [h], [flip_x], [flip_y])
        public int spr(object _n = null, object _x = null, object _y = null, object _w = null, object _h = null, object _flip_x = null, object _flip_y = null)
        {
            if (_n == null)
                return 0;

            int n = (int)Math.Floor(Convert.ToDouble(_n)) & 0xFF;
            int x = _x == null ? 0 : (int)Math.Floor(Convert.ToDouble(_x));
            int y = _y == null ? 0 : (int)Math.Floor(Convert.ToDouble(_y));
            int w = _w == null ? 1 : Convert.ToInt32(_w);
            int h = _h == null ? 1 : Convert.ToInt32(_h);
            bool flip_x = _flip_x != null && Convert.ToBoolean(_flip_x);
            bool flip_y = _flip_y != null && Convert.ToBoolean(_flip_y);

            m_emulator.draw_sprites(n, x, y, w, h, flip_x, flip_y);

            return 0;
        }


        // sset(x, y, [c])
        public void sset(object _x = null, object _y = null, object _c = null)
        {
            int x = (_x == null ? 0 : Convert.ToInt32(_x));
            int y = (_y == null ? 0 : Convert.ToInt32(_y));
            int c = (_c == null ? -1 : Convert.ToInt32(_c));

            m_emulator.sset(x, y, c);
        }

        // sspr(sx, sy, sw, sh, dx, dy, [dw,] [dh,] [flip_x,] [flip_y])
        public int sspr(object _sx = null, object _sy = null, object _sw = null, object _sh = null, object _dx = null, object _dy = null, object _dw = null, object _dh = null, object _flip_x = null, object _flip_y = null)
        {
            int sx = (_sx == null ? 0 : Convert.ToInt32(_sx));
            int sy = (_sy == null ? 0 : Convert.ToInt32(_sy));
            int sw = (_sw == null ? 0 : Convert.ToInt32(_sw));
            int sh = (_sh == null ? 0 : Convert.ToInt32(_sh));
            int dx = (_dx == null ? 0 : Convert.ToInt32(_dx));
            int dy = (_dy == null ? 0 : Convert.ToInt32(_dy));
            int dw = (_dw == null ? -1 : Convert.ToInt32(_dw));
            int dh = (_dh == null ? -1 : Convert.ToInt32(_dh));
            bool flip_x = (_flip_x == null ? false : Convert.ToBoolean(_flip_x));
            bool flip_y = (_flip_y == null ? false : Convert.ToBoolean(_flip_y));
            float scale_x = (float)dw / sw;
            float scale_y = (float)dh / sh;

            m_emulator.draw_scaled_sprite(sx, sy, sw, sh, dx, dy, scale_x, scale_y, flip_x, flip_y);

            return 0;
        }

        // tline( x0, y0, x1, y1, mx, my, [mdx,] [mdy])
        public void tline(object _x0 = null, object _y0 = null, object _x1 = null, object _y1 = null, object _mx = null, object _my = null, object _mdx = null, object _mdy = null)
        {
            int x0 = (_x0 == null ? 0 : Convert.ToInt32(_x0));
            int y0 = (_y0 == null ? 0 : Convert.ToInt32(_y0));
            int x1 = (_x1 == null ? 0 : Convert.ToInt32(_x1));
            int y1 = (_y1 == null ? 0 : Convert.ToInt32(_y1));
            int mx = (_mx == null ? 0 : Convert.ToInt32(_mx));
            int my = (_my == null ? 0 : Convert.ToInt32(_my));
            int mdx = (_mdx == null ? 0 : Convert.ToInt32(_mdx));
            int mdy = (_mdy == null ? 0 : Convert.ToInt32(_mdy));

            m_emulator.tline(x0, y0, x1, y1, mx, my, mdx, mdy);
        }

        // ****************************************************************
        // *** Tables ***
        // ****************************************************************

        // add(tbl, v)
        public object add(LuaTable tbl = null, object v = null)
        {
            return m_emulator.add(tbl, v);
        }
        // all(tbl)
        public object all(LuaTable tbl = null)
        {
            return m_emulator.all(tbl);
        }

        // count(tbl)
        public int count(LuaTable tbl)
        {
            return m_emulator.count(tbl);
        }

        // del(tbl, v)
        public object del(LuaTable tbl, object dv)
        {
            return m_emulator.del(tbl, dv);
        }

        // foreach(tbl, func)
        public void _foreach(LuaTable tbl, LuaFunction func)
        {
            m_emulator._foreach(tbl, func);
        }

        // pairs(tbl) 

        // ****************************************************************
        // *** Input ***
        // ****************************************************************

        // btn([i,] [p])
        public object btn(object _i = null, object _p = null)
        {
            // allow emulator to return mask (int) when _i == null, or bool when indexed
            return m_emulator.btn(_i, _p);
        }

        // btnp([i,] [p])
        public object btnp(object _i = null, object _p = null)
        {
            return m_emulator.btnp(_i, _p);
        }

        // ****************************************************************
        // *** Sound ***
        // ****************************************************************

        // music(n, [fadems,] [channelmask])
        public void music(object _n, object _fadems = null, object _channelmask = null)
        {
            int n = (_n == null ? 0 : Convert.ToInt32(_n));
            int fadems = (_fadems == null ? 0 : Convert.ToInt32(_fadems));
            int channelmask = (_channelmask == null ? 0 : Convert.ToInt32(_channelmask));

            m_emulator.music(n, fadems, channelmask);
        }

        // sfx(n, [channel,] [offset,] [length])
        public void sfx(object _n = null, object _channel = null, object _offset = null, object _length = null)
        {
            int n = (_n == null ? 0 : Convert.ToInt32(_n));
            int channel = (_channel == null ? -1 : Convert.ToInt32(_channel));
            int offset = (_offset == null ? 0 : Convert.ToInt32(_offset));
            int length = (_length == null ? 32 : Convert.ToInt32(_length));

            m_emulator.sfx(n, channel, offset, length);
        }

        // ****************************************************************
        // *** Map ***
        // ****************************************************************

        // map(celx, cely, sx, sy, celw, celh, [layer])
        public void map(object _celx = null, object _cely = null, object _sx = null, object _sy = null, object _celw = null, object _celh = null, object _layer = null)
        {
            int celx = (_celx == null ? 0 : Convert.ToInt32(_celx));
            int cely = (_cely == null ? 0 : Convert.ToInt32(_cely));
            int sx = (_sx == null ? 0 : Convert.ToInt32(_sx));
            int sy = (_sy == null ? 0 : Convert.ToInt32(_sy));
            int celw = (_celw == null ? 0 : Convert.ToInt32(_celw));
            int celh = (_celh == null ? 0 : Convert.ToInt32(_celh));
            int layer = (_layer == null ? 0 : Convert.ToInt32(_layer));

            m_emulator.map(celx, cely, sx, sy, celw, celh, layer);
        }

        // mget(celx, cely)
        public byte mget(object _celx = null, object _cely = null)
        {
            int celx = (_celx == null ? 0 : Convert.ToInt32(_celx));
            int cely = (_cely == null ? 0 : Convert.ToInt32(_cely));

            return m_emulator.mget(celx, cely);
        }

        // mset(celx, cely, snum)
        public void mset(object _celx = null, object _cely = null, object _snum = null)
        {
            int celx = (_celx == null ? 0 : Convert.ToInt32(_celx));
            int cely = (_cely == null ? 0 : Convert.ToInt32(_cely));
            int snum = (_snum == null ? 0 : Convert.ToInt32(_snum));

            m_emulator.mset(celx, cely, snum);
        }

        // ****************************************************************
        // *** Memory ***
        // ****************************************************************

        // cstore(destaddr, sourceaddr, len, [filename])
        // memcpy(destaddr, sourceaddr, len)
        public void memcpy(object _destaddr = null, object _sourceaddr = null, object _len = null)
        {
            int destaddr = (_destaddr == null ? 0 : Convert.ToInt32(_destaddr));
            int sourceaddr = (_sourceaddr == null ? 0 : Convert.ToInt32(_sourceaddr));
            int len = (_len == null ? 0 : Convert.ToInt32(_len));

            m_emulator.memcpy(destaddr, sourceaddr, len);
        }

        // memset(destaddr, val, len)
        public void memset(object _destaddr = null, object _val = null, object _len = null)
        {
            int destaddr = (_destaddr == null ? 0 : Convert.ToInt32(_destaddr));
            int val = (_val == null ? 0 : Convert.ToInt32(_val));
            int len = (_len == null ? 0 : Convert.ToInt32(_len));

            m_emulator.memset(destaddr, val, len);
        }

        // peek(addr)
        public byte peek(object _addr = null)
        {
            int addr = (_addr == null ? 0 : Convert.ToInt32(_addr));

            return m_emulator.peek(addr);
        }

        // peek2(addr)
        public short peek2(object _addr = null)
        {
            int addr = (_addr == null ? 0 : Convert.ToInt32(_addr));

            return m_emulator.peek2(addr);
        }

        // peek4(addr)
        public int peek4(object _addr = null)
        {
            int addr = (_addr == null ? 0 : Convert.ToInt32(_addr));

            return m_emulator.peek4(addr);
        }

        // poke(addr, val)
        public void poke(object _addr = null, object _val = null)
        {
            int addr = (_addr == null ? 0 : Convert.ToInt32(_addr));
            int val = (_val == null ? 0 : Convert.ToInt32(_val));

            m_emulator.poke(addr, new byte[] { (byte)val });
        }

        // poke2(addr, val)
        public void poke2(object _addr = null, object _val = null)
        {
            int addr = (_addr == null ? 0 : Convert.ToInt32(_addr));
            int val = (_val == null ? 0 : Convert.ToInt32(_val));

            m_emulator.poke2(addr, new short[] { (short)val });
        }

        // poke4(addr, val)
        public void poke4(object _addr = null, object _val = null)
        {
            int addr = (_addr == null ? 0 : Convert.ToInt32(_addr));
            int val = (_val == null ? 0 : Convert.ToInt32(_val));

            m_emulator.poke4(addr, new int[] { (int)val });
        }

        // reload(destaddr, sourceaddr, len, [filename])
        public void reload(object _destaddr = null, object _sourceaddr = null, object _len = null)
        {
            int destaddr = (_destaddr == null ? 0 : Convert.ToInt32(_destaddr));
            int sourceaddr = (_sourceaddr == null ? 0 : Convert.ToInt32(_sourceaddr));
            int len = (_len == null ? 0 : Convert.ToInt32(_len));

            m_emulator.reload(destaddr, sourceaddr, len);
        }

        // ****************************************************************
        // *** Math ***
        // ****************************************************************

        // abs(num)
        public int abs(object _num = null)
        {
            float num = (_num == null ? 0 : Convert.ToSingle(_num));

            return m_emulator.abs(num);
        }

        // atan2(dx, dy)
        public float atan2(object _dx = null, object _dy = null)
        {
            float dx = (_dx == null ? 0 : Convert.ToSingle(_dx));
            float dy = (_dy == null ? 0 : Convert.ToSingle(_dy));

            return m_emulator.atan2(dx, dy);
        }

        // band(first, second)
        public int band(object _first = null, object _second = null)
        {
            int first = (_first == null ? 0 : Convert.ToInt32(_first));
            int second = (_second == null ? 0 : Convert.ToInt32(_second));

            return m_emulator.band(first, second);
        }

        // bnot(num)
        public int bnot(object _num = null)
        {
            int num = (_num == null ? 0 : Convert.ToInt32(_num));

            return m_emulator.bnot(num);
        }

        // bor(first, second)
        public int bor(object _first = null, object _second = null)
        {
            int first = (_first == null ? 0 : Convert.ToInt32(_first));
            int second = (_second == null ? 0 : Convert.ToInt32(_second));

            return m_emulator.bor(first, second);
        }

        // bxor(first, second)
        public int bxor(object _first = null, object _second = null)
        {
            int first = (_first == null ? 0 : Convert.ToInt32(_first));
            int second = (_second == null ? 0 : Convert.ToInt32(_second));

            return m_emulator.bxor(first, second);
        }

        // ceil(num)
        public float ceil(object _num = null)
        {
            float num = (_num == null ? 0 : Convert.ToSingle(_num));

            return m_emulator.ceil(num);
        }

        // cos(angle)
        public float cos(object _angle = null)
        {
            float angle = (_angle == null ? 0 : Convert.ToSingle(_angle));

            return m_emulator.cos(angle);
        }

        // flr(num)
        public int flr(object _num = null)
        {
            float num = (_num == null ? 0 : Convert.ToSingle(_num));

            return m_emulator.flr(num);
        }

        // lshr(num, bits)
        public int lshr(object _num = null, object _bits = null)
        {
            int num = (_num == null ? 0 : Convert.ToInt32(_num));
            int bits = (_bits == null ? 0 : Convert.ToInt32(_bits));

            return m_emulator.lshr(num, bits);
        }

        // max(first, [second])
        public float max(object _first = null, object _second = null)
        {
            float first = (_first == null ? 0 : Convert.ToSingle(_first));
            float second = (_second == null ? 0 : Convert.ToSingle(_second));

            return m_emulator.max(first, second);
        }

        // mid(first, second, third)
        public float mid(object _first = null, object _second = null, object _third = null)
        {
            float first = (_first == null ? 0 : Convert.ToSingle(_first));
            float second = (_second == null ? 0 : Convert.ToSingle(_second));
            float third = (_third == null ? 0 : Convert.ToSingle(_third));

            return m_emulator.mid(first, second, third);
        }

        // min(first, [second])
        public float min(object _first = null, object _second = null)
        {
            float first = (_first == null ? 0 : Convert.ToSingle(_first));
            float second = (_second == null ? 0 : Convert.ToSingle(_second));

            return m_emulator.min(first, second);
        }

        // rnd(max)
        public float rnd(object _max = null)
        {
            float max = (_max == null ? 1 : Convert.ToSingle(_max));

            return m_emulator.rnd(max);
        }

        // rotl(num, bits)
        public int rotl(object _num = null, object _bits = null)
        {
            int num = (_num == null ? 0 : Convert.ToInt32(_num));
            int bits = (_bits == null ? 0 : Convert.ToInt32(_bits));

            return m_emulator.rotl(num, bits);
        }

        // rotr(num, bits)
        public int rotr(object _num = null, object _bits = null)
        {
            int num = (_num == null ? 0 : Convert.ToInt32(_num));
            int bits = (_bits == null ? 0 : Convert.ToInt32(_bits));

            return m_emulator.rotr(num, bits);
        }

        // sgn([number])
        public float sgn(object _number = null)
        {
            float number = (_number == null ? 0 : Convert.ToSingle(_number));

            return m_emulator.sgn(number);
        }

        // shl(num, bits)
        public int shl(object _num = null, object _bits = null)
        {
            int num = (_num == null ? 0 : Convert.ToInt32(_num));
            int bits = (_bits == null ? 0 : Convert.ToInt32(_bits));

            return m_emulator.shl(num, bits);
        }

        // shr(num, bits)
        public int shr(object _num = null, object _bits = null)
        {
            int num = (_num == null ? 0 : Convert.ToInt32(_num));
            int bits = (_bits == null ? 0 : Convert.ToInt32(_bits));

            return m_emulator.shr(num, bits);
        }

        // sin(angle)
        public float sin(object _angle = null)
        {
            float angle = (_angle == null ? 0.0f : Convert.ToSingle(_angle));

            return m_emulator.sin(angle);
        }

        // sqrt(num)
        public float sqrt(object _num = null)
        {
            float num = (_num == null ? 0.0f : Convert.ToSingle(_num));

            return m_emulator.sqrt(num);
        }

        // srand(val)
        public void srand(object _val = null)
        {
            int val = (_val == null ? 0 : Convert.ToInt32(_val));
            m_emulator.srand(val);
        }

        // ****************************************************************
        // *** Cartridge data ***
        // ****************************************************************

        // cartdata(id)
        public void cartdata(object _id = null)
        {
            string id = (_id == null ? null : _id.ToString());

            m_emulator.cartdata(id);
        }

        // dget(index)
        public int dget(object _index = null)
        {
            int index = (_index == null ? 0 : Convert.ToInt32(_index));

            return m_emulator.dget(index);
        }

        // dset(index, value)
        public void dset(object _index = null, object _value = null)
        {
            int index = (_index == null ? 0 : Convert.ToInt32(_index));
            int value = (_value == null ? 0 : Convert.ToInt32(_value));

            m_emulator.dset(index, value);
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
        public int chr(object _ord = null)
        {
            int ord = (_ord == null ? 0 : Convert.ToInt32(_ord));

            return m_emulator.chr(ord);
        }

        // setmetatable(tbl, metatbl)
        // getmetatable(tbl)
        // type(v)
        // sub(str, from, [to])
        public string sub(object _str = null, object _start = null, object _end = null)
        {
            string str = (_str == null ? null : _str.ToString());
            int start = (_start == null ? 0 : Convert.ToInt32(_start));
            int end = (_end == null ? -1 : Convert.ToInt32(_end));

            return m_emulator.sub(str, start, end);
        }

        // tonum(str)
        // tostr(val, [usehex])
        public string tostr(object _val = null, object _usehex = null)
        {
            int val = (_val == null ? 0 : Convert.ToInt32(_val));
            bool usehex = (_usehex == null ? false : Convert.ToBoolean(_usehex));

            return m_emulator.tostr(val, usehex);
        }

        // ****************************************************************
        // *** Time ***
        // ****************************************************************

        // time()
        public float time()
        {
            return m_emulator.time();
        }

        // ****************************************************************
        // *** System ***
        // ****************************************************************

        // menuitem(index, [label, callback])
        public void menuitem(object _index = null, object _label = null, object _callback = null)
        {
            int index = (_index == null ? 0 : Convert.ToInt32(_index));
            string label = (_label == null ? null : _label.ToString());

            m_emulator.menuitem(index, label, _callback);
        }

        // extcmd(cmd)
        public void extcmd(object _cmd = null)
        {
            string cmd = (_cmd == null ? "" : _cmd.ToString());
            m_emulator.extcmd(cmd);
        }

        // ****************************************************************
        // *** Debugging ***
        // ****************************************************************

        // assert(cond, [message])
        // printh(str, [filename], [overwrite])
        public void printh(object _str = null, object _label = null, object _overwrite = null)
        {
            string str = (_str == null ? null : _str.ToString());
            string label = (_label == null ? null : _label.ToString());
            bool overwrite = (_overwrite == null ? false : Convert.ToBoolean(_overwrite));

            m_emulator.printh(str, label, overwrite);
        }

        // stat(n)
        public object stat(object _n = null)
        {
            int n = (_n == null ? 0 : Convert.ToInt32(_n));

            return m_emulator.stat(n);
        }

        // stop() (undocumented)
        // trace() (undocumented)


        // ****************************************************************
        // *** Misc ***
        // ****************************************************************


        // ****************************************************************
        // ****************************************************************
    }
}
