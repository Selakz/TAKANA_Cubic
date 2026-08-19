/**
 * Base class for plugins. Extend it and override the protected callbacks to
 * react to chart/node changes. The events are subscribed automatically when
 * the instance is constructed.
 */
declare abstract class T3PluginBase {
  constructor();

  /**
   * NOTE: when a note is just added, note.track is not set.
   * If there is need for the track, delay it to the next tick and ensure "shouldTick" is true in manifest.
   */
  protected onNoteAdded(note: NoteSnapshot): void;

  protected onNoteRemoved(note: NoteSnapshot): void;

  protected onTrackAdded(track: TrackSnapshot): void;

  protected onTrackRemoved(track: TrackSnapshot): void;

  /**
   * NOTE: Nodes are added when a track is selected, and are removed when a track is deselected. It means
   * the trigger of this callback doesn't necessarily mean a track is being edited.
   */
  protected onNodeAdded(node: TrackNode): void;

  /**
   * NOTE: Nodes are added when a track is selected, and are removed when a track is deselected. It means
   * the trigger of this callback doesn't necessarily mean a track is being edited.
   */
  protected onNodeRemoved(node: TrackNode): void;
}
