using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParametricCreature
{
    private CreatureParameters p;
    public List<List<Segment>> legs { get; private set; }
    public List<Segment> torso { get; private set; }
    public List<Segment> neck { get; private set; }
    public List<Ball> negative { get; private set; }
    public List<Segment> feet { get; private set; }

    public List<Vector3> legAttachJoints { get; private set; }
    private int legPairs;
    private List<float> legHeights;
    private float torsoSize;
    private float headSize;
    private Vector3 headPosition;
    private Segment snout;
    private float snoutTip;
    public ParametricCreature()
    {

    }

    public ParametricCreature(CreatureParameters parameters)
    {
        setParameters(parameters);
    }

    public void setParameters(CreatureParameters parameters)
    {
        if (parameters.isValid())
        {
            p = parameters;
        }
    }

    public void buildCreature(int seed = 0)
    {
        if (seed != 0)
        {
            Random.InitState(seed);
        }
        buildLegs();
        buildTorso();
        moveLegsToTorso();
        buildNeck();
        buildHead();
        debugDraw();
    }

    public void buildLegs()
    {
        legPairs = Random.Range(p.minLegPairs, p.maxLegPairs+1);
        legs = new List<List<Segment>>();
        List<float> thickness = new List<float>();

        if (legPairs == 1)
        {
            // biped
            float legHeight = Random.Range(p.minLegSize, p.maxLegSize);
            legHeights = new List<float>() { legHeight };

            thickness.Add(Random.Range(p.minLegThickness, p.maxLegThickness));
            thickness.Add(Random.Range(p.minLegThickness, p.maxLegThickness));
            thickness.Sort();

            List<Segment> l1 = new List<Segment>();
            float legSplit = Random.Range(0.25f * legHeight, 0.75f * legHeight);
            l1.Add(new Segment(Vector3.zero, new Vector3(0, legSplit, 0), thickness[0]));
            l1.Add(new Segment(new Vector3(0, legSplit, 0), new Vector3(0, legHeight, 0), thickness[1]));

            List<Segment> l2 = new List<Segment>();
            l2.Add(new Segment(Vector3.zero, new Vector3(0, legSplit, 0), thickness[0]));
            l2.Add(new Segment(new Vector3(0, legSplit, 0), new Vector3(0, legHeight, 0), thickness[1]));

            legs.Add(l1);
            legs.Add(l2);
        }
        else if (legPairs == 2)
        {
            // quadruped
            float frontLegHeight = Random.Range(p.minLegSize, p.maxLegSize);
            float hindLegHeight = Random.Range(p.minLegSize, p.maxLegSize);
            legHeights = new List<float>() { hindLegHeight, frontLegHeight };

            for (int i=0; i<2; i++)
            {
                float height = hindLegHeight;
                if (i == 1)
                    height = frontLegHeight;
                float legSplit = Random.Range(0.1f * height, 0.5f * height);
                float upperLegSplit = Random.Range(legSplit + 0.25f * (hindLegHeight - legSplit), legSplit + 0.75f * (height - legSplit));
                float lowerLegSplit = Random.Range(0.25f * legSplit, 0.75f * legSplit);


                thickness.Add(Random.Range(p.minLegThickness, p.maxLegThickness));
                thickness.Add(Random.Range(p.minLegThickness, p.maxLegThickness));
                thickness.Add(Random.Range(p.minLegThickness, p.maxLegThickness));
                thickness.Add(Random.Range(p.minLegThickness, p.maxLegThickness));
                thickness.Sort();

                float footAngle = 1.2f;
                
                for (int j=0; j<2; j++)
                {
                    List<Segment> leg = new List<Segment>();
                    Vector3 startPoint = new Vector3(0, 0, Mathf.Sin(footAngle) * lowerLegSplit);
                    //Vector3 endPoint = new Vector3(0, Mathf.Cos(footAngle) * lowerLegSplit, -Mathf.Sin(footAngle) * lowerLegSplit);
                    Vector3 endPoint = new Vector3(0, lowerLegSplit, 0);
                    leg.Add(new Segment(startPoint, endPoint, thickness[0]));
                    startPoint = endPoint;
                    //endPoint = new Vector3(0, legSplit, -Mathf.Tan(legAngle) * legSplit);
                    endPoint = new Vector3(0, legSplit, 0);
                    leg.Add(new Segment(startPoint, endPoint, thickness[1]));
                    startPoint = endPoint;
                    //endPoint = new Vector3(0, upperLegSplit, startPoint.z + Mathf.Tan(legAngle) * (upperLegSplit - legSplit));
                    endPoint = new Vector3(0, upperLegSplit, 0);
                    leg.Add(new Segment(startPoint, endPoint, thickness[2]));
                    startPoint = endPoint;
                    endPoint = new Vector3(0, height, 0);
                    leg.Add(new Segment(startPoint, endPoint, thickness[2]));
                    legs.Add(leg);
                }
            }
        }
        invertLegs();
    }

    public void buildTorso()
    {
        torso = new List<Segment>();
        torsoSize = Random.Range(p.minTorsoSize, p.maxTorsoSize);

        List<float> torsoSplits = new List<float>()
        {
            Random.Range(0.1f * torsoSize, 0.9f * torsoSize)
        };
        do
        {
            float split = Random.Range(0.1f * torsoSize, 0.9f * torsoSize);
            if (Mathf.Abs(split - torsoSplits[0]) >= 0.1f * torsoSize)
                torsoSplits.Add(split);
        } while (torsoSplits.Count < 2);
        torsoSplits.Sort();

        if (legPairs == 1)
        {
            // biped
            Vector3 startPoint = new Vector3(0, legHeights[0], 0);
            Vector3 endPoint = new Vector3(0, legHeights[0] + torsoSplits[0], 0);
            torso.Add(new Segment(startPoint, endPoint, Random.Range(p.minTorsoThickness, p.maxTorsoThickness)));
            startPoint = endPoint;
            endPoint = new Vector3(0, legHeights[0] + torsoSplits[1], 0);
            torso.Add(new Segment(startPoint, endPoint, Random.Range(p.minTorsoThickness, p.maxTorsoThickness)));
            startPoint = endPoint;
            endPoint = new Vector3(0, legHeights[0] + torsoSize, 0);
            torso.Add(new Segment(startPoint, endPoint, Random.Range(p.minTorsoThickness, p.maxTorsoThickness)));
        }
        else if (legPairs == 2)
        {
            // quadruped
            List<float> splitY = new List<float>()
            {
                Random.Range(Mathf.Min(legHeights[0], legHeights[1]), Mathf.Max(legHeights[0], legHeights[1])),
                Random.Range(Mathf.Min(legHeights[0], legHeights[1]), Mathf.Max(legHeights[0], legHeights[1]))
            };
            splitY.Sort();
            if (legHeights[0] > legHeights[1])
                splitY.Reverse();

            Vector3 startPoint = new Vector3(0, legHeights[0], -torsoSize*0.5f);
            Vector3 endPoint = new Vector3(0, splitY[0], torsoSplits[0] - torsoSize*0.5f);
            torso.Add(new Segment(startPoint, endPoint, Random.Range(p.minTorsoThickness, p.maxTorsoThickness)));
            startPoint = endPoint;
            endPoint = new Vector3(0, splitY[1], torsoSplits[1] - torsoSize * 0.5f);
            torso.Add(new Segment(startPoint, endPoint, Random.Range(p.minTorsoThickness, p.maxTorsoThickness)));
            startPoint = endPoint;
            endPoint = new Vector3(0, legHeights[1], torsoSize - torsoSize * 0.5f);
            torso.Add(new Segment(startPoint, endPoint, Random.Range(p.minTorsoThickness, p.maxTorsoThickness)));
        }
    }

    public void moveLegsToTorso()
    {
        feet = new();
        legAttachJoints = new();
        if (legPairs == 1)
        {
            // biped
            for (int i=0; i<legs.Count; i++)
            {
                legAttachJoints.Add(torso[0].startPoint);
                Vector3 xDir = Vector3.left;
                if (i % 2 == 1)
                    xDir = Vector3.right;

                foreach (Segment segment in legs[i])
                {
                    segment.startPoint = segment.startPoint + xDir * torso[0].thickness;
                    segment.endPoint = segment.endPoint + xDir * torso[0].thickness;
                }
            }
        }
        else if (legPairs == 2)
        {
            // quadruped
            for (int i = 0; i < legs.Count; i++)
            {
                Vector3 xDir = Vector3.left;
                if (i % 2 == 1)
                    xDir = Vector3.right;

                Vector3 zDir = Vector3.back;
                float thickness = torso[0].thickness;
                if (i > 1)
                {
                    zDir = Vector3.forward;
                    thickness = torso[2].thickness;
                    legAttachJoints.Add(torso[torso.Count - 1].endPoint);
                }
                else
                    legAttachJoints.Add(torso[0].startPoint);

                foreach (Segment segment in legs[i])
                {
                    segment.startPoint = segment.startPoint + xDir * thickness + zDir * torsoSize * 0.5f;
                    segment.endPoint = segment.endPoint + xDir * thickness + zDir * torsoSize * 0.5f;
                }
                feet.Add(legs[i][legs[i].Count - 1]);
                legs[i].RemoveAt(legs[i].Count - 1);
            }
        }
    }

    public void buildNeck()
    {
        neck = new List<Segment>();

        int neckSegments = Random.Range(p.minNeckSegments, p.maxNeckSegments + 1);
        float neckSize = Random.Range(p.minNeckSize, p.maxNeckSize);
        float segmentLength = neckSize / neckSegments;
        float neckThickness = 0.2f;

        Vector3 fwd = 0.5f * ((torso[2].endPoint - torso[2].startPoint).normalized + Vector3.up);

        Vector3 startPoint = torso[2].endPoint + fwd * torso[2].thickness;
        for (int i=0; i<neckSegments; i++)
        {
            Vector3 endPoint = startPoint + segmentLength * fwd;
            neck.Add(new Segment(startPoint, endPoint, neckThickness));
            startPoint = endPoint;
        }
    }

    public void buildHead()
    {
        // ball as base shape for head
        headSize = Random.Range(p.minHeadSize, p.maxHeadSize);
        Segment lastNeck = neck[neck.Count - 1];
        Vector3 fwd = (lastNeck.endPoint - lastNeck.startPoint).normalized;
        headPosition = lastNeck.endPoint + fwd * Mathf.Max(lastNeck.thickness, headSize);

        // snout
        float snoutThickness = Random.Range(p.minSnoutThickness, p.maxSnoutThickness);
        float snoutLength = Random.Range(p.minSnoutSize, p.maxSnoutSize);
        Vector3 snoutPos = headPosition + Vector3.down * (headSize - snoutThickness);
        snout = new Segment(snoutPos, snoutPos + Vector3.forward * snoutLength, snoutThickness);
        snoutTip = Random.Range(0f, snoutThickness);

        // eyes
        negative = new List<Ball>();
        Vector3 eyesPos = headPosition + Vector3.forward * headSize + Vector3.up * 0.5f * headSize;
        float eyeThickness = Random.Range(0f, headSize*0.5f);
        negative.Add(new Ball(eyeThickness, eyesPos + Vector3.right * headSize, MetaballFunction.Polynomial2, true));
        negative.Add(new Ball(eyeThickness, eyesPos + Vector3.left * headSize, MetaballFunction.Polynomial2, true));
    }

    private void invertLegs()
    {
        foreach(var leg in legs)
        {
            leg.Reverse();
            foreach (var segment in leg)
            {
                Vector3 start = segment.startPoint;
                segment.startPoint = segment.endPoint;
                segment.endPoint = start;
            }
        }
    }

    public void debugDraw()
    {
        foreach (List<Segment> leg in legs)
        {
            foreach (Segment segment in leg)
            {
                Debug.DrawLine(segment.startPoint, segment.endPoint, Color.red, 999999f, false);
            }
        }
        foreach (Segment segment in torso)
        {
            Debug.DrawLine(segment.startPoint, segment.endPoint, Color.red, 999999f, false);
        }
        foreach (Segment segment in neck)
        {
            Debug.DrawLine(segment.startPoint, segment.endPoint, Color.red, 999999f, false);
        }
        foreach (Segment segment in feet)
        {
            Debug.DrawLine(segment.startPoint, segment.endPoint, Color.red, 999999f, false);
        }
        Debug.DrawLine(snout.startPoint, snout.endPoint, Color.red, 999999f, false);
    }

    public Metaball makeMetaball()
    {
        List<Segment> segments = new List<Segment>();
        segments.AddRange(torso);
        foreach (List<Segment> leg in legs)
        {
            segments.AddRange(leg);
        }
        segments.AddRange(neck);
        segments.AddRange(feet);

        Metaball m = Metaball.BuildFromSegments(segments.ToArray());

        // head
        m.AddBall(headSize, headPosition);
        m.AddCone(snout, snoutTip);

        foreach(Ball b in negative)
        {
            m.AddBall(b);
        }

        return m;
    }
}
