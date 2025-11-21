#if UNITY_EDITOR
#nullable enable

using System;
using System.IO;
using T3Framework.Static.Easing;
using UnityEditor;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.Editor
{
	public class LineMeshGeneratorWindow : EditorWindow
	{
		[SerializeField] private Eases ease = Eases.Unmove;

		[SerializeField] private string saveDirectory = "Assets/Meshes";
		[SerializeField] private float lineWidth = 1;
		[SerializeField] private float width = 1;
		[SerializeField] private float height = 1;
		[SerializeField] private float precision = 0.1f;
		[SerializeField] private float maxSegment = 100;

		[MenuItem("Tools/Line Mesh Generator")]
		public static void ShowWindow()
		{
			GetWindow<LineMeshGeneratorWindow>("Line Mesh Generator");
		}

		private void OnGUI()
		{
			EditorGUILayout.BeginScrollView(Vector2.zero);
			GUILayout.Label("Line Mesh Generator", EditorStyles.boldLabel);

			ease = (Eases)EditorGUILayout.EnumPopup("Line Type:", ease);

			GUILayout.Label("Save Directory:", EditorStyles.label);
			EditorGUILayout.BeginHorizontal();
			saveDirectory = EditorGUILayout.TextField(saveDirectory);
			if (GUILayout.Button("Browse...", GUILayout.Width(60)))
			{
				string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
				if (!string.IsNullOrEmpty(path))
				{
					if (path.StartsWith(Application.dataPath))
					{
						saveDirectory = "Assets" + path[Application.dataPath.Length..];
						Repaint();
					}
					else
					{
						Debug.LogWarning("Ignoring folders that are not in Assets.");
					}
				}
			}

			EditorGUILayout.EndHorizontal();

			lineWidth = EditorGUILayout.FloatField("Line Width:", lineWidth);
			width = EditorGUILayout.FloatField("Width:", width);
			height = EditorGUILayout.FloatField("Height:", height);
			precision = EditorGUILayout.FloatField("Precision:", precision);
			maxSegment = EditorGUILayout.FloatField("Max Segment:", maxSegment);

			if (GUILayout.Button("Generate and Save Mesh", GUILayout.Height(30)))
			{
				if (ValidateInputs())
				{
					Mesh mesh = new();
					LineDrawer.DrawMesh(mesh, ease, lineWidth, width, height, precision, maxSegment);
					mesh.name = $"{ease}_Mesh";
					SaveMesh(mesh);
				}
			}

			if (GUILayout.Button("Generate and Save All Mesh", GUILayout.Height(30)))
			{
				if (ValidateInputs())
				{
					foreach (var value in Enum.GetValues(typeof(Eases)))
					{
						var e = (Eases)value;
						Mesh mesh = new();
						LineDrawer.DrawMesh(mesh, e, lineWidth, width, height, precision, maxSegment);
						mesh.name = $"{e}_Mesh";
						SaveMesh(mesh);
					}
				}
			}

			EditorGUILayout.EndScrollView();
		}

		private bool ValidateInputs()
		{
			string fullPath = Application.dataPath + saveDirectory[6..];
			return Directory.Exists(fullPath) &&
			       precision > 0 &&
			       maxSegment > 0 &&
			       height >= 0 &&
			       width >= 0;
		}

		private void SaveMesh(Mesh mesh)
		{
			try
			{
				string assetPath = Path.Combine(saveDirectory, $"{mesh.name}.asset");
				assetPath = assetPath.Replace("\\", "/");

#if UNITY_EDITOR
				if (File.Exists(Application.dataPath + assetPath[6..]))
				{
					AssetDatabase.DeleteAsset(assetPath);
				}

				AssetDatabase.CreateAsset(mesh, assetPath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
#endif
			}
			catch (Exception e)
			{
				Debug.LogError($"Error when saving {mesh.name}: {e.Message}");
			}
		}
	}
}

#endif