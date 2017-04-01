using System;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Unsigned Unbound int.
/// This class will handle some of the basic numeric operations (+,-,*)
/// of an unsignedInteger value but will not be limited by the processor numeric limits.
/// </summary>

[System.Serializable]
public class UnsignedUnboundInt : System.Object
{
	private const ushort bucketSizePerElement = 3;
	//Postfix letters using short scale ref: 
	//https://en.wikipedia.org/wiki/Long_and_short_scales and https://en.wikipedia.org/wiki/Names_of_large_numbers
	//K = ###*10^3 Thousands
	//M = ###*10^6 Million
	//B = ###*10^9 Billion
	//T = ###*10^12 Trillion
	//Qd = ###*10^15 Quadrillion
	//Qn = ###*10^18 Quintillion
	//Sx = ###*10^21 Sextillion
	//St = ###*10^24 Septillion
	//O = ###*10^27 Octillion
	//N = ###*10^30 Nonillion
	//D = ###*10^33 Decillion

	private static string[] postfixes = new string[]{"","K","M","B","T","Qd","Qn","Sx","St","O","N","D"};

	private StringBuilder strBuilder;

	public enum TO_STRING_FORMAT
	{
		SIMPLE,//as number without any format
		SIMPLE_COMMA,//as a number with commas between each bucket
		SHORT_SCALE_2,//shows only the 2 more representative buckets and appends letters following the postfixes table without K
		SHORT_SCALE_K_2,//shows only the 2 more representative buckets and appends letters following the postfixes table
		SIMPLE_COMMA_2,//shows only the 2 more representative buckets and separetes them with commas, it puts also the prefix of the lowest part if available
		SHORT_SCALE_FIXED_POINT_1_4,//shows only the most representative bucket as integer part and the next 4 representative values as floating part, postfix at the end without K
		SHORT_SCALE_FIXED_POINT_K_1_4,//shows only the most representative bucket as integer part and the next 4 representative values as floating part, postfix at the end 
		SHORT_SCALE_FIXED_POINT_COMMA_1_4//shows only the most representative bucket as integer part and the next 4 representative values as floating part, postfix at the end without K, it uses commas if the value is less thn a million
	}
		
	/// <summary>
	/// The values array that will handle all the numbers, this list will contain ints that will represent
	/// numbers from 0 to 999, and each index will be a multiplier of 1000, this means that the number 123,456 will be:
	/// [0] = 456
	/// [1] = 123
	/// and the number 123,456,789:
	/// [0] = 789
	/// [1] = 456
	/// [2] = 123
	/// </summary>
	public List<uint> values = new List<uint>();
	private uint bucketLimit = 10;
	private float bucketConverter = 0.1f;

	public UnsignedUnboundInt()
	{
		RecalculateBucketLimitAndConverter();
	}

	public UnsignedUnboundInt( UnsignedUnboundInt otherUnboundInt)
	{
		RecalculateBucketLimitAndConverter();
		for(int i = 0; i < otherUnboundInt.values.Count; i++)
		{
			values.Add(otherUnboundInt.values[i]);
		}
	}

	public UnsignedUnboundInt( uint uintValue)
	{
		RecalculateBucketLimitAndConverter();
		values = uintToList(uintValue);
	}

	public UnsignedUnboundInt( ulong ulongValue)
	{
		RecalculateBucketLimitAndConverter();
		values = ulongToList(ulongValue);
	}

	public void SetToZero()
	{
		values.Clear();
	}

	public void AddUInt(uint uintValue)
	{
		if(uintValue != 0)
		{
			if(values.Count == 0)
			{
				values = uintToList(uintValue);
			}
			else
			{
				List<uint> newValue = uintToList(uintValue);

				//add  start from less significant part
				for(int i = 0; i < newValue.Count; i++)
				{
					AddToBucket(newValue[i],i);
				}
			}
		}
	}

	public void RemoveUInt(uint uintValue)
	{
		if(uintValue != 0)
		{
			if(values.Count == 0)
			{
				values = uintToList(uintValue);
			}
			else
			{
				if(uintValue.GetNumberOfDigits() > this.GetNumberOfDigits())
				{
					SetToZero();
				}
				else
				{

					List<uint> newValue = uintToList(uintValue);
					//remove first from most significant part to less significant part
					for(int i = newValue.Count-1; i >= 0; i--)
					{
						RemoveFromBucket(newValue[i],i);
					}
				}
			}
		}
	}

