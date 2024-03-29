using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Encoder.Bptc;
using BCnEncoder.Encoder.Options;
using BCnEncoder.Shared;
using Microsoft.Toolkit.HighPerformance;

namespace BCnEncoder.Encoder
{
	/// <summary>
	/// Handles all encoding of images into compressed or uncompressed formats. For decoding, <see cref="Decoder.BcDecoder"/>
	/// </summary>
	public class BcEncoder
	{
		/// <summary>
		/// The input options of the encoder.
		/// </summary>
		public EncoderInputOptions InputOptions { get; } = new EncoderInputOptions();

		/// <summary>
		/// The output options of the encoder.
		/// </summary>
		public EncoderOutputOptions OutputOptions { get; } = new EncoderOutputOptions();

		/// <summary>
		/// The encoder options.
		/// </summary>
		public EncoderOptions Options { get; } = new EncoderOptions();

		/// <summary>
		/// Creates a new instance of <see cref="BcEncoder"/>.
		/// </summary>
		/// <param name="format">The block compression Format to encode an image with.</param>
		public BcEncoder(CompressionFormat format = CompressionFormat.Bc1)
		{
			OutputOptions.Format = format;
		}

		#region LDR

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode. This can be in any supported format.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public BCnTextureData Encode(BCnTextureData input)
		{
			return EncodeInternal(input, default);
		}

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode. This can be in any supported format.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public Task<BCnTextureData> EncodeAsync(BCnTextureData input, CancellationToken token = default)
		{
			return Task.Run(() => EncodeInternal(input, token), token);
		}

		/// <summary>
		/// Encode a cubemap as raw bytes into a <see cref="BCnTextureData"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public BCnTextureData EncodeCubeMap(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front)
		{
			var inputData = new BCnTextureData(CompressionFormat.Rgba32, right.Width, right.Height, 1, true, false);

			if (
				right.Width != left.Width || right.Height != left.Height ||
				right.Width != top.Width || right.Height != top.Height ||
				right.Width != down.Width || right.Height != down.Height ||
				right.Width != back.Width || right.Height != back.Height ||
				right.Width != front.Width || right.Height != front.Height
			)
			{
				throw new ArgumentException("All faces of a cubeMap must be of equal width and height!");
			}

			inputData[CubeMapFaceDirection.XPositive].Mips[0].Data = right.CopyAsBytes();
			inputData[CubeMapFaceDirection.XNegative].Mips[0].Data = left.CopyAsBytes();
			inputData[CubeMapFaceDirection.YPositive].Mips[0].Data = top.CopyAsBytes();
			inputData[CubeMapFaceDirection.YNegative].Mips[0].Data = down.CopyAsBytes();
			inputData[CubeMapFaceDirection.ZPositive].Mips[0].Data = back.CopyAsBytes();
			inputData[CubeMapFaceDirection.ZNegative].Mips[0].Data = front.CopyAsBytes();

			return EncodeInternal(inputData, default);
		}

		/// <summary>
		/// Encode a cubemap as raw bytes into a <see cref="BCnTextureData"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public Task<BCnTextureData> EncodeCubeMapAsync(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front, CancellationToken token = default)
		{
			var inputData = new BCnTextureData(CompressionFormat.Rgba32, right.Width, right.Height, 1, true, false);

			if (
				right.Width != left.Width || right.Height != left.Height ||
				right.Width != top.Width || right.Height != top.Height ||
				right.Width != down.Width || right.Height != down.Height ||
				right.Width != back.Width || right.Height != back.Height ||
				right.Width != front.Width || right.Height != front.Height
			)
			{
				throw new ArgumentException("All faces of a cubeMap must be of equal width and height!");
			}

			inputData[CubeMapFaceDirection.XPositive].Mips[0].Data = right.CopyAsBytes();
			inputData[CubeMapFaceDirection.XNegative].Mips[0].Data = left.CopyAsBytes();
			inputData[CubeMapFaceDirection.YPositive].Mips[0].Data = top.CopyAsBytes();
			inputData[CubeMapFaceDirection.YNegative].Mips[0].Data = down.CopyAsBytes();
			inputData[CubeMapFaceDirection.ZPositive].Mips[0].Data = back.CopyAsBytes();
			inputData[CubeMapFaceDirection.ZNegative].Mips[0].Data = front.CopyAsBytes();

			return Task.Run(() => EncodeInternal(inputData, token), token);
		}

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public BCnTextureData Encode(ReadOnlyMemory2D<ColorRgba32> input)
		{
			var inputData = new BCnTextureData(CompressionFormat.Rgba32, input.Width, input.Height, input.CopyAsBytes());
			return EncodeInternal(inputData, default);
		}

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public Task<BCnTextureData> EncodeAsync(ReadOnlyMemory2D<ColorRgba32> input, CancellationToken token = default)
		{
			var inputData = new BCnTextureData(CompressionFormat.Rgba32, input.Width, input.Height, input.CopyAsBytes());
			return Task.Run(() => EncodeInternal(inputData, token), token);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer asynchronously. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>The raw encoded input.</returns>
		/// <remarks>To get the width and height of the encoded mip level, see <see cref="CalculateMipMapSize"/>.</remarks>
		public Task<byte[]> EncodeToRawBytesAsync(ReadOnlyMemory2D<ColorRgba32> input, int mipLevel, CancellationToken token = default)
		{
			return Task.Run(() => EncodeSingleLdrInternal(input.Flatten(), input.Width, input.Height, mipLevel, token), token);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer asynchronously. This data does not contain any file headers, just the raw encoded pixel data.
		/// Note that even if the input data already contains mipLevels, new mips are generated from the first mip.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="BCnTextureData"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>The raw encoded input.</returns>
		/// <remarks>To get the width and height of the encoded mip level, see <see cref="CalculateMipMapSize"/>.</remarks>
		public Task<byte[]> EncodeToRawBytesAsync(BCnTextureData input, int mipLevel, CancellationToken token = default)
		{
			return Task.Run(() => EncodeSingleInternal(input.MipLevels[0].Data, input.Format, input.Width, input.Height, mipLevel, token), token);
		}

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode. This can be in any supported format.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="inputFormat">The pixel format the input data is in.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public Task<BCnTextureData> EncodeBytesAsync(byte[] input, int width, int height, CompressionFormat inputFormat, CancellationToken token = default)
		{
			var inputData = new BCnTextureData(inputFormat, width, height, input);
			return Task.Run(() => EncodeInternal(inputData, token), token);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input data to encode. This can be in any supported format.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="inputFormat">The pixel format the input data is in.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <returns>A byte buffer containing the encoded data of the requested mip-level.</returns>
		/// <remarks>To get the width and height of the encoded mip level, see <see cref="CalculateMipMapSize"/>.</remarks>
		public Task<byte[]> EncodeBytesToRawBytesAsync(byte[] input, int width, int height, CompressionFormat inputFormat, int mipLevel, CancellationToken token = default)
		{
			return Task.Run(() => EncodeSingleInternal(input, inputFormat, width, height, mipLevel, token), token);
		}

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode. This can be in any supported format.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="inputFormat">The pixel format the input data is in.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public BCnTextureData EncodeBytes(byte[] input, int width, int height, CompressionFormat inputFormat)
		{
			var inputData = new BCnTextureData(inputFormat, width, height, input);
			return EncodeInternal(inputData, default);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input data to encode. This can be in any supported format.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="inputFormat">The pixel format the input data is in.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="mipWidth">The width of the mipmap.</param>
		/// <param name="mipHeight">The height of the mipmap.</param>
		/// <returns>A byte buffer containing the encoded data of the requested mip-level.</returns>
		public byte[] EncodeBytesToRawBytes(byte[] input, int width, int height, CompressionFormat inputFormat, int mipLevel, out int mipWidth, out int mipHeight)
		{
			CalculateMipMapSize(width, height, mipLevel, out mipWidth, out mipHeight);
			return EncodeSingleInternal(input, inputFormat, width, height, mipLevel, default);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="mipWidth">The width of the mipmap.</param>
		/// <param name="mipHeight">The height of the mipmap.</param>
		/// <returns>A byte buffer containing the encoded data of the requested mip-level.</returns>
		public byte[] EncodeToRawBytes(ReadOnlyMemory2D<ColorRgba32> input, int mipLevel, out int mipWidth, out int mipHeight)
		{
			CalculateMipMapSize(input.Width, input.Height, mipLevel, out mipWidth, out mipHeight);
			return EncodeSingleLdrInternal(input.Flatten(), input.Width, input.Height, mipLevel, default);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		/// Note that even if the input data already contains mipLevels, new mips are generated from the first mip.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="mipWidth">The width of the mipmap.</param>
		/// <param name="mipHeight">The height of the mipmap.</param>
		/// <returns>A byte buffer containing the encoded data of the requested mip-level.</returns>
		public byte[] EncodeToRawBytes(BCnTextureData input, int mipLevel, out int mipWidth, out int mipHeight)
		{
			CalculateMipMapSize(input.Width, input.Height, mipLevel, out mipWidth, out mipHeight);
			return EncodeSingleInternal(input.MipLevels[0].Data, input.Format, input.Width, input.Height, mipLevel, default);
		}

		/// <summary>
		/// Encodes a single 4x4 block to raw encoded bytes. Input Span length must be exactly 16.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <returns>Raw encoded data</returns>
		public byte[] EncodeBlock(ReadOnlySpan<ColorRgba32> inputBlock)
		{
			if (inputBlock.Length != 16)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			return EncodeBlockLdrInternal(inputBlock.AsSpan2D(4, 4));
		}

		/// <summary>
		/// Encodes a single 4x4 block to raw encoded bytes. Input Span width and height must be exactly 4.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <returns>Raw encoded data</returns>
		public byte[] EncodeBlock(ReadOnlySpan2D<ColorRgba32> inputBlock)
		{
			if (inputBlock.Width != 4 || inputBlock.Height != 4)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			return EncodeBlockLdrInternal(inputBlock);
		}

		/// <summary>
		/// Encodes a single 4x4 block and writes the encoded block to a stream. Input Span length must be exactly 16.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <param name="outputStream">Output stream where the encoded block will be written to.</param>
		public void EncodeBlock(ReadOnlySpan<ColorRgba32> inputBlock, Stream outputStream)
		{
			if (inputBlock.Length != 16)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			EncodeBlockLdrInternal(inputBlock.AsSpan2D(4, 4), outputStream);
		}

		/// <summary>
		/// Encodes a single 4x4 block and writes the encoded block to a stream. Input Span width and height must be exactly 4.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <param name="outputStream">Output stream where the encoded block will be written to.</param>
		public void EncodeBlock(ReadOnlySpan2D<ColorRgba32> inputBlock, Stream outputStream)
		{
			if (inputBlock.Width != 4 || inputBlock.Height != 4)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			EncodeBlockLdrInternal(inputBlock, outputStream);
		}

		/// <summary>
		/// Gets the block size of the currently selected compression format in bytes.
		/// </summary>
		/// <returns>The size of a single 4x4 block in bytes</returns>
		public int GetBlockSize()
		{
			var compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				var hdrEncoder = GetFloatBlockEncoder(OutputOptions.Format);
				
				if (hdrEncoder == null)
				{
					throw new NotSupportedException($"This format is either not supported or does not use block compression: {OutputOptions.Format}");
				}

				return hdrEncoder.GetBlockSize();
			}
			return compressedEncoder.GetBlockSize();
		}

		/// <summary>
		/// Gets the number of total blocks in an image with the given pixel width and height.
		/// </summary>
		/// <param name="pixelWidth">The pixel width of the image</param>
		/// <param name="pixelHeight">The pixel height of the image</param>
		/// <returns>The total number of blocks.</returns>
		public int GetBlockCount(int pixelWidth, int pixelHeight)
		{
			return ImageToBlocks.CalculateNumOfBlocks(pixelWidth, pixelHeight);
		}

		/// <summary>
		/// Gets the number of blocks in an image with the given pixel width and height.
		/// </summary>
		/// <param name="pixelWidth">The pixel width of the image</param>
		/// <param name="pixelHeight">The pixel height of the image</param>
		/// <param name="blocksWidth">The amount of blocks in the x-axis</param>
		/// <param name="blocksHeight">The amount of blocks in the y-axis</param>
		public void GetBlockCount(int pixelWidth, int pixelHeight, out int blocksWidth, out int blocksHeight)
		{
			ImageToBlocks.CalculateNumOfBlocks(pixelWidth, pixelHeight, out blocksWidth, out blocksHeight);
		}

		#endregion

		#region HDR

		/// <summary>
		/// Encode a cubemap as raw bytes into a <see cref="BCnTextureData"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public BCnTextureData EncodeCubeMapHdr(ReadOnlyMemory2D<ColorRgbaFloat> right, ReadOnlyMemory2D<ColorRgbaFloat> left,
			ReadOnlyMemory2D<ColorRgbaFloat> top, ReadOnlyMemory2D<ColorRgbaFloat> down,
			ReadOnlyMemory2D<ColorRgbaFloat> back, ReadOnlyMemory2D<ColorRgbaFloat> front)
		{
			var inputData = new BCnTextureData(CompressionFormat.RgbaFloat, right.Width, right.Height, 1, true, false);

			if (
				right.Width != left.Width || right.Height != left.Height ||
				right.Width != top.Width || right.Height != top.Height ||
				right.Width != down.Width || right.Height != down.Height ||
				right.Width != back.Width || right.Height != back.Height ||
				right.Width != front.Width || right.Height != front.Height
			)
			{
				throw new ArgumentException("All faces of a cubeMap must be of equal width and height!");
			}

			inputData[CubeMapFaceDirection.XPositive].Mips[0].Data = right.CopyAsBytes();
			inputData[CubeMapFaceDirection.XNegative].Mips[0].Data = left.CopyAsBytes();
			inputData[CubeMapFaceDirection.YPositive].Mips[0].Data = top.CopyAsBytes();
			inputData[CubeMapFaceDirection.YNegative].Mips[0].Data = down.CopyAsBytes();
			inputData[CubeMapFaceDirection.ZPositive].Mips[0].Data = back.CopyAsBytes();
			inputData[CubeMapFaceDirection.ZNegative].Mips[0].Data = front.CopyAsBytes();

			return EncodeInternal(inputData, default);
		}

		/// <summary>
		/// Encode a cubemap as raw bytes into a <see cref="BCnTextureData"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public Task<BCnTextureData> EncodeCubeMapHdrAsync(ReadOnlyMemory2D<ColorRgbaFloat> right, ReadOnlyMemory2D<ColorRgbaFloat> left,
			ReadOnlyMemory2D<ColorRgbaFloat> top, ReadOnlyMemory2D<ColorRgbaFloat> down,
			ReadOnlyMemory2D<ColorRgbaFloat> back, ReadOnlyMemory2D<ColorRgbaFloat> front, CancellationToken token = default)
		{
			var inputData = new BCnTextureData(CompressionFormat.RgbaFloat, right.Width, right.Height, 1, true, false);

			if (
				right.Width != left.Width || right.Height != left.Height ||
				right.Width != top.Width || right.Height != top.Height ||
				right.Width != down.Width || right.Height != down.Height ||
				right.Width != back.Width || right.Height != back.Height ||
				right.Width != front.Width || right.Height != front.Height
			)
			{
				throw new ArgumentException("All faces of a cubeMap must be of equal width and height!");
			}

			inputData[CubeMapFaceDirection.XPositive].Mips[0].Data = right.CopyAsBytes();
			inputData[CubeMapFaceDirection.XNegative].Mips[0].Data = left.CopyAsBytes();
			inputData[CubeMapFaceDirection.YPositive].Mips[0].Data = top.CopyAsBytes();
			inputData[CubeMapFaceDirection.YNegative].Mips[0].Data = down.CopyAsBytes();
			inputData[CubeMapFaceDirection.ZPositive].Mips[0].Data = back.CopyAsBytes();
			inputData[CubeMapFaceDirection.ZNegative].Mips[0].Data = front.CopyAsBytes();

			return Task.Run(() => EncodeInternal(inputData, token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels of a HDR image into <see cref="BCnTextureData"/>. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		public Task<BCnTextureData> EncodeHdrAsync(ReadOnlyMemory2D<ColorRgbaFloat> input, CancellationToken token = default)
		{
			var inputData = new BCnTextureData(
				CompressionFormat.RgbaFloat,
				input.Width,
				input.Height,
				input.CopyAsBytes());
			return Task.Run(() => EncodeInternal(inputData, token), token);
		}

		/// <summary>
		/// Encodes a single mip level of the input HDR image to a byte buffer asynchronously. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>The raw encoded input.</returns>
		/// <remarks>To get the width and height of the encoded mip level, see <see cref="CalculateMipMapSize"/>.</remarks>
		public Task<byte[]> EncodeToRawBytesHdrAsync(ReadOnlyMemory2D<ColorRgbaFloat> input, int mipLevel, CancellationToken token = default)
		{
			return Task.Run(() => EncodeSingleHdrInternal(input.Flatten(), input.Width, input.Height, mipLevel, token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels of a HDR image into <see cref="BCnTextureData"/>. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		public BCnTextureData EncodeHdr(ReadOnlyMemory2D<ColorRgbaFloat> input)
		{
			var inputData = new BCnTextureData(
				CompressionFormat.RgbaFloat,
				input.Width,
				input.Height,
				input.CopyAsBytes());
			return EncodeInternal(inputData, default);
		}

		/// <summary>
		/// Encodes a single mip level of the HDR input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="mipWidth">The width of the mipmap.</param>
		/// <param name="mipHeight">The height of the mipmap.</param>
		/// <returns>A byte buffer containing the encoded data of the requested mip-level.</returns>
		public byte[] EncodeToRawBytesHdr(ReadOnlyMemory2D<ColorRgbaFloat> input, int mipLevel, out int mipWidth, out int mipHeight)
		{
			CalculateMipMapSize(input.Width, input.Height, mipLevel, out mipWidth, out mipHeight);
			return EncodeSingleHdrInternal(input.Flatten(), input.Width, input.Height, mipLevel, default);
		}

		/// <summary>
		/// Encodes a single 4x4 HDR block to raw encoded bytes. Input Span length must be exactly 16.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <returns>Raw encoded data</returns>
		public byte[] EncodeBlockHdr(ReadOnlySpan<ColorRgbFloat> inputBlock)
		{
			if (inputBlock.Length != 16)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			return EncodeBlockHdrInternal(inputBlock.AsSpan2D(4, 4));
		}

		/// <summary>
		/// Encodes a single 4x4 HDR block to raw encoded bytes. Input Span width and height must be exactly 4.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <returns>Raw encoded data</returns>
		public byte[] EncodeBlockHdr(ReadOnlySpan2D<ColorRgbFloat> inputBlock)
		{
			if (inputBlock.Width != 4 || inputBlock.Height != 4)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			return EncodeBlockHdrInternal(inputBlock);
		}

		/// <summary>
		/// Encodes a single 4x4 HDR block and writes the encoded block to a stream. Input Span length must be exactly 16.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <param name="outputStream">Output stream where the encoded block will be written to.</param>
		public void EncodeBlockHdr(ReadOnlySpan<ColorRgbFloat> inputBlock, Stream outputStream)
		{
			if (inputBlock.Length != 16)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			EncodeBlockHdrInternal(inputBlock.AsSpan2D(4, 4), outputStream);
		}

		/// <summary>
		/// Encodes a single 4x4 HDR block and writes the encoded block to a stream. Input Span width and height must be exactly 4.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <param name="outputStream">Output stream where the encoded block will be written to.</param>
		public void EncodeBlockHdr(ReadOnlySpan2D<ColorRgbFloat> inputBlock, Stream outputStream)
		{
			if (inputBlock.Width != 4 || inputBlock.Height != 4)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			EncodeBlockHdrInternal(inputBlock, outputStream);
		}

		#endregion
		#region MipMap operations

		/// <summary>
		/// Calculates the number of mipmap levels that will be generated for the given input image.
		/// This method takes into account <see cref="EncoderOutputOptions.GenerateMipMaps"/> and
		/// <see cref="EncoderOutputOptions.MaxMipMapLevel"/>.
		/// </summary>
		/// <param name="imagePixelWidth">The width of the input image in pixels</param>
		/// <param name="imagePixelHeight">The height of the input image in pixels</param>
		/// <returns>The number of mipmap levels that will be generated for the input image.</returns>
		public int CalculateNumberOfMipLevels(int imagePixelWidth, int imagePixelHeight)
		{
			return MipMapper.CalculateMipChainLength(imagePixelWidth, imagePixelHeight,
				OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1);
		}

		/// <summary>
		/// Calculates the size of a given mipmap level.
		/// </summary>
		/// <param name="imagePixelWidth">The width of the input image in pixels</param>
		/// <param name="imagePixelHeight">The height of the input image in pixels</param>
		/// <param name="mipLevel">The mipLevel to calculate (0 is original image)</param>
		/// <param name="mipWidth">The mipmap width calculated</param>
		/// <param name="mipHeight">The mipmap height calculated</param>
		public void CalculateMipMapSize(int imagePixelWidth, int imagePixelHeight, int mipLevel, out int mipWidth, out int mipHeight)
		{
			MipMapper.CalculateMipLevelSize(imagePixelWidth, imagePixelHeight, mipLevel, out mipWidth,
				out mipHeight);
		}

		/// <summary>
		/// Calculates the byte size of a given mipmap level.
		/// This takes into account the current <see cref="EncoderOutputOptions.Format"/>
		/// </summary>
		/// <param name="imagePixelWidth">The width of the input image in pixels</param>
		/// <param name="imagePixelHeight">The height of the input image in pixels</param>
		/// <param name="mipLevel">The mipLevel to calculate (0 is original image)</param>
		public long CalculateMipMapByteSize(int imagePixelWidth, int imagePixelHeight, int mipLevel)
		{
			MipMapper.CalculateMipLevelSize(imagePixelWidth, imagePixelHeight, mipLevel, out var mipWidth,
				out var mipHeight);
			return OutputOptions.Format.CalculateMipByteSize(mipWidth, mipHeight);
		}

		#endregion

		#region Private

		private byte[] EncodeSingleHdrInternal(ReadOnlyMemory<ColorRgbaFloat> inputData, int width, int height, int mipLevel, CancellationToken token)
			=> EncodeSingleInternal(inputData.AsBytes(), CompressionFormat.RgbaFloat, width, height, mipLevel, token);

		private byte[] EncodeSingleLdrInternal(ReadOnlyMemory<ColorRgba32> inputData, int width, int height, int mipLevel,
			CancellationToken token)
			=> EncodeSingleInternal(inputData.AsBytes(), CompressionFormat.Rgba32, width, height, mipLevel, token);

		private byte[] EncodeSingleInternal(ReadOnlyMemory<byte> input, CompressionFormat inputFormat, int width,
			int height, int mipLevel, CancellationToken token)
		{
			var encoder = GetEncoder(OutputOptions.Format);

			if (encoder == null)
			{
				throw new NotSupportedException($"This format is not supported for this method: {OutputOptions.Format}");
			}

			if (encoder.EncodedFormat != OutputOptions.Format)
			{
				throw new InvalidOperationException("Encoder format did not match expected format!");
			}

			if (inputFormat.IsBlockCompressedFormat())
			{
				throw new InvalidOperationException("Single mip encoding only supports raw formats as inputs!");
			}

			MipMapper.CalculateMipLevelSize(width, height, mipLevel, out var mipWidth, out var mipHeight);

			var totalBlocks = encoder.EncodedFormat.CalculateMipByteSize(mipWidth, mipHeight) / encoder.EncodedFormat.BytesPerBlock();

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount,
				Progress = new OperationProgress(Options.Progress, totalBlocks)
			};
			
			if (OutputOptions.Format.IsHdrFormat())
			{
				ReadOnlyMemory<ColorRgbaFloat> floatData;
				if (inputFormat != CompressionFormat.RgbaFloat)
				{
					floatData = ColorExtensions.InternalConvertToAsBytesFromBytes(input, inputFormat, CompressionFormat.RgbaFloat)
						.AsMemory().Cast<byte, ColorRgbaFloat>();
				}
				else
				{
					floatData = input.Cast<byte, ColorRgbaFloat>();
				}
				MipMapper.GenerateSingleMip(floatData.AsMemory2D(height, width), mipLevel).TryGetMemory(out floatData);

				return encoder.Encode(floatData, mipWidth, mipHeight, OutputOptions.Quality, context);
			}
			else
			{
				var ldrEncoder = encoder as IBcLdrEncoder;
				if (ldrEncoder == null)
				{
					throw new InvalidOperationException($"No LDR encoder found for supposedly ldr format: {OutputOptions.Format}.");
				}
				ReadOnlyMemory<ColorRgba32> rgbaData;
				if (inputFormat != CompressionFormat.Rgba32)
				{
					rgbaData = ColorExtensions.InternalConvertToAsBytesFromBytes(input, inputFormat, CompressionFormat.Rgba32)
						.AsMemory().Cast<byte, ColorRgba32>();
				}
				else
				{
					rgbaData = input.Cast<byte, ColorRgba32>();
				}
				MipMapper.GenerateSingleMip(rgbaData.AsMemory2D(height, width), mipLevel).TryGetMemory(out rgbaData);

				return ldrEncoder.Encode(rgbaData, mipWidth, mipHeight, OutputOptions.Quality, context);
			}
		}

		private BCnTextureData EncodeInternal(BCnTextureData textureData, CancellationToken token)
		{
			var encoder = GetEncoder(OutputOptions.Format);

			if (encoder.EncodedFormat != OutputOptions.Format)
			{
				throw new InvalidOperationException("Encoder format did not match expected format!");
			}

			var isLdr = !OutputOptions.Format.IsHdrFormat();
			var ldrEncoder = encoder as IBcLdrEncoder;

			if (isLdr && ldrEncoder == null)
			{
				throw new InvalidOperationException($"No LDR encoder found for supposedly ldr format: {OutputOptions.Format}.");
			}

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;

			if (isLdr)
			{
				textureData = MipMapper.GenerateMipChainLdr(textureData, ref numMipMaps);
			}
			else
			{
				textureData = MipMapper.GenerateMipChainHdr(textureData, ref numMipMaps);
			}

			var newData = new BCnTextureData(
				encoder.EncodedFormat,
				textureData.Width,
				textureData.Height,
				numMipMaps,
				textureData.IsCubeMap, false);

			var totalBlocks = newData.TotalSize / encoder.EncodedFormat.BytesPerBlock();

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount,
				Progress = new OperationProgress(Options.Progress, totalBlocks)
			};

			for (var f = 0; f < newData.NumFaces; f++)
			{
				for (var m = 0; m < newData.NumMips; m++)
				{
					if (isLdr)
					{
						var mipWidth = textureData.Faces[f].Mips[m].Width;
						var mipHeight = textureData.Faces[f].Mips[m].Height;
						var colorMemory = textureData.Faces[f].Mips[m].AsMemory<ColorRgba32>();
						var encoded = ldrEncoder.Encode(colorMemory, mipWidth, mipHeight, OutputOptions.Quality, context);

						if (newData.Faces[f].Mips[m].SizeInBytes != encoded.Length)
						{
							throw new InvalidOperationException("Encoded size does not match expected!");
						}

						newData.Faces[f].Mips[m].Data = encoded;
					}
					else
					{
						var mipWidth = textureData.Faces[f].Mips[m].Width;
						var mipHeight = textureData.Faces[f].Mips[m].Height;
						var colorMemory = textureData.Faces[f].Mips[m].AsMemory<ColorRgbaFloat>();
						var encoded = encoder.Encode(colorMemory, mipWidth, mipHeight, OutputOptions.Quality, context);

						if (newData.Faces[f].Mips[m].SizeInBytes != encoded.Length)
						{
							throw new InvalidOperationException("Encoded size does not match expected!");
						}

						newData.Faces[f].Mips[m].Data = encoded;
					}
				}
			}

			return newData;
		}

		private byte[] EncodeBlockHdrInternal(ReadOnlySpan2D<ColorRgbFloat> input)
		{
			var compressedEncoder = GetFloatBlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported for hdr single block encoding: {OutputOptions.Format}");
			}

			var output = new byte[compressedEncoder.GetBlockSize()];

			var rawBlock = new RawBlock4X4RgbFloat();

			var pixels = rawBlock.AsSpan;

			input.GetRowSpan(0).CopyTo(pixels);
			input.GetRowSpan(1).CopyTo(pixels.Slice(4));
			input.GetRowSpan(2).CopyTo(pixels.Slice(8));
			input.GetRowSpan(3).CopyTo(pixels.Slice(12));

			compressedEncoder.EncodeBlock(rawBlock, OutputOptions.Quality, output);

			return output;
		}

		private void EncodeBlockHdrInternal(ReadOnlySpan2D<ColorRgbFloat> input, Stream outputStream)
		{
			var compressedEncoder = GetFloatBlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported for hdr single block encoding: {OutputOptions.Format}");
			}
			if (input.Width != 4 || input.Height != 4)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}

			Span<byte> output = stackalloc byte[16];
			output = output.Slice(0, compressedEncoder.GetBlockSize());

			var rawBlock = new RawBlock4X4RgbFloat();

			var pixels = rawBlock.AsSpan;

			input.GetRowSpan(0).CopyTo(pixels);
			input.GetRowSpan(1).CopyTo(pixels.Slice(4));
			input.GetRowSpan(2).CopyTo(pixels.Slice(8));
			input.GetRowSpan(3).CopyTo(pixels.Slice(12));

			compressedEncoder.EncodeBlock(rawBlock, OutputOptions.Quality, output);

			outputStream.Write(output);
		}

		private byte[] EncodeBlockLdrInternal(ReadOnlySpan2D<ColorRgba32> input)
		{
			var compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported for ldr single block encoding: {OutputOptions.Format}");
			}

			var output = new byte[compressedEncoder.GetBlockSize()];

			var rawBlock = new RawBlock4X4Rgba32();

			var pixels = rawBlock.AsSpan;

			input.GetRowSpan(0).CopyTo(pixels);
			input.GetRowSpan(1).CopyTo(pixels.Slice(4));
			input.GetRowSpan(2).CopyTo(pixels.Slice(8));
			input.GetRowSpan(3).CopyTo(pixels.Slice(12));

			compressedEncoder.EncodeBlock(rawBlock, OutputOptions.Quality, output);

			return output;
		}

		private void EncodeBlockLdrInternal(ReadOnlySpan2D<ColorRgba32> input, Stream outputStream)
		{
			var compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported for ldr single block encoding: {OutputOptions.Format}");
			}
			if (input.Width != 4 || input.Height != 4)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}

			Span<byte> output = stackalloc byte[16];
			output = output.Slice(0, compressedEncoder.GetBlockSize());

			var rawBlock = new RawBlock4X4Rgba32();

			var pixels = rawBlock.AsSpan;

			input.GetRowSpan(0).CopyTo(pixels);
			input.GetRowSpan(1).CopyTo(pixels.Slice(4));
			input.GetRowSpan(2).CopyTo(pixels.Slice(8));
			input.GetRowSpan(3).CopyTo(pixels.Slice(12));

			compressedEncoder.EncodeBlock(rawBlock, OutputOptions.Quality, output);

			outputStream.Write(output);
		}

		#endregion

		#region Support

		private IBcBlockEncoder<RawBlock4X4Rgba32> GetRgba32BlockEncoder(CompressionFormat format)
		{
			return GetEncoder(format) as IBcBlockEncoder<RawBlock4X4Rgba32>;
		}

		private IBcBlockEncoder<RawBlock4X4RgbFloat> GetFloatBlockEncoder(CompressionFormat format)
		{
			return GetEncoder(format) as IBcBlockEncoder<RawBlock4X4RgbFloat>;
		}

		private IBcEncoder GetEncoder(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R8:
					return new RawPixelLdrEncoder<ColorR8>(format);
				case CompressionFormat.R8G8:
					return new RawPixelLdrEncoder<ColorR8G8>(format);
				case CompressionFormat.Rgb24:
					return new RawPixelLdrEncoder<ColorRgb24>(format);
				case CompressionFormat.Bgr24:
					return new RawPixelLdrEncoder<ColorBgr24>(format);
				case CompressionFormat.Rgba32:
					return new RawPixelLdrEncoder<ColorRgba32>(format);
				case CompressionFormat.Bgra32:
					return new RawPixelLdrEncoder<ColorBgra32>(format);
				case CompressionFormat.RgbaFloat:
					return new RawPixelEncoder<ColorRgbaFloat>(format);
				case CompressionFormat.RgbaHalf:
					return new RawPixelEncoder<ColorRgbaHalf>(format);
				case CompressionFormat.RgbFloat:
					return new RawPixelEncoder<ColorRgbFloat>(format);
				case CompressionFormat.RgbHalf:
					return new RawPixelEncoder<ColorRgbHalf>(format);
				case CompressionFormat.Rgbe:
					return new RawPixelEncoder<ColorRgbe>(format);
				case CompressionFormat.Xyze:
					return new RawPixelEncoder<ColorXyze>(format);
				case CompressionFormat.Bc1:
					return new Bc1BlockEncoder();
				case CompressionFormat.Bc1WithAlpha:
					return new Bc1AlphaBlockEncoder();
				case CompressionFormat.Bc2:
					return new Bc2BlockEncoder();
				case CompressionFormat.Bc3:
					return new Bc3BlockEncoder();
				case CompressionFormat.Bc4:
					return new Bc4BlockEncoder(InputOptions.Bc4Component);
				case CompressionFormat.Bc5:
					return new Bc5BlockEncoder(InputOptions.Bc5Component1, InputOptions.Bc5Component2);
				case CompressionFormat.Bc6U:
					return new Bc6Encoder(false);
				case CompressionFormat.Bc6S:
					return new Bc6Encoder(true);
				case CompressionFormat.Bc7:
					return new Bc7Encoder();
				case CompressionFormat.Atc:
					return new AtcBlockEncoder();
				case CompressionFormat.AtcExplicitAlpha:
					return new AtcExplicitAlphaBlockEncoder();
				case CompressionFormat.AtcInterpolatedAlpha:
					return new AtcInterpolatedAlphaBlockEncoder();
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}
		#endregion
	}
}
