// ../../UnityProjects/TAKANA_Cubic/Assets/Scripts/EditorPlugin/PluginSystem/ts/model.ts
var EmptyWrapper = class {
  constructor(val) {
    this.value = val;
  }
};
var T3Time = class {
  constructor(_milli) {
    this._milli = _milli;
  }
  get milli() {
    return this._milli;
  }
  get second() {
    return this._milli / 1e3;
  }
  equals(other) {
    return other != null && this._milli === other._milli;
  }
};
var T3TimeWrapper = class {
  constructor(inner) {
    this.inner = inner;
  }
  get value() {
    return new T3Time(this.inner.value);
  }
  set value(v) {
    this.inner.value = v.milli;
  }
};
var params = /* @__PURE__ */ new Map();
function __params_init(ids, wrappers) {
  const map = /* @__PURE__ */ new Map();
  for (let i = 0; i < ids.Length; i++) {
    const raw = wrappers.get_Item(i);
    map.set(ids.get_Item(i), {
      get value() {
        return raw.value;
      },
      // PuerTS ExpressionWrap wraps number to int if corresponding C# type is object; so if not stringify, float value will be truncated.
      set value(v) {
        raw.value = typeof v === "number" ? String(v) : v;
      }
    });
  }
  params = map;
  globalThis.params = params;
}
function Param(key) {
  return function(target, propertyKey) {
    Object.defineProperty(target, propertyKey, {
      get: function() {
        return params.get(key).value;
      },
      set: function(value) {
        params.get(key).value = value;
      },
      enumerable: true,
      configurable: true
    });
  };
}

// ../../UnityProjects/TAKANA_Cubic/Assets/Scripts/EditorPlugin/PluginSystem/ts/t3/t3notes.ts
var HitType = /* @__PURE__ */ ((HitType2) => {
  HitType2[HitType2["Tap"] = 0] = "Tap";
  HitType2[HitType2["Slide"] = 1] = "Slide";
  return HitType2;
})(HitType || {});
var HitModel = class {
  constructor(_hitType, _timeJudge, _isDummy = false) {
    this._hitType = _hitType;
    this._timeJudge = _timeJudge;
    this._isDummy = _isDummy;
  }
  get hitType() {
    return this._hitType;
  }
  set hitType(v) {
    this._hitType = v;
  }
  get timeJudge() {
    return this._timeJudge;
  }
  set timeJudge(v) {
    this._timeJudge = v;
  }
  get isDummy() {
    return this._isDummy;
  }
  set isDummy(v) {
    this._isDummy = v;
  }
  get timeMin() {
    return this._timeJudge;
  }
  get timeMax() {
    return this._timeJudge;
  }
  nudge(distance) {
    this._timeJudge = new T3Time(this._timeJudge.milli + distance.milli);
  }
  toCSharp() {
    return new CS.MusicGame.Models.Note.Hit(
      new CS.T3Framework.Runtime.T3Time(this.timeJudge.milli),
      this.hitType
    );
  }
};
var HitSnapshot = class {
  constructor(raw, chart) {
    this.raw = raw;
    this.chart = chart;
    this.id = raw.id;
    this.name = raw.name;
    this.hitType = raw.hitType;
    this.timeJudge = new T3TimeWrapper(raw.timeJudge);
    this.isDummy = raw.isDummy;
  }
  get track() {
    const track = this.chart.resolveTrack(this.raw.track);
    if (track === void 0) throw new Error("Track not found");
    return track;
  }
  getRaw() {
    return this.raw;
  }
  get timeMin() {
    return new T3Time(this.raw.timeJudge.value);
  }
  get timeMax() {
    return new T3Time(this.raw.timeJudge.value);
  }
  nudge(distance) {
    this.timeJudge.value = new T3Time(
      this.timeJudge.value.milli + distance.milli
    );
  }
  getModel() {
    return new HitModel(
      this.raw.hitType.value,
      new T3Time(this.raw.timeJudge.value),
      this.raw.isDummy.value
    );
  }
};
var HoldModel = class {
  constructor(_timeJudge, _timeEnd, _isDummy = false) {
    this._timeJudge = _timeJudge;
    this._timeEnd = _timeEnd;
    this._isDummy = _isDummy;
  }
  get timeJudge() {
    return this._timeJudge;
  }
  set timeJudge(v) {
    this._timeJudge = v;
  }
  get timeEnd() {
    return this._timeEnd;
  }
  set timeEnd(v) {
    this._timeEnd = v;
  }
  get isDummy() {
    return this._isDummy;
  }
  set isDummy(v) {
    this._isDummy = v;
  }
  get timeMin() {
    return this._timeJudge;
  }
  get timeMax() {
    return this._timeEnd;
  }
  nudge(distance) {
    this._timeJudge = new T3Time(this._timeJudge.milli + distance.milli);
    this._timeEnd = new T3Time(this._timeEnd.milli + distance.milli);
  }
  toCSharp() {
    return new CS.MusicGame.Models.Note.Hold(
      new CS.T3Framework.Runtime.T3Time(this.timeJudge.milli),
      new CS.T3Framework.Runtime.T3Time(this.timeEnd.milli)
    );
  }
};
var HoldSnapshot = class {
  constructor(raw, chart) {
    this.raw = raw;
    this.chart = chart;
    this.id = raw.id;
    this.name = raw.name;
    this.timeJudge = new T3TimeWrapper(raw.timeJudge);
    this.timeEnd = new T3TimeWrapper(raw.timeEnd);
    this.isDummy = raw.isDummy;
  }
  get track() {
    const track = this.chart.resolveTrack(this.raw.track);
    if (track === void 0) throw new Error("Track not found");
    return track;
  }
  getRaw() {
    return this.raw;
  }
  get timeMin() {
    return new T3Time(this.raw.timeJudge.value);
  }
  get timeMax() {
    return new T3Time(this.raw.timeEnd.value);
  }
  nudge(distance) {
    if (distance.milli > 0) {
      this.timeEnd.value = new T3Time(
        this.timeEnd.value.milli + distance.milli
      );
      this.timeJudge.value = new T3Time(
        this.timeJudge.value.milli + distance.milli
      );
    } else {
      this.timeJudge.value = new T3Time(
        this.timeJudge.value.milli + distance.milli
      );
      this.timeEnd.value = new T3Time(
        this.timeEnd.value.milli + distance.milli
      );
    }
  }
  getModel() {
    return new HoldModel(
      new T3Time(this.raw.timeJudge.value),
      new T3Time(this.raw.timeEnd.value),
      this.raw.isDummy.value
    );
  }
};
var DraftHitModel = class extends HitModel {
  constructor(hitType, timeJudge, _position, _width, isDummy = false) {
    super(hitType, timeJudge, isDummy);
    this._position = _position;
    this._width = _width;
  }
  get position() {
    return this._position;
  }
  set position(v) {
    this._position = v;
  }
  get width() {
    return this._width;
  }
  set width(v) {
    this._width = v;
  }
  toCSharp() {
    return new CS.MusicGame.Models.Note.DraftHit(
      new CS.T3Framework.Runtime.T3Time(this.timeJudge.milli),
      this.hitType,
      this.position,
      this.width
    );
  }
};
var DraftHoldModel = class extends HoldModel {
  constructor(timeJudge, timeEnd, _position, _width, isDummy = false) {
    super(timeJudge, timeEnd, isDummy);
    this._position = _position;
    this._width = _width;
  }
  get position() {
    return this._position;
  }
  set position(v) {
    this._position = v;
  }
  get width() {
    return this._width;
  }
  set width(v) {
    this._width = v;
  }
  toCSharp() {
    return new CS.MusicGame.Models.Note.DraftHold(
      new CS.T3Framework.Runtime.T3Time(this.timeJudge.milli),
      new CS.T3Framework.Runtime.T3Time(this.timeEnd.milli),
      this.position,
      this.width
    );
  }
};
var DraftHitSnapshot = class {
  constructor(raw, chart) {
    this.raw = raw;
    this.chart = chart;
    this.id = raw.id;
    this.name = raw.name;
    this.hitType = raw.hitType;
    this.timeJudge = new T3TimeWrapper(raw.timeJudge);
    this.position = raw.position;
    this.width = raw.width;
    this.isDummy = raw.isDummy;
  }
  getRaw() {
    return this.raw;
  }
  get timeMin() {
    return new T3Time(this.raw.timeJudge.value);
  }
  get timeMax() {
    return new T3Time(this.raw.timeJudge.value);
  }
  nudge(distance) {
    this.timeJudge.value = new T3Time(
      this.timeJudge.value.milli + distance.milli
    );
  }
  getModel() {
    return new DraftHitModel(
      this.raw.hitType.value,
      new T3Time(this.raw.timeJudge.value),
      this.raw.position.value,
      this.raw.width.value,
      this.raw.isDummy.value
    );
  }
};
var DraftHoldSnapshot = class {
  constructor(raw, chart) {
    this.raw = raw;
    this.chart = chart;
    this.id = raw.id;
    this.name = raw.name;
    this.timeJudge = new T3TimeWrapper(raw.timeJudge);
    this.timeEnd = new T3TimeWrapper(raw.timeEnd);
    this.position = raw.position;
    this.width = raw.width;
    this.isDummy = raw.isDummy;
  }
  getRaw() {
    return this.raw;
  }
  get timeMin() {
    return new T3Time(this.raw.timeJudge.value);
  }
  get timeMax() {
    return new T3Time(this.raw.timeEnd.value);
  }
  nudge(distance) {
    this.timeJudge.value = new T3Time(
      this.timeJudge.value.milli + distance.milli
    );
    this.timeEnd.value = new T3Time(this.timeEnd.value.milli + distance.milli);
  }
  getModel() {
    return new DraftHoldModel(
      new T3Time(this.raw.timeJudge.value),
      new T3Time(this.raw.timeEnd.value),
      this.raw.position.value,
      this.raw.width.value,
      this.raw.isDummy.value
    );
  }
};

