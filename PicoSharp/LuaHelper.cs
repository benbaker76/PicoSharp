using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PicoSharp.Emulator;

namespace PicoSharp
{
    public partial class Emulator
    {
        public void clear_screen(int col)
        {
            for (int y = 0; y < P8_HEIGHT; y++)
                for (int x = 0; x < P8_WIDTH; x++)
                    gfx_set(x, y, MEMORY_SCREEN, MEMORY_SCREEN_SIZE, col);

            clip_set(0, 0, P8_WIDTH, P8_HEIGHT);
            cursor_set(0, 0, -1);
        }

        public int dash_direction_x(int facing)
        {
            return 0;
        }

        public int dash_direction_y(int facing)
        {
            return 0;
        }

        public bool is_bbox_offscreen(int wx0, int wy0, int wx1, int wy1)
        {
            int cx, cy;
            camera_get(out cx, out cy);
            int ccx0, ccy0, ccx1, ccy1;
            clip_get(out ccx0, out ccy0, out ccx1, out ccy1);
            if (ccx0 < 0) ccx0 = 0;
            if (ccy0 < 0) ccy0 = 0;
            if (ccx1 > P8_WIDTH) ccx1 = P8_WIDTH;
            if (ccy1 > P8_HEIGHT) ccy1 = P8_HEIGHT;
            int sx0 = wx0 - cx, sy0 = wy0 - cy;
            int sx1 = wx1 - cx, sy1 = wy1 - cy;
            return sx1 < ccx0 || sx0 >= ccx1 || sy1 < ccy0 || sy0 >= ccy1;
        }

        public void draw_oval_segment(int xc, int yc, int x, int y, int r, int xr, int yr, int col, int fillp, int mask)
        {
            if ((mask & 1) != 0)
                pixel_set(xc + x * xr / r, yc + y * yr / r, col, fillp, DrawType.Graphic);
            if ((mask & 2) != 0)
                pixel_set(xc - x * xr / r, yc + y * yr / r, col, fillp, DrawType.Graphic);
            if ((mask & 4) != 0)
                pixel_set(xc + x * xr / r, yc - y * yr / r, col, fillp, DrawType.Graphic);
            if ((mask & 8) != 0)
                pixel_set(xc - x * xr / r, yc - y * yr / r, col, fillp, DrawType.Graphic);
            if ((mask & 16) != 0)
                pixel_set(xc + y * xr / r, yc + x * yr / r, col, fillp, DrawType.Graphic);
            if ((mask & 32) != 0)
                pixel_set(xc - y * xr / r, yc + x * yr / r, col, fillp, DrawType.Graphic);
            if ((mask & 64) != 0)
                pixel_set(xc + y * xr / r, yc - x * yr / r, col, fillp, DrawType.Graphic);
            if ((mask & 128) != 0)
                pixel_set(xc - y * xr / r, yc - x * yr / r, col, fillp, DrawType.Graphic);
        }

        public void draw_oval_mask(int xc, int yc, int xr, int yr, int col, int fillp, int mask)
        {
            int r = Math.Max(xr, yr);
            if (r <= 0)
                return;
            if (is_bbox_offscreen(xc - xr, yc - yr, xc + xr, yc + yr)) return;
            int x = 0, y = Math.Abs(r);
            int d = 3 - 2 * Math.Abs(r);

            draw_oval_segment(xc, yc, x, y, r, xr, yr, col, fillp, mask);

            while (y >= x)
            {
                x++;

                if (d > 0)
                {
                    y--;
                    d = d + 4 * (x - y) + 10;
                }
                else
                    d = d + 4 * x + 6;

                draw_oval_segment(xc, yc, x, y, r, xr, yr, col, fillp, mask);
            }
        }

        public void draw_circ_mask(int xc, int yc, int r, int col, int fillp, int mask)
        {
            draw_oval_mask(xc, yc, r, r, col, fillp, mask);
        }

        public void draw_circ(int xc, int yc, int r, int col, int fillp)
        {
            draw_circ_mask(xc, yc, r, col, fillp, 0xff);
        }