	public void AddUnboundInt(UnsignedUnboundInt otherUnboundInt)
	{
		for(int i = 0; i < otherUnboundInt.values.Count; i++)
		{
			AddToBucket(otherUnboundInt.values[i],i);
		}
	}

	public void RemoveUnboundInt(UnsignedUnboundInt otherUnboundInt)
	{
		//remove first from most significant part to less significant part
		for(int i = otherUnboundInt.values.Count-1; i >= 0; i--)
		{
			RemoveFromBucket(otherUnboundInt.values[i],i);
		}
	}
		
	//n must be a number with bucketSizePerElement digits
	void AddToBucket(uint n, int bucketIndex)
	{
		if(bucketIndex < values.Count && bucketIndex >= 0)
		{
			if(values[bucketIndex] == 0)
			{
				values[bucketIndex] = n;
			}
			else
			{
				values[bucketIndex] += n;
				if(values[bucketIndex] >= bucketLimit)//exceed the limit
				{
					//temp = 21.234
					float temp = values[bucketIndex]*bucketConverter;
					//carry = 21
					uint carry = Convert.ToUInt32(Math.Floor(temp));
					//values[bucketIndex] = (0.234)*1000
					values[bucketIndex] = Convert.ToUInt32( (temp - carry)*bucketLimit);

					AddToBucket(carry,bucketIndex + 1);
				}
			}
		}
		else
		{
			values.Add(n);
		}
	}
		
	//n must be a number with bucketSizePerElement digits
	void RemoveFromBucket(uint n, int bucketIndex)
	{
		//UnityEngine.Debug.Log("Removing ["+n+"] from bucket["+bucketIndex+"]");
		if(bucketIndex < values.Count && bucketIndex >= 0  )
		{
			if(values[bucketIndex] >= n)
			{
				values[bucketIndex] = values[bucketIndex] - n;

				if(bucketIndex + 1 >= values.Count)//if there arent bigger units
				{
					//the chunk was the most significant and now it is zero
					if(values[bucketIndex] == 0)
					{
						values.RemoveAt(bucketIndex);
					}
				}
			}
			else//we are removing a bigger number
			{
				if(bucketIndex + 1 < values.Count)//there is a bigger unit in this number
				{
					//being the limit 1000
					//this will make the difference of: 1,001 - 0,003
					//1,000 - (3-1) => 1,000 - (2) = 1,998 
					//then we remove 1 from the next bucket
					//and we end with 998
					values[bucketIndex] = bucketLimit - (n - values[bucketIndex]);
					RemoveFromBucket(1,bucketIndex+1);
				}
				else//this means we are in the most significant part and the difference is negative
				{
					values.Clear();
				}
			}
		}
		else//the number we are using in the difference is bigger, as this class is unsigned, it will be zero
		{
			values.Clear();
		}
	}
		
	public void MultiplyByFloat(float n)
	{
		if(n <= 0.0f)
		{
			values.Clear();
		}
		else
		{
			float carry = 0;
			uint excedent = 0;
			float temp = 0;
			uint convertedTemp = 0;
			float floatingPart = 0;
			//lets assume values = 800,035 and n = 0.5f and n = 1.5f
			for(int i = values.Count-1; i >= 0; i--)
			{
				//lets assume values = 77,035 and n = 0.5f
				if(n < 1.0f)//0.5f
				{
					//temp = 77*0.5f + 0 = 38.5f | temp = 35*0.5f + 500 = 512
					temp = values[i]*n + carry;
					carry = 0.0f;

					//convertedTemp = 38 | 
					convertedTemp = Convert.ToUInt32(Math.Floor(temp));
					floatingPart = temp - convertedTemp;
					if(i > 0)
					{
						carry = floatingPart*bucketLimit;
					}
					if(temp < 1.0f)
					{
						if(i == values.Count-1)
						{
							values.RemoveAt(values.Count-1);
						}
						else
						{
							values[i] = convertedTemp;
						}
					}
					else
					{
						values[i] = convertedTemp;
					}
				}
				else//1.5f
				{
					//lets assume values = 801,035 n = 1.5f
					//temp = 801*1.5f + 0 = 151.5f | temp = 35*1.5f + 500 = 552
					temp = values[i]*n + carry;
					carry = 0.0f;
					//convertedTemp = 1201 | 
					convertedTemp = Convert.ToUInt32(Math.Floor(temp));
					//floating part = 0.5
					floatingPart = temp - convertedTemp;
					if(i > 0)
					{
						//carry = 500
						carry = floatingPart*bucketLimit;
					}

					if(convertedTemp >= bucketLimit)
					{
						//need to add excedent and left remains
						//temp = 1.2015 
						temp *= bucketConverter;
						//excedent = 1 
						excedent = Convert.ToUInt32(Math.Floor(temp));
						//value = 1201 - 1*1000 = 201
						values[i] = convertedTemp - excedent*bucketLimit ;
						//add excedent
						AddToBucket(excedent,i + 1);
					}
					else
					{
						values[i] = convertedTemp;
					}
				}
			}
		}
	}

