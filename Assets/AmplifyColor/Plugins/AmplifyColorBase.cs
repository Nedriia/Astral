// Amplify Color - Advanced Color Grading for Unity Pro
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4  || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#define UNITY_4
#endif

using System;
using UnityEngine;

namespace AmplifyColor
{
	public enum Quality
	{
		Mobile,
		Standard
	}
}

[AddComponentMenu( "" )]
public class AmplifyColorBase : MonoBehaviour
{
	public AmplifyColor.Quality QualityLevel = AmplifyColor.Quality.Standard;	
	public float BlendAmount = 0f;
	public Texture2D LutTexture;
	public Texture2D LutBlendTexture;
	public Texture MaskTexture;
	public bool UseVolumes = false;	
	public float ExitVolumeBlendTime = 1.0f;
	public Transform TriggerVolumeProxy = null;
	public LayerMask VolumeCollisionMask = ~0;

	private Shader shaderBase;
	private Shader shaderBlend;
	private Shader shaderBlendCache;
	private Shader shaderMask;
	private Shader shaderBlendMask;			
	private RenderTexture blendCacheLut = null;
	private Texture2D normalLut = null;
	private ColorSpace colorSpace = ColorSpace.Uninitialized;
	private AmplifyColor.Quality qualityLevel = AmplifyColor.Quality.Standard;
	private bool use3d = false;

#if UNITY_4
	private Texture lutTexture3d = null;
	private Texture lutBlendTexture3d = null;
	private Texture normalLut3d = null;
	private bool managedLutTexture3d = false;
	private bool managedLutBlendTexture3d = false;
	public Texture3D LutTexture3d { get { return lutTexture3d as Texture3D; } set { lutTexture3d = value; } }
	public Texture3D LutBlendTexture3d { get { return lutBlendTexture3d as Texture3D; } set { lutBlendTexture3d = value; } }
	private Texture3D midBlendLUT3d = null;
#endif

	private Material materialBase;
	private Material materialBlend;
	private Material materialBlendCache;
	private Material materialMask;
	private Material materialBlendMask;

	private bool blending;
	private float blendingTime;
	private float blendingTimeCountdown;

	private Action onFinishBlend;

	public bool IsBlending { get { return blending; } }

	internal bool JustCopy = false;
	
	private Texture2D worldLUT;
	private AmplifyColorVolumeBase currentVolumeLut = null;
	private RenderTexture midBlendLUT = null;
	private bool blendingFromMidBlend = false;

#if TRIAL
	private Texture2D m_watermark = null;
#endif

	public bool WillItBlend { get { return LutTexture != null && LutBlendTexture != null && !blending; } }

	void ReportMissingShaders()
	{
		Debug.LogError( "[AmplifyColor] Error initializing shaders. Please reinstall Amplify Color." );
		enabled = false;
	}

	void ReportNotSupported()
	{
		Debug.LogError( "[AmplifyColor] This image effect is not supported on this platform. Please make sure your Unity license supports Full-Screen Post-Processing Effects which is usually reserved forn Pro licenses." );
		enabled = false;
	}

	bool CheckShader( Shader s )
	{
		if ( s == null )
		{
			ReportMissingShaders();
			return false;
		}
		if ( !s.isSupported )
		{
			ReportNotSupported();
			return false;
		}
		return true;
	}

	bool CheckShaders()
	{
		return CheckShader( shaderBase ) && CheckShader( shaderBlend ) && CheckShader( shaderBlendCache ) &&
			CheckShader( shaderMask ) && CheckShader( shaderBlendMask );
	}

	bool CheckSupport()
	{
		// Disable if we don't support image effect or render textures
		if ( !SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures )
		{
			ReportNotSupported();
			return false;
		}
		return true;
	}

	void OnEnable()
	{
		if ( !CheckSupport() )
			return;

		CreateMaterials();

		if ( ( LutTexture != null && LutTexture.mipmapCount > 1 ) || ( LutBlendTexture != null && LutBlendTexture.mipmapCount > 1 ) )
			Debug.LogError( "[AmplifyColor] Please disable \"Generate Mip Maps\" import settings on all LUT textures to avoid visual glitches. " +
				"Change Texture Type to \"Advanced\" to access Mip settings." );

	#if TRIAL
		m_watermark = new Texture2D( 4, 4 ) { hideFlags = HideFlags.HideAndDontSave };
		m_watermark.LoadImage( AmplifyColor.Watermark.ImageData );
	#endif
	}

	void OnDisable()
	{
		ReleaseMaterials();
		ReleaseTextures();

	#if TRIAL
		if ( m_watermark != null )
		{
			DestroyImmediate( m_watermark );
			m_watermark = null;
		}
	#endif
	}