        public void draw_line(int x0, int y0, int x1, int y1, int col, int fillp)
        {
            if (is_bbox_offscreen(Math.Min(x0, x1), Math.Min(y0, y1), Math.Max(x0, x1), Math.Max(y0, y1))) return;
            int dx = Math.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            while (true)
            {
                pixel_set(x0, y0, col, fillp, DrawType.Graphic);

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;

                if (e2 >= dy)
                {
                    err += dy;

                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public void draw_hline(int x0, int y, int x1, int col, int fillp)
        {
            for (int x = x0; x <= x1; x++)
                pixel_set(x, y, col, fillp, DrawType.Graphic);
        }

        public void draw_vline(int x, int y0, int y1, int col, int fillp)
        {
            for (int y = y0; y <= y1; y++)
                pixel_set(x, y, col, fillp, DrawType.Graphic);
        }

        public void draw_ovalfill_segment(int xc, int yc, int x, int y, int r, int xr, int yr, int col, int fillp, int mask)
        {
            if ((mask & 1) != 0)
                draw_hline(xc, yc + y * yr / r, xc + x * xr / r, col, fillp);
            if ((mask & 2) != 0)
                draw_hline(xc - x * xr / r, yc + y * yr / r, xc, col, fillp);
            if ((mask & 4) != 0)
                draw_hline(xc, yc - y * yr / r, xc + x * xr / r, col, fillp);
            if ((mask & 8) != 0)
                draw_hline(xc - x * xr / r, yc - y * yr / r, xc, col, fillp);
            if ((mask & 16) != 0)
                draw_hline(xc, yc + x * yr / r, xc + y * xr / r, col, fillp);
            if ((mask & 32) != 0)
                draw_hline(xc - y * xr / r, yc + x * yr / r, xc, col, fillp);
            if ((mask & 64) != 0)
                draw_hline(xc, yc - x * yr / r, xc + y * xr / r, col, fillp);
            if ((mask & 128) != 0)
                draw_hline(xc - y * xr / r, yc - x * yr / r, xc, col, fillp);
        }

        public void draw_ovalfill_mask(int xc, int yc, int xr, int yr, int col, int fillp, int mask)
        {
            int r = Math.Max(xr, yr);
            if (r <= 0)
                return;
            if (is_bbox_offscreen(xc - xr, yc - yr, xc + xr, yc + yr)) return;
            int x = 0, y = Math.Abs(r);
            int d = 3 - 2 * Math.Abs(r);

            draw_ovalfill_segment(xc, yc, x, y, r, xr, yr, col, fillp, mask);

            while (y >= x)
            {
                x++;

                if (d > 0)
                {
                    y--;
                    d = d + 4 * (x - y) + 10;
                }
                else
                    d = d + 4 * x + 6;

                draw_ovalfill_segment(xc, yc, x, y, r, xr, yr, col, fillp, mask);
            }
        }

        public void draw_circfill_mask(int xc, int yc, int r, int col, int fillp, int mask)
        {
            draw_ovalfill_mask(xc, yc, r, r, col, fillp, mask);
        }

        public void draw_circfill(int xc, int yc, int r, int col, int fillp)
        {
            draw_circfill_mask(xc, yc, r, col, fillp, 0xff);
        }

        public void draw_rect(int x0, int y0, int x1, int y1, int col, int fillp)
        {
            if (is_bbox_offscreen(x0, y0, x1, y1)) return;
            draw_hline(x0, y0, x1, col, fillp);
            draw_hline(x0, y1, x1, col, fillp);
            draw_vline(x0, y0, y1, col, fillp);
            draw_vline(x1, y0, y1, col, fillp);
        }

        public void draw_rectfill(int x0, int y0, int x1, int y1, int col, int fillp)
        {
            for (int x = x0; x <= x1; x++)
                for (int y = y0; y <= y1; y++)
                    pixel_set(x, y, col, fillp, DrawType.Graphic);
        }

        public void draw_oval(int x0, int y0, int x1, int y1, int col, int fillp)
        {
            int x = (x0 + x1) / 2;
            int y = (y0 + y1) / 2;
            int xr = (x1 - x0) / 2;
            int yr = (y1 - y0) / 2;
            draw_oval_mask(x, y, xr, yr, col, fillp, 0xff);
        }

        public void draw_ovalfill(int x0, int y0, int x1, int y1, int col, int fillp)
        {
            int x = (x0 + x1) / 2;
            int y = (y0 + y1) / 2;
            int xr = (x1 - x0) / 2;
            int yr = (y1 - y0) / 2;
            draw_ovalfill_mask(x, y, xr, yr, col, fillp, 0xff);
        }

        public void pixel_set(int x, int y, int c, int fillp, DrawType draw_type)
        {
            int cx, cy;
            camera_get(out cx, out cy);
            x -= cx;
            y -= cy;
            int x0, y0, x1, y1;
            clip_get(out x0, out y0, out x1, out y1);

            if (x >= x0 && x < x1 && y >= y0 && y < y1)
            {
                bool fillp_sprites, fillp_graphics_secondary, transparency, invert;
                if ((c & 0x1000) != 0)
                {
                    transparency = (c & 0x100) != 0;
                    fillp_sprites = (c & 0x200) != 0;
                    fillp_graphics_secondary = (c & 0x400) != 0;
                    invert = (c & 0x800) != 0;
                }
                else
                {
                    fillp = m_memory[MEMORY_FILLP] | (m_memory[MEMORY_FILLP + 1] << 8);
                    transparency = (m_memory[MEMORY_FILLP_ATTR] & 1) != 0;
                    fillp_sprites = (m_memory[MEMORY_FILLP_ATTR] & 2) != 0;
                    fillp_graphics_secondary = (m_memory[MEMORY_FILLP_ATTR] & 4) != 0;
                    invert = false;
                }
                int bit = ((3 - y) & 0x3) * 4 + ((3 - x) & 0x3);
                bool on = (fillp & (1 << bit)) != 0;
                if (invert) on = !on;
                bool use_fillp = (draw_type == DrawType.Graphic) || (draw_type == DrawType.Sprite && fillp_sprites);
                bool use_secondary_palette = (draw_type == DrawType.Sprite && fillp_sprites) || (draw_type == DrawType.Graphic && fillp_graphics_secondary);
                if (!use_fillp || (use_fillp && !transparency) || !on)
                {
                    byte col;
                    if (use_secondary_palette)
                    {
                        byte col_draw = color_get(PaletteType.Draw, c);
                        byte col_secondary = color_get(PaletteType.Secondary, col_draw);
                        if (on)
                            col = (byte)((col_secondary >> 4) & 0xf);
                        else
                            col = (byte)(col_secondary & 0xf);
                    }
                    else
                    {
                        if (use_fillp)
                        {
                            if (on)
                                c = (c >> 4) & 0xf;
                            else
                                c = c & 0xf;
                        }
                        col = color_get(PaletteType.Draw, c);
                    }
                    gfx_set(x, y, MEMORY_SCREEN, MEMORY_SCREEN_SIZE, col);
                }
            }
        }

        public void draw_scaled_sprite(int sx, int sy, int sw, int sh, int dx, int dy, float scale_x, float scale_y, bool flip_x, bool flip_y)
        {
            int dw = (int)Math.Round(sw * scale_x);
            int dh = (int)Math.Round(sh * scale_y);

            if (dw <= 0 || dh <= 0)
                return;

            for (int y = 0; y < dh; y++)
            {
                for (int x = 0; x < dw; x++)
                {
                    int src_x = sx + (flip_x ? (sw - 1 - (x * sw) / dw) : (x * sw) / dw);
                    int src_y = sy + (flip_y ? (sh - 1 - (y * sh) / dh) : (y * sh) / dh);
                    byte index = gfx_get(src_x, src_y, MEMORY_SPRITES, MEMORY_SPRITES_SIZE);
                    byte color = color_get(PaletteType.Draw, (int)index);

                    if ((color & 0xf0) == 0)
                        pixel_set(dx + x, dy + y, index, 0, DrawType.Sprite);
                }
            }
        }

        public void draw_sprite(int n, int left, int top, bool flip_x = false, bool flip_y = false)
        {
            int sx = (n & 0xF) * SPRITE_WIDTH;
            int sy = (n >> 4) * SPRITE_HEIGHT;

            for (int y = 0; y < SPRITE_HEIGHT; y++)
            {
                for (int x = 0; x < SPRITE_WIDTH; x++)
                {
                    int fx = flip_x ? (SPRITE_WIDTH - x - 1) : x;
                    int fy = flip_y ? (SPRITE_HEIGHT - y - 1) : y;
                    byte index = gfx_get(sx + x, sy + y, MEMORY_SPRITES, MEMORY_SPRITES_SIZE);
                    byte color = color_get(PaletteType.Draw, index);

                    if ((color & 0xf0) == 0)
                        pixel_set(left + fx, top + fy, index, 0, DrawType.Sprite);
                }
            }
        }

        public void draw_sprites(int n, int x, int y, int w, int h, bool flip_x, bool flip_y)
        {
            for (int ty = 0; ty < h; ty++)
            {
                for (int tx = 0; tx < w; tx++)
                {
                    int idx = (n + tx + ty * 16) & 0xFF;

                    int dstX = x + (flip_x ? (w - 1 - tx) : tx) * 8;
                    int dstY = y + (flip_y ? (h - 1 - ty) : ty) * 8;

                    draw_sprite(idx, dstX, dstY, flip_x, flip_y);
                }
            }
        }

        public void draw_char(int n, int left, int top, int col)
        {
            int sx = n % 16 * 8;
            int sy = n / 16 * 8;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    byte index = gfx_addr_get(sx + x, sy + y, m_font, 0, m_font.Length);
                    if (index == 7)
                        pixel_set(left + x, top + y, col, 0, DrawType.Default);
                }
            }
        }

        private int get_p8_symbol(byte[] strBytes, int offset, int str_len, out int symbol_length)
        {
            for (int i = 0; i < m_p8Symbols.Length; i++)
            {
                var sym = m_p8Symbols[i];
                symbol_length = sym.length;

                if (symbol_length > str_len)
                    continue;

                bool match = true;
                for (int k = 0; k < symbol_length; k++)
                {
                    if (strBytes[offset + k] != sym.encoding[k]) { match = false; break; }
                }

                if (match)
                    return sym.index;
            }

            symbol_length = 0;
            return -1;
        }

        // Simple form used internally; returns the advanced X position.
        public int draw_text(string str, int x, int y, int col)
        {
            int outX, outY, outRight;
            draw_text_full(str, x, y, col, x, false, out outX, out outY, out outRight);
            return outX - x;
        }

        private static int hexy(int c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'z') return c - 'a' + 10;
            if (c >= 'A' && c <= 'Z') return c - 'A' + 10;
            return c;
        }

