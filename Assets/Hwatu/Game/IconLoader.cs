using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;

namespace Hwatu.Game
{
    /// <summary>
    /// game-icons SVG를 프로젝트에 미리 받아둔 TextAsset(Resources/Icons/{name}.txt)에서 읽어
    /// 런타임에 Sprite로 만든다(네트워크 0 · 동기 로드 · 메모리 캐시). 흰색 SVG라 Image.color로 틴트.
    /// 아이콘 SVG는 api.iconify.design/game-icons에서 빌드 타임에 받아 Resources/Icons에 저장해 둔다.
    /// </summary>
    public static class IconLoader
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        /// <summary>아이콘을 Image에 적용(즉시·동기). 못 찾으면 Image를 숨긴다. host는 호출부 호환용(미사용).</summary>
        public static void Apply(MonoBehaviour host, Image target, string iconName, Color color, float size)
        {
            if (target == null) { return; }
            target.rectTransform.sizeDelta = new Vector2(size, size);
            target.color = color;            // 흰색 SVG를 토큰 색으로 틴트
            target.preserveAspect = true;
            Sprite sprite = Load(iconName);
            target.sprite = sprite;
            target.enabled = sprite != null;
        }

        /// <summary>아이콘 스프라이트(캐시 → Resources TextAsset → SVG 테셀레이션).</summary>
        public static Sprite Load(string iconName)
        {
            if (_cache.TryGetValue(iconName, out Sprite cached)) { return cached; }
            Sprite sprite = null;
            var ta = Resources.Load<TextAsset>("Icons/" + iconName);
            if (ta != null) { sprite = BuildSprite(ta.text); }
            _cache[iconName] = sprite;       // 실패(null)도 캐시 — 매 프레임 재시도 방지
            return sprite;
        }

        // SVG 텍스트 → 벡터 Sprite(VectorGraphics 모듈).
        private static Sprite BuildSprite(string svg)
        {
            try
            {
                // game-icons SVG는 width/height가 "1em"(상대 단위) → VectorGraphics가 뷰포트를 못 잡아 빈 스프라이트가 된다.
                // viewBox(0 0 512 512) 기준 고정 픽셀로 치환해 테셀레이션이 되게 한다.
                svg = svg.Replace("width=\"1em\"", "width=\"512\"").Replace("height=\"1em\"", "height=\"512\"");
                SVGParser.SceneInfo scene = SVGParser.ImportSVG(new StringReader(svg));
                var options = new VectorUtils.TessellationOptions
                {
                    StepDistance = 1f,
                    MaxCordDeviation = 0.5f,
                    MaxTanAngleDeviation = 0.1f,
                    SamplingStepSize = 0.01f,
                };
                List<VectorUtils.Geometry> geoms = VectorUtils.TessellateScene(scene.Scene, options);
                return VectorUtils.BuildSprite(geoms, 128f, VectorUtils.Alignment.Center, Vector2.zero, 128);
            }
            catch
            {
                return null;
            }
        }
    }
}