// ../../UnityProjects/TAKANA_Cubic/Assets/Scripts/EditorPlugin/PluginSystem/ts/t3/t3track.ts
var Eases = /* @__PURE__ */ ((Eases2) => {
  Eases2[Eases2["Unmove"] = 0] = "Unmove";
  Eases2[Eases2["Linear"] = 1] = "Linear";
  Eases2[Eases2["InSine"] = 2] = "InSine";
  Eases2[Eases2["OutSine"] = 3] = "OutSine";
  Eases2[Eases2["InOutSine"] = 4] = "InOutSine";
  Eases2[Eases2["OutInSine"] = 5] = "OutInSine";
  Eases2[Eases2["InQuad"] = 6] = "InQuad";
  Eases2[Eases2["OutQuad"] = 7] = "OutQuad";
  Eases2[Eases2["InOutQuad"] = 8] = "InOutQuad";
  Eases2[Eases2["OutInQuad"] = 9] = "OutInQuad";
  Eases2[Eases2["InCubic"] = 10] = "InCubic";
  Eases2[Eases2["OutCubic"] = 11] = "OutCubic";
  Eases2[Eases2["InOutCubic"] = 12] = "InOutCubic";
  Eases2[Eases2["OutInCubic"] = 13] = "OutInCubic";
  Eases2[Eases2["InQuart"] = 14] = "InQuart";
  Eases2[Eases2["OutQuart"] = 15] = "OutQuart";
  Eases2[Eases2["InOutQuart"] = 16] = "InOutQuart";
  Eases2[Eases2["OutInQuart"] = 17] = "OutInQuart";
  Eases2[Eases2["InQuint"] = 18] = "InQuint";
  Eases2[Eases2["OutQuint"] = 19] = "OutQuint";
  Eases2[Eases2["InOutQuint"] = 20] = "InOutQuint";
  Eases2[Eases2["OutInQuint"] = 21] = "OutInQuint";
  Eases2[Eases2["InExpo"] = 22] = "InExpo";
  Eases2[Eases2["OutExpo"] = 23] = "OutExpo";
  Eases2[Eases2["InOutExpo"] = 24] = "InOutExpo";
  Eases2[Eases2["OutInExpo"] = 25] = "OutInExpo";
  Eases2[Eases2["InCirc"] = 26] = "InCirc";
  Eases2[Eases2["OutCirc"] = 27] = "OutCirc";
  Eases2[Eases2["InOutCirc"] = 28] = "InOutCirc";
  Eases2[Eases2["OutInCirc"] = 29] = "OutInCirc";
  Eases2[Eases2["InBack"] = 30] = "InBack";
  Eases2[Eases2["OutBack"] = 31] = "OutBack";
  Eases2[Eases2["InOutBack"] = 32] = "InOutBack";
  Eases2[Eases2["OutInBack"] = 33] = "OutInBack";
  Eases2[Eases2["InElastic"] = 34] = "InElastic";
  Eases2[Eases2["OutElastic"] = 35] = "OutElastic";
  Eases2[Eases2["InOutElastic"] = 36] = "InOutElastic";
  Eases2[Eases2["OutInElastic"] = 37] = "OutInElastic";
  Eases2[Eases2["InBounce"] = 38] = "InBounce";
  Eases2[Eases2["OutBounce"] = 39] = "OutBounce";
  Eases2[Eases2["InOutBounce"] = 40] = "InOutBounce";
  Eases2[Eases2["OutInBounce"] = 41] = "OutInBounce";
  return Eases2;
})(Eases || {});
Object.freeze(Eases);
var EaseMoveItem = class _EaseMoveItem {
  constructor(position, ease) {
    this.position = position;
    this.ease = ease;
  }
  getPosition(thisTime, targetTime, nextTime, nextPosition) {
    const t = (targetTime.second - thisTime.second) / (nextTime.second - thisTime.second);
    const opposite = CS.EditorPlugin.Shared.To.RawMoveItem.opposite(this.ease);
    return CS.EditorPlugin.Shared.To.RawMoveItem.calcCoord(
      opposite,
      this.position,
      nextPosition,
      t
    );
  }
  clone() {
    return new _EaseMoveItem(this.position, this.ease);
  }
  toCSharp() {
    return new CS.MusicGame.Models.Track.Movement.V1EMoveItem(
      this.position,
      this.ease
    );
  }
};
var BezierMoveItem = class _BezierMoveItem {
  constructor(position, startTimeFactor, startPositionFactor, endTimeFactor, endPositionFactor) {
    this.position = position;
    this.startTimeFactor = startTimeFactor;
    this.startPositionFactor = startPositionFactor;
    this.endTimeFactor = endTimeFactor;
    this.endPositionFactor = endPositionFactor;
  }
  getPosition(thisTime, targetTime, nextTime, nextPosition) {
    if (thisTime.milli === nextTime.milli) return nextPosition;
    const iterationTimes = 5;
    const timeT = (targetTime.second - thisTime.second) / (nextTime.second - thisTime.second);
    let factorT = timeT;
    for (let i = 0; i < iterationTimes; i++) {
      const currentT = cubicBezier(
        0,
        this.startTimeFactor,
        this.endTimeFactor,
        1,
        factorT
      );
      const slope = cubicDerivative(
        0,
        this.startTimeFactor,
        this.endTimeFactor,
        1,
        factorT
      );
      if (Math.abs(slope) < 1e-6) break;
      factorT -= (currentT - timeT) / slope;
      factorT = Math.max(0, Math.min(1, factorT));
    }
    const positionT = cubicBezier(
      0,
      this.startPositionFactor,
      this.endPositionFactor,
      1,
      factorT
    );
    return this.position + (nextPosition - this.position) * positionT;
  }
  clone() {
    return new _BezierMoveItem(
      this.position,
      this.startTimeFactor,
      this.startPositionFactor,
      this.endTimeFactor,
      this.endPositionFactor
    );
  }
  toCSharp() {
    return new CS.MusicGame.Models.Track.Movement.V1BMoveItem(
      this.position,
      new CS.UnityEngine.Vector2(this.startTimeFactor, this.startPositionFactor),
      new CS.UnityEngine.Vector2(this.endTimeFactor, this.endPositionFactor)
    );
  }
};
function cubicBezier(start, startControl, endControl, end, t) {
  const u = 1 - t;
  return u * u * u * start + 3 * u * u * t * startControl + 3 * u * t * t * endControl + t * t * t * end;
}
function cubicDerivative(start, startControl, endControl, end, t) {
  const u = 1 - t;
  return 3 * u * u * (startControl - start) + 6 * u * t * (endControl - startControl) + 3 * t * t * (end - endControl);
}
var MoveList = class {
  constructor(items) {
    this.items = /* @__PURE__ */ new Map();
    if (items === void 0) return;
    for (const [time, item] of items) {
      this.items.set(time.milli, item);
    }
  }
  set(time, item) {
    this.items.set(time.milli, item);
    return true;
  }
  delete(time) {
    return this.items.delete(time.milli);
  }
  getPosition(time) {
    const times = [...this.items.keys()].sort((a, b) => a - b);
    if (times.length === 0) return 0;
    const first = times[0];
    if (time.milli <= first) return this.items.get(first).position;
    const last = times[times.length - 1];
    if (time.milli >= last) return this.items.get(last).position;
    let prev = first;
    for (const key of times) {
      if (key > time.milli) break;
      prev = key;
    }
    let next = first;
    for (const key of times) {
      if (key > prev) {
        next = key;
        break;
      }
    }
    return this.items.get(prev).getPosition(new T3Time(prev), time, new T3Time(next), this.items.get(next).position);
  }
  nudge(distance) {
    const entries = [...this.items.entries()];
    this.items.clear();
    for (const [time, item] of entries) {
      this.items.set(time + distance.milli, item);
    }
  }
  shift(offset) {
    for (const item of this.items.values()) item.position += offset;
  }
  toCSharp() {
    const list = CS.EditorPlugin.Shared.To.RawTrackData.NewMoveList();
    for (const [time, item] of this.items) {
      CS.EditorPlugin.Shared.To.RawTrackData.Insert(list, new CS.T3Framework.Runtime.T3Time(time), item.toCSharp());
    }
    return list;
  }
};
var TrackEdgeMovement = class {
  constructor(leftMoveList, rightMoveList) {
    this.leftMoveList = leftMoveList;
    this.rightMoveList = rightMoveList;
  }
  getPosition(time) {
    return (this.leftMoveList.getPosition(time) + this.rightMoveList.getPosition(time)) / 2;
  }
  getWidth(time) {
    return Math.abs(
      this.leftMoveList.getPosition(time) - this.rightMoveList.getPosition(time)
    );
  }
  getLeftPosition(time) {
    return this.leftMoveList.getPosition(time);
  }
  getRightPosition(time) {
    return this.rightMoveList.getPosition(time);
  }
  nudge(distance) {
    this.leftMoveList.nudge(distance);
    this.rightMoveList.nudge(distance);
  }
  shift(offset) {
    this.leftMoveList.shift(offset);
    this.rightMoveList.shift(offset);
  }
  insert(time, position, width) {
    this.leftMoveList.set(
      time,
      new EaseMoveItem(position - width / 2, 0 /* Unmove */)
    );
    this.rightMoveList.set(
      time,
      new EaseMoveItem(position + width / 2, 0 /* Unmove */)
    );
  }
  toCSharp() {
    return new CS.MusicGame.Models.Track.Movement.TrackEdgeMovement(
      this.leftMoveList.toCSharp(),
      this.rightMoveList.toCSharp()
    );
  }
};
var TrackDirectMovement = class {
  constructor(positionMoveList, widthMoveList) {
    this.positionMoveList = positionMoveList;
    this.widthMoveList = widthMoveList;
  }
  getPosition(time) {
    return this.positionMoveList.getPosition(time);
  }
  getWidth(time) {
    return this.widthMoveList.getPosition(time);
  }
  getLeftPosition(time) {
    return this.getPosition(time) - this.getWidth(time) / 2;
  }
  getRightPosition(time) {
    return this.getPosition(time) + this.getWidth(time) / 2;
  }
  nudge(distance) {
    this.positionMoveList.nudge(distance);
    this.widthMoveList.nudge(distance);
  }
  shift(offset) {
    this.positionMoveList.shift(offset);
    this.widthMoveList.shift(offset);
  }
  insert(time, position, width) {
    this.positionMoveList.set(time, new EaseMoveItem(position, 0 /* Unmove */));
    this.widthMoveList.set(time, new EaseMoveItem(width, 0 /* Unmove */));
  }
  toCSharp() {
    return new CS.MusicGame.Models.Track.Movement.TrackDirectMovement(
      this.positionMoveList.toCSharp(),
      this.widthMoveList.toCSharp()
    );
  }
};
var TrackModel = class {
  constructor(timeStart, timeEnd, movement) {
    this.timeStart = timeStart;
    this.timeEnd = timeEnd;
    this.movement = movement;
  }
  get timeMin() {
    return this.timeStart;
  }
  get timeMax() {
    return this.timeEnd;
  }
  nudge(distance) {
    this.timeStart = new T3Time(this.timeStart.milli + distance.milli);
    this.timeEnd = new T3Time(this.timeEnd.milli + distance.milli);
    this.movement.nudge(distance);
  }
  shift(offset) {
    this.movement.shift(offset);
  }
  toCSharp() {
    const track = new CS.MusicGame.Models.Track.Track(
      new CS.T3Framework.Runtime.T3Time(this.timeStart.milli),
      new CS.T3Framework.Runtime.T3Time(this.timeEnd.milli)
    );
    track.Movement = this.movement.toCSharp();
    return track;
  }
};
function createMoveItem(raw) {
  if (raw.type === "ease") return new EaseMoveItem(raw.position, raw.ease);
  return new BezierMoveItem(
    raw.position,
    raw.startTimeFactor,
    raw.startPositionFactor,
    raw.endTimeFactor,
    raw.endPositionFactor
  );
}
function createMoveList(raw, flag) {
  const list = new MoveList();
  const items = raw.getItems(flag);
  for (let i = 0; i < items.Length; i++) {
    const itemRaw = items.get_Item(i);
    list.set(new T3Time(itemRaw.time), createMoveItem(itemRaw));
  }
  return list;
}
var TrackEdgeMovementWrapper = class {
  constructor(raw) {
    this.raw = raw;
  }
  getPosition(time) {
    return this.raw.getPosition(time.milli);
  }
  getWidth(time) {
    return this.raw.getWidth(time.milli);
  }
  getLeftPosition(time) {
    return this.raw.getLeftPosition(time.milli);
  }
  getRightPosition(time) {
    return this.raw.getRightPosition(time.milli);
  }
  get(time, isLeft) {
    const raw = this.raw.getItem(time.milli, isLeft);
    return raw === null || raw === void 0 ? void 0 : createMoveItem(raw);
  }
  getModel() {
    return new TrackEdgeMovement(
      createMoveList(this.raw, true),
      createMoveList(this.raw, false)
    );
  }
  set(time, item, isLeft) {
    return this.raw.set(time.milli, item.toCSharp(), isLeft);
  }
  delete(time, isLeft) {
    return this.raw.delete(time.milli, isLeft);
  }
  insert(time, position, width) {
    this.raw.insert(time.milli, position, width);
  }
};
var TrackDirectMovementWrapper = class {
  constructor(raw) {
    this.raw = raw;
  }
  getPosition(time) {
    return this.raw.getPosition(time.milli);
  }
  getWidth(time) {
    return this.raw.getWidth(time.milli);
  }
  getLeftPosition(time) {
    return this.raw.getLeftPosition(time.milli);
  }
  getRightPosition(time) {
    return this.raw.getRightPosition(time.milli);
  }
  get(time, isPosition) {
    const raw = this.raw.getItem(time.milli, isPosition);
    return raw === null || raw === void 0 ? void 0 : createMoveItem(raw);
  }
  getModel() {
    return new TrackDirectMovement(
      createMoveList(this.raw, true),
      createMoveList(this.raw, false)
    );
  }
  set(time, item, isPosition) {
    return this.raw.set(time.milli, item.toCSharp(), isPosition);
  }
  delete(time, isPosition) {
    return this.raw.delete(time.milli, isPosition);
  }
  insert(time, position, width) {
    this.raw.insert(time.milli, position, width);
  }
};
var TrackSnapshot = class {
  constructor(raw, chart) {
    this.raw = raw;
    this.chart = chart;
    this.id = raw.id;
    this.name = raw.name;
    this.movement = raw.movement.type === "Edge" ? new TrackEdgeMovementWrapper(raw.movement) : new TrackDirectMovementWrapper(raw.movement);
  }
  get notes() {
    return this.getNotes();
  }
  *getNotes() {
    for (const note of this.chart.notes) {
      if (note.track === this) yield note;
    }
  }
  getRaw() {
    return this.raw;
  }
  get timeMin() {
    return new T3Time(this.raw.timeStart.value);
  }
  get timeMax() {
    return new T3Time(this.raw.timeEnd.value);
  }
  get layer() {
    const layer = this.raw.getLayer();
    return {
      id: layer.id,
      name: layer.name,
      color: {
        r: layer.color.r,
        g: layer.color.g,
        b: layer.color.b,
        a: layer.color.a
      },
      isDecoration: layer.isDecoration,
      isSelected: layer.isSelected
    };
  }
  setLayer(id) {
    this.raw.setLayer(id);
  }
  nudge(distance) {
    this.raw.nudge(distance.milli);
  }
  shift(offset) {
    this.raw.shift(offset);
  }
  getModel() {
    return new TrackModel(
      new T3Time(this.raw.timeStart.value),
      new T3Time(this.raw.timeEnd.value),
      this.movement.getModel()
    );
  }
};

