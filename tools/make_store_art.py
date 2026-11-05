"""Builds the app icon, splash and Google Play graphics from the hero sprites.

    python tools/make_store_art.py [hero_file_stem]

Writes:
  store-assets/play-icon-512.png         Play Console: app icon (512x512)
  store-assets/feature-graphic-1024x500.png   Play Console: feature graphic
  MDRNJfitness/Resources/AppIcon/appiconfg.png   launcher icon foreground
  MDRNJfitness/Resources/Splash/splash.png       splash screen image

Sprites are only ever scaled by whole numbers with NEAREST so the pixels stay crisp.
"""
import os
import sys
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SPRITES = os.path.join(ROOT, 'MDRNJfitness', 'Helpers', 'Sprites', 'SpriteReferences')
FONTS = os.path.join(ROOT, 'MDRNJfitness', 'Resources', 'Fonts')
STORE = os.path.join(ROOT, 'store-assets')

# Same colours as Resources/Styles/PixelTheme.xaml
GROUND = (21, 18, 43, 255)
PANEL = (36, 32, 74, 255)
RAISED = (50, 44, 98, 255)
INK = (11, 9, 24, 255)
GOLD = (255, 201, 74, 255)
TEAL = (63, 214, 195, 255)
TEXT = (244, 241, 255, 255)


def sprite(stem):
    """Loads a hero and crops it to its visible pixels."""
    im = Image.open(os.path.join(SPRITES, stem + '.png')).convert('RGBA')
    return im.crop(im.getbbox())


def scaled(im, factor):
    return im.resize((im.width * factor, im.height * factor), Image.NEAREST)


def paste_center(canvas, im, cx, cy):
    canvas.alpha_composite(im, (cx - im.width // 2, cy - im.height // 2))


def bricks(canvas, top, bottom, size=32):
    """The dungeon wall from the Home tab, as a backdrop."""
    d = ImageDraw.Draw(canvas)
    w = size * 2
    for row, y in enumerate(range(top, bottom, size)):
        offset = (row % 2) * size
        for x in range(-w, canvas.width + w, w):
            d.rectangle([x + offset, y, x + offset + w - 5, y + size - 5], fill=PANEL)


def play_icon(hero):
    """Full-bleed square: Google Play rounds the corners itself."""
    canvas = Image.new('RGBA', (512, 512), GROUND)
    bricks(canvas, 0, 512)
    d = ImageDraw.Draw(canvas)
    d.rectangle([56, 400, 456, 416], fill=GOLD)  # floor line under the hero's feet
    factor = min(340 // hero.width, 340 // hero.height)
    big = scaled(hero, factor)
    canvas.alpha_composite(big, (256 - big.width // 2, 400 - big.height))
    return canvas


def launcher_foreground(hero):
    """Transparent 1024 canvas. Android masks adaptive icons, so keep the hero in the middle ~60%."""
    canvas = Image.new('RGBA', (1024, 1024), (0, 0, 0, 0))
    factor = min(600 // hero.width, 600 // hero.height)
    paste_center(canvas, scaled(hero, factor), 512, 512)
    return canvas


def splash(hero):
    canvas = Image.new('RGBA', (512, 512), (0, 0, 0, 0))
    factor = min(420 // hero.width, 420 // hero.height)
    paste_center(canvas, scaled(hero, factor), 256, 256)
    return canvas


def feature_graphic(lineup):
    canvas = Image.new('RGBA', (1024, 500), GROUND)
    bricks(canvas, 0, 500)
    d = ImageDraw.Draw(canvas)
    title = ImageFont.truetype(os.path.join(FONTS, 'PressStart2P-Regular.ttf'), 48)
    tag = ImageFont.truetype(os.path.join(FONTS, 'VT323-Regular.ttf'), 52)

    d.rectangle([0, 0, 1024, 196], fill=GROUND)
    d.rectangle([0, 196, 1024, 203], fill=GOLD)
    for text, font, y, colour in (('MDRNJ FITNESS', title, 50, GOLD),
                                  ('TURN WORKOUTS INTO XP', tag, 122, TEXT)):
        width = d.textlength(text, font=font)
        x = (1024 - width) / 2
        d.text((x + 4, y + 4), text, font=font, fill=INK)
        d.text((x, y), text, font=font, fill=colour)

    floor = 462
    d.rectangle([0, floor, 1024, 500], fill=INK)
    d.rectangle([0, floor, 1024, floor + 6], fill=RAISED)
    slot = 1024 // len(lineup)
    for i, stem in enumerate(lineup):
        hero = scaled(sprite(stem), 7)
        canvas.alpha_composite(hero, (slot * i + slot // 2 - hero.width // 2, floor + 4 - hero.height))
    return canvas


def main():
    stem = sys.argv[1] if len(sys.argv) > 1 else 'tennisAvatar'
    hero = sprite(stem)
    os.makedirs(STORE, exist_ok=True)
    play_icon(hero).convert('RGB').save(os.path.join(STORE, 'play-icon-512.png'))
    feature_graphic(['StarterArcher', 'StartingBuffNinja', 'tennisAvatar', 'StartingNinja']) \
        .convert('RGB').save(os.path.join(STORE, 'feature-graphic-1024x500.png'))
    launcher_foreground(hero).save(os.path.join(ROOT, 'MDRNJfitness', 'Resources', 'AppIcon', 'appiconfg.png'))
    splash(hero).save(os.path.join(ROOT, 'MDRNJfitness', 'Resources', 'Splash', 'splash.png'))
    print('done:', stem)


if __name__ == '__main__':
    main()
