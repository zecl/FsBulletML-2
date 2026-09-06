window.playground = {
  _playerX: 240,
  _playerY: 600,
  _running: false,
  _dotNet: null,
  _frames: 0,
  _t0: 0,
  _lastN: -1,
  _canvas: null,
  _ctx: null,
  attach: function () {
    var c = document.getElementById("stage");
    if (!c || c.dataset.attached) return;
    c.dataset.attached = "1";
    var self = this;
    c.addEventListener("mousemove", function (e) {
      var r = c.getBoundingClientRect();
      var sx = c.width / r.width;
      var sy = c.height / r.height;
      self._playerX = (e.clientX - r.left) * sx;
      self._playerY = (e.clientY - r.top) * sy;
    });
    c.addEventListener("mouseleave", function () {
      self._playerX = 240;
      self._playerY = 600;
    });
  },
  hud: function (n, t) {
    this._frames += 1;
    if (!this._t0) this._t0 = t;
    var elapsed = t - this._t0;
    if (elapsed >= 500) {
      var fps = Math.round(this._frames * 1000 / elapsed);
      this._frames = 0;
      this._t0 = t;
      var fpsEl = document.getElementById("fps");
      if (fpsEl) fpsEl.textContent = String(fps);
    }
    var b = document.getElementById("bullet-count");
    if (b && n !== this._lastN) {
      this._lastN = n;
      b.textContent = String(n);
    }
  },
  ctx: function () {
    if (this._ctx) return this._ctx;
    var canvas = document.getElementById("stage");
    if (!canvas) return null;
    this._canvas = canvas;
    this._ctx = canvas.getContext("2d", { alpha: false });
    return this._ctx;
  },
  heapF32: function () {
    var rt = globalThis.getDotnetRuntime && globalThis.getDotnetRuntime(0);
    if (rt && typeof rt.localHeapViewF32 === "function") return rt.localHeapViewF32();
    throw new Error("WASM heap が見えない");
  },
  draw: function (packed, n) {
    var ctx = this.ctx();
    if (!ctx) return 0;
    var canvas = this._canvas;
    n = n | 0;
    ctx.fillStyle = "#101018";
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = "#66ccff";
    ctx.beginPath();
    ctx.arc(this._playerX, this._playerY, 5, 0, Math.PI * 2);
    ctx.fill();
    ctx.fillStyle = "#ffffff";
    if (n <= 0) {
      ctx.beginPath();
      ctx.arc(240, 80, 6, 0, Math.PI * 2);
      ctx.fill();
      return 0;
    }
    for (var i = 0; i < n; i++) {
      ctx.fillRect(packed[i * 2] - 2, packed[i * 2 + 1] - 2, 4, 4);
    }
    return n;
  },
  call: function (name, arg) {
    var errEl = document.getElementById("loop-error");
    if (!this._dotNet) {
      if (errEl) errEl.textContent = "まだ起動していない";
      return;
    }
    var p = arg === undefined
      ? this._dotNet.invokeMethodAsync(name)
      : this._dotNet.invokeMethodAsync(name, arg);
    p.then(function (err) {
      if (errEl && typeof err === "string") errEl.textContent = err;
    }).catch(function (err) {
      if (errEl) errEl.textContent = String(err);
    });
  },
  apply: function () {
    var sel = document.getElementById("pattern");
    if (sel) sel.value = "";
    var text = (document.getElementById("source") || {}).value || "";
    this.call("ApplySource", text);
  },
  fillPatterns: function () {
    var sel = document.getElementById("pattern");
    if (!sel || !this._dotNet) return;
    var names = this._dotNet.invokeMethod("ListPatterns");
    sel.innerHTML = "";
    var blank = document.createElement("option");
    blank.value = "";
    blank.textContent = "（XML 編集 / Open）";
    sel.appendChild(blank);
    for (var i = 0; i < names.length; i++) {
      var o = document.createElement("option");
      o.value = String(i);
      o.textContent = names[i];
      sel.appendChild(o);
    }
  },
  pick: function () {
    var sel = document.getElementById("pattern");
    var errEl = document.getElementById("loop-error");
    if (!sel || sel.value === "") return;
    if (!this._dotNet) {
      if (errEl) errEl.textContent = "まだ起動していない";
      return;
    }
    var i = Number(sel.value);
    var self = this;
    this._dotNet.invokeMethodAsync("SelectPattern", i).then(function (xml) {
      if (typeof xml === "string" && xml.indexOf("ERROR:") === 0) {
        if (errEl) errEl.textContent = xml.slice(6);
        return;
      }
      var source = document.getElementById("source");
      if (source) source.value = xml;
      if (errEl) errEl.textContent = "";
    }).catch(function (err) {
      if (errEl) errEl.textContent = String(err);
    });
  },
  open: function () {
    var input = document.getElementById("open-file");
    if (!input) return;
    input.value = "";
    input.click();
  },
  loadFile: function (file) {
    var errEl = document.getElementById("loop-error");
    if (!file) return;
    var reader = new FileReader();
    reader.onload = function () {
      var text = String(reader.result || "");
      var source = document.getElementById("source");
      if (source) source.value = text;
      var sel = document.getElementById("pattern");
      if (sel) sel.value = "";
      playground.apply();
    };
    reader.onerror = function () {
      if (errEl) errEl.textContent = "ファイルを読めなかった";
    };
    reader.readAsText(file);
  },
  onReady: function (dotNet) {
    this._dotNet = dotNet;
    this.attach();
    try {
      this.fillPatterns();
    } catch (err) {
      var fillErr = document.getElementById("loop-error");
      if (fillErr) fillErr.textContent = String(err);
    }
    var errEl = document.getElementById("loop-error");
    if (errEl && errEl.textContent === "起動待ち") errEl.textContent = "";
    if (this._running) return;
    this._running = true;
    var self = this;
    var loop = function (t) {
      if (!self._running) return;
      requestAnimationFrame(loop);
      try {
        var ret = self._dotNet.invokeMethod("StepFrame", t, self._playerX, self._playerY);
        var n = ret[0] | 0;
        var packed = null;
        if (n > 0) {
          var off = (ret[1] / 4) | 0;
          packed = self.heapF32().subarray(off, off + n * 2);
        }
        self.draw(packed, n);
        self.hud(n, t);
      } catch (err) {
        console.error(err);
        var e = document.getElementById("loop-error");
        if (e) e.textContent = String(err);
      }
    };
    requestAnimationFrame(loop);
  }
};

window.playground.attach();
window.playground.draw([]);
(function () {
  var input = document.getElementById("open-file");
  if (!input) return;
  input.addEventListener("change", function () {
    var file = input.files && input.files[0];
    playground.loadFile(file);
  });
})();