	public void BlendTo( Texture2D blendTargetLUT, float blendTimeInSec, Action onFinishBlend )
	{
		LutBlendTexture = blendTargetLUT;
		BlendAmount = 0.0f;
		this.onFinishBlend = onFinishBlend;
		blendingTime = blendTimeInSec;
		blendingTimeCountdown = blendTimeInSec;
		blending = true;
	}

	private void Start()
	{
		worldLUT = LutTexture;
	}

	private void Update()
	{
		if ( blending )
		{
			BlendAmount = ( blendingTime - blendingTimeCountdown ) / blendingTime;
			blendingTimeCountdown -= Time.smoothDeltaTime;

			if ( BlendAmount >= 1.0f )
			{
				LutTexture = LutBlendTexture;
				BlendAmount = 0.0f;
				blending = false;
				LutBlendTexture = null;

				if ( blendingFromMidBlend )
				{
					if ( midBlendLUT != null )
						midBlendLUT.DiscardContents();

				#if UNITY_4
					if ( use3d )
						ReleaseLutMidBlendTexture3d();
				#endif
				}

				blendingFromMidBlend = false;

				if ( onFinishBlend != null )
					onFinishBlend();
			}
		}
		else
			BlendAmount = Mathf.Clamp01( BlendAmount );

		if ( UseVolumes )
		{
			Transform proxy = ( TriggerVolumeProxy == null ) ? camera.transform : TriggerVolumeProxy;
			Collider[] hitColliders = Physics.OverlapSphere( proxy.position, 0.01f, VolumeCollisionMask );

			AmplifyColorVolumeBase foundVolume = null;
			foreach ( Collider c in hitColliders )
			{
				AmplifyColorVolumeBase colliderVolume = c.GetComponent<AmplifyColorVolumeBase>();
				if ( colliderVolume != null )
				{
					foundVolume = colliderVolume;					
					break;
				}
			}

			if ( foundVolume != currentVolumeLut )
			{
				currentVolumeLut = foundVolume;
				Texture2D blendTex = ( foundVolume == null ? worldLUT : foundVolume.LutTexture );
				float blendTime = ( foundVolume == null ? ExitVolumeBlendTime : foundVolume.EnterBlendTime );				

				if ( IsBlending && !blendingFromMidBlend && blendTex == LutTexture )
				{
					// Going back to previous volume optimization
					LutTexture = LutBlendTexture;
					LutBlendTexture = blendTex;
					blendingTimeCountdown = blendTime * ( ( blendingTime - blendingTimeCountdown ) / blendingTime );
					blendingTime = blendTime;
					BlendAmount = 1 - BlendAmount;
				}
				else
				{
					if ( IsBlending )
					{
					#if UNITY_4
						if( use3d)
						{
							BlendAmount = Mathf.Clamp01( BlendAmount );
							materialBlendCache.SetFloat( "_lerpAmount", BlendAmount );

							if ( blendingFromMidBlend )
								materialBlendCache.SetTexture( "_RgbTex", midBlendLUT );
							else
								materialBlendCache.SetTexture( "_RgbTex", LutTexture );
							
							materialBlendCache.SetTexture( "_LerpRgbTex", ( LutBlendTexture != null ) ? LutBlendTexture : normalLut );
							Graphics.Blit( LutTexture, midBlendLUT, materialBlendCache );

							Texture2D tempTex =new Texture2D( midBlendLUT.width, midBlendLUT.height, TextureFormat.RGB24, false, true ) { hideFlags = HideFlags.HideAndDontSave };
							tempTex.wrapMode = TextureWrapMode.Clamp;
							tempTex.anisoLevel = 0;

							RenderTexture.active = midBlendLUT;
							tempTex.ReadPixels( new Rect( 0, 0, midBlendLUT.width, midBlendLUT.height ), 0, 0 );
							tempTex.Apply();
							midBlendLUT.DiscardContents();
							ReleaseLutMidBlendTexture3d();
							midBlendLUT3d = ConvertLutTo3D( tempTex );
							DestroyImmediate( tempTex );
						}
						else
						{
					#endif
						materialBlendCache.SetFloat( "_lerpAmount", BlendAmount );
						if ( blendingFromMidBlend )
							materialBlendCache.SetTexture( "_RgbTex", midBlendLUT );
						else
							materialBlendCache.SetTexture( "_RgbTex", LutTexture );
						materialBlendCache.SetTexture( "_LerpRgbTex", ( LutBlendTexture != null ) ? LutBlendTexture : normalLut );
						Graphics.Blit( LutTexture, midBlendLUT, materialBlendCache );
					#if UNITY_4
							midBlendLUT.MarkRestoreExpected();					
						}
					#endif
						blendingFromMidBlend = true;
					}
					BlendTo( blendTex, blendTime, null );
				}
			}
		}

	}