// ../../UnityProjects/TAKANA_Cubic/Assets/Scripts/EditorPlugin/PluginSystem/ts/t3/t3chart.ts
function toArray(arr) {
  if (Array.isArray(arr)) return arr;
  const result = [];
  for (let i = 0; i < arr.Length; i++) {
    result.push(arr.get_Item(i));
  }
  return result;
}
var BpmListWrapper = class {
  constructor(raw) {
    this.raw = raw;
  }
  getFloorTime(time, gridDivision) {
    return new T3Time(this.raw.getFloorTime(time.milli, gridDivision));
  }
  getCeilTime(time, gridDivision) {
    return new T3Time(this.raw.getCeilTime(time.milli, gridDivision));
  }
  has(key) {
    return this.raw.has(key.milli);
  }
  get(key) {
    const value = this.raw.get(key.milli);
    return value === null || value === void 0 ? void 0 : value;
  }
  delete(key) {
    return this.raw.delete(key.milli);
  }
  clear() {
    this.raw.clear();
  }
  get size() {
    return this.raw.size;
  }
  set(key, value) {
    this.raw.set(key.milli, value);
    return this;
  }
  *keys() {
    for (const milli of toArray(this.raw.keys())) {
      yield new T3Time(milli);
    }
  }
  *values() {
    for (const value of toArray(this.raw.values())) {
      yield value;
    }
  }
  *entries() {
    for (const milli of toArray(this.raw.keys())) {
      yield [new T3Time(milli), this.get(new T3Time(milli))];
    }
  }
  forEach(callbackfn, thisArg) {
    for (const [key, value] of this.entries()) {
      callbackfn.call(thisArg, value, key, this);
    }
  }
  *[Symbol.iterator]() {
    yield* this.entries();
  }
  get [Symbol.toStringTag]() {
    return "Map";
  }
};
var LayersInfoWrapper = class {
  constructor(raw) {
    this.raw = raw;
  }
  get layers() {
    return toArray(this.raw.layers);
  }
  get defaultLayer() {
    return this.raw.defaultLayer;
  }
  add(layer) {
    return this.raw.add(toCSharpLayer(layer));
  }
  remove(layerId) {
    return this.raw.remove(layerId);
  }
  update(layerId, layer) {
    return this.raw.update(layerId, toCSharpLayer(layer));
  }
};
function toCSharpLayer(layer) {
  const info = new CS.MusicGame.ChartEditor.TrackLayer.LayerInfo();
  info.Name = layer.name;
  info.Color = new CS.UnityEngine.Color(
    layer.color.r,
    layer.color.g,
    layer.color.b,
    layer.color.a
  );
  info.IsDecoration = layer.isDecoration;
  info.IsSelected = layer.isSelected;
  return info;
}
var SetView = class {
  constructor(map) {
    this.map = map;
  }
  get size() {
    return this.map.size;
  }
  has(value) {
    for (const v of this.map.values()) {
      if (v === value) return true;
    }
    return false;
  }
  forEach(callbackfn, thisArg) {
    for (const v of this.map.values()) callbackfn.call(thisArg, v, v, this);
  }
  keys() {
    return this.map.values();
  }
  values() {
    return this.map.values();
  }
  *entries() {
    for (const v of this.map.values()) yield [v, v];
  }
  [Symbol.iterator]() {
    return this.map.values();
  }
  get [Symbol.toStringTag]() {
    return "Set";
  }
};
var ChartSnapshot = class {
  constructor(chartApi) {
    this.chartApi = chartApi;
    this.noteByRaw = /* @__PURE__ */ new Map();
    this.trackByRaw = /* @__PURE__ */ new Map();
    this.noteAddedListeners = [];
    this.noteRemovedListeners = [];
    this.trackAddedListeners = [];
    this.trackRemovedListeners = [];
    this.notes = new SetView(this.noteByRaw);
    this.tracks = new SetView(this.trackByRaw);
    this.bpmList = new BpmListWrapper(this.chartApi.bpmList);
    this.layersInfo = new LayersInfoWrapper(this.chartApi.layersInfo);
    this.chartApi.onNoteAdded((raw) => {
      const note = this.createNote(raw);
      this.noteByRaw.set(raw, note);
      this.fireNoteAdded(note);
    });
    this.chartApi.onNoteRemoved((raw) => {
      const note = this.noteByRaw.get(raw);
      if (note) {
        this.fireNoteRemoved(note);
        this.noteByRaw.delete(raw);
      }
    });
    this.chartApi.onTrackAdded((raw) => {
      const track = this.createTrack(raw);
      this.trackByRaw.set(raw, track);
      this.fireTrackAdded(track);
    });
    this.chartApi.onTrackRemoved((raw) => {
      const track = this.trackByRaw.get(raw);
      if (track) {
        this.fireTrackRemoved(track);
        this.trackByRaw.delete(raw);
      }
    });
    const initialNotes = this.chartApi.getAllNotes();
    for (let i = 0; i < initialNotes.Length; i++) {
      const raw = initialNotes.get_Item(i);
      this.noteByRaw.set(raw, this.createNote(raw));
    }
    const initialTracks = this.chartApi.getAllTracks();
    for (let i = 0; i < initialTracks.Length; i++) {
      const raw = initialTracks.get_Item(i);
      this.trackByRaw.set(raw, this.createTrack(raw));
    }
  }
  get offset() {
    return new T3Time(this.chartApi.offsetMilli);
  }
  getChartApi() {
    return this.chartApi;
  }
  addTrack(model, notes = []) {
    let arr = CS.System.Array.CreateInstance(puer.$typeof(CS.System.Object), notes.length);
    for (let i = 0; i < notes.length; i++) {
      arr.set_Item(i, notes[i].toCSharp());
    }
    this.chartApi.addTrack(model.toCSharp(), arr);
    return true;
  }
  addNote(model, track) {
    this.chartApi.addNote(model.toCSharp(), track.getRaw());
    return true;
  }
  addDraftNote(model) {
    this.chartApi.addDraftNote(model.toCSharp());
    return true;
  }
  removeComponent(component) {
    this.chartApi.removeComponent(component.getRaw());
  }
  resolveNote(raw) {
    return this.noteByRaw.get(raw);
  }
  resolveTrack(raw) {
    return this.trackByRaw.get(raw);
  }
  _onNoteAdded(listener) {
    this.noteAddedListeners.push(listener);
  }
  _onNoteRemoved(listener) {
    this.noteRemovedListeners.push(listener);
  }
  _onTrackAdded(listener) {
    this.trackAddedListeners.push(listener);
  }
  _onTrackRemoved(listener) {
    this.trackRemovedListeners.push(listener);
  }
  fireNoteAdded(note) {
    for (const listener of this.noteAddedListeners) listener(note);
  }
  fireNoteRemoved(note) {
    for (const listener of this.noteRemovedListeners) listener(note);
  }
  fireTrackAdded(track) {
    for (const listener of this.trackAddedListeners) listener(track);
  }
  fireTrackRemoved(track) {
    for (const listener of this.trackRemovedListeners) listener(track);
  }
  createNote(raw) {
    switch (raw.type) {
      case "Hit":
        return new HitSnapshot(raw, this);
      case "Hold":
        return new HoldSnapshot(raw, this);
      case "DraftHit":
        return new DraftHitSnapshot(raw, this);
      case "DraftHold":
        return new DraftHoldSnapshot(raw, this);
      default:
        return new HoldSnapshot(raw, this);
    }
  }
  createTrack(raw) {
    return new TrackSnapshot(raw, this);
  }
  // TODO: addNote、addTrack
};
var ChartSelectSet = class {
  constructor(api, chart) {
    this.api = api;
    this.chart = chart;
  }
  get currentSelecting() {
    const raw = this.api.getCurrentSelecting();
    if (raw === null || raw === void 0) return void 0;
    return this.resolve(raw);
  }
  get size() {
    return toArray(this.api.getAllSelected()).length;
  }
  has(value) {
    const raw = value.getRaw();
    for (const selected of toArray(this.api.getAllSelected())) {
      if (selected === raw) return true;
    }
    return false;
  }
  add(value) {
    this.api.addSelected(value.getRaw());
    return this;
  }
  delete(value) {
    const existed = this.has(value);
    this.api.removeSelected(value.getRaw());
    return existed;
  }
  clear() {
    this.api.clearSelected();
  }
  forEach(callbackfn, thisArg) {
    for (const v of this.values()) callbackfn.call(thisArg, v, v, this);
  }
  keys() {
    return this.values();
  }
  *values() {
    for (const raw of toArray(this.api.getAllSelected())) {
      const resolved = this.resolve(raw);
      if (resolved !== void 0) yield resolved;
    }
  }
  *entries() {
    for (const v of this.values()) yield [v, v];
  }
  [Symbol.iterator]() {
    return this.values();
  }
  get [Symbol.toStringTag]() {
    return "Set";
  }
  resolve(raw) {
    return this.chart.resolveNote(raw) ?? this.chart.resolveTrack(raw);
  }
};

