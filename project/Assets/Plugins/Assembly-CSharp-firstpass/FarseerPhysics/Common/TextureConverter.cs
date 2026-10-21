using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UnityEngine;

namespace FarseerPhysics.Common
{
	public sealed class TextureConverter
	{
		private const int _CLOSEPIXELS_LENGTH = 8;

		private static int[,] ClosePixels = new int[8, 2]
		{
			{ -1, -1 },
			{ 0, -1 },
			{ 1, -1 },
			{ 1, 0 },
			{ 1, 1 },
			{ 0, 1 },
			{ -1, 1 },
			{ -1, 0 }
		};

		private uint[] _data;

		private int _dataLength;

		private int _width;

		private int _height;

		private VerticesDetectionType _polygonDetectionType;

		private uint _alphaTolerance;

		private float _hullTolerance;

		private bool _holeDetection;

		private bool _multipartDetection;

		private bool _pixelOffsetOptimization;

		private FMatrix _transform = FMatrix.Identity;

		private int _tempIsSolidX;

		private int _tempIsSolidY;

		public VerticesDetectionType PolygonDetectionType
		{
			get
			{
				return _polygonDetectionType;
			}
			set
			{
				_polygonDetectionType = value;
			}
		}

		public bool HoleDetection
		{
			get
			{
				return _holeDetection;
			}
			set
			{
				_holeDetection = value;
			}
		}

		public bool MultipartDetection
		{
			get
			{
				return _multipartDetection;
			}
			set
			{
				_multipartDetection = value;
			}
		}

		public bool PixelOffsetOptimization
		{
			get
			{
				return _pixelOffsetOptimization;
			}
			set
			{
				_pixelOffsetOptimization = value;
			}
		}

		public FMatrix Transform
		{
			get
			{
				return _transform;
			}
			set
			{
				_transform = value;
			}
		}

		public byte AlphaTolerance
		{
			get
			{
				return (byte)(_alphaTolerance >> 24);
			}
			set
			{
				_alphaTolerance = (uint)(value << 24);
			}
		}

		public float HullTolerance
		{
			get
			{
				return _hullTolerance;
			}
			set
			{
				if (value > 4f)
				{
					_hullTolerance = 4f;
				}
				else if (value < 0.9f)
				{
					_hullTolerance = 0.9f;
				}
				else
				{
					_hullTolerance = value;
				}
			}
		}

		public TextureConverter()
		{
			Initialize(null, null, null, null, null, null, null, null);
		}

		public TextureConverter(byte? alphaTolerance, float? hullTolerance, bool? holeDetection, bool? multipartDetection, bool? pixelOffsetOptimization, FMatrix? transform)
		{
			Initialize(null, null, alphaTolerance, hullTolerance, holeDetection, multipartDetection, pixelOffsetOptimization, transform);
		}

		public TextureConverter(uint[] data, int width)
		{
			Initialize(data, width, null, null, null, null, null, null);
		}

		public TextureConverter(uint[] data, int width, byte? alphaTolerance, float? hullTolerance, bool? holeDetection, bool? multipartDetection, bool? pixelOffsetOptimization, FMatrix? transform)
		{
			Initialize(data, width, alphaTolerance, hullTolerance, holeDetection, multipartDetection, pixelOffsetOptimization, transform);
		}

