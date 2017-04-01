using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerticalBarChart : MonoBehaviour 
{
	public string infoElementPoolId = "ChartInfoElement";
	public string barChartGraphicElementPoolId = "BarChartElement";
	public string YAxisDataLabelPoolId = "Y_Data";

	public Transform graphicElementsHolder;
	public Transform YAxisDataHolder;
	public Transform labelElementsHolder;

	public List<VBarChartElement> allDatas = new List<VBarChartElement>();
	public List<YAxisData> allYAxisDatas = new List<YAxisData>();
	public List<ChartInfoLabel> allDataLabels = new List<ChartInfoLabel>();

	public void UpdateGraph(float[] dataSamples_Group1, float[] dataSamples_Group2,float[] barRanges, uint YAxisGrowRate, uint maxYMarkers, string[] dataNames,Color[] dataColors)
	{
		if(dataNames.Length < 2 || dataColors.Length < 2 || barRanges.Length < 2|| YAxisGrowRate == 0 || maxYMarkers == 0)
		{
			return;
		}

		List<float> g1Samples = new List<float>(dataSamples_Group1);
		List<float> g2Samples = new List<float>(dataSamples_Group2);

		uint YAxisMarkersNeeded = maxYMarkers+1;
		float totalSamples = YAxisMarkersNeeded*YAxisGrowRate;

		//spawn Y Axis markers
		for(uint i = YAxisMarkersNeeded; i > 0; i--)
		{
			YAxisData yData = ObjectManager.SpawnLike<YAxisData>(YAxisDataLabelPoolId,Vector3.zero,Quaternion.identity,YAxisDataHolder);
			if(yData != null)
			{
				yData.InitWithData(  (i-1)*YAxisGrowRate + YAxisGrowRate ,i == YAxisMarkersNeeded);
				allYAxisDatas.Add(yData);
				yData.CachedRectTransform.ResetToFillParent();
			}
		}

		//print info labels
		for(int i = 0; i < dataNames.Length; i++)
		{
			ChartInfoLabel label = ObjectManager.SpawnLike<ChartInfoLabel>(infoElementPoolId,Vector3.zero,Quaternion.identity,labelElementsHolder);
			label.CachedRectTransform.ResetToFillParent();

			if(label != null)
			{
				label.CachedRectTransform.ResetToFillParent();
				label.InitWithData(dataNames[i],dataColors[i],dataNames[i]);
				allDataLabels.Add(label);
			}
		}


		float g1Concurrencies = 0;
		float g2Concurrencies = 0;
		float maxRange = 0;
		float minRange = -1;//this is to include 0 in the minor range
		//spawn chart bars
		//bar ranges represent the max limit of a bar, example: a value of [20,25], represents all the values that are between 0-20,21-25 in 2 general bars
		//ordered from min to max
		for(uint i = 0; i < barRanges.Length+1; i++ )
		{
			g1Concurrencies = 0;
			g2Concurrencies = 0;
			if(i == barRanges.Length)
			{
				maxRange = float.MaxValue;
			}
			else
			{
				maxRange = barRanges[i];
			}
			if(i > 0)
			{
				minRange = barRanges[i-1];
			}

			//count concurrencies minor to the current barRange for both data groups
			for(int j = 0; j < g1Samples.Count; j++)
			{
				if(g1Samples[j] > minRange  && g1Samples[j] <= maxRange)
				{
					g1Concurrencies += 1.0f;
				}
			}
			for(int j = 0; j < g2Samples.Count; j++)
			{
				if(g2Samples[j] > minRange  && g2Samples[j] <= maxRange)
				{
					g2Concurrencies += 1.0f;
				}
			}

			VBarChartElement barController = ObjectManager.SpawnLike<VBarChartElement>(barChartGraphicElementPoolId,
				Vector3.zero,Quaternion.identity,graphicElementsHolder);
			if(barController != null)
			{
				barController.CachedRectTransform.ResetToFillParent();
				barController.InitWithData(	CHART_DATA_TYPE.BAR_CHART,
					dataColors[0],dataColors[1],
					g1Concurrencies,g2Concurrencies,totalSamples,
					(uint)Mathf.FloorToInt(minRange),(uint)Mathf.FloorToInt(maxRange),
					(i == 0),(i == barRanges.Length)
				);
				allDatas.Add(barController);						
			}
		}
	}

	public void Clear()
	{
		if(allDatas != null)
		{
			for(int i = 0; i < allDatas.Count; i++)
			{
				if(allDatas[i] != null)
				{
					allDatas[i].Despawn();
				}
			}
			allDatas.Clear();
		}
		if(allDataLabels != null)
		{
			for(int i = 0; i < allDataLabels.Count; i++)
			{
				if(allDataLabels[i] != null)
				{
					allDataLabels[i].Despawn();
				}
			}
			allDataLabels.Clear();
		}
		if(allYAxisDatas != null)
		{
			for(int i = 0; i < allYAxisDatas.Count; i++)
			{
				if(allYAxisDatas[i] != null)
				{
					allYAxisDatas[i].Despawn();
				}
			}
			allYAxisDatas.Clear();
		}
	}
		
}
