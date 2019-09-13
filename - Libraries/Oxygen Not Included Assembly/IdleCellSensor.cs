﻿// Decompiled with JetBrains decompiler
// Type: IdleCellSensor
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BA533216-CB4F-43C8-8FF5-02CE00126C4B
// Assembly location: C:\Games\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class IdleCellSensor : Sensor
{
  private MinionBrain brain;
  private Navigator navigator;
  private KPrefabID prefabid;
  private int cell;

  public IdleCellSensor(Sensors sensors)
    : base(sensors)
  {
    this.navigator = this.GetComponent<Navigator>();
    this.brain = this.GetComponent<MinionBrain>();
    this.prefabid = this.GetComponent<KPrefabID>();
  }

  public override void Update()
  {
    if (!this.prefabid.HasTag(GameTags.Idle))
    {
      this.cell = Grid.InvalidCell;
    }
    else
    {
      MinionPathFinderAbilities currentAbilities = (MinionPathFinderAbilities) this.navigator.GetCurrentAbilities();
      currentAbilities.SetIdleNavMaskEnabled(true);
      IdleCellQuery idleCellQuery = PathFinderQueries.idleCellQuery.Reset(this.brain, Random.Range(30, 60));
      this.navigator.RunQuery((PathFinderQuery) idleCellQuery);
      currentAbilities.SetIdleNavMaskEnabled(false);
      this.cell = idleCellQuery.GetResultCell();
    }
  }

  public int GetCell()
  {
    return this.cell;
  }
}