	public void MultiplyByDouble(double n)
	{
		if(n <= 0.0f)
		{
			values.Clear();
		}
		else
		{
			double carry = 0;
			uint excedent = 0;
			double temp = 0;
			uint convertedTemp = 0;
			double floatingPart = 0;
			//lets assume values = 800,035 and n = 0.5f and n = 1.5
			for(int i = values.Count-1; i >= 0; i--)
			{
				//lets assume values = 77,035 and n = 0.5
				if(n < 1.0f)//0.5
				{
					//temp = 77*0.5 + 0 = 38.5 | temp = 35*0.5 + 500 = 512
					temp = values[i]*n + carry;
					carry = 0.0f;

					//convertedTemp = 38
					convertedTemp = Convert.ToUInt32(Math.Floor(temp));
					floatingPart = temp - convertedTemp;
					if(i > 0)
					{
						carry = floatingPart*bucketLimit;
					}
					if(temp < 1.0f)
					{
						if(i == values.Count-1)
						{
							values.RemoveAt(values.Count-1);
						}
						else
						{
							values[i] = convertedTemp;
						}
					}
					else
					{
						values[i] = convertedTemp;
					}
				}
				else//1.5
				{
					//lets assume values = 801,035 n = 1.5
					//temp = 801*1.5 + 0 = 151.5 | temp = 35*1.5 + 500 = 552
					temp = values[i]*n + carry;
					carry = 0.0f;
					//convertedTemp = 1201 | 
					convertedTemp = Convert.ToUInt32(Math.Floor(temp));
					//floating part = 0.5
					floatingPart = temp - convertedTemp;
					if(i > 0)
					{
						//carry = 500
						carry = floatingPart*bucketLimit;
					}

					if(convertedTemp >= bucketLimit)
					{
						//need to add excedent and left remains
						//temp = 1.2015 
						temp *= bucketConverter;
						//excedent = 1 
						excedent = Convert.ToUInt32(Math.Floor(temp));
						//value = 1201 - 1*1000 = 201
						values[i] = convertedTemp - excedent*bucketLimit ;
						//add excedent
						AddToBucket(excedent,i + 1);
					}
					else
					{
						values[i] = convertedTemp;
					}
				}
			}
		}
	}

	public void Multiply(uint n)
	{
		if(n == 0)
		{
			values.Clear();
		}
		else
		{
			uint carry = 0;
			float temp = 0;
			for(int i = values.Count-1; i >= 0; i--)
			{
				//assume starting value is 512 and n is 2
				values[i] = values[i]*n;
				//result will be 1024
				if(values[i] >= bucketLimit)
				{
					//temp = 1.024 
					temp = values[i]*bucketConverter;
					//carry = 1 
					carry = Convert.ToUInt32(Math.Floor(temp));
					//1024 - 1*1000 = 024
					values[i] -= carry*bucketLimit ;

					AddToBucket(carry,i + 1);
				}
			}
		}
	}

	private List<uint> uintToList(uint n)
	{
		if(n == 0)
		{
			List<uint> list = new List<uint>();
			list.Add(0);
			return list;
		}
		else
		{
			//no data loss
			double convertedN = n;
			float multiplier = bucketLimit;
			int digitsInN = n.GetNumberOfDigits();
			int elementsNeeded = (digitsInN/bucketSizePerElement);
			if(digitsInN%bucketSizePerElement > 0)
			{
				elementsNeeded += 1;
			}
			//UnityEngine.Debug.Log("elements needed for["+n+"] : ["+elementsNeeded+"]. Digits["+digitsInN+"] BucketLimit["+bucketLimit+"] digitsperbucket["+bucketSizePerElement+"] converter["+bucketConverter+"]");
			List<uint> list = new List<uint>();
			double temp = 0;
			//N = 1234567
			//with 3 digits per element
			for (int i = 0; i < elementsNeeded; i++)
			{
				//temp = 1234.567
				temp = convertedN*bucketConverter;
				//UnityEngine.Debug.Log("temp["+temp+"]");
				//convertedN = 1234
				convertedN = (double)Convert.ToUInt32(Math.Floor(temp));
				//UnityEngine.Debug.Log("convertedN ["+convertedN+"]");
				//list[i] = (0.567)*1000
				temp = (temp - convertedN)*multiplier ;
				//UnityEngine.Debug.Log("future element in bucket ["+temp+"]");
				list.Add(Convert.ToUInt32( temp ) );
			}
			return list;
		}
	}

