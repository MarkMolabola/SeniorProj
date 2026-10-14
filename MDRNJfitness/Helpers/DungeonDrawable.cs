using MDRNJ.Shared.Game;
using MDRNJfitness.Helpers.Sprites;

namespace MDRNJfitness.Helpers
{
    /// <summary>
    /// The Home tab's dungeon loop: a monster walks in, the hero lunges and slashes,
    /// the monster fades out and drops a coin, then the next one comes. Everything is
    /// laid out in "logical" pixels and scaled by a whole number so it stays crisp.
    /// Advance <see cref="Tick"/> every ~120 ms.
    /// </summary>
    public class DungeonDrawable : IDrawable
    {
        // Timeline, in ticks, of one fight
        private const int EnterEnd = 5;
        private const int WindUp = 6;
        private const int Lunge = 8;
        private const int Hit = 9;
        private const int Recover = 11;
        private const int DeathEnd = 15;
        private const int CoinEnd = 20;
        private const int CycleLength = 28;

        private const int FloorHeight = 12;
        private const int MonsterPixel = 2; // monsters are 16×16 art drawn at 2× next to the 32×32 hero

        private static readonly Color Wall = Color.FromArgb("#15122B");
        private static readonly Color Mortar = Color.FromArgb("#221D45");
        private static readonly Color Floor = Color.FromArgb("#322C62");
        private static readonly Color FloorLine = Color.FromArgb("#262050");
        private static readonly Color Ink = Color.FromArgb("#0B0918");
        private static readonly Color TorchWood = Color.FromArgb("#5A4130");
        private static readonly Color FlameOuter = Color.FromArgb("#FF9D57");
        private static readonly Color FlameInner = Color.FromArgb("#FFC94A");

        private readonly PixelAvatarDrawable _hero = new();
        private readonly Random _random = new();
        private string _currentMonster = "slime";
        private int _cycle;

        public int Tick { get; set; }

        /// <summary>Toughest monster the hero faces (DungeonRules.MonsterFor); weaker ones show up too.</summary>
        public string MonsterId { get; set; } = "slime";

        public string AvatarType { get => _hero.AvatarType; set => _hero.AvatarType = value; }
        public string AvatarId { get => _hero.AvatarId; set => _hero.AvatarId = value; }
        public IReadOnlyList<string> Gear { get => _hero.Gear; set => _hero.Gear = value; }

        /// <summary>Moves the animation on one step; picks the next monster at the start of each fight.</summary>
        public void Advance()
        {
            Tick = (Tick + 1) % CycleLength;
            if (Tick == 0)
            {
                _cycle++;
                _currentMonster = PickMonster();
            }
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.Antialias = false;
            var s = MathF.Max(2, MathF.Floor(dirtyRect.Height / 50));
            var w = dirtyRect.Width / s;
            var h = dirtyRect.Height / s;
            var floorY = MathF.Floor(h - FloorHeight);

            DrawBackground(canvas, dirtyRect, s, w, h, floorY);

            // Hero (left) and monster (right) stand on the floor
            var heroX = MathF.Floor(w * 0.24f - 16);
            var monsterHome = MathF.Floor(w * 0.66f - 16);
            var monsterY = floorY + 2 - DungeonSprites.MonsterSize * MonsterPixel;
            if (_currentMonster == "bat")
                monsterY -= 10 + (Tick / 2 % 2) * 2; // flies and bobs

            var heroDx = Tick switch
            {
                WindUp or WindUp + 1 => -2,
                >= Lunge and < Recover => MathF.Max(6, monsterHome - heroX - 22),
                _ => 0
            };
            _hero.Frame = Tick is >= WindUp and < Recover ? 0 : Tick / 4 % 2;
            _hero.Draw(canvas, new RectF((heroX + heroDx) * s, (floorY + 2 - 32) * s, 32 * s, 32 * s));

            // Monster: walks in, flashes when hit, fades out
            if (Tick < DeathEnd)
            {
                var monsterX = Tick <= EnterEnd
                    ? MathF.Floor(Lerp(w + 4, monsterHome, Tick / (float)EnterEnd))
                    : monsterHome;
                var alpha = Tick < Recover ? 1f : 1f - (Tick - Recover) / (float)(DeathEnd - Recover);
                var flash = Tick is Hit or Hit + 1 ? Colors.White : null;
                DrawSprite(canvas, DungeonSprites.Monster(_currentMonster), monsterX, monsterY + (Tick >= Recover ? Tick - Recover : 0),
                           MonsterPixel, s, flash, alpha);
            }

            // Slash across the monster
            if (Tick is Hit or Hit + 1)
            {
                var left = monsterHome + 4;
                var top = monsterY + 4;
                canvas.StrokeColor = Colors.White;
                canvas.StrokeSize = s;
                canvas.DrawLine(left * s, top * s, (left + 24) * s, (top + 24) * s);
                if (Tick == Hit + 1)
                    canvas.DrawLine((left + 24) * s, top * s, left * s, (top + 24) * s);
            }

            // Coin pops out and floats up
            if (Tick is >= Recover and < CoinEnd)
            {
                var rise = (Tick - Recover) * 2;
                var alpha = Tick < CoinEnd - 3 ? 1f : (CoinEnd - Tick) / 3f;
                DrawSprite(canvas, DungeonSprites.Coin, monsterHome + 10, floorY - 26 - rise, 2, s, null, alpha);
            }
        }

