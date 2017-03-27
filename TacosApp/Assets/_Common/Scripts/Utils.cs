using UnityEngine;
using System;
using System.Collections;

//Version: 1.0.0

using Random = UnityEngine.Random;

/// <summary>
/// A variety of utils for different functionalities.
/// </summary>
public class Utils
{
	/// <summary>
	/// Gets the random index from the float array, this array must have values between 0-1 that all summed give 1
	///  and sorted in ascendant order, the Value will be taken randomly as a Roulette or by weigth.
	/// The min value must be 0.01, as it needs to represent at least 1%.
	/// </summary>
	/// <returns>The random index from roulette.</returns>
	/// <param name="roulette">Roulette.</param>
	public static int GetRandomIndexFromRoulette(ref float[] roulette)
	{
		float randomValue = Random.Range(0.01f,1.0f);
		float Accumulated = 0;
		for(int i = 0 ; i < roulette.Length; i++)
		{
			Accumulated += roulette[i];
			if( randomValue <= Accumulated)
			{
				return i;
			}
		}
		return 0;
	}

	/// <summary>
	/// Creates a probability roulette from an array of weights. For this to generate a useful array the minimum value
	/// must be at least 1% of the summed values.
	/// </summary>
	/// <returns>The probabilities roulette.</returns>
	/// <param name="weights">The weights array.</param>
	public static float[] CreateRouletteFromProbabilities(ref int[] weights)
	{
		float[] roulette = new float[weights.Length];
		float total = 0.0f;
		for(int i = 0; i < weights.Length; i++)
		{
			total += weights[i];
		}
		for(int i = 0; i < weights.Length; i++)
		{
			roulette[i] = (float)weights[i]/total;
		}

		return roulette;
	}

	public static string GetTimeSpanToDHMSString(TimeSpan span)
	{
		if(span.Ticks > 0)
		{
			DateTime date = new DateTime(span.Ticks);

			if(span.TotalDays >= 1)
			{
				return date.ToString("dd.hh:mm:ss");
			}
			else if(span.TotalHours >= 1)
			{
				return date.ToString("hh:mm:ss");
			}
			else
			{
				return date.ToString("mm:ss");
			}
		}
		else
		{
			return "00:00";
		}
	}

	public static string GetTimeSpanToMinutesString(TimeSpan span)
	{
		if(span.Ticks > 0)
		{
			int totalMinutes = span.Hours*60 + span.Days*1440 + span.Minutes;
			return totalMinutes+":"+span.Seconds;
		}
		else
		{
			return "00:00";
		}
	}

	public static string GetTimeSpanToHoursString(TimeSpan span)
	{
		if(span.Ticks > 0)
		{
			int totalHours = span.Days*24 + span.Hours;
			return totalHours+":"+span.Minutes;
		}
		else
		{
			return "00:00";
		}
	}

}