	private void SetupShader()
	{
		Shader.EnableKeyword( "" );
		colorSpace = QualitySettings.activeColorSpace;
		qualityLevel = QualityLevel;
		string linear = ( colorSpace == ColorSpace.Linear ) ? "Linear" : "";
		string ext3d = "";

	#if UNITY_4
		if ( SystemInfo.supports3DTextures )
		{			
			ext3d = "3d";
			use3d = true;
		}
	#endif

		if ( QualityLevel == AmplifyColor.Quality.Mobile )
		{
			Shader.EnableKeyword( "QUALITY_MOBILE" );
			Shader.DisableKeyword( "QUALITY_STANDARD" );
		}
		else
		{
			Shader.DisableKeyword( "QUALITY_MOBILE" );
			Shader.EnableKeyword( "QUALITY_STANDARD" );
		}

		shaderBase = Shader.Find( "Hidden/Amplify Color/Base" + linear + ext3d );
		shaderBlend = Shader.Find( "Hidden/Amplify Color/Blend" + linear + ext3d );
		shaderBlendCache = Shader.Find( "Hidden/Amplify Color/BlendCache" );
		shaderMask = Shader.Find( "Hidden/Amplify Color/Mask" + linear + ext3d );
		shaderBlendMask = Shader.Find( "Hidden/Amplify Color/BlendMask" + linear + ext3d );		
	}

	private void ReleaseMaterials()
	{
		if ( materialBase != null )
		{
			DestroyImmediate( materialBase );
			materialBase = null;
		}
		if ( materialBlend != null )
		{
			DestroyImmediate( materialBlend );
			materialBlend = null;
		}
		if ( materialBlendCache != null )
		{
			DestroyImmediate( materialBlendCache );
			materialBlendCache = null;
		}
		if ( materialMask != null )
		{
			DestroyImmediate( materialMask );
			materialMask = null;
		}
		if ( materialBlendMask != null )
		{
			DestroyImmediate( materialBlendMask );
			materialBlendMask = null;
		}
	}

	private void CreateHelperTextures()
	{
		const int size = 32;
		const int maxSize = size - 1;
		int width = size * size;
		int height = size;

		ReleaseTextures();

		blendCacheLut = new RenderTexture( width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear ) { hideFlags = HideFlags.HideAndDontSave };
		blendCacheLut.name = "BlendCacheLut";
		blendCacheLut.wrapMode = TextureWrapMode.Clamp;
		blendCacheLut.useMipMap = false;
		blendCacheLut.anisoLevel = 0;
		blendCacheLut.Create();

		midBlendLUT = new RenderTexture( width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear ) { hideFlags = HideFlags.HideAndDontSave };
		midBlendLUT.name = "MidBlendLut";
		midBlendLUT.wrapMode = TextureWrapMode.Clamp;
		midBlendLUT.useMipMap = false;
		midBlendLUT.anisoLevel = 0;
		midBlendLUT.Create();
	#if UNITY_4
		midBlendLUT.MarkRestoreExpected();
	#endif

		normalLut = new Texture2D( width, height, TextureFormat.RGB24, false, true ) { hideFlags = HideFlags.HideAndDontSave };
		normalLut.name = "NormalLut";
		normalLut.hideFlags = HideFlags.DontSave;
		normalLut.anisoLevel = 1;
		normalLut.filterMode = FilterMode.Bilinear;
		Color32[] colors = new Color32[ width * height ];
		for ( int z = 0; z < size; z++ )
		{
			int zoffset = z * size;
			for ( int y = 0; y < size; y++ )
			{
				int yoffset = zoffset + y * width;
				for ( int x = 0; x < size; x++ )
				{
					float fr = x / ( float ) maxSize;
					float fg = y / ( float ) maxSize;
					float fb = z / ( float ) maxSize;
					byte br = ( byte ) ( fr * 255 );
					byte bg = ( byte ) ( fg * 255 );
					byte bb = ( byte ) ( fb * 255 );
					colors[ yoffset + x ] = new Color32( br, bg, bb, 255 );
				}
			}
		}
		normalLut.SetPixels32( colors );
		normalLut.Apply();

	#if UNITY_4
		if ( use3d )		
		{
			normalLut3d = ConvertLutTo3D( normalLut );
			normalLut3d.name = "NormalLut3d";
		}
	#endif
	}

