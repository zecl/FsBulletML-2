using System;
using System.Collections.Generic;
using System.Linq;
using FsBulletML2;
using FsBulletML2.Bullets.Dsl;

namespace FsBulletML2.Sample.Server.MagicOnion.Engine
{
    /// <summary>
    /// 走らせられる弾幕の一覧。同梱の 176 本 をそのまま引く。
    ///
    /// 名前で引く表 を自分で書かない。 書くと、弾幕が増えたときに
    /// 片方 だけ古びる（この repo が何度も踏んでいる形）。
    /// 出どころは <c>All.bullets</c> 1 か所。
    /// </summary>
    public static class Catalog
    {
        static readonly BulletmlInfo[] Items = All.bullets.ToArray();

        static readonly Dictionary<string, BulletmlInfo> ByName = Build();

        static Dictionary<string, BulletmlInfo> Build()
        {
            var map = new Dictionary<string, BulletmlInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var info in Items)
            {
                // 名前が重なる弾幕が在りうる。 先に出たほうを採る
                // （後 で上書きすると、一覧の順 と引ける中身がずれる）
                if (!map.ContainsKey(info.Name))
                {
                    map.Add(info.Name, info);
                }
            }

            return map;
        }

        public static int Count => Items.Length;

        public static string[] Names() => Items.Select(x => x.Name).ToArray();

        public static BulletmlInfo At(int index) => Items[index];

        /// <summary>
        /// 名前で引く。無ければ既定。
        ///
        /// 引き方が 3 通り 在る。 一覧の名前は日本語の説明文
        ///
        /// 「無かった」を黙って既定にすり替える。 サンプルなので
        /// 繋がらないより出るほうを採るが、返した名前 は <c>RoomInfo</c> に
        /// 載せる —— 何を走らせているかは client 側 で読める。
        public static BulletmlInfo Resolve(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Default;
            }

            if (ByName.TryGetValue(name, out var found))
            {
                return found;
            }

            if (int.TryParse(name, out var index) && index >= 0 && index < Items.Length)
            {
                return Items[index];
            }

            foreach (var info in Items)
            {
                if (info.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return info;
                }
            }

            return Default;
        }

        /// <summary>
        /// 既定 の弾幕。
        ///
        /// 一覧の頭 ではなく、名指し。 頭 を採ると、弾幕が 1 本
        /// 増えただけで既定 が変わる（並びは生成物なので、順 は約束されていない）。
        /// 一覧が空 のときは落とす —— 弾幕 0 本 のサーバーは、
        /// 静かに立つほうが困る。
        /// </summary>
        public static BulletmlInfo Default { get; } = ResolveDefault();

        /// <summary>既定 の弾幕の名前。丸ごと一致で引く（一部 で引くと別のものに当たる）</summary>
        public const string DefaultName = "全方位弾";

        static BulletmlInfo ResolveDefault()
        {
            if (Items.Length == 0)
            {
                throw new InvalidOperationException(
                    "同梱弾幕が 1 本 も引けなかった（All.bullets が空）");
            }

            foreach (var info in Items)
            {
                if (string.Equals(info.Name, DefaultName, StringComparison.Ordinal))
                {
                    return info;
                }
            }

            // 黙って頭 に落ちない。 名指ししたものが消えたら、
            // 既定 が別の弾幕にすり替わったことが誰にも見えない
            throw new InvalidOperationException(
                $"既定 の弾幕『{DefaultName}』が同梱 {Items.Length} 本 の中に無い");
        }
    }
}
