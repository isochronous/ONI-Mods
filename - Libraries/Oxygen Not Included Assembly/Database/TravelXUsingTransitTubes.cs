﻿// Decompiled with JetBrains decompiler
// Type: Database.TravelXUsingTransitTubes
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BA533216-CB4F-43C8-8FF5-02CE00126C4B
// Assembly location: C:\Games\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using STRINGS;
using System.IO;
using UnityEngine;

namespace Database
{
  public class TravelXUsingTransitTubes : ColonyAchievementRequirement
  {
    private int distanceToTravel;
    private NavType navType;

    public TravelXUsingTransitTubes(NavType navType, int distanceToTravel)
    {
      this.navType = navType;
      this.distanceToTravel = distanceToTravel;
    }

    public override bool Success()
    {
      int num = 0;
      foreach (Component component1 in Components.MinionIdentities.Items)
      {
        Navigator component2 = component1.GetComponent<Navigator>();
        if ((Object) component2 != (Object) null && component2.distanceTravelledByNavType.ContainsKey(this.navType))
          num += component2.distanceTravelledByNavType[this.navType];
      }
      return num >= this.distanceToTravel;
    }

    public override void Deserialize(IReader reader)
    {
      this.navType = (NavType) reader.ReadByte();
      this.distanceToTravel = reader.ReadInt32();
    }

    public override void Serialize(BinaryWriter writer)
    {
      byte navType = (byte) this.navType;
      writer.Write(navType);
      writer.Write(this.distanceToTravel);
    }

    public override string GetProgress(bool complete)
    {
      int num = 0;
      foreach (Component component1 in Components.MinionIdentities.Items)
      {
        Navigator component2 = component1.GetComponent<Navigator>();
        if ((Object) component2 != (Object) null && component2.distanceTravelledByNavType.ContainsKey(this.navType))
          num += component2.distanceTravelledByNavType[this.navType];
      }
      return string.Format((string) COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.STATUS.TRAVELED_IN_TUBES, (object) (!complete ? num : this.distanceToTravel), (object) this.distanceToTravel);
    }
  }
}