// This is a template plugin code provided by TAKANA³ editor.

// Extends T3PluginBase if you need custom callbacks
class PluginImpl extends T3PluginBase {
  // Get parameters declared in manifest using @Param decorator
  @Param<number>("yourParamId")
  yourParam!: number;

  // The only entry called by C#, can either be sync or async
  execute = async () => {
    // Write your plugin logic here
    const ctx = getT3Context();
    ctx.showHeader({ en: "Ciallo ~(∠・ω< )⌒★" }, LogType.Success);

    // Operations related to ctx.chart needs context.commit() to take effect as a single undoable operation
    ctx.commit();
  }

  // An example of overriding callback
  protected override onNoteAdded(note: NoteSnapshot): void {
    const ctx = getT3Context();
    if (note instanceof HitSnapshot && note.hitType.value == HitType.Tap) {
      ctx.showHeader({ en: "A new tap is just added!" }, LogType.Info);
    }
  }
}

const instance = new PluginImpl();
export default instance;
