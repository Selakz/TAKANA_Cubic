namespace MusicGame.ChartEditor.InScreenEdit.CopyPaste
{
	public interface IPasteHandler
	{
		public CopyPastePlugin CopyPaste { get; }

		public string GetDescription();

		public void Cut();

		public bool Paste(out string message);

		public bool ExactPaste(out string message);

		public const string NoPasteObjectMessage = "剪贴板中没有可复制的元件";
	}
}