	private List<uint> ulongToList(ulong n)
	{
		if(n == 0)
		{
			List<uint> list = new List<uint>();
			list.Add(0);
			return list;
		}
		else
		{
			//no data loss
			double convertedN = n;
			float multiplier = bucketLimit;
			int digitsInN = n.GetNumberOfDigits();
			int elementsNeeded = (digitsInN/bucketSizePerElement);
			if(digitsInN%bucketSizePerElement > 0)
			{
				elementsNeeded += 1;
			}
			//UnityEngine.Debug.Log("elements needed for["+n+"] : ["+elementsNeeded+"]. Digits["+digitsInN+"] BucketLimit["+bucketLimit+"] digitsperbucket["+bucketSizePerElement+"] converter["+bucketConverter+"]");
			List<uint> list = new List<uint>();
			double temp = 0;
			//N = 1234567
			//with 3 digits per element
			for (int i = 0; i < elementsNeeded; i++)
			{
				//temp = 1234.567
				temp = convertedN*bucketConverter;
				//UnityEngine.Debug.Log("temp["+temp+"]");
				//convertedN = 1234
				convertedN = (double)Convert.ToUInt32(Math.Floor(temp));
				//UnityEngine.Debug.Log("convertedN ["+convertedN+"]");
				//list[i] = (0.567)*1000
				temp = (temp - convertedN)*multiplier ;
				//UnityEngine.Debug.Log("future element in bucket ["+temp+"]");
				list.Add(Convert.ToUInt32( temp ) );
			}
			return list;
		}
	}

	private int GetNumberOfDigits()
	{
		int digits = 1;

		if(values.Count > 0)
		{
			int lastIndex = values.Count-1;
			digits = values[lastIndex].GetNumberOfDigits();
			if(lastIndex > 0)
			{
				digits += (lastIndex)*bucketSizePerElement;
			}
		}

		return digits;
	}

	private void RecalculateBucketLimitAndConverter()
	{
		bucketLimit = 10;
		bucketConverter = 0.1f;
		for(int i = 1; i < bucketSizePerElement; i++)
		{
			bucketLimit *= 10;
			bucketConverter *= 0.1f;
		}
	}

	public static UnsignedUnboundInt operator +(UnsignedUnboundInt ubInt, uint intValue) 
	{
		UnsignedUnboundInt uui = new UnsignedUnboundInt(ubInt);
		uui.AddUInt(intValue);
		return uui;
	}

	public static UnsignedUnboundInt operator -(UnsignedUnboundInt ubInt, uint intValue) 
	{
		UnsignedUnboundInt uui = new UnsignedUnboundInt(ubInt);
		uui.RemoveUInt(intValue);
		return uui;
	}

	public static UnsignedUnboundInt operator +(UnsignedUnboundInt ubInt1, UnsignedUnboundInt ubInt2) 
	{
		UnsignedUnboundInt uui = new UnsignedUnboundInt(ubInt1);
		uui.AddUnboundInt(ubInt2);
		return uui;
	}

	public static UnsignedUnboundInt operator -(UnsignedUnboundInt ubInt1, UnsignedUnboundInt ubInt2) 
	{
		UnsignedUnboundInt uui = new UnsignedUnboundInt(ubInt1);
		uui.RemoveUnboundInt(ubInt2);
		return uui;
	}

	public static UnsignedUnboundInt operator *(UnsignedUnboundInt ubInt1, float floatValue) 
	{
		UnsignedUnboundInt uui = new UnsignedUnboundInt(ubInt1);
		uui.MultiplyByFloat(floatValue);
		return uui;
	}

	public static UnsignedUnboundInt operator *(float floatValue,UnsignedUnboundInt ubInt1) 
	{
		UnsignedUnboundInt uui = new UnsignedUnboundInt(ubInt1);
		uui.MultiplyByFloat(floatValue);
		return uui;
	}

