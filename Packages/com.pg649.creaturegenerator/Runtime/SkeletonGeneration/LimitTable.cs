using System;
using System.Collections.Generic;
public class LimitTable {
    private Dictionary<(BoneCategory, BoneCategory), JointLimits> table
    { get; }

    public LimitTable(Dictionary<(BoneCategory, BoneCategory), JointLimits> table) {
        this.table = table;
    }

    public LimitTable()
    {
        table = new();
    }

    /// <summary>
    /// Adds joint limits to given categories
    /// Replaces already existing limits for these categories
    /// </summary>
    public void Add((BoneCategory, BoneCategory) bones, JointLimits limits)
    {
        table[bones] = limits;
    }

    /// <summary>
    /// Combines two LimitTables into one
    /// The new new limits replace existing ones in case of duplicates
    /// </summary>
    public void Add(LimitTable newLimits)
    {
        foreach (KeyValuePair<(BoneCategory, BoneCategory), JointLimits> limits in newLimits.table)
        {
            this.table[limits.Key] = limits.Value;
        }
    }

    public bool HasLimits((BoneCategory, BoneCategory) t) {
        return table.ContainsKey(t);
    }
    public JointLimits this[(BoneCategory, BoneCategory) t]
    {
        get { return table[t]; }
        set { table[t] = value; }
    }


}