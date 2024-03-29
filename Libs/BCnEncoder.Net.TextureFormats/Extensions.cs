using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Encoder.Options;
using BCnEncoder.Shared;
using Microsoft.Toolkit.HighPerformance;

namespace BCnEncoder.TextureFormats
{
	public static class EncoderExtensions
	{
		/// <summary>
		/// Encodes all mipmap levels into a texture file container.
		/// </summary>
		/// <param name="inputData">The input to encode represented by a <see cref="BCnTextureData"/>.</param>
		public static TTextureFormat EncodeToTexture<TTextureFormat>(this BcEncoder encoder, BCnTextureData inputData)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (inputData.IsCubeMap && !tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = encoder.Encode(inputData);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file container.
		/// </summary>
		/// <param name="inputData">The input to encode represented by a <see cref="BCnTextureData"/>.</param>
		public static async Task<TTextureFormat> EncodeToTextureAsync<TTextureFormat>(this BcEncoder encoder, BCnTextureData inputData, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (inputData.IsCubeMap && !tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = await encoder.EncodeAsync(inputData, token);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file and writes it to the output stream.
		/// </summary>
		/// <param name="inputData">The input to encode represented by a <see cref="BCnTextureData"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static void EncodeToStream<TTextureFormat>(this BcEncoder encoder, BCnTextureData inputData, Stream outputStream)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (inputData.IsCubeMap && !tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = encoder.Encode(inputData);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file and writes it to the output stream.
		/// </summary>
		/// <param name="inputData">The input to encode represented by a <see cref="BCnTextureData"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static async Task EncodeToStreamAsync<TTextureFormat>(this BcEncoder encoder, BCnTextureData inputData, Stream outputStream, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (inputData.IsCubeMap && !tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = await encoder.EncodeAsync(inputData, token);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file container.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		public static TTextureFormat EncodeToTextureHdr<TTextureFormat>(this BcEncoder encoder, ReadOnlyMemory2D<ColorRgbaFloat> input)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = encoder.EncodeHdr(input);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file container.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		public static async Task<TTextureFormat> EncodeToTextureHdrAsync<TTextureFormat>(this BcEncoder encoder, ReadOnlyMemory2D<ColorRgbaFloat> input, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = await encoder.EncodeHdrAsync(input, token);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file container.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		public static TTextureFormat EncodeToTexture<TTextureFormat>(this BcEncoder encoder, ReadOnlyMemory2D<ColorRgba32> input)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();
			if (!tex.SupportsLdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support LDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = encoder.Encode(input);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file container.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		public static async Task<TTextureFormat> EncodeToTextureAsync<TTextureFormat>(this BcEncoder encoder, ReadOnlyMemory2D<ColorRgba32> input, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();
			if (!tex.SupportsLdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support LDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = await encoder.EncodeAsync(input, token);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file and writes it to the output stream.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static void EncodeToStreamHdr<TTextureFormat>(this BcEncoder encoder, ReadOnlyMemory2D<ColorRgbaFloat> input, Stream outputStream)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();
			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = encoder.EncodeHdr(input);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file and writes it to the output stream.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static async Task EncodeToStreamHdrAsync<TTextureFormat>(this BcEncoder encoder, ReadOnlyMemory2D<ColorRgbaFloat> input, Stream outputStream, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();
			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = await encoder.EncodeHdrAsync(input, token);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file and writes it to the output stream.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static void EncodeToStream<TTextureFormat>(this BcEncoder encoder, ReadOnlyMemory2D<ColorRgba32> input, Stream outputStream)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();
			if (!tex.SupportsLdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support LDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = encoder.Encode(input);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file and writes it to the output stream.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static async Task EncodeToStreamAsync<TTextureFormat>(this BcEncoder encoder, ReadOnlyMemory2D<ColorRgba32> input, Stream outputStream, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();
			if (!tex.SupportsLdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support LDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = await encoder.EncodeAsync(input, token);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}






		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		public static TTextureFormat EncodeCubeMapToTextureHdr<TTextureFormat>(this BcEncoder encoder,
			ReadOnlyMemory2D<ColorRgbaFloat> right, ReadOnlyMemory2D<ColorRgbaFloat> left,
			ReadOnlyMemory2D<ColorRgbaFloat> top, ReadOnlyMemory2D<ColorRgbaFloat> down,
			ReadOnlyMemory2D<ColorRgbaFloat> back, ReadOnlyMemory2D<ColorRgbaFloat> front)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = encoder.EncodeCubeMapHdr(right, left, top, down, back, front);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		public static async Task<TTextureFormat> EncodeCubeMapToTextureHdrAsync<TTextureFormat>(this BcEncoder encoder,
			ReadOnlyMemory2D<ColorRgbaFloat> right, ReadOnlyMemory2D<ColorRgbaFloat> left,
			ReadOnlyMemory2D<ColorRgbaFloat> top, ReadOnlyMemory2D<ColorRgbaFloat> down,
			ReadOnlyMemory2D<ColorRgbaFloat> back, ReadOnlyMemory2D<ColorRgbaFloat> front, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = await encoder.EncodeCubeMapHdrAsync(right, left, top, down, back, front, token);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		public static TTextureFormat EncodeCubeMapToTexture<TTextureFormat>(this BcEncoder encoder,
			ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.SupportsLdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support LDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = encoder.EncodeCubeMap(right, left, top, down, back, front);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		public static async Task<TTextureFormat> EncodeCubeMapToTextureAsync<TTextureFormat>(this BcEncoder encoder,
			ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.SupportsLdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support LDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = await encoder.EncodeCubeMapAsync(right, left, top, down, back, front, token);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static void EncodeCubeMapToStreamHdr<TTextureFormat>(this BcEncoder encoder,
			ReadOnlyMemory2D<ColorRgbaFloat> right, ReadOnlyMemory2D<ColorRgbaFloat> left,
			ReadOnlyMemory2D<ColorRgbaFloat> top, ReadOnlyMemory2D<ColorRgbaFloat> down,
			ReadOnlyMemory2D<ColorRgbaFloat> back, ReadOnlyMemory2D<ColorRgbaFloat> front, Stream outputStream)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = encoder.EncodeCubeMapHdr(right, left, top, down, back, front);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static async Task EncodeCubeMapToStreamHdrAsync<TTextureFormat>(this BcEncoder encoder,
			ReadOnlyMemory2D<ColorRgbaFloat> right, ReadOnlyMemory2D<ColorRgbaFloat> left,
			ReadOnlyMemory2D<ColorRgbaFloat> top, ReadOnlyMemory2D<ColorRgbaFloat> down,
			ReadOnlyMemory2D<ColorRgbaFloat> back, ReadOnlyMemory2D<ColorRgbaFloat> front, Stream outputStream, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = await encoder.EncodeCubeMapHdrAsync(right, left, top, down, back, front, token);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static void EncodeCubeMapToStream<TTextureFormat>(this BcEncoder encoder,
			ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front, Stream outputStream)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.SupportsLdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support LDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = encoder.EncodeCubeMap(right, left, top, down, back, front);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static async Task EncodeCubeMapToStreamAsync<TTextureFormat>(this BcEncoder encoder,
			ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front, Stream outputStream, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.SupportsLdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support LDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = await encoder.EncodeCubeMapAsync(right, left, top, down, back, front, token);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}
	}

	public static class DecoderExtensions
	{
		/// <inheritdoc cref="BcDecoder.Decode"/>
		public static BCnTextureData Decode<T>(this BcDecoder decoder, T textureFile)
			where T : class, ITextureFileFormat, new()
		{
			var encoded = textureFile.ToTextureData();
			return decoder.Decode(encoded);
		}

		/// <inheritdoc cref="BcDecoder.Decode"/>
		public static BCnTextureData Decode<T>(this BcDecoder decoder, Stream inputStream)
			where T : class, ITextureFileFormat, new()
		{
			var tex = new T();
			tex.ReadFromStream(inputStream);
			return Decode(decoder, tex);
		}

		/// <summary>
		/// Decode the first mip level of a textureFile into a <see cref="Memory2D{T}"/> of <see cref="ColorRgba32"/>.
		/// </summary>
		/// <returns></returns>
		public static Memory2D<ColorRgba32> DecodeToColorRgba2D<T>(this BcDecoder decoder, T textureFile)
			where T : ITextureFileFormat
		{

			var encoded = textureFile.ToTextureData();
			return decoder.DecodeRawLdr2D(
				encoded.MipLevels[0].Data,
				encoded.MipLevels[0].Width,
				encoded.MipLevels[0].Height,
				encoded.Format);
		}

		/// <summary>
		/// Decode the first mip level of a textureFile into a <see cref="Memory2D{T}"/> of <see cref="ColorRgbaFloat"/>.
		/// </summary>
		/// <returns></returns>
		public static Memory2D<ColorRgbaFloat> DecodeToColorRgbaHdr2D<T>(this BcDecoder decoder, T textureFile)
			where T : ITextureFileFormat
		{

			var encoded = textureFile.ToTextureData();
			return decoder.DecodeRawHdr2D(
				encoded.MipLevels[0].Data,
				encoded.MipLevels[0].Width,
				encoded.MipLevels[0].Height,
				encoded.Format);
		}

		/// <inheritdoc cref="BcDecoder.Decode"/>
		public static Task<BCnTextureData> DecodeAsync<T>(this BcDecoder decoder, T textureFile, CancellationToken token = default)
			where T : ITextureFileFormat
		{
			var encoded = textureFile.ToTextureData();
			return decoder.DecodeAsync(encoded, token);
		}

		/// <summary>
		/// Decode the first mip level of a textureFile into a <see cref="Memory2D{T}"/> of <see cref="ColorRgba32"/>.
		/// </summary>
		/// <returns></returns>
		public static Task<Memory2D<ColorRgba32>> DecodeToColorRgba2DAsync<T>(this BcDecoder decoder, T textureFile, CancellationToken token = default)
			where T : ITextureFileFormat
		{

			var encoded = textureFile.ToTextureData();
			return decoder.DecodeRawLdr2DAsync(
				encoded.MipLevels[0].Data,
				encoded.MipLevels[0].Width,
				encoded.MipLevels[0].Height,
				encoded.Format, token);
		}

		/// <summary>
		/// Decode the first mip level of a textureFile into a <see cref="Memory2D{T}"/> of <see cref="ColorRgbaFloat"/>.
		/// </summary>
		/// <returns></returns>
		public static Task<Memory2D<ColorRgbaFloat>> DecodeToColorRgbaHdr2DAsync<T>(this BcDecoder decoder, T textureFile, CancellationToken token = default)
			where T : ITextureFileFormat
		{

			var encoded = textureFile.ToTextureData();
			return decoder.DecodeRawHdr2DAsync(
				encoded.MipLevels[0].Data,
				encoded.MipLevels[0].Width,
				encoded.MipLevels[0].Height,
				encoded.Format, token);
		}
	}
}