	public static UnsignedUnboundInt operator *(UnsignedUnboundInt ubInt1, double doubleValue) 
	{
		UnsignedUnboundInt uui = new UnsignedUnboundInt(ubInt1);
		uui.MultiplyByDouble(doubleValue);
		return uui;
	}

	public static UnsignedUnboundInt operator *(UnsignedUnboundInt ubInt1, uint uintValue) 
	{
		UnsignedUnboundInt uui = new UnsignedUnboundInt(ubInt1);
		uui.Multiply(uintValue);
		return uui;
	}

	public static UnsignedUnboundInt operator /(UnsignedUnboundInt ubInt1, uint uintValue) 
	{
		UnsignedUnboundInt uui = new UnsignedUnboundInt(ubInt1);
		float denominator = (float)(1.0f/uintValue);
		uui.MultiplyByFloat(denominator);
		return uui;
	}
		
	public static bool operator <(UnsignedUnboundInt a, UnsignedUnboundInt b)
	{
		// If both are null, or both are same instance, return true.
		if (System.Object.ReferenceEquals(a, b))
		{
			return false;
		}

		// If one is null, but not both, return false.
		if (((object)a == null) || ((object)b == null))
		{
			return false;
		}

		if(a.values.Count < b.values.Count)
		{
			return true;
		}

		if(a.values.Count > b.values.Count)
		{
			return false;
		}

		for(int  i = a.values.Count-1 ; i >= 0; i--)
		{
			if(a.values[i] < b.values[i])
			{
				return true;
			}
		}

		return false;
	}

	public static bool operator <=(UnsignedUnboundInt a, UnsignedUnboundInt b)
	{
		// If both are null, or both are same instance, return true.
		if (System.Object.ReferenceEquals(a, b))
		{
			return false;
		}

		// If one is null, but not both, return false.
		if (((object)a == null) || ((object)b == null))
		{
			return false;
		}

		if(a.values.Count < b.values.Count)
		{
			return true;
		}

		if(a.values.Count > b.values.Count)
		{
			return false;
		}

		for(int  i = a.values.Count-1 ; i >= 0; i--)
		{
			if(a.values[i] <= b.values[i])
			{
				return true;
			}
		}

		return false;
	}
		
	public static bool operator >(UnsignedUnboundInt a, UnsignedUnboundInt b)
	{
		// If both are null, or both are same instance, return true.
		if (System.Object.ReferenceEquals(a, b))
		{
			return false;
		}

		// If one is null, but not both, return false.
		if (((object)a == null) || ((object)b == null))
		{
			return false;
		}

		if(a.values.Count > b.values.Count)
		{
			return true;
		}

		if(a.values.Count < b.values.Count)
		{
			return false;
		}

		for(int  i = a.values.Count-1 ; i >= 0; i--)
		{
			if(a.values[i] > b.values[i])
			{
				return true;
			}
		}

		return false;
	}

	public static bool operator >=(UnsignedUnboundInt a, UnsignedUnboundInt b)
	{
		// If both are null, or both are same instance, return true.
		if (System.Object.ReferenceEquals(a, b))
		{
			return false;
		}

		// If one is null, but not both, return false.
		if (((object)a == null) || ((object)b == null))
		{
			return false;
		}

		if(a.values.Count > b.values.Count)
		{
			return true;
		}

		if(a.values.Count < b.values.Count)
		{
			return false;
		}

		for(int  i = a.values.Count-1 ; i >= 0; i--)
		{
			if(a.values[i] >= b.values[i])
			{
				return true;
			}
		}

		return false;
	}

	public static bool operator <(UnsignedUnboundInt a, uint b)
	{
		//optimization first check against number of digits
		int aDigits = a.GetNumberOfDigits();
		int bDigits = b.GetNumberOfDigits();
		if(aDigits != bDigits)
		{
			return aDigits < bDigits;
		}
		else
		{
			return a < new UnsignedUnboundInt(b);
		}
	}

	public static bool operator <=(UnsignedUnboundInt a, uint b)
	{
		//optimization first check against number of digits
		int aDigits = a.GetNumberOfDigits();
		int bDigits = b.GetNumberOfDigits();
		if(aDigits != bDigits)
		{
			return aDigits < bDigits;
		}
		else
		{
			return a <= new UnsignedUnboundInt(b);
		}
	}

	public static bool operator >(UnsignedUnboundInt a, uint b)
	{
		int aDigits = a.GetNumberOfDigits();
		int bDigits = b.GetNumberOfDigits();
		if(aDigits != bDigits)
		{
			return aDigits > bDigits;
		}
		else
		{
			return a > new UnsignedUnboundInt(b);
		}
	}

