/** Model factories shared by the test scenarios. */

export function makeTrackModel(timeStart: number, timeEnd: number): TrackModel {
  const movement = new TrackDirectMovement(new MoveList(), new MoveList());
  movement.insert(new T3Time(timeStart), 0, 1);
  return new TrackModel(new T3Time(timeStart), new T3Time(timeEnd), movement);
}

export function makeHitModel(timeJudge: number): HitModel {
  return new HitModel(HitType.Tap, new T3Time(timeJudge));
}

export function makeDraftHitModel(timeJudge: number): DraftHitModel {
  return new DraftHitModel(HitType.Tap, new T3Time(timeJudge), 0, 1);
}
