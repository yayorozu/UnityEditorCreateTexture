using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Yorozu.EditorTool
{
	public class CreateTextureEditorWindow : EditorWindow
	{
		[MenuItem("Tools/CreateTexture")]
		private static void ShowWindow()
		{
			var window = GetWindow<CreateTextureEditorWindow>();
			window.titleContent = new GUIContent("CreateTexture");
			window.Show();
		}

		private enum TextureType
		{
			Default,
			Outline,
			Gradient,
		}

		private TextureType _type;

		private Vector2Int _size = new Vector2Int(1, 1);
		private Color _color = Color.white;

		private Color _outlineColor = Color.white;
		private int _outlineLength;

		[SerializeField]
		private Gradient _gradient = new Gradient();
		private GridLayoutGroup.Axis _axis;

		private Texture2D _sampleTexture;

		private void OnDisable()
		{
			if (_sampleTexture != null)
				DestroyImmediate(_sampleTexture);
		}

		private void OnGUI()
		{
			_type = (TextureType) EditorGUILayout.EnumPopup(_type);
			_size = EditorGUILayout.Vector2IntField("Size", _size);
			if (_type == TextureType.Default || _type == TextureType.Outline)
				_color = EditorGUILayout.ColorField("Color", _color);

			if (_type == TextureType.Outline)
			{
				_outlineColor = EditorGUILayout.ColorField("Outline Color", _outlineColor);
				_outlineLength = EditorGUILayout.IntField("Outline Length", _outlineLength);
			}

			if (_type == TextureType.Gradient)
			{
				_axis = (GridLayoutGroup.Axis) EditorGUILayout.EnumPopup("Axis", _axis);
				_gradient = EditorGUILayout.GradientField("GradientColor", _gradient);
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Show Sample"))
				{
					_sampleTexture = CreateTexture();
				}
				if (GUILayout.Button("Create"))
				{
					SaveTexture();
				}
			}

			if (_sampleTexture != null)
				GUILayout.Label(_sampleTexture);
		}

		private void SaveTexture()
		{
			var path = EditorUtility.SaveFilePanel("Save Texture", Application.dataPath, "Texture", "png");
			if (string.IsNullOrEmpty(path))
				return;

			var texture = CreateTexture();

			File.WriteAllBytes(path, texture.EncodeToPNG());

			var projectPath = path.Replace(Application.dataPath, "Assets");
			DestroyImmediate(texture);
			AssetDatabase.ImportAsset(projectPath);
		}

		private Texture2D CreateTexture()
		{
			var texture = new Texture2D(_size.x, _size.y, TextureFormat.RGB24, false);
			for (var x = 0; x < _size.x; x++)
				for (var y = 0; y < _size.y; y++)
				{
					if (_type == TextureType.Default || _type == TextureType.Outline)
					{
						texture.SetPixel(x, y, _color);
						continue;
					}

					var t = _axis == GridLayoutGroup.Axis.Horizontal
						? Mathf.InverseLerp(0, _size.x - 1, x)
						: Mathf.InverseLerp(_size.y - 1, 0, y);

					texture.SetPixel(x, y, _gradient.Evaluate(t));
				}

			if (_type == TextureType.Outline)
			{
				for (var x = 0; x < _size.x; x++)
					for (var y = 0; y < _size.y; y++)
					{
						if (_outlineLength <= x && _size.x - x > _outlineLength && _outlineLength <= y && _size.y - y > _outlineLength)
							continue;

						texture.SetPixel(x, y, _outlineColor);
					}
			}

			texture.Apply(false, false);
			return texture;
		}
	}
}
