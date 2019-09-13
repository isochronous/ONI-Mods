﻿// Decompiled with JetBrains decompiler
// Type: Accumulators
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BA533216-CB4F-43C8-8FF5-02CE00126C4B
// Assembly location: C:\Games\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

public class Accumulators
{
  private const float TIME_WINDOW = 3f;
  private float elapsedTime;
  private KCompactedVector<float> accumulated;
  private KCompactedVector<float> average;

  public Accumulators()
  {
    this.elapsedTime = 0.0f;
    this.accumulated = new KCompactedVector<float>(0);
    this.average = new KCompactedVector<float>(0);
  }

  public HandleVector<int>.Handle Add(string name, KMonoBehaviour owner)
  {
    HandleVector<int>.Handle handle = this.accumulated.Allocate(0.0f);
    this.average.Allocate(0.0f);
    return handle;
  }

  public HandleVector<int>.Handle Remove(HandleVector<int>.Handle handle)
  {
    if (!handle.IsValid())
      return HandleVector<int>.InvalidHandle;
    this.accumulated.Free(handle);
    this.average.Free(handle);
    return HandleVector<int>.InvalidHandle;
  }

  public void Sim200ms(float dt)
  {
    this.elapsedTime += dt;
    if ((double) this.elapsedTime < 3.0)
      return;
    this.elapsedTime -= 3f;
    List<float> dataList1 = this.accumulated.GetDataList();
    List<float> dataList2 = this.average.GetDataList();
    int count = dataList1.Count;
    float num = 0.3333333f;
    for (int index = 0; index < count; ++index)
    {
      dataList2[index] = dataList1[index] * num;
      dataList1[index] = 0.0f;
    }
  }

  public float GetAverageRate(HandleVector<int>.Handle handle)
  {
    if (handle.IsValid())
      return this.average.GetData(handle);
    return 0.0f;
  }

  public void Accumulate(HandleVector<int>.Handle handle, float amount)
  {
    float data = this.accumulated.GetData(handle);
    this.accumulated.SetData(handle, data + amount);
  }
}