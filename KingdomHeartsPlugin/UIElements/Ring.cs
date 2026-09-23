using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin.UIElements
{
    internal class Ring : IDisposable
    {
        public Ring(string imagePath, float colorR = 1f, float colorG = 1f, float colorB = 1f, float alpha = 1f)
        {
            ImagePath = imagePath;
            Color = new Vector3(colorR, colorG, colorB);
            Alpha = alpha;
        }

        public void Draw(ImDrawListPtr drawList, float percent, Vector2 position, int segments, float scale = 1f)
        {
            if (percent <= 0f) return;

            segments = Math.Clamp(segments, 1, 4);
            percent = Math.Clamp(percent, 0.002f, 1f);

            var textureWrap = Image?.GetWrapOrEmpty();
            if (textureWrap == null || textureWrap.Handle == IntPtr.Zero) return;

            float size = 256f * scale;
            float halfSize = size * 0.5f;
            Vector2 center = position + new Vector2(halfSize, halfSize);
            uint color = ImGui.GetColorU32(new Vector4(Color.X, Color.Y, Color.Z, Alpha));

            // Increased bleed to match halfSize to prevent cutoff when the texture rotates bounds
            float bleed = halfSize;

            // Quadrant clip boundaries defined around origin (Top-Left, Top-Right, Bottom-Right, Bottom-Left)
            Span<Vector4> quadrantClips = stackalloc Vector4[4]
            {
                new(position.X - bleed, position.Y - bleed, position.X + halfSize + 0.5f, position.Y + halfSize + 0.5f),
                new(position.X + halfSize - 0.5f, position.Y - bleed, position.X + size + bleed, position.Y + halfSize + 0.5f),
                new(position.X + halfSize - 0.5f, position.Y + halfSize - 0.5f, position.X + size + bleed, position.Y + size + bleed),
                new(position.X - bleed, position.Y + halfSize - 0.5f, position.X + halfSize + 0.5f, position.Y + size + bleed)
            };

            const float stepPerSegment = 0.25f;
            float totalScaledPercent = percent * stepPerSegment * segments;

            for (int i = 0; i < segments; i++)
            {
                float minThreshold = i * stepPerSegment;
                if (totalScaledPercent < minThreshold) break;

                float maxThreshold = (i + 1) * stepPerSegment;
                float clampedProgress = Math.Min(Math.Max(totalScaledPercent, minThreshold), maxThreshold);
                float angle = (-0.25f + clampedProgress) * MathF.PI * 2f;

                Vector4 clip = quadrantClips[i];
                
                // Set the third parameter to false so that it explicitly bypasses ImGui's window bounding box
                drawList.PushClipRect(new Vector2(clip.X, clip.Y), new Vector2(clip.Z, clip.W), false);

                ImageDrawing.ImageRotated(
                    drawList,
                    textureWrap.Handle,
                    center,
                    new Vector2(size, size),
                    angle,
                    color
                );

                drawList.PopClipRect();
            }
        }

        public void Dispose()
        {
        }

        private ISharedImmediateTexture Image => ImageDrawing.GetSharedTexture(ImagePath);
        private string ImagePath { get; }
        internal Vector3 Color { get; set; }
        internal float Alpha { get; set; }
    }
}