	public static bool operator >=(UnsignedUnboundInt a, uint b)
	{
		int aDigits = a.GetNumberOfDigits();
		int bDigits = b.GetNumberOfDigits();
		if(aDigits != bDigits)
		{
			return aDigits > bDigits;
		}
		else
		{
			return a >= new UnsignedUnboundInt(b);
		}
	}
		
	public static bool operator <(UnsignedUnboundInt a, ulong b)
	{
		//optimization first check against number of digits
		int aDigits = a.GetNumberOfDigits();
		int bDigits = b.GetNumberOfDigits();
		if(aDigits != bDigits)
		{
			return aDigits < bDigits;
		}
		else
		{
			return a < new UnsignedUnboundInt(b);
		}
	}

	public static bool operator <=(UnsignedUnboundInt a, ulong b)
	{
		//optimization first check against number of digits
		int aDigits = a.GetNumberOfDigits();
		int bDigits = b.GetNumberOfDigits();
		if(aDigits != bDigits)
		{
			return aDigits < bDigits;
		}
		else
		{
			return a <= new UnsignedUnboundInt(b);
		}
	}

	public static bool operator >(UnsignedUnboundInt a, ulong b)
	{
		int aDigits = a.GetNumberOfDigits();
		int bDigits = b.GetNumberOfDigits();
		if(aDigits != bDigits)
		{
			return aDigits > bDigits;
		}
		else
		{
			return a > new UnsignedUnboundInt(b);
		}
	}

	public static bool operator >=(UnsignedUnboundInt a, ulong b)
	{
		int aDigits = a.GetNumberOfDigits();
		int bDigits = b.GetNumberOfDigits();
		if(aDigits != bDigits)
		{
			return aDigits > bDigits;
		}
		else
		{
			return a >= new UnsignedUnboundInt(b);
		}
	}

