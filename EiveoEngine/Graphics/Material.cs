namespace EiveoEngine.Graphics;

using Extensions;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;
using Textures;

public class Material
{
	private struct Data
	{
		public Vector4 AlbedoColor;
		public Vector4 SpecularColor;
		public Vector4 EmissiveColor;
		public Vector4 CubeColor;

		public int AlbedoMap;
		public int NormalMap;
		public int SpecularMap;
		public int EmissiveMap;
		public int CubeMap;
	}

	private readonly int buffer;
	private bool dirty = true;

	public int Buffer
	{
		get
		{
			if (!this.dirty)
				return this.buffer;

			this.Update();
			this.dirty = false;

			return this.buffer;
		}
	}

	private Texture? albedoMap;

	public Texture? AlbedoMap
	{
		get => this.albedoMap;
		set
		{
			this.albedoMap = value;
			this.dirty = true;
		}
	}

	private Color? albedoColor;

	public Color? AlbedoColor
	{
		get => this.albedoColor;
		set
		{
			this.albedoColor = value;
			this.dirty = true;
		}
	}

	private Texture? normalMap;

	public Texture? NormalMap
	{
		get => this.normalMap;
		set
		{
			this.normalMap = value;
			this.dirty = true;
		}
	}

	private Texture? specularMap;

	public Texture? SpecularMap
	{
		get => this.specularMap;
		set
		{
			this.specularMap = value;
			this.dirty = true;
		}
	}

	private Color? specularColor;

	public Color? SpecularColor
	{
		get => this.specularColor;
		set
		{
			this.specularColor = value;
			this.dirty = true;
		}
	}

	private Texture? emissiveMap;

	public Texture? EmissiveMap
	{
		get => this.emissiveMap;
		set
		{
			this.emissiveMap = value;
			this.dirty = true;
		}
	}

	private Color? emissiveColor;

	public Color? EmissiveColor
	{
		get => this.emissiveColor;
		set
		{
			this.emissiveColor = value;
			this.dirty = true;
		}
	}

	private Texture? cubeMap;

	public Texture? CubeMap
	{
		get => this.cubeMap;
		set
		{
			this.cubeMap = value;
			this.dirty = true;
		}
	}

	private Color? cubeColor;

	public Color? CubeColor
	{
		get => this.cubeColor;
		set
		{
			this.cubeColor = value;
			this.dirty = true;
		}
	}

	public Material()
	{
		this.buffer = GL.GenBuffer();
	}

	private void Update()
	{
		this.UpdateBuffer(
			new Data
			{
				AlbedoColor = (this.AlbedoColor ?? (this.AlbedoMap != null ? Color.White : Color.Empty)).ToVector4(),
				SpecularColor = (this.SpecularColor ?? (this.SpecularMap != null ? Color.White : Color.Empty)).ToVector4(),
				EmissiveColor = (this.EmissiveColor ?? (this.EmissiveMap != null ? Color.White : Color.Empty)).ToVector4(),
				CubeColor = (this.CubeColor ?? (this.CubeMap != null ? Color.White : Color.Empty)).ToVector4(),
				AlbedoMap = this.AlbedoMap == null ? 0 : 1,
				NormalMap = this.NormalMap == null ? 0 : 1,
				SpecularMap = this.SpecularMap == null ? 0 : 1,
				EmissiveMap = this.EmissiveMap == null ? 0 : 1,
				CubeMap = this.CubeMap == null ? 0 : 1
			}
		);
	}

	private unsafe void UpdateBuffer<TData>(TData data)
		where TData : unmanaged
	{
		GL.BindBuffer(BufferTarget.UniformBuffer, this.buffer);
		GL.BufferData(BufferTarget.UniformBuffer, sizeof(TData), ref data, BufferUsageHint.StaticDraw);
		GL.BindBuffer(BufferTarget.UniformBuffer, 0);
	}
}