// ../../UnityProjects/TAKANA_Cubic/Assets/Scripts/EditorPlugin/PluginSystem/ts/t3/t3nodes.ts
var TrackEdgeNode = class {
  constructor(raw, chart) {
    this.raw = raw;
    this.chart = chart;
    this.track = this.resolveTrack();
  }
  get isLeft() {
    return this.raw.isLeft;
  }
  get time() {
    return new T3Time(this.raw.time);
  }
  getNextTime() {
    return new T3Time(this.raw.getNextTime());
  }
  getModel() {
    return createMoveItem(this.raw.getMoveItem());
  }
  getRaw() {
    return this.raw;
  }
  resolveTrack() {
    const track = this.chart.resolveTrack(this.raw.track);
    if (track === void 0) throw new Error("Track not found");
    return track;
  }
};
var TrackDirectNode = class {
  constructor(raw, chart) {
    this.raw = raw;
    this.chart = chart;
    this.track = this.resolveTrack();
  }
  get isPosition() {
    return this.raw.isPosition;
  }
  get time() {
    return new T3Time(this.raw.time);
  }
  getNextTime() {
    return new T3Time(this.raw.getNextTime());
  }
  getModel() {
    return createMoveItem(this.raw.getMoveItem());
  }
  getRaw() {
    return this.raw;
  }
  resolveTrack() {
    const track = this.chart.resolveTrack(this.raw.track);
    if (track === void 0) throw new Error("Track not found");
    return track;
  }
};
var NodeDataset = class {
  constructor(api, chart) {
    this.api = api;
    this.chart = chart;
    this.nodeByRaw = /* @__PURE__ */ new Map();
    this.nodeAddedListeners = [];
    this.nodeRemovedListeners = [];
    this.nodes = new SetView(this.nodeByRaw);
    this.api.onNodeAdded((raw) => {
      const node = this.createNode(raw);
      this.nodeByRaw.set(raw, node);
      this.fireNodeAdded(node);
    });
    this.api.onNodeRemoved((raw) => {
      const node = this.nodeByRaw.get(raw);
      if (node) {
        this.fireNodeRemoved(node);
        this.nodeByRaw.delete(raw);
      }
    });
    for (const raw of toArray(this.api.getAllNodes())) {
      this.nodeByRaw.set(raw, this.createNode(raw));
    }
  }
  get size() {
    return this.nodes.size;
  }
  has(value) {
    return this.nodes.has(value);
  }
  forEach(callbackfn, thisArg) {
    this.nodes.forEach(callbackfn, thisArg);
  }
  keys() {
    return this.nodes.keys();
  }
  values() {
    return this.nodes.values();
  }
  entries() {
    return this.nodes.entries();
  }
  [Symbol.iterator]() {
    return this.nodes[Symbol.iterator]();
  }
  get [Symbol.toStringTag]() {
    return "Set";
  }
  resolveNode(raw) {
    return this.nodeByRaw.get(raw);
  }
  _onNodeAdded(listener) {
    this.nodeAddedListeners.push(listener);
  }
  _onNodeRemoved(listener) {
    this.nodeRemovedListeners.push(listener);
  }
  fireNodeAdded(node) {
    for (const listener of this.nodeAddedListeners) listener(node);
  }
  fireNodeRemoved(node) {
    for (const listener of this.nodeRemovedListeners) listener(node);
  }
  createNode(raw) {
    if (raw.type === "Edge") return new TrackEdgeNode(raw, this.chart);
    return new TrackDirectNode(raw, this.chart);
  }
};
var NodeSelectSet = class {
  constructor(api, nodes) {
    this.api = api;
    this.nodes = nodes;
  }
  get currentSelecting() {
    const raw = this.api.getCurrentSelecting();
    if (raw === null || raw === void 0) return void 0;
    return this.nodes.resolveNode(raw);
  }
  get size() {
    return toArray(this.api.getAllSelected()).length;
  }
  has(value) {
    const raw = value.getRaw();
    for (const selected of toArray(this.api.getAllSelected())) {
      if (selected === raw) return true;
    }
    return false;
  }
  add(value) {
    this.api.addSelected(value.getRaw());
    return this;
  }
  delete(value) {
    const existed = this.has(value);
    this.api.removeSelected(value.getRaw());
    return existed;
  }
  clear() {
    this.api.clearSelected();
  }
  forEach(callbackfn, thisArg) {
    for (const v of this.values()) callbackfn.call(thisArg, v, v, this);
  }
  keys() {
    return this.values();
  }
  *values() {
    for (const raw of toArray(this.api.getAllSelected())) {
      const resolved = this.nodes.resolveNode(raw);
      if (resolved !== void 0) yield resolved;
    }
  }
  *entries() {
    for (const v of this.values()) yield [v, v];
  }
  [Symbol.iterator]() {
    return this.values();
  }
  get [Symbol.toStringTag]() {
    return "Set";
  }
};

