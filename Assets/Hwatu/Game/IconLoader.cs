using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Unity.VectorGraphics;

namespace Hwatu.Game
{
    /// <summary>
    /// Iconify(game-icons) SVG를 런타임에 받아 Sprite로 만들어 Image에 적용한다(+ 메모리 캐시).
    /// 단색 흰색 SVG로 받아 Image.color(디자인 토큰)로 틴트 → 같은 아이콘을 여러 색으로 재사용.
    /// 크기는 호출 시 지정(DesignTokens.IconSm/Md/Lg 3규격만 사용해 일관성 유지).
    /// </summary>
    public static class IconLoader
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        /// <summary>아이콘을 Image에 적용. 캐시에 있으면 즉시, 없으면 host 코루틴으로 비동기 로드.</summary>
        public static void Apply(MonoBehaviour host, Image target, string iconName, Color color, float size)
        {
            if (target == null) { return; }
            target.rectTransform.sizeDelta = new Vector2(size, size);
            target.color = color;                 // 흰색 SVG를 토큰 색으로 틴트
            target.preserveAspect = true;
            if (_cache.TryGetValue(iconName, out Sprite cached))
            {
                target.sprite = cached;
                target.enabled = cached != null;
                return;
            }
            if (host != null && host.isActiveAndEnabled)
            {
                host.StartCoroutine(Fetch(iconName, target));
            }
        }

        private static IEnumerator Fetch(string iconName, Image target)
        {
            string url = $"https://api.iconify.design/{IconCatalog.Prefix}/{iconName}.svg?color=white";
            using (var req = UnityWebRequest.Get(url))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    _cache[iconName] = null;       // 실패도 캐시(재시도 폭주 방지)
                    yield break;
                }
                Sprite sprite = BuildSprite(req.downloadHandler.text);
                _cache[iconName] = sprite;
                if (target != null)
                {
                    target.sprite = sprite;
                    target.enabled = sprite != null;
                }
            }
        }

        // SVG 텍스트 → 벡터 Sprite(VectorGraphics 모듈).
        private static Sprite BuildSprite(string svg)
        {
            try
            {
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
