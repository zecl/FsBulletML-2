namespace FsBulletML2

/// 走行の記録の受け口。既定は ignore。面ごと・弾ごとの付け替えは呼ぶ側の仕事。
module NodeTrace =

    /// そのコマに通ったノード。型が 2 つ あるので obj。
    let mutable visit: obj -> unit = ignore

    /// 止まったノードと祖先。再開点ではない。呼び出しは 0 か 2 以上で、1 は出ない。
    let mutable stop: obj -> unit = ignore

/// 組む段が作ったノードと、その元の対。繋がないときは歩かない（enabled）。
module NodeOrigin =

    /// 対を作るか。既定は false。歩きそのものを飛ばす。
    let mutable enabled = false

    /// 作った物から元の物。同じ物のときは呼ばない（vanish は singleton で自己対になる）。
    let mutable pair: obj -> obj -> unit = fun _ _ -> ()