	public static bool operator ==(UnsignedUnboundInt a, UnsignedUnboundInt b)
	{
		// If both are null, or both are same instance, return true.
		if (System.Object.ReferenceEquals(a, b))
		{
			return true;
		}

		// If one is null, but not both, return false.
		if (((object)a == null) || ((object)b == null))
		{
			return false;
		}

		if(a.values.Count != b.values.Count)
		{
			return false;
		}
		// Return false if at least one of the fields match:
		for(int i = 0 ; i < a.values.Count ; i++)
		{
			if(a.values[i] != b.values[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool operator ==(UnsignedUnboundInt a, uint b)
	{
		// If one is null, but not both, return false.
		if (((object)a == null))
		{
			return false;
		}

		if(a.values.Count == 0)
		{
			return b == 0;
		}

		int aDigits = a.GetNumberOfDigits();
		int bDigits = b.GetNumberOfDigits();
		if(aDigits != bDigits)
		{
			return false;
		}
		else
		{
			// Return true if the fields match:
			return a == new UnsignedUnboundInt(b);
		}


	}

	public static bool operator ==(UnsignedUnboundInt a, ulong b)
	{
		// If one is null, but not both, return false.
		if (((object)a == null))
		{
			return false;
		}

		if(a.values.Count == 0)
		{
			return b == 0;
		}

		int aDigits = a.GetNumberOfDigits();
		int bDigits = b.GetNumberOfDigits();
		if(aDigits != bDigits)
		{
			return false;
		}
		else
		{
			// Return true if the fields match:
			return a == new UnsignedUnboundInt(b);
		}
	}

	public static bool operator !=(UnsignedUnboundInt a, UnsignedUnboundInt b)
	{
		return !(a == b);
	}

	public static bool operator !=(UnsignedUnboundInt a, uint b)
	{
		return !(a == new UnsignedUnboundInt(b));
	}

	public static bool operator !=(UnsignedUnboundInt a, ulong b)
	{
		return !(a == new UnsignedUnboundInt(b));
	}

	public override bool Equals(System.Object obj)
	{
		// If parameter is null return false.
		if (obj == null)
		{
			return false;
		}

		// If parameter cannot be cast to Point return false.
		UnsignedUnboundInt p = obj as UnsignedUnboundInt;
		if ((System.Object)p == null)
		{
			return false;
		}

		// Return true if the fields match:
		if(values.Count != p.values.Count)
		{
			return false;
		}

		return values.Equals(p.values);

	}

	public override int GetHashCode()
	{
		return values.GetHashCode();
	}

	public static UnsignedUnboundInt FromString(string stringToParse)
	{
		UnsignedUnboundInt result = new UnsignedUnboundInt();
		if(!string.IsNullOrEmpty(stringToParse))
		{
			int elementsNeeded = (stringToParse.Length/bucketSizePerElement);
			if(stringToParse.Length%bucketSizePerElement > 0)
			{
				elementsNeeded += 1;
			}
			int startOffset = stringToParse.Length-1;
			for(int i = 0 ; i < elementsNeeded; i++)
			{
				uint parsed = 0;
				//0,1,2,3,4,5,6
				//1000000
			
				int startIndex = startOffset - i*bucketSizePerElement - (bucketSizePerElement-1);//4,1,0
				int size = bucketSizePerElement;
				if(startIndex < 0)
				{
					startIndex = 0;
					size = 1;
				}


				if(uint.TryParse( stringToParse.Substring(startIndex, size),out parsed ) )
				{
					result.values.Add(parsed);
				}
				else//error
				{
					result.SetToZero();
					break;
				}
			}


		}
		return result;
	}

	public override string ToString()
	{
		return ToFormattedString(TO_STRING_FORMAT.SIMPLE);
	}

	public string ToFormattedString(TO_STRING_FORMAT format)
	{
		if(values.Count == 0)
		{
			return "0";
		}
		else
		{
			if(strBuilder == null)
			{
				strBuilder = new StringBuilder(10);
			}
			else//clear it
			{
				strBuilder.Clear();
			}
//			string result = string.Empty;
			switch(format)
			{
			case TO_STRING_FORMAT.SIMPLE://as number without any format
				{
					for(int i = values.Count-1 ;  i >= 0 ; i--)
					{
						if(i == values.Count-1)
						{
							strBuilder.Append(values[i]);
						}
						else
						{
							strBuilder.Append(values[i].ToString("D3"));
						}
					}
					break;
				}
			case TO_STRING_FORMAT.SIMPLE_COMMA://as a number with commas between each bucket
				{
					for(int i = values.Count-1 ;  i >= 0 ; i--)
					{
						if(i == values.Count-1)
						{
							strBuilder.Append( (i < values.Count-1 ? "," :"") ).Append(values[i]);
						}
						else
						{
							strBuilder.Append( (i < values.Count-1 ? "," :"") ).Append(values[i].ToString("D3"));
						}
					}
					break;
				}
			case TO_STRING_FORMAT.SHORT_SCALE_2://shows only the 2 more representative buckets and appends letters following the postfixes table without K
				{
					string postfix = string.Empty;
					int limit = values.Count - 2;
					if(limit < 0)
					{
						limit = 0;
					}
					for(int i = values.Count-1 ;  i >= limit ; i--)
					{
						if(i > 1)
						{
							//calculate postfix
							if(i < postfixes.Length)
							{
								postfix = postfixes[i]+" ";
							}
							if(i == values.Count-1)
							{
								strBuilder.Append(values[i]).Append(postfix);
							}
							else
							{
								strBuilder.Append(values[i].ToString("D3")).Append(postfix);
							}
						}
						else
						{
							if(i == values.Count-1)
							{
								strBuilder.Append(values[i]);
							}
							else
							{
								strBuilder.Append(values[i].ToString("D3"));	
							}
						}
					}
					break;
				}
			case TO_STRING_FORMAT.SHORT_SCALE_K_2://shows only the 2 more representative buckets and appends letters following the postfixes table
				{
					string postfix = string.Empty;
					int limit = values.Count - 2;
					if(limit < 0)
					{
						limit = 0;
					}
					for(int i = values.Count-1 ;  i >= limit ; i--)
					{
						if(i > 0)
						{
							//calculate postfix
							if(i < postfixes.Length)
							{
								postfix = postfixes[i]+" ";
							}
							if(i == values.Count-1)
							{
								strBuilder.Append(values[i]).Append(postfix);
							}
							else
							{
								strBuilder.Append(values[i].ToString("D3")).Append(postfix);
							}
						}
						else
						{
							if(i == values.Count-1)
							{
								strBuilder.Append(values[i]);
							}
							else
							{
								strBuilder.Append(values[i].ToString("D3"));	
							}
						}
					}
					break;
				}
			case TO_STRING_FORMAT.SIMPLE_COMMA_2://shows only the 2 more representative buckets and separetes them with commas, it puts also the prefix of the lowest part if available
				{
					int limit = values.Count - 2;
					if(limit < 0)
					{
						limit = 0;
					}
					string postfix = postfixes[postfixes.Length-1];

					for(int i = values.Count-1 ;  i >= limit ; i--)
					{
						if(i == values.Count-1)
						{
							strBuilder.Append((i < values.Count-1 ? "," :"")).Append(values[i]);
						}
						else
						{
							strBuilder.Append((i < values.Count-1 ? "," :"")).Append(values[i].ToString("D3"));
						}
						if(i > 0 && i < postfixes.Length)
						{
							postfix = postfixes[i];
						}
					}
					strBuilder.Append(postfix);
					break;
				}
			case TO_STRING_FORMAT.SHORT_SCALE_FIXED_POINT_1_4://shows only the most representative bucket as integer part and the next 4 representative values as floating part, postfix at the end without K
				{
					//only units and thousands
					if(values.Count < 3)
					{
						for(int i = values.Count-1 ;  i >= 0 ; i--)
						{
							if(i == values.Count-1)
							{
								strBuilder.Append(values[i]);
							}
							else
							{
								strBuilder.Append(values[i].ToString("D3"));
							}
						}
					}
					else
					{
						string postfix = postfixes[postfixes.Length-1];
						int index = values.Count-1;
						if(index < postfixes.Length)
						{
							postfix = postfixes[index];
						}
						//most important bucket
						strBuilder.Append(values[index]);	
						//this index is 2 or more
						index--;
						//now index is 1 or more
						strBuilder.Append(".").Append(values[index].ToString("D3"));
						index--;
						//now index is 0 or more
						//we must take only the most important value if there is one
						int digits = values[index].GetNumberOfDigits();
						if(digits == 3)
						{
							strBuilder.Append(Math.Floor(values[index]*(bucketConverter*10)).ToString("#"));	
						}
						strBuilder.Append(postfix);
					}
					break;
				}
			case TO_STRING_FORMAT.SHORT_SCALE_FIXED_POINT_COMMA_1_4://shows only the most representative bucket as integer part and the next 4 representative values as floating part, postfix at the end without K
				{
					//only units and thousands
					if(values.Count < 3)
					{
						for(int i = values.Count-1 ;  i >= 0 ; i--)
						{
							if(i == values.Count-1)
							{
								strBuilder.Append(values[i]);
							}
							else
							{
								strBuilder.Append(",").Append(values[i].ToString("D3"));
							}
						}
					}
					else
					{
						string postfix = postfixes[postfixes.Length-1];
						int index = values.Count-1;
						if(index < postfixes.Length)
						{
							postfix = postfixes[index];
						}
						//most important bucket
						strBuilder.Append(values[index]);	
						//this index is 2 or more
						index--;
						//now index is 1 or more
						strBuilder.Append(".").Append(values[index].ToString("D3"));
						index--;
						//now index is 0 or more
						//we must take only the most important value if there is one
						int digits = values[index].GetNumberOfDigits();
						if(digits == 3)
						{
							strBuilder.Append(( Math.Floor(values[index]*(bucketConverter*10))).ToString("#"));	
						}
						strBuilder.Append(postfix);
					}
					break;
				}
			case TO_STRING_FORMAT.SHORT_SCALE_FIXED_POINT_K_1_4:
				{
					//only units
					if(values.Count < 2)
					{
						for(int i = values.Count-1 ;  i >= 0 ; i--)
						{
							if(i == values.Count-1)
							{
								strBuilder.Append(values[i]);
							}
							else
							{
								strBuilder.Append(values[i].ToString("D3"));
							}
						}
					}
					else
					{
						string postfix = postfixes[postfixes.Length-1];
						int index = values.Count-1;
						if(index < postfixes.Length)
						{
							postfix = postfixes[index];
						}
						//most important bucket
						strBuilder.Append(values[index]);	
						//this index is 1 or more
						index--;
						//now index is 0 or more
						if(index == 0)
						{
							if(values[index] == 0)
							{
								strBuilder.Append(".0");
							}
							else
							{
								strBuilder.Append(".").Append(values[index].ToString("D3"));
							}
						}
						else//index is 1 or more
						{
							strBuilder.Append(".").Append(values[index].ToString("D3"));
							index--;
							//now index is 0 or more
							//we must take only the most important value if there is one
							int digits = values[index].GetNumberOfDigits();
							if(digits == 3)
							{
								strBuilder.Append(( Math.Floor(values[index]*(bucketConverter*10))).ToString("#"));	
							}
						}
						strBuilder.Append(postfix);
					}
					break;
				}
			}

			return strBuilder.ToString();
		}
	}

}


