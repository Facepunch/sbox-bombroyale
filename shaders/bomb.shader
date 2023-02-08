//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	Description = "Bomb";
}

FEATURES
{
    #include "common/features.hlsl"
}

COMMON
{
	#include "common/shared.hlsl"

	float g_ExplodeTime < UiType( Slider ); Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "Settings,10/20" ); >;
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VertexInput i ) )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{
    #include "common/pixel.hlsl"

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = GatherMaterial( i );

		if ( g_ExplodeTime > 0.0 )
		{
			m.Albedo = lerp( m.Albedo, float3( 1.0, 1.0, 1.0 ), g_ExplodeTime );
			m.Emission = lerp( m.Emission, float3( 1.0, 1.0, 1.0 ), g_ExplodeTime );
			m.Metalness = lerp( m.Metalness, float3( 0.0, 0.0, 0.0 ), g_ExplodeTime );
			m.Roughness = lerp( m.Roughness, float3( 0.0, 0.0, 0.0 ), g_ExplodeTime );
			m.AmbientOcclusion = lerp( m.AmbientOcclusion, float3( 0.0, 0.0, 0.0 ), g_ExplodeTime );
		}

		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
