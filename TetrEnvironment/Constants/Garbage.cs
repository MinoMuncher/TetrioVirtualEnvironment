﻿namespace TetrEnvironment.Constants;

public abstract class Garbage
{
	public const int SINGLE = 0;
	public const int DOUBLE = 1;
	public const int TRIPLE = 2;
	public const int QUAD = 4;
	public const int PENTA = 5;

	public const int TSPIN_MINI = 0;
	public const int TSPIN = 0;
	public const int TSPIN_MINI_SINGLE = 0;
	public const int TSPIN_SINGLE = 2;
	public const int TSPIN_MINI_DOUBLE = 1;
	public const int TSPIN_DOUBLE = 4;
	public const int TSPIN_TRIPLE = 6;
	public const int TSPIN_QUAD = 10;
	public const int TSPIN_PENTA = 12;

	public const int BACKTOBACK_BONUS = 1;
	public const double BACKTOBACK_BONUS_LOG = 0.8;
	public const int COMBO_MINIFIER = 1;
	public const double COMBO_MINIFIER_LOG = 1.25;
	public const double COMBO_BONUS = 0.25;
	public const int ALL_CLEAR = 10;

	public static readonly int[][] COMBO_TABLE = new int[][]
	{
		new int[] { 0 },
		new int[] { 0, 1, 1, 2, 2, 3, 3, 4, 4, 4, 5 },
		new int[] { 0, 1, 1, 2, 2, 2, 3, 3, 3, 3, 3, 3, 4 }
	};
}