// ../../UnityProjects/TAKANA_Cubic/Assets/Scripts/EditorPlugin/PluginSystem/ts/t3/t3context.ts
function createContext(api) {
  return new T3ContextImpl(api);
}
var T3ContextImpl = class {
  constructor(api) {
    this.api = api;
    this.chart = new ChartSnapshot(api.chart);
    this.chartTime = new T3TimeWrapper(api.editor.chartTime);
    this.chartSelectDataset = new ChartSelectSet(api.chart, this.chart);
    this.nodes = new NodeDataset(api.nodes, this.chart);
    this.nodeSelectDataset = new NodeSelectSet(api.nodes, this.nodes);
    this.mouseInfoRetriever = new MouseInfoRetrieverImpl(api.mouse);
  }
  get audioLength() {
    return new T3Time(this.api.editor.audioLengthMilli);
  }
  showHeader(content, logType) {
    this.api.editor.showHeader(this.buildI18NString(content), logType);
  }
  showConfirm(content, callback) {
    this.api.editor.showConfirm(this.buildI18NString(content), callback);
  }
  showConfirmAndCancel(content, callback) {
    this.api.editor.showConfirmAndCancel(
      this.buildI18NString(content),
      callback
    );
  }
  loadChart(path) {
    const api = this.api.loadChart(path);
    if (api === null || api === void 0) return void 0;
    return new ChartSnapshot(api);
  }
  createNewChart() {
    return new ChartSnapshot(this.api.createNewChart());
  }
  saveChart(path, chart) {
    return this.api.saveChart(path, chart.getChartApi());
  }
  commit() {
    this.api.staging.commit();
  }
  buildI18NString(content) {
    var CSharpI18NString = CS.T3Framework.Runtime.I18N.I18NString;
    var Language = CS.T3Framework.Runtime.I18N.Language;
    var i18nString = new CSharpI18NString();
    if (content.en) i18nString.Add(Language.English, content.en);
    if (content.zh_Hans)
      i18nString.Add(Language.SimplifiedChinese, content.zh_Hans);
    if (content.zh_Hant)
      i18nString.Add(Language.TraditionalChinese, content.zh_Hant);
    if (content.ja) i18nString.Add(Language.Japanese, content.ja);
    return i18nString;
  }
};
var MouseInfoRetrieverImpl = class {
  constructor(api) {
    this.api = api;
  }
  getTimeStart() {
    const milli = this.api.getTimeStart();
    return milli === null || milli === void 0 ? void 0 : new T3Time(milli);
  }
  getHoldTimeEnd() {
    const milli = this.api.getHoldTimeEnd();
    return milli === null || milli === void 0 ? void 0 : new T3Time(milli);
  }
  getTrackTimeEnd() {
    const milli = this.api.getTrackTimeEnd();
    return milli === null || milli === void 0 ? void 0 : new T3Time(milli);
  }
  getWidth() {
    const width = this.api.getWidth();
    return width === null || width === void 0 ? void 0 : width;
  }
  getPosition() {
    const position = this.api.getPosition();
    return position === null || position === void 0 ? void 0 : position;
  }
  getAttachedPosition() {
    const position = this.api.getAttachedPosition();
    return position === null || position === void 0 ? void 0 : position;
  }
};

