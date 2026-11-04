"""One-off font step. You only need this to change the hero name or the embedded character set.

Reads the two variable fonts from Google Fonts (google/fonts: ofl/bricolagegrotesque and
ofl/cascadiamono) and writes next to this file:

    display.woff2    Bricolage Grotesque 700, ASCII
    mono.woff2       Cascadia Mono 400, ASCII plus a few symbols
    name-paths.txt   the hero name as one SVG path per glyph

Usage:
    pip install fonttools brotli uharfbuzz skia-pathops
    python make_fonts.py <BricolageGrotesque.ttf> <CascadiaMono.ttf> "Seif Hesham"
"""

import io
import sys
from pathlib import Path

import uharfbuzz as hb
from fontTools import subset
from fontTools.pens.svgPathPen import SVGPathPen
from fontTools.pens.transformPen import TransformPen
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import OverlapMode, instantiateVariableFont

OUT = Path(__file__).resolve().parent
ASCII = "".join(chr(c) for c in range(0x20, 0x7F))
MONO_EXTRA = "·→—…"  # middle dot, arrow, em dash, ellipsis
NAME_WIDTH = 760.0  # hero name width in SVG units


def write_subset(font, text, features, path):
    options = subset.Options()
    options.flavor = "woff2"
    options.layout_features = features
    options.hinting = False
    options.desubroutinize = True
    options.notdef_outline = True
    options.name_IDs = []  # Cascadia's licence reserves its name, so modified copies carry none
    options.drop_tables += ["DSIG", "meta", "STAT", "MVAR", "HVAR", "avar", "fvar", "gvar"]
    subsetter = subset.Subsetter(options)
    subsetter.populate(text=text)
    subsetter.subset(font)
    font.flavor = "woff2"
    font.save(path)
    print(f"{path.name}: {path.stat().st_size} bytes")


def name_paths(font_path, name):
    """Shape the name with kerning, then write each glyph as a path scaled to NAME_WIDTH."""
    font = instantiateVariableFont(TTFont(font_path), {"opsz": 96, "wdth": 100, "wght": 800}, overlap=OverlapMode.REMOVE)
    buffer = io.BytesIO()
    font.save(buffer)

    hb_font = hb.Font(hb.Face(buffer.getvalue()))
    run = hb.Buffer()
    run.add_str(name)
    run.guess_segment_properties()
    hb.shape(hb_font, run, {"kern": True, "liga": False})

    total = sum(p.x_advance for p in run.glyph_positions)
    scale = NAME_WIDTH / total
    order, glyphs = font.getGlyphOrder(), font.getGlyphSet()

    lines, x = [], 0
    for info, pos in zip(run.glyph_infos, run.glyph_positions):
        pen = SVGPathPen(glyphs, ntos=lambda v: f"{v:.1f}".rstrip("0").rstrip("."))
        glyphs[order[info.codepoint]].draw(
            TransformPen(pen, (scale, 0, 0, -scale, (x + pos.x_offset) * scale, 0))
        )
        if pen.getCommands():
            lines.append(pen.getCommands())
        x += pos.x_advance

    (OUT / "name-paths.txt").write_text("\n".join(lines) + "\n", encoding="ascii")
    capital = font["OS/2"].sCapHeight * scale
    print(f"name-paths.txt: {len(lines)} glyphs, width {NAME_WIDTH}, cap height {capital:.1f}")


def main():
    display_path, mono_path, name = sys.argv[1:4]

    display = instantiateVariableFont(TTFont(display_path), {"opsz": 24, "wdth": 100, "wght": 700})
    write_subset(display, ASCII, ["kern"], OUT / "display.woff2")

    mono = instantiateVariableFont(TTFont(mono_path), {"wght": 400})
    head, hmtx = mono["head"], mono["hmtx"]
    print(f"mono advance: {hmtx['a'][0] / head.unitsPerEm:.4f} em")
    write_subset(mono, ASCII + MONO_EXTRA, [], OUT / "mono.woff2")

    name_paths(display_path, name)


if __name__ == "__main__":
    main()