	bool CheckMaterialAndShader( Material material, string name )
	{
		if ( material == null || material.shader == null )
		{
			Debug.LogError( "[AmplifyColor] Error creating " + name + " material. Effect disabled." );
			enabled = false;
		}
		else if ( !material.shader.isSupported )
		{
			Debug.LogError( "[AmplifyColor] " + name + " shader not supported on this platform. Effect disabled." );
			enabled = false;
		}
		else
			material.hideFlags = HideFlags.HideAndDontSave;
		return enabled;
	}

	private void CreateMaterials()
	{
		SetupShader();

		ReleaseMaterials();

		materialBase = new Material( shaderBase );
		materialBlend = new Material( shaderBlend );
		materialBlendCache = new Material( shaderBlendCache );
		materialMask = new Material( shaderMask );
		materialBlendMask = new Material( shaderBlendMask );

		CheckMaterialAndShader( materialBase, "BaseMaterial" );
		CheckMaterialAndShader( materialBlend, "BlendMaterial" );
		CheckMaterialAndShader( materialBlendCache, "BlendCacheMaterial" );
		CheckMaterialAndShader( materialMask, "MaskMaterial" );
		CheckMaterialAndShader( materialBlendMask, "BlendMaskMaterial" );

		if ( !enabled )
			return;		

		CreateHelperTextures();
	}

#if UNITY_4
	private void ReleaseLutTexture3d()
	{
		if ( lutTexture3d != null )
		{
			DestroyImmediate( lutTexture3d );
			lutTexture3d = null;
			managedLutTexture3d = false;
		}
	}

	private void ReleaseLutBlendTexture3d()
	{
		if ( lutBlendTexture3d != null )
		{
			DestroyImmediate( lutBlendTexture3d );
			lutBlendTexture3d = null;
			managedLutBlendTexture3d = false;
		}
	}

	private void ReleaseLutMidBlendTexture3d()
	{
		if (midBlendLUT3d != null) 
		{
			DestroyImmediate(midBlendLUT3d);
			midBlendLUT3d=null;
		}
	}
#endif

	private void ReleaseTextures()
	{
		if ( blendCacheLut != null )
		{
			DestroyImmediate( blendCacheLut );
			blendCacheLut = null;
		}

		if ( midBlendLUT != null )
		{
			DestroyImmediate( midBlendLUT );
			midBlendLUT = null;
		}

		if ( normalLut != null )
		{
			DestroyImmediate( normalLut );
			normalLut = null;
		}

	#if UNITY_4
		if ( use3d )
		{
			if ( managedLutTexture3d )
				ReleaseLutTexture3d();

			if ( managedLutBlendTexture3d )
				ReleaseLutBlendTexture3d();

			if ( normalLut3d != null )
			{
				DestroyImmediate( normalLut3d );
				normalLut3d = null;
			}

			if ( midBlendLUT3d != null ) 
				ReleaseLutMidBlendTexture3d();
		}
	#endif
	}

	public static bool ValidateLutDimensions( Texture2D lut )
	{
		bool valid = true;
		if ( lut != null )
		{
			if ( ( lut.width / lut.height ) != lut.height )
			{
				Debug.LogWarning( "[AmplifyColor] Lut " + lut.name + " has invalid dimensions." );
				valid = false;
			}
			else
			{
				if ( lut.anisoLevel != 0 )
					lut.anisoLevel = 0;
			}
		}
		return valid;
	}