// ../../UnityProjects/TAKANA_Cubic/Assets/Scripts/EditorPlugin/PluginSystem/ts/t3/t3pluginbase.ts
var T3PluginBase = class {
  constructor() {
    this.ctx = getT3Context();
    this.ctx.chart._onNoteAdded((note) => this.onNoteAdded(note));
    this.ctx.chart._onNoteRemoved((note) => this.onNoteRemoved(note));
    this.ctx.chart._onTrackAdded((track) => this.onTrackAdded(track));
    this.ctx.chart._onTrackRemoved((track) => this.onTrackRemoved(track));
    this.ctx.nodes._onNodeAdded((node) => this.onNodeAdded(node));
    this.ctx.nodes._onNodeRemoved((node) => this.onNodeRemoved(node));
  }
  onNoteAdded(note) {
  }
  onNoteRemoved(note) {
  }
  onTrackAdded(track) {
  }
  onTrackRemoved(track) {
  }
  onNodeAdded(node) {
  }
  onNodeRemoved(node) {
  }
};

// ../../UnityProjects/TAKANA_Cubic/Assets/Scripts/EditorPlugin/PluginSystem/ts/t3/main.ts
var emptyChartApi = {
  offsetMilli: 0,
  bpmList: {
    getFloorTime: (t, d) => t,
    getCeilTime: (t, d) => t,
    has: () => false,
    get: () => null,
    delete: () => false,
    clear: () => {
    },
    set: () => {
    },
    keys: () => [],
    values: () => [],
    size: 0
  },
  layersInfo: {
    layers: [],
    defaultLayer: null,
    add: () => false,
    remove: () => false,
    update: () => false
  },
  onNoteAdded: () => {
  },
  onNoteRemoved: () => {
  },
  getAllNotes: () => [],
  onTrackAdded: () => {
  },
  onTrackRemoved: () => {
  },
  getAllTracks: () => [],
  getAllSelected: () => [],
  getCurrentSelecting: () => void 0,
  addSelected: () => {
  },
  removeSelected: () => {
  },
  clearSelected: () => {
  },
  addTrack: () => {
  },
  addNote: () => {
  },
  addDraftNote: () => {
  },
  removeComponent: () => {
  }
};
var emptyApi = {
  chart: emptyChartApi,
  staging: {
    hasPending: false,
    commit: () => {
    }
  },
  editor: {
    chartTime: new EmptyWrapper(0),
    audioLengthMilli: 0,
    showHeader: () => {
    },
    showConfirm: () => {
    },
    showConfirmAndCancel: () => {
    }
  },
  nodes: {
    getAllNodes: () => [],
    onNodeAdded: () => {
    },
    onNodeRemoved: () => {
    },
    getAllSelected: () => [],
    getCurrentSelecting: () => void 0,
    addSelected: () => {
    },
    removeSelected: () => {
    },
    clearSelected: () => {
    }
  },
  mouse: {
    getTimeStart: () => void 0,
    getHoldTimeEnd: () => void 0,
    getTrackTimeEnd: () => void 0,
    getWidth: () => void 0,
    getPosition: () => void 0,
    getAttachedPosition: () => void 0
  },
  loadChart: () => void 0,
  createNewChart: () => emptyChartApi,
  saveChart: () => false
};
Object.freeze(HitType);
globalThis.HitType = HitType;
globalThis.HitModel = HitModel;
globalThis.HoldModel = HoldModel;
globalThis.HitSnapshot = HitSnapshot;
globalThis.HoldSnapshot = HoldSnapshot;
globalThis.DraftHitModel = DraftHitModel;
globalThis.DraftHoldModel = DraftHoldModel;
globalThis.DraftHitSnapshot = DraftHitSnapshot;
globalThis.DraftHoldSnapshot = DraftHoldSnapshot;
globalThis.Eases = Eases;
globalThis.MoveList = MoveList;
globalThis.EaseMoveItem = EaseMoveItem;
globalThis.BezierMoveItem = BezierMoveItem;
globalThis.TrackEdgeMovement = TrackEdgeMovement;
globalThis.TrackEdgeMovementWrapper = TrackEdgeMovementWrapper;
globalThis.TrackDirectMovement = TrackDirectMovement;
globalThis.TrackDirectMovementWrapper = TrackDirectMovementWrapper;
globalThis.TrackModel = TrackModel;
globalThis.TrackSnapshot = TrackSnapshot;
globalThis.TrackEdgeNode = TrackEdgeNode;
globalThis.TrackDirectNode = TrackDirectNode;
globalThis.T3PluginBase = T3PluginBase;
var stubbedCtx = null;
globalThis.getT3Context = () => {
  if (stubbedCtx === null) {
    stubbedCtx = createContext(emptyApi);
  }
  return stubbedCtx;
};
function __t3_bridge_init(api) {
  const ctx = createContext(api);
  globalThis.getT3Context = () => ctx;
}

// ../../UnityProjects/TAKANA_Cubic/Assets/Scripts/EditorPlugin/PluginSystem/ts/main.ts
globalThis.LogType = Object.freeze({ Info: 0, Success: 1, Warn: 2, Error: 3 });
globalThis.T3Time = T3Time;
globalThis.params = params;
globalThis.Param = Param;
export {
  __params_init,
  __t3_bridge_init
};