        private struct PrintState
        {
            public bool wide;
            public bool tall;
            public bool stripey;
            public bool invert;
            public bool border;
            public bool solid_bg;
            public int char_w;
            public int char_h;
            public int char_w2;
            public bool outline_enabled;
            public int outline_colour;
            public int outline_mask;
            public bool outline_skip_interior;
            public bool use_custom_font;
        }

        private bool get_font_pixel(int char_index, int x, int y)
        {
            int sx = (char_index % 16) * 8;
            int sy = (char_index / 16) * 8;
            byte index = gfx_addr_get(sx + x, sy + y, m_font, 0, m_font.Length);
            return index == 7;
        }

        private void draw_char_styled(int n, int left, int top, int fg, int bg, ref PrintState state)
        {
            int scale_x = state.wide ? 2 : 1;
            int scale_y = state.tall ? 2 : 1;

            int render_width = (n >= 0x80 ? state.char_w2 : state.char_w);
            int render_height = state.char_h;
            if (render_width > 8) render_width = 8;
            if (render_height > 8) render_height = 8;

            int[] outline_dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] outline_dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

            for (int y = 0; y < render_height; y++)
            {
                for (int x = 0; x < render_width; x++)
                {
                    bool is_fg = get_font_pixel(n, x, y);
                    if (state.invert) is_fg = !is_fg;

                    int colour = is_fg ? fg : bg;
                    if (colour == -1 && !is_fg) continue;

                    if (state.outline_enabled && is_fg)
                    {
                        for (int nb = 0; nb < 8; nb++)
                        {
                            if ((state.outline_mask & (1 << nb)) == 0) continue;
                            int nx = x + outline_dx[nb];
                            int ny = y + outline_dy[nb];
                            bool neighbour_is_fg = false;
                            if (nx >= 0 && nx < 8 && ny >= 0 && ny < 8)
                            {
                                neighbour_is_fg = get_font_pixel(n, nx, ny);
                                if (state.invert) neighbour_is_fg = !neighbour_is_fg;
                            }
                            if (!neighbour_is_fg)
                            {
                                for (int dy = 0; dy < scale_y; dy++)
                                {
                                    for (int dx = 0; dx < scale_x; dx++)
                                    {
                                        if (state.stripey && ((dx + dy) & 1) == 0) continue;
                                        int px = left + x * scale_x + dx + outline_dx[nb];
                                        int py = top + y * scale_y + dy + outline_dy[nb];
                                        pixel_set(px, py, state.outline_colour, 0, DrawType.Default);
                                    }
                                }
                            }
                        }
                    }

                    if (!state.outline_skip_interior || !is_fg)
                    {
                        for (int dy = 0; dy < scale_y; dy++)
                        {
                            for (int dx = 0; dx < scale_x; dx++)
                            {
                                if (state.stripey && ((dx + dy) & 1) == 0) continue;
                                int px = left + x * scale_x + dx;
                                int py = top + y * scale_y + dy;
                                pixel_set(px, py, colour, 0, DrawType.Default);
                            }
                        }
                    }
                }
            }
        }