	private void OnRenderImage( RenderTexture source, RenderTexture destination )
	{
		BlendAmount = Mathf.Clamp01( BlendAmount );

		if ( colorSpace != QualitySettings.activeColorSpace || qualityLevel != QualityLevel )
			CreateMaterials();

		bool validLut = ValidateLutDimensions( LutTexture );
		bool validLutBlend = ValidateLutDimensions( LutBlendTexture );
		bool skip = ( LutTexture == null && LutBlendTexture == null );

		if ( JustCopy || !validLut || !validLutBlend || skip )
		{
			Graphics.Blit( source, destination );
			return;
		}

		Texture2D lut = ( LutTexture == null ) ? normalLut : LutTexture;
		Texture2D lutBlend = LutBlendTexture;		
		
	#if UNITY_4		
		Texture lutBlend3d = null;
		Texture lut3d = null;
		if ( use3d )
		{
			CacheConvertLutsTo3D();
			lut3d = ( LutTexture == null ) ? normalLut3d : LutTexture3d;
			lutBlend3d = LutBlendTexture3d;
		}
	#endif

		int pass = !camera.hdr ? 0 : 1;
		bool blend = ( BlendAmount != 0.0f ) || blending;
		bool requiresBlend = blend || ( blend && lutBlend != null );
		bool useBlendCache = requiresBlend && !use3d;

		Material material;
		if ( requiresBlend )
		{
			if ( MaskTexture != null )
				material = materialBlendMask;
			else
				material = materialBlend;
		}
		else
		{
			if ( MaskTexture != null )
				material = materialMask;
			else
				material = materialBase;
		}

		material.SetFloat( "_lerpAmount", BlendAmount );
		if ( MaskTexture != null )
			material.SetTexture( "_MaskTex", MaskTexture );

	#if UNITY_4
		if ( use3d )
		{
			if( blendingFromMidBlend )
				material.SetTexture( "_RgbTex3d", midBlendLUT3d );
			else
				material.SetTexture( "_RgbTex3d", lut3d );
			material.SetTexture( "_LerpRgbTex3d", ( lutBlend3d != null ) ? lutBlend3d : normalLut3d );
		}
	#endif

		if ( useBlendCache )
		{
			materialBlendCache.SetFloat( "_lerpAmount", BlendAmount );
			if ( UseVolumes && blendingFromMidBlend )
				materialBlendCache.SetTexture( "_RgbTex", midBlendLUT );
			else
				materialBlendCache.SetTexture( "_RgbTex", lut );
			materialBlendCache.SetTexture( "_LerpRgbTex", ( lutBlend != null ) ? lutBlend : normalLut );

			Graphics.Blit( lut, blendCacheLut, materialBlendCache );

			material.SetTexture( "_RgbBlendCacheTex", blendCacheLut );
		}
		else if ( !use3d )
		{
			if ( lut != null )
				material.SetTexture( "_RgbTex", lut );
			if ( lutBlend != null )
				material.SetTexture( "_LerpRgbTex", lutBlend );
		}

		Graphics.Blit( source, destination, material, pass );
		if ( useBlendCache )
			blendCacheLut.DiscardContents();
	}

#if UNITY_4
	public void CacheConvertLutsTo3D()
	{
		if ( LutTexture != null )
		{
			if ( lutTexture3d == null || lutTexture3d.name != LutTexture.GetHashCode().ToString() )
			{
				lutTexture3d = ConvertLutTo3D( LutTexture );
				managedLutTexture3d = true;
			}
		}
		else if ( managedLutTexture3d )
			ReleaseLutTexture3d();		

		if ( LutBlendTexture != null )
		{
			if ( lutBlendTexture3d == null || lutBlendTexture3d.name != LutBlendTexture.GetHashCode().ToString() )
			{
				lutBlendTexture3d = ConvertLutTo3D( LutBlendTexture );
				managedLutBlendTexture3d = true;
			}
		}
		else if ( managedLutBlendTexture3d )
			ReleaseLutBlendTexture3d();
	}

	public static Texture3D ConvertLutTo3D( Texture2D lut )
	{
		Texture3D lut3d = null;

		if ( lut != null )
		{
			if ( ValidateLutDimensions( lut ) )
			{
				try
				{
					Color[] src = lut.GetPixels();
					Color[] dst = new Color[ src.Length ];

					for ( int x = 0; x < 32; x++ )
					{
						for ( int y = 0; y < 32; y++ )
						{
							int src_off = x + ( y << 10 );
							int dst_off = x + ( y << 5 );

							for ( int z = 0; z < 32; z++ )
								dst[ dst_off + ( z << 10 ) ] = src[ src_off + ( z << 5 ) ];
						}
					}

					lut3d = new Texture3D( 32, 32, 32, TextureFormat.RGB24, false ) { hideFlags = HideFlags.HideAndDontSave };
					lut3d.wrapMode = TextureWrapMode.Clamp;
					lut3d.SetPixels( dst );
					lut3d.Apply();
				}
				catch ( System.Exception )
				{
				}
			}

			if ( lut3d == null )
				Debug.LogWarning( "[AmplifyColor] ConvertLutTo3D failed on " + lut.name + ". Please ensure the texture is 1024x32 and has Read/Write Enabled." );
			else
				lut3d.name = lut.GetHashCode().ToString();
		}	
		else
			Debug.LogWarning( "[AmplifyColor] An invalid/null Lut texture was passed into ConvertLutTo3D" );

		return lut3d;
	}
#endif

#if TRIAL
	void OnGUI()
	{
		if ( m_watermark != null )
			GUI.DrawTexture( new Rect( 15, Screen.height - m_watermark.height - 12, m_watermark.width, m_watermark.height ), m_watermark );
	}
#endif
}