        private void DrawBackground(ICanvas canvas, RectF area, float s, float w, float h, float floorY)
        {
            canvas.FillColor = Wall;
            canvas.FillRectangle(area);

            // Brick wall: 16×8 bricks, every other row offset by half a brick
            canvas.FillColor = Mortar;
            for (var row = 0; row * 8 < floorY; row++)
            {
                var y = row * 8;
                canvas.FillRectangle(0, y * s, area.Width, s);
                for (var x = row % 2 == 0 ? 0 : 8; x < w; x += 16)
                    canvas.FillRectangle(x * s, y * s, s, 8 * s);
            }

            // Torches flicker on both walls
            var flicker = Tick / 2 % 2 == 0;
            foreach (var torchX in new[] { MathF.Floor(w * 0.06f), MathF.Floor(w * 0.94f) - 4 })
            {
                var top = floorY - 34;
                canvas.FillColor = TorchWood;
                canvas.FillRectangle((torchX + 1) * s, (top + 6) * s, 2 * s, 8 * s);
                canvas.FillColor = FlameOuter;
                canvas.FillRectangle(torchX * s, (top + (flicker ? 1 : 2)) * s, 4 * s, (flicker ? 5 : 4) * s);
                canvas.FillColor = FlameInner;
                canvas.FillRectangle((torchX + 1) * s, (top + 3) * s, 2 * s, 3 * s);
            }

            // Floor with a dark edge and tile seams
            canvas.FillColor = Floor;
            canvas.FillRectangle(0, floorY * s, area.Width, (h - floorY) * s + s);
            canvas.FillColor = Ink;
            canvas.FillRectangle(0, floorY * s, area.Width, s);
            canvas.FillColor = FloorLine;
            for (var x = 8; x < w; x += 24)
                canvas.FillRectangle(x * s, (floorY + 1) * s, s, (h - floorY) * s);
        }

        /// <summary>Draws a sprite at logical (x, y), each art pixel <paramref name="pixel"/> logical pixels big.</summary>
        private static void DrawSprite(ICanvas canvas, SpriteLayer layer, float x, float y, int pixel, float s, Color? solid, float alpha)
        {
            if (alpha <= 0) return;
            canvas.SaveState();
            canvas.Alpha = Math.Clamp(alpha, 0, 1);
            var size = pixel * s;
            for (var row = 0; row < layer.Height; row++)
            {
                var line = layer.Rows[row];
                for (var col = 0; col < line.Length; col++)
                {
                    var ch = line[col];
                    if (ch == '.' || !layer.Palette.TryGetValue(ch, out var color)) continue;
                    canvas.FillColor = solid ?? color;
                    canvas.FillRectangle(x * s + col * size, y * s + row * size, size, size);
                }
            }
            canvas.RestoreState();
        }

        /// <summary>Mostly the toughest monster the hero can face, sometimes a weaker one.</summary>
        private string PickMonster()
        {
            var unlocked = DungeonRules.Monsters.TakeWhile(m => m.Id != MonsterId).Count() + 1;
            unlocked = Math.Min(unlocked, DungeonRules.Monsters.Count);
            var index = _cycle % 3 == 2 ? _random.Next(unlocked) : unlocked - 1;
            return DungeonRules.Monsters[index].Id;
        }

        private static float Lerp(float from, float to, float t) => from + (to - from) * Math.Clamp(t, 0, 1);

        /// <summary>Call when the hero's toughest monster changes, so the next fight uses it.</summary>
        public void ResetMonster() => _currentMonster = PickMonster();
    }
}