		private void Initialize(uint[] data, int? width, byte? alphaTolerance, float? hullTolerance, bool? holeDetection, bool? multipartDetection, bool? pixelOffsetOptimization, FMatrix? transform)
		{
			if (data != null && !width.HasValue)
			{
				throw new ArgumentNullException("width", "'width' can't be null if 'data' is set.");
			}
			if (data == null && width.HasValue)
			{
				throw new ArgumentNullException("data", "'data' can't be null if 'width' is set.");
			}
			if (data != null && width.HasValue)
			{
				SetTextureData(data, width.Value);
			}
			if (alphaTolerance.HasValue)
			{
				AlphaTolerance = alphaTolerance.Value;
			}
			else
			{
				AlphaTolerance = 20;
			}
			if (hullTolerance.HasValue)
			{
				HullTolerance = hullTolerance.Value;
			}
			else
			{
				HullTolerance = 1.5f;
			}
			if (holeDetection.HasValue)
			{
				HoleDetection = holeDetection.Value;
			}
			else
			{
				HoleDetection = false;
			}
			if (multipartDetection.HasValue)
			{
				MultipartDetection = multipartDetection.Value;
			}
			else
			{
				MultipartDetection = false;
			}
			if (pixelOffsetOptimization.HasValue)
			{
				PixelOffsetOptimization = pixelOffsetOptimization.Value;
			}
			else
			{
				PixelOffsetOptimization = false;
			}
			if (transform.HasValue)
			{
				Transform = transform.Value;
			}
			else
			{
				Transform = FMatrix.Identity;
			}
		}

