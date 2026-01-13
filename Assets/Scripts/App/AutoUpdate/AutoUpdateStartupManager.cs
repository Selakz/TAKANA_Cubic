#nullable enable

using Cysharp.Threading.Tasks;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace App.AutoUpdate
{
	public class AutoUpdateStartupManager : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private AutoUpdateWebRequestHandler handler = default!;

		public static AutoUpdateStartupManager Instance { get; private set; } = default!;

		// Defined Functions
		private async UniTaskVoid StartCheck()
		{
			await handler.BeginCheckUpdateProcess();
		}

		// System Functions
		void Start()
		{
			if (!ISingletonSetting<AutoUpdateSetting>.Instance.CheckUpdateOnStartup) return;

			StartCheck().Forget();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Instance = this;
		}
	}
}