        // Full P8SCII-aware text drawing. Mirrors femto8's draw_text in p8_print_helper.h.
        public void draw_text_full(string str, int x, int y, int col, int left_margin, bool add_newline,
                                   out int out_left, out int out_top, out int out_right)
        {
            if (str == null) str = string.Empty;

            byte[] bytes = Encoding.GetEncoding(28591).GetBytes(str);
            int str_len = bytes.Length;

            int right = x;
            int repeat = 1;
            int bg = 0;
            int fg = col;
            int tab_width = 16;
            int wrap_boundary = 128;
            bool wrap_enabled = (m_memory[MEMORY_MISCFLAGS] & 0x80) != 0;
            int last_advance = GLYPH_WIDTH;
            int home_x = x;
            int home_y = y;

            byte text_attrs = m_memory[MEMORY_TEXT_ATTRS];
            bool use_defaults = (text_attrs & 0x1) != 0;

            PrintState state = new PrintState
            {
                wide = use_defaults && (text_attrs & 0x4) != 0,
                tall = use_defaults && (text_attrs & 0x8) != 0,
                stripey = use_defaults && (text_attrs & 0x40) != 0,
                invert = use_defaults && (text_attrs & 0x20) != 0,
                border = use_defaults ? ((text_attrs & 0x2) != 0) : true,
                solid_bg = use_defaults && (text_attrs & 0x10) != 0,
                char_w = GLYPH_WIDTH,
                char_h = GLYPH_HEIGHT,
                char_w2 = GLYPH_WIDTH * 2,
                outline_enabled = false,
                outline_colour = 0,
                outline_mask = 0,
                outline_skip_interior = false,
                use_custom_font = use_defaults && (text_attrs & 0x80) != 0,
            };

            byte char_size = m_memory[MEMORY_TEXT_CHAR_SIZE];
            byte char_size2 = m_memory[MEMORY_TEXT_CHAR_SIZE2];
            int cw = char_size & 0xf;
            int ch = (char_size >> 4) & 0xf;
            int cw2 = char_size2 & 0xf;
            int tw = (char_size2 >> 4) & 0xf;
            if (cw > 0) state.char_w = cw;
            if (cw2 > 0) state.char_w2 = cw2;
            if (ch > 0) state.char_h = ch;
            if (tw > 0) tab_width = tw;

            for (int i = 0; i < str_len; i++)
            {
                byte character = bytes[i];
                int old_repeat = repeat;
                repeat = 1;

                for (int j = 0; j < old_repeat; j++)
                {
                    if (character < 16)
                    {
                        switch (character)
                        {
                            case 0: // "\0"
                                add_newline = false;
                                str_len = 0;
                                break;
                            case 1: // "\*"
                                if (i + 1 < str_len) repeat = hexy(bytes[++i]);
                                break;
                            case 2: // "\#"
                                if (i + 1 < str_len) { bg = hexy(bytes[++i]); state.solid_bg = true; }
                                break;
                            case 3: // "\-"
                                if (i + 1 < str_len) x += hexy(bytes[++i]) - 16;
                                break;
                            case 4: // "\|"
                                if (i + 1 < str_len) y += hexy(bytes[++i]) - 16;
                                break;
                            case 5: // "\+"
                                if (i + 2 < str_len) { x += hexy(bytes[++i]) - 16; y += hexy(bytes[++i]) - 16; }
                                break;
                            case 6: // "\^"
                                if (i + 1 < str_len)
                                {
                                    byte cmd = bytes[++i];
                                    switch ((char)cmd)
                                    {
                                        case '1': case '2': case '3': case '4': case '5':
                                        case '6': case '7': case '8': case '9':
                                            // \^N — flip N times. PicoSharp doesn't support
                                            // synchronous flip within a print, so we skip.
                                            break;
                                        case 'c':
                                            if (i + 1 < str_len)
                                            {
                                                int clear_col = hexy(bytes[++i]);
                                                clear_screen(clear_col);
                                                x = 0; y = 0; home_x = 0; home_y = 0; left_margin = 0;
                                            }
                                            break;
                                        case 'd':
                                            if (i + 1 < str_len) { hexy(bytes[++i]); }
                                            break;
                                        case 'g':
                                            x = home_x; y = home_y;
                                            break;
                                        case 'h':
                                            home_x = x; home_y = y;
                                            break;
                                        case 'j':
                                            if (i + 2 < str_len) { x = hexy(bytes[++i]) * 4; y = hexy(bytes[++i]) * 4; }
                                            break;
                                        case 'r':
                                            if (i + 1 < str_len) { wrap_boundary = hexy(bytes[++i]) * 4; wrap_enabled = true; }
                                            break;
                                        case 's':
                                            if (i + 1 < str_len) { tab_width = hexy(bytes[++i]); }
                                            break;
                                        case 'x':
                                            if (i + 1 < str_len) { state.char_w = hexy(bytes[++i]); }
                                            break;
                                        case 'y':
                                            if (i + 1 < str_len) { state.char_h = hexy(bytes[++i]); }
                                            break;
                                        case 'w': state.wide = true; break;
                                        case 't': state.tall = true; break;
                                        case '=': state.stripey = true; break;
                                        case 'p': state.wide = true; state.tall = true; state.stripey = true; break;
                                        case 'i': state.invert = true; break;
                                        case 'b': state.border = !state.border; break;
                                        case '#': state.solid_bg = true; break;
                                        case '-':
                                            if (i + 1 < str_len)
                                            {
                                                byte flag = bytes[++i];
                                                switch ((char)flag)
                                                {
                                                    case 'w': state.wide = false; break;
                                                    case 't': state.tall = false; break;
                                                    case '=': state.stripey = false; break;
                                                    case 'p': state.wide = false; state.tall = false; state.stripey = false; break;
                                                    case 'i': state.invert = false; break;
                                                    case 'b': state.border = !state.border; break;
                                                    case '#': state.solid_bg = false; break;
                                                }
                                            }
                                            break;
                                        case '.':
                                        case ',':
                                            if (i + 8 < str_len)
                                            {
                                                bool respect_padding = ((char)cmd == ',');
                                                int draw_x = x + (respect_padding && state.border ? 1 : 0);
                                                int draw_y = y + (respect_padding && state.border ? 1 : 0);
                                                for (int row = 0; row < 8 && i + 1 < str_len; row++)
                                                {
                                                    byte b = bytes[++i];
                                                    for (int bit = 0; bit < 8; bit++)
                                                    {
                                                        if ((b & (1 << bit)) != 0)
                                                            pixel_set(draw_x + bit, draw_y + row, fg, 0, DrawType.Default);
                                                        else if (state.solid_bg)
                                                            pixel_set(draw_x + bit, draw_y + row, bg, 0, DrawType.Default);
                                                    }
                                                }
                                                x += 8;
                                                last_advance = 8;
                                            }
                                            break;
                                        case ':':
                                        case ';':
                                            if (i + 16 < str_len)
                                            {
                                                bool respect_padding = ((char)cmd == ';');
                                                int draw_x = x + (respect_padding && state.border ? 1 : 0);
                                                int draw_y = y + (respect_padding && state.border ? 1 : 0);
                                                for (int row = 0; row < 8; row++)
                                                {
                                                    int b = (hexy(bytes[i + row * 2 + 1]) << 4) | hexy(bytes[i + row * 2 + 2]);
                                                    for (int bit = 0; bit < 8; bit++)
                                                    {
                                                        if ((b & (1 << bit)) != 0)
                                                            pixel_set(draw_x + bit, draw_y + row, fg, 0, DrawType.Default);
                                                        else if (state.solid_bg)
                                                            pixel_set(draw_x + bit, draw_y + row, bg, 0, DrawType.Default);
                                                    }
                                                }
                                                i += 16;
                                                x += 8;
                                                last_advance = 8;
                                            }
                                            break;
                                        case '@':
                                            // \^@addrnnnn[data] — poke nnnn bytes
                                            if (i + 8 < str_len)
                                            {
                                                int addr = (hexy(bytes[i + 1]) << 12) | (hexy(bytes[i + 2]) << 8) |
                                                           (hexy(bytes[i + 3]) << 4) | hexy(bytes[i + 4]);
                                                int count = (hexy(bytes[i + 5]) << 12) | (hexy(bytes[i + 6]) << 8) |
                                                            (hexy(bytes[i + 7]) << 4) | hexy(bytes[i + 8]);
                                                i += 8;
                                                for (int k = 0; k < count && i + 1 < str_len; k++)
                                                {
                                                    byte b = bytes[++i];
                                                    if ((uint)(addr + k) < MEMORY_SIZE)
                                                        m_memory[addr + k] = b;
                                                }
                                            }
                                            break;
                                        case '!':
                                            // \^!addr[data] — poke all remaining chars to addr
                                            if (i + 4 < str_len)
                                            {
                                                int addr = (hexy(bytes[i + 1]) << 12) | (hexy(bytes[i + 2]) << 8) |
                                                           (hexy(bytes[i + 3]) << 4) | hexy(bytes[i + 4]);
                                                i += 4;
                                                int k = 0;
                                                while (i + 1 < str_len)
                                                {
                                                    byte b = bytes[++i];
                                                    if ((uint)(addr + k) < MEMORY_SIZE)
                                                        m_memory[addr + k] = b;
                                                    k++;
                                                }
                                            }
                                            break;
                                        case 'o':
                                            if (i + 3 < str_len)
                                            {
                                                byte colour_char = bytes[++i];
                                                int mask_h = hexy(bytes[++i]);
                                                int mask_l = hexy(bytes[++i]);
                                                state.outline_mask = (mask_h << 4) | mask_l;
                                                if (colour_char == '$') { state.outline_colour = fg; state.outline_skip_interior = false; }
                                                else if (colour_char == '!') { state.outline_colour = fg; state.outline_skip_interior = true; }
                                                else { state.outline_colour = hexy(colour_char); state.outline_skip_interior = false; }
                                                state.outline_enabled = state.outline_mask != 0;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case 7: // "\a" — sfx
                                if (i + 1 < str_len && hexy(bytes[i + 1]) >= 0 && hexy(bytes[i + 1]) < 16)
                                {
                                    if (i + 2 < str_len && hexy(bytes[i + 2]) >= 0 && hexy(bytes[i + 2]) < 16)
                                    {
                                        int sfx_index = (hexy(bytes[i + 1]) << 4) | hexy(bytes[i + 2]);
                                        sfx(sfx_index, -1, 0, 32);
                                        i += 2;
                                    }
                                }
                                break;
                            case 8: // "\b"
                                x -= last_advance;
                                break;
                            case 9: // "\t"
                                x = ((x / tab_width) + 1) * tab_width;
                                break;
                            case 0xa: // "\n"
                                x = left_margin;
                                y += state.char_h;
                                break;
                            case 0xb: // "\v"
                                if (i + 2 < str_len)
                                {
                                    int offset_code = hexy(bytes[++i]);
                                    byte decoration_char = bytes[++i];
                                    int offset_x = (offset_code % 4) - 2;
                                    int offset_y = (offset_code / 4) - 8;
                                    int dec_x = x - last_advance + offset_x;
                                    int dec_y = y + offset_y;
                                    if (decoration_char >= 0x20 && decoration_char < 0x7F)
                                        draw_char(decoration_char, dec_x, dec_y, fg);
                                }
                                break;
                            case 0xc: // "\f"
                                if (i + 1 < str_len)
                                {
                                    fg = hexy(bytes[++i]);
                                    if (fg > 15) fg = 16;
                                    m_memory[MEMORY_PENCOLOR] = (byte)fg;
                                }
                                break;
                            case 0xd: // "\r"
                                x = left_margin;
                                break;
                            case 0xe: // 0x0e — custom font on
                                state.use_custom_font = true;
                                break;
                            case 0xf: // 0x0f — custom font off
                                state.use_custom_font = false;
                                break;
                        }
                    }
                    else
                    {
                        int index;
                        if (character >= 0x20 && character < 0x7F)
                        {
                            index = character;
                        }
                        else
                        {
                            int sym_len;
                            index = get_p8_symbol(bytes, i, str_len - i, out sym_len);
                            if (index != -1)
                                i += sym_len - 1;
                            else
                                index = character;
                        }

                        if (index >= 0)
                        {
                            int base_char_width = (index >= 0x80 ? state.char_w2 : state.char_w);
                            int char_width = base_char_width * (state.wide ? 2 : 1);

                            bool use_styled = state.wide || state.tall || state.invert || !state.border ||
                                              state.outline_enabled || state.use_custom_font ||
                                              state.char_w != GLYPH_WIDTH || state.char_w2 != GLYPH_WIDTH ||
                                              state.char_h != GLYPH_HEIGHT;

                            int draw_bg = state.solid_bg ? bg : -1;
                            if (use_styled)
                            {
                                draw_char_styled(index, x, y, fg, draw_bg, ref state);
                            }
                            else if (draw_bg != -1)
                            {
                                int char_height = state.char_h * (state.tall ? 2 : 1);
                                draw_rectfill(x, y, x + char_width - 1, y + char_height - 1, draw_bg, 0);
                                draw_char(index, x, y, fg);
                            }
                            else
                            {
                                draw_char(index, x, y, fg);
                            }

                            x += char_width;
                            last_advance = char_width;

                            if (wrap_enabled && x >= wrap_boundary)
                            {
                                x = left_margin;
                                y += state.char_h;
                            }

                            if (x > right) right = x;
                        }
                    }
                }
            }

            if (add_newline)
            {
                x = left_margin;
                y += state.char_h * (state.tall ? 2 : 1);
            }

            out_left = x;
            out_top = y;
            out_right = right;
        }

        private int gfx_addr_remap(int location)
        {
            if (location == MEMORY_SPRITES)
                return m_memory[MEMORY_SPRITE_PHYS] << 8;
            else if (location == MEMORY_SCREEN)
                return m_memory[MEMORY_SCREEN_PHYS] << 8;
            else
                return location;
        }

        public byte gfx_addr_get(int x, int y, byte[] array, int location, int size)
        {
            if (x < 0 || y < 0 || x > P8_WIDTH || y > P8_HEIGHT)
                return 0;

            int offset = gfx_addr_remap(location) + (x >> 1) + y * 64;

            return (byte)(Tools.IsEven(x) ? array[offset] & 0xF : array[offset] >> 4);
        }

        public byte gfx_get(int x, int y, int location, int size)
        {
            if (x < 0 || y < 0 || x > P8_WIDTH || y > P8_HEIGHT)
                return 0;

            return gfx_addr_get(x, y, m_memory, location, size);
        }

        public byte gfx_get(int x, int y, byte[] array, int location, int size)
        {
            if (x < 0 || y < 0 || x > P8_WIDTH || y > P8_HEIGHT)
                return 0;

            return gfx_addr_get(x, y, array, location, size);
        }

        public void gfx_set(int x, int y, byte[] array, int location, int size, int col)
        {
            if (x < 0 || y < 0 || x > P8_WIDTH || y > P8_HEIGHT)
                return;
            int offset = gfx_addr_remap(location) + (x >> 1) + y * 64;

            byte color = (col == -1 ? pencolor_get() : color_get(PaletteType.Draw, col));

            byte rw_mask = m_memory[MEMORY_RW_MASK];
            if (location == MEMORY_SCREEN && rw_mask != 0xff)
            {
                byte write_mask = (byte)(rw_mask & 0xf);
                byte read_mask = (byte)((rw_mask >> 4) & 0xf);
                byte dst = (byte)(Tools.IsEven(x) ? (array[offset] & 0xf) : (array[offset] >> 4));
                byte src = (byte)(color & 0xf);
                byte result = (byte)((dst & ~write_mask) | (src & write_mask & read_mask));
                array[offset] = (byte)(Tools.IsEven(x) ? (array[offset] & 0xF0) | result : (result << 4) | (array[offset] & 0xF));
            }
            else
            {
                array[offset] = (byte)(Tools.IsEven(x) ? (array[offset] & 0xF0) | (color & 0xF) : (color << 4) | (array[offset] & 0xF));
            }
        }

        public void gfx_set(int x, int y, int location, int size, int col)
        {
            gfx_set(x, y, m_memory, location, size, col);
        }

        public void camera_get(out int x, out int y)
        {
            short cx = (short)((m_memory[MEMORY_CAMERA + 1] << 8) | m_memory[MEMORY_CAMERA]);
            short cy = (short)((m_memory[MEMORY_CAMERA + 3] << 8) | m_memory[MEMORY_CAMERA + 2]);
            x = cx;
            y = cy;
        }

        public void camera_set(int x, int y)
        {
            m_memory[MEMORY_CAMERA] = (byte)(x & 0xff);
            m_memory[MEMORY_CAMERA + 1] = (byte)(x >> 8);
            m_memory[MEMORY_CAMERA + 2] = (byte)(y & 0xff);
            m_memory[MEMORY_CAMERA + 3] = (byte)(y >> 8);
        }

        public byte pencolor_get()
        {
            return m_memory[MEMORY_PENCOLOR];
        }

        public void pencolor_set(byte col)
        {
            m_memory[MEMORY_PENCOLOR] = col;
        }

        public byte color_get(PaletteType type, int index)
        {
            if (type == PaletteType.Secondary)
                return m_memory[MEMORY_PALETTE_SECONDARY + (index & 0xf)];
            else
                return m_memory[MEMORY_PALETTES + (int)type * 16 + (index & 0xf)];
        }

        public void color_set(PaletteType type, int index, int col)
        {
            if (type == PaletteType.Secondary)
                m_memory[MEMORY_PALETTE_SECONDARY + (index & 0xf)] = (byte)col;
            else
                m_memory[MEMORY_PALETTES + (int)type * 16 + (index & 0xf)] = (byte)col;
        }

        public void clip_set(int x, int y, int w, int h)
        {
            int x1 = x + w;
            int y1 = y + h;
            if (x1 < 0) x1 = 0;
            if (y1 < 0) y1 = 0;
            m_memory[MEMORY_CLIPRECT] = (byte)x;
            m_memory[MEMORY_CLIPRECT + 1] = (byte)y;
            m_memory[MEMORY_CLIPRECT + 2] = (byte)x1;
            m_memory[MEMORY_CLIPRECT + 3] = (byte)y1;
        }

        public void clip_get(out int x0, out int y0, out int x1, out int y1)
        {
            x0 = m_memory[MEMORY_CLIPRECT];
            y0 = m_memory[MEMORY_CLIPRECT + 1];
            x1 = m_memory[MEMORY_CLIPRECT + 2];
            y1 = m_memory[MEMORY_CLIPRECT + 3];
        }

        public void cursor_get(out int x, out int y)
        {
            x = (int)m_memory[MEMORY_CURSOR];
            y = (int)m_memory[MEMORY_CURSOR + 1];
        }

        public void cursor_set(int x = 0, int y = 0, int col = -1)
        {
            m_memory[MEMORY_CURSOR] = (byte)x;
            m_memory[MEMORY_CURSOR + 1] = (byte)y;

            if (col != -1)
                pencolor_set((byte)col);
        }

        public bool is_button_set(int index, int button, bool btnp)
        {
            byte mask = (btnp ? m_buttonsp : m_buttons)[index];

            if (mask == 0)
                return false;

            switch (button)
            {
                case 0:
                    return ((mask & (byte)ButtonMask.Left) != 0);
                case 1:
                    return ((mask & (byte)ButtonMask.Right) != 0);
                case 2:
                    return ((mask & (byte)ButtonMask.Up) != 0);
                case 3:
                    return ((mask & (byte)ButtonMask.Down) != 0);
                case 4:
                    return ((mask & (byte)ButtonMask.Action1) != 0);
                case 5:
                    return ((mask & (byte)ButtonMask.Action2) != 0);
            }

            return false;
        }

        public void update_buttons(int index, int button, bool state)
        {
            byte mask = m_buttons[index];
            mask = (byte)(state ? mask | (1 << button) : mask & ~(1 << button));
            m_buttons[index] = mask;
            m_memory[MEMORY_BUTTON_STATE + index] = (byte)(mask & 0xff);
        }

        public int map_cell_addr(int celx, int cely)
        {
            if (celx < 0 || cely < 0)
                return 0;

            byte map_start = m_memory[MEMORY_MAP_START];
            if ((map_start >= 0x10 && map_start < 0x20) ||
                (map_start >= 0x30 && map_start < 0x3f))
                return 0;
            if (map_start < 0x10 ||
                (map_start >= 0x40 && map_start < 0x80))
                map_start = 0x20;
            if (cely >= 32 && map_start < 0x80)
            {
                map_start = 0x10;
                cely -= 32;
            }

            int map_width = m_memory[MEMORY_MAP_WIDTH];
            if (map_width == 0)
                map_width = 128;

            int map_base = map_start << 8;
            int offset = celx + cely * map_width;
            int address = map_base + offset;

            if (address < 0x1000 || address >= 0x10000 ||
                (address >= 0x3000 && address < 0x8000))
                return 0;

            return address;
        }

        public byte map_get(int celx, int cely)
        {
            int address = map_cell_addr(celx, cely);
            if (address == 0)
                return 0;
            return m_memory[address];
        }

        public void map_set(int celx, int cely, int snum)
        {
            int address = map_cell_addr(celx, cely);
            if (address == 0)
                return;
            m_memory[address] = (byte)snum;
        }

        public void reset_color()
        {
            for (int i = 0; i < 16; i++)
            {
                color_set(PaletteType.Draw, i, i == 0 ? i | 0x10 : i);
                color_set(PaletteType.Screen, i, i);
            }

            color_set(PaletteType.Secondary, 0, 0x00);
            color_set(PaletteType.Secondary, 1, 0x01);
            color_set(PaletteType.Secondary, 2, 0x12);
            color_set(PaletteType.Secondary, 3, 0x13);
            color_set(PaletteType.Secondary, 4, 0x24);
            color_set(PaletteType.Secondary, 5, 0x15);
            color_set(PaletteType.Secondary, 6, 0xd6);
            color_set(PaletteType.Secondary, 7, 0x67);
            color_set(PaletteType.Secondary, 8, 0x48);
            color_set(PaletteType.Secondary, 9, 0x49);
            color_set(PaletteType.Secondary, 10, 0x9a);
            color_set(PaletteType.Secondary, 11, 0x3b);
            color_set(PaletteType.Secondary, 12, 0xdc);
            color_set(PaletteType.Secondary, 13, 0x5d);
            color_set(PaletteType.Secondary, 14, 0x8e);
            color_set(PaletteType.Secondary, 15, 0xef);
        }

        public void scroll()
        {
            int x, y;
            cursor_get(out x, out y);
            int bottom = P8_HEIGHT - (GLYPH_HEIGHT + 2);
            int scrolly = Math.Max(y - bottom, 0);
            if (scrolly == 0)
                return;
            Buffer.BlockCopy(m_memory, 0x6000 + 64 * scrolly, m_memory, 0x6000, 0x2000 - 64 * scrolly);
            Array.Clear(m_memory, 0x6000 + 0x2000 - 64 * scrolly, 64 * scrolly);

            cursor_set(x, bottom, -1);
        }

        public int left_margin_get()
        {
            return m_memory[MEMORY_LEFT_MARGIN];
        }

        public void left_margin_set(int x)
        {
            m_memory[MEMORY_LEFT_MARGIN] = (byte)x;
        }
    }
}