		private void SetTextureData(uint[] data, int width)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data", "'data' can't be null.");
			}
			if (data.Length < 4)
			{
				throw new ArgumentOutOfRangeException("data", "'data' length can't be less then 4. Your texture must be at least 2 x 2 pixels in size.");
			}
			if (width < 2)
			{
				throw new ArgumentOutOfRangeException("width", "'width' can't be less then 2. Your texture must be at least 2 x 2 pixels in size.");
			}
			if (data.Length % width != 0)
			{
				throw new ArgumentException("'width' has an invalid value.");
			}
			_data = data;
			_dataLength = _data.Length;
			_width = width;
			_height = _dataLength / width;
		}

		public static Vertices DetectVertices(uint[] data, int width)
		{
			TextureConverter textureConverter = new TextureConverter(data, width);
			List<DetectedVertices> list = textureConverter.DetectVertices();
			return list[0];
		}

		public static Vertices DetectVertices(uint[] data, int width, bool holeDetection)
		{
			TextureConverter textureConverter = new TextureConverter(data, width);
			textureConverter.HoleDetection = holeDetection;
			TextureConverter textureConverter2 = textureConverter;
			List<DetectedVertices> list = textureConverter2.DetectVertices();
			return list[0];
		}

		public static List<Vertices> DetectVertices(uint[] data, int width, float hullTolerance, byte alphaTolerance, bool multiPartDetection, bool holeDetection)
		{
			TextureConverter textureConverter = new TextureConverter(data, width);
			textureConverter.HullTolerance = hullTolerance;
			textureConverter.AlphaTolerance = alphaTolerance;
			textureConverter.MultipartDetection = multiPartDetection;
			textureConverter.HoleDetection = holeDetection;
			TextureConverter textureConverter2 = textureConverter;
			List<DetectedVertices> list = textureConverter2.DetectVertices();
			List<Vertices> list2 = new List<Vertices>();
			for (int i = 0; i < list.Count; i++)
			{
				list2.Add(list[i]);
			}
			return list2;
		}

		public List<DetectedVertices> DetectVertices()
		{
			if (_data == null)
			{
				throw new Exception("'_data' can't be null. You have to use SetTextureData(uint[] data, int width) before calling this method.");
			}
			if (_data.Length < 4)
			{
				throw new Exception("'_data' length can't be less then 4. Your texture must be at least 2 x 2 pixels in size. You have to use SetTextureData(uint[] data, int width) before calling this method.");
			}
			if (_width < 2)
			{
				throw new Exception("'_width' can't be less then 2. Your texture must be at least 2 x 2 pixels in size. You have to use SetTextureData(uint[] data, int width) before calling this method.");
			}
			if (_data.Length % _width != 0)
			{
				throw new Exception("'_width' has an invalid value. You have to use SetTextureData(uint[] data, int width) before calling this method.");
			}
			List<DetectedVertices> detectedPolygons = new List<DetectedVertices>();
			FVector2? lastHoleEntrance = null;
			FVector2? entrance = null;
			List<FVector2> list = new List<FVector2>();
			bool flag;
			do
			{
				DetectedVertices detectedVertices;
				if (detectedPolygons.Count == 0)
				{
					detectedVertices = new DetectedVertices(CreateSimplePolygon(FVector2.Zero, FVector2.Zero));
					if (detectedVertices.Count > 2)
					{
						entrance = GetTopMostVertex(detectedVertices);
					}
				}
				else
				{
					if (!entrance.HasValue)
					{
						break;
					}
					detectedVertices = new DetectedVertices(CreateSimplePolygon(entrance.Value, new FVector2(entrance.Value.X - 1f, entrance.Value.Y)));
				}
				flag = false;
				if (detectedVertices.Count > 2)
				{
					if (_holeDetection)
					{
						while (true)
						{
							lastHoleEntrance = SearchHoleEntrance(detectedVertices, lastHoleEntrance);
							if (!lastHoleEntrance.HasValue || list.Contains(lastHoleEntrance.Value))
							{
								break;
							}
							list.Add(lastHoleEntrance.Value);
							Vertices vertices = CreateSimplePolygon(lastHoleEntrance.Value, new FVector2(lastHoleEntrance.Value.X + 1f, lastHoleEntrance.Value.Y));
							if (vertices == null || vertices.Count <= 2)
							{
								continue;
							}
							switch (_polygonDetectionType)
							{
							case VerticesDetectionType.Integrated:
							{
								vertices.Add(vertices[0]);
								int vertex1Index;
								int vertex2Index;
								if (SplitPolygonEdge(detectedVertices, lastHoleEntrance.Value, out vertex1Index, out vertex2Index))
								{
									detectedVertices.InsertRange(vertex2Index, vertices);
								}
								break;
							}
							case VerticesDetectionType.Separated:
								if (detectedVertices.Holes == null)
								{
									detectedVertices.Holes = new List<Vertices>();
								}
								detectedVertices.Holes.Add(vertices);
								Debug.LogError("HOLE ADDED");
								break;
							}
						}
					}
					detectedPolygons.Add(detectedVertices);
				}
				if ((_multipartDetection || detectedVertices.Count <= 2) && SearchNextHullEntrance(detectedPolygons, entrance.Value, out entrance))
				{
					flag = true;
				}
			}
			while (flag);
			if (detectedPolygons == null || (detectedPolygons != null && detectedPolygons.Count == 0))
			{
				throw new Exception("Couldn't detect any vertices.");
			}
			if (PolygonDetectionType == VerticesDetectionType.Separated)
			{
				ApplyTriangulationCompatibleWinding(ref detectedPolygons);
			}
			if (_pixelOffsetOptimization)
			{
				ApplyPixelOffsetOptimization(ref detectedPolygons);
			}
			if (_transform != FMatrix.Identity)
			{
				ApplyTransform(ref detectedPolygons);
			}
			return detectedPolygons;
		}

		private void ApplyTriangulationCompatibleWinding(ref List<DetectedVertices> detectedPolygons)
		{
			for (int i = 0; i < detectedPolygons.Count; i++)
			{
				detectedPolygons[i].Reverse();
				if (detectedPolygons[i].Holes != null && detectedPolygons[i].Holes.Count > 0)
				{
					for (int j = 0; j < detectedPolygons[i].Holes.Count; j++)
					{
						detectedPolygons[i].Holes[j].Reverse();
					}
				}
			}
		}

		private void ApplyPixelOffsetOptimization(ref List<DetectedVertices> detectedPolygons)
		{
		}

		private void ApplyTransform(ref List<DetectedVertices> detectedPolygons)
		{
			for (int i = 0; i < detectedPolygons.Count; i++)
			{
				detectedPolygons[i].Transform(_transform);
			}
		}

		public bool IsSolid(ref FVector2 v)
		{
			_tempIsSolidX = (int)v.X;
			_tempIsSolidY = (int)v.Y;
			if (_tempIsSolidX >= 0 && _tempIsSolidX < _width && _tempIsSolidY >= 0 && _tempIsSolidY < _height)
			{
				return _data[_tempIsSolidX + _tempIsSolidY * _width] >= _alphaTolerance;
			}
			return false;
		}

		public bool IsSolid(int x, int y)
		{
			if (x >= 0 && x < _width && y >= 0 && y < _height)
			{
				return _data[x + y * _width] >= _alphaTolerance;
			}
			return false;
		}

		public bool IsSolid(int index)
		{
			if (index >= 0 && index < _dataLength)
			{
				return _data[index] >= _alphaTolerance;
			}
			return false;
		}

		public bool InBounds(ref FVector2 coord)
		{
			return coord.X >= 0f && coord.X < (float)_width && coord.Y >= 0f && coord.Y < (float)_height;
		}

		private FVector2? SearchHoleEntrance(Vertices polygon, FVector2? lastHoleEntrance)
		{
			if (polygon == null)
			{
				throw new ArgumentNullException("'polygon' can't be null.");
			}
			if (polygon.Count < 3)
			{
				throw new ArgumentException("'polygon.MainPolygon.Count' can't be less then 3.");
			}
			int num = 0;
			int num2 = ((!lastHoleEntrance.HasValue) ? ((int)GetTopMostCoord(polygon)) : ((int)lastHoleEntrance.Value.Y));
			int num3 = (int)GetBottomMostCoord(polygon);
			if (num2 > 0 && num2 < _height && num3 > 0 && num3 < _height)
			{
				for (int i = num2; i <= num3; i++)
				{
					List<float> list = SearchCrossingEdges(polygon, i);
					if (list.Count > 1 && list.Count % 2 == 0)
					{
						for (int j = 0; j < list.Count; j += 2)
						{
							bool flag = false;
							bool flag2 = false;
							for (int k = (int)list[j]; k <= (int)list[j + 1]; k++)
							{
								if (IsSolid(k, i))
								{
									if (!flag2)
									{
										flag = true;
										num = k;
									}
									if (flag && flag2)
									{
										FVector2? result = new FVector2(num, i);
										if (DistanceToHullAcceptable(polygon, result.Value, true))
										{
											return result;
										}
										result = null;
										break;
									}
								}
								else if (flag)
								{
									flag2 = true;
								}
							}
						}
					}
					else if (list.Count % 2 != 0)
					{
					}
				}
			}
			return null;
		}

		private bool DistanceToHullAcceptable(DetectedVertices polygon, FVector2 point, bool higherDetail)
		{
			if (polygon == null)
			{
				throw new ArgumentNullException("polygon", "'polygon' can't be null.");
			}
			if (polygon.Count < 3)
			{
				throw new ArgumentException("'polygon.MainPolygon.Count' can't be less then 3.");
			}
			if (DistanceToHullAcceptable((Vertices)polygon, point, higherDetail))
			{
				if (polygon.Holes != null)
				{
					for (int i = 0; i < polygon.Holes.Count; i++)
					{
						if (!DistanceToHullAcceptable(polygon.Holes[i], point, higherDetail))
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		private bool DistanceToHullAcceptable(Vertices polygon, FVector2 point, bool higherDetail)
		{
			if (polygon == null)
			{
				throw new ArgumentNullException("polygon", "'polygon' can't be null.");
			}
			if (polygon.Count < 3)
			{
				throw new ArgumentException("'polygon.Count' can't be less then 3.");
			}
			FVector2 lineEndPoint = polygon[polygon.Count - 1];
			if (higherDetail)
			{
				for (int i = 0; i < polygon.Count; i++)
				{
					FVector2 lineEndPoint2 = polygon[i];
					if (LineTools.DistanceBetweenPointAndLineSegment(ref point, ref lineEndPoint2, ref lineEndPoint) <= _hullTolerance || LineTools.DistanceBetweenPointAndPoint(ref point, ref lineEndPoint2) <= _hullTolerance)
					{
						return false;
					}
					lineEndPoint = polygon[i];
				}
				return true;
			}
			for (int j = 0; j < polygon.Count; j++)
			{
				FVector2 lineEndPoint2 = polygon[j];
				if (LineTools.DistanceBetweenPointAndLineSegment(ref point, ref lineEndPoint2, ref lineEndPoint) <= _hullTolerance)
				{
					return false;
				}
				lineEndPoint = polygon[j];
			}
			return true;
		}

		private bool InPolygon(DetectedVertices polygon, FVector2 point)
		{
			if (DistanceToHullAcceptable(polygon, point, true))
			{
				List<float> list = SearchCrossingEdges(polygon, (int)point.Y);
				if (list.Count > 0 && list.Count % 2 == 0)
				{
					for (int i = 0; i < list.Count; i += 2)
					{
						if (list[i] <= point.X && list[i + 1] >= point.X)
						{
							return true;
						}
					}
				}
				return false;
			}
			return true;
		}

		private FVector2? GetTopMostVertex(Vertices vertices)
		{
			float num = float.MaxValue;
			FVector2? result = null;
			for (int i = 0; i < vertices.Count; i++)
			{
				if (num > vertices[i].Y)
				{
					num = vertices[i].Y;
					result = vertices[i];
				}
			}
			return result;
		}

		private float GetTopMostCoord(Vertices vertices)
		{
			float num = float.MaxValue;
			for (int i = 0; i < vertices.Count; i++)
			{
				if (num > vertices[i].Y)
				{
					num = vertices[i].Y;
				}
			}
			return num;
		}

		private float GetBottomMostCoord(Vertices vertices)
		{
			float num = float.MinValue;
			for (int i = 0; i < vertices.Count; i++)
			{
				if (num < vertices[i].Y)
				{
					num = vertices[i].Y;
				}
			}
			return num;
		}

		private List<float> SearchCrossingEdges(DetectedVertices polygon, int y)
		{
			if (polygon == null)
			{
				throw new ArgumentNullException("polygon", "'polygon' can't be null.");
			}
			if (polygon.Count < 3)
			{
				throw new ArgumentException("'polygon.MainPolygon.Count' can't be less then 3.");
			}
			List<float> list = SearchCrossingEdges((Vertices)polygon, y);
			if (polygon.Holes != null)
			{
				for (int i = 0; i < polygon.Holes.Count; i++)
				{
					list.AddRange(SearchCrossingEdges(polygon.Holes[i], y));
				}
			}
			list.Sort();
			return list;
		}

		private List<float> SearchCrossingEdges(Vertices polygon, int y)
		{
			List<float> list = new List<float>();
			if (polygon.Count > 2)
			{
				FVector2 fVector = polygon[polygon.Count - 1];
				for (int i = 0; i < polygon.Count; i++)
				{
					FVector2 fVector2 = polygon[i];
					if (((fVector2.Y >= (float)y && fVector.Y <= (float)y) || (fVector2.Y <= (float)y && fVector.Y >= (float)y)) && fVector2.Y != fVector.Y)
					{
						bool flag = true;
						FVector2 fVector3 = fVector - fVector2;
						if (fVector2.Y == (float)y)
						{
							FVector2 fVector4 = polygon[(i + 1) % polygon.Count];
							FVector2 fVector5 = fVector2 - fVector4;
							flag = ((!(fVector3.Y > 0f)) ? (fVector5.Y >= 0f) : (fVector5.Y <= 0f));
						}
						if (flag)
						{
							list.Add(((float)y - fVector2.Y) / fVector3.Y * fVector3.X + fVector2.X);
						}
					}
					fVector = fVector2;
				}
			}
			list.Sort();
			return list;
		}

		private bool SplitPolygonEdge(Vertices polygon, FVector2 coordInsideThePolygon, out int vertex1Index, out int vertex2Index)
		{
			int num = 0;
			int index = 0;
			bool flag = false;
			float num2 = float.MaxValue;
			bool flag2 = false;
			FVector2 point = FVector2.Zero;
			List<float> list = SearchCrossingEdges(polygon, (int)coordInsideThePolygon.Y);
			vertex1Index = 0;
			vertex2Index = 0;
			point.Y = coordInsideThePolygon.Y;
			if (list != null && list.Count > 1 && list.Count % 2 == 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] < coordInsideThePolygon.X)
					{
						float num3 = coordInsideThePolygon.X - list[i];
						if (num3 < num2)
						{
							num2 = num3;
							point.X = list[i];
							flag2 = true;
						}
					}
				}
				if (flag2)
				{
					num2 = float.MaxValue;
					int num4 = polygon.Count - 1;
					for (int j = 0; j < polygon.Count; j++)
					{
						FVector2 lineEndPoint = polygon[j];
						FVector2 lineEndPoint2 = polygon[num4];
						float num3 = LineTools.DistanceBetweenPointAndLineSegment(ref point, ref lineEndPoint, ref lineEndPoint2);
						if (num3 < num2)
						{
							num2 = num3;
							num = j;
							index = num4;
							flag = true;
						}
						num4 = j;
					}
					if (flag)
					{
						FVector2 fVector = polygon[index] - polygon[num];
						fVector.Normalize();
						FVector2 point2 = polygon[num];
						float num3 = LineTools.DistanceBetweenPointAndPoint(ref point2, ref point);
						vertex1Index = num;
						vertex2Index = num + 1;
						polygon.Insert(num, num3 * fVector + polygon[vertex1Index]);
						polygon.Insert(num, num3 * fVector + polygon[vertex2Index]);
						return true;
					}
				}
			}
			return false;
		}

		private Vertices CreateSimplePolygon(FVector2 entrance, FVector2 last)
		{
			bool flag = false;
			bool flag2 = false;
			Vertices vertices = new Vertices(32);
			Vertices vertices2 = new Vertices(32);
			Vertices vertices3 = new Vertices(32);
			FVector2 current = FVector2.Zero;
			if (entrance == FVector2.Zero || !InBounds(ref entrance))
			{
				flag = SearchHullEntrance(out entrance);
				if (flag)
				{
					current = new FVector2(entrance.X - 1f, entrance.Y);
				}
			}
			else if (IsSolid(ref entrance))
			{
				FVector2 foundPixel;
				if (IsNearPixel(ref entrance, ref last))
				{
					current = last;
					flag = true;
				}
				else if (SearchNearPixels(false, ref entrance, out foundPixel))
				{
					current = foundPixel;
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
			if (flag)
			{
				vertices.Add(entrance);
				vertices2.Add(entrance);
				FVector2 next = entrance;
				while (true)
				{
					FVector2 outstanding;
					int outstandingIndex;
					if (SearchForOutstandingVertex(vertices2, out outstanding, out outstandingIndex))
					{
						if (flag2)
						{
							if (vertices3.Contains(outstanding))
							{
								vertices.Add(outstanding);
							}
							break;
						}
						vertices.Add(outstanding);
						vertices2.RemoveRange(0, outstandingIndex);
					}
					last = current;
					current = next;
					if (!GetNextHullPoint(ref last, ref current, out next))
					{
						break;
					}
					vertices2.Add(next);
					if (next == entrance && !flag2)
					{
						flag2 = true;
						vertices3.AddRange(vertices2);
						if (vertices3.Contains(entrance))
						{
							vertices3.Remove(entrance);
						}
					}
				}
			}
			return vertices;
		}

		private bool SearchNearPixels(bool searchingForSolidPixel, ref FVector2 current, out FVector2 foundPixel)
		{
			for (int i = 0; i < 8; i++)
			{
				int num = (int)current.X + ClosePixels[i, 0];
				int num2 = (int)current.Y + ClosePixels[i, 1];
				if (!searchingForSolidPixel ^ IsSolid(num, num2))
				{
					foundPixel = new FVector2(num, num2);
					return true;
				}
			}
			foundPixel = FVector2.Zero;
			return false;
		}

		private bool IsNearPixel(ref FVector2 current, ref FVector2 near)
		{
			for (int i = 0; i < 8; i++)
			{
				int num = (int)current.X + ClosePixels[i, 0];
				int num2 = (int)current.Y + ClosePixels[i, 1];
				if (num >= 0 && num <= _width && num2 >= 0 && num2 <= _height && num == (int)near.X && num2 == (int)near.Y)
				{
					return true;
				}
			}
			return false;
		}

		private bool SearchHullEntrance(out FVector2 entrance)
		{
			for (int i = 0; i <= _height; i++)
			{
				for (int j = 0; j <= _width; j++)
				{
					if (IsSolid(j, i))
					{
						entrance = new FVector2(j, i);
						return true;
					}
				}
			}
			entrance = FVector2.Zero;
			return false;
		}

		private bool SearchNextHullEntrance(List<DetectedVertices> detectedPolygons, FVector2 start, out FVector2? entrance)
		{
			bool flag = false;
			bool flag2 = false;
			for (int i = (int)start.X + (int)start.Y * _width; i <= _dataLength; i++)
			{
				if (IsSolid(i))
				{
					if (!flag)
					{
						continue;
					}
					int num = i % _width;
					entrance = new FVector2(num, (float)(i - num) / (float)_width);
					flag2 = false;
					for (int j = 0; j < detectedPolygons.Count; j++)
					{
						if (InPolygon(detectedPolygons[j], entrance.Value))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						return true;
					}
					flag = false;
				}
				else
				{
					flag = true;
				}
			}
			entrance = null;
			return false;
		}

		private bool GetNextHullPoint(ref FVector2 last, ref FVector2 current, out FVector2 next)
		{
			int indexOfFirstPixelToCheck = GetIndexOfFirstPixelToCheck(ref last, ref current);
			for (int i = 0; i < 8; i++)
			{
				int num = (indexOfFirstPixelToCheck + i) % 8;
				int num2 = (int)current.X + ClosePixels[num, 0];
				int num3 = (int)current.Y + ClosePixels[num, 1];
				if (num2 >= 0 && num2 < _width && num3 >= 0 && num3 <= _height && IsSolid(num2, num3))
				{
					next = new FVector2(num2, num3);
					return true;
				}
			}
			next = FVector2.Zero;
			return false;
		}

		private bool SearchForOutstandingVertex(Vertices hullArea, out FVector2 outstanding, out int outstandingIndex)
		{
			FVector2 fVector = FVector2.Zero;
			outstandingIndex = -1;
			bool result = false;
			if (hullArea.Count > 2)
			{
				int num = hullArea.Count - 1;
				FVector2 lineEndPoint = hullArea[0];
				FVector2 lineEndPoint2 = hullArea[num];
				for (int i = 1; i < num; i++)
				{
					FVector2 point = hullArea[i];
					if (LineTools.DistanceBetweenPointAndLineSegment(ref point, ref lineEndPoint, ref lineEndPoint2) >= _hullTolerance)
					{
						fVector = hullArea[i];
						outstandingIndex = i;
						result = true;
						break;
					}
				}
			}
			outstanding = fVector;
			return result;
		}

		private int GetIndexOfFirstPixelToCheck(ref FVector2 last, ref FVector2 current)
		{
			switch ((int)(current.X - last.X))
			{
			case 1:
				switch ((int)(current.Y - last.Y))
				{
				case 1:
					return 1;
				case 0:
					return 0;
				case -1:
					return 7;
				}
				break;
			case 0:
				switch ((int)(current.Y - last.Y))
				{
				case 1:
					return 2;
				case -1:
					return 6;
				}
				break;
			case -1:
				switch ((int)(current.Y - last.Y))
				{
				case 1:
					return 3;
				case 0:
					return 4;
				case -1:
					return 5;
				}
				break;
			}
			return 0;
		}
	}
}
