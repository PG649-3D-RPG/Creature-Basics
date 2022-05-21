using System;
using System.Collections.Generic;
using UnityEngine;


// TODO: Houdini Syntax; in grammatik variable länge erlauben
// https://www.sidefx.com/docs/houdini/nodes/sop/lsystem.html
public class LSystem
{
    private readonly int default_dist;
    private readonly short default_angle;

    private readonly int default_cross_sections;
    private readonly int default_cross_section_divisions;

    public LSystem(int d = 10, short a = 90, int cs = 4, int csd = 2)
    {
        default_dist = d;
        default_angle = a;
        default_cross_sections = cs;
        default_cross_section_divisions = csd;
    }

    private Tuple<char, int[]> ParseArguments(ref string str, ref int index, char symbol, int default_value)
    {
        Tuple<char, int[]> result;
        if (index < str.Length - 1 && str[index + 1] == '(')
        {
            // search index of ')'
            var end = index + str[index..].IndexOf(')');
            var tmp = str[(index + 2)..end];
            var d = int.Parse(tmp);
            result = new(symbol, new int[] { d });
            index = end + 1;
        }
        else
        {
            result = new(symbol, new int[] { default_value });
            index++;
        }
        return result;
    }

    public List<Tuple<char, int[]>> Tokenize(string str)
    {
        List<Tuple<char, int[]>> tokens = new();
        for (int i = 0; i < str.Length;)
        {
            switch (str[i])
            {
                case 'F':
                    tokens.Add(ParseArguments(ref str, ref i, str[i], default_dist));
                    break;
                case '+':
                    tokens.Add(ParseArguments(ref str, ref i, str[i], default_angle));
                    break;
                case '-':
                    tokens.Add(ParseArguments(ref str, ref i, str[i], default_angle));
                    break;
                case '&':
                    tokens.Add(ParseArguments(ref str, ref i, str[i], default_angle));
                    break;
                case '^':
                    tokens.Add(ParseArguments(ref str, ref i, str[i], default_angle));
                    break;
                case '\\':
                    tokens.Add(ParseArguments(ref str, ref i, str[i], default_angle));
                    break;
                case '/':
                    tokens.Add(ParseArguments(ref str, ref i, str[i], default_angle));
                    break;
                default:
                    tokens.Add(new(str[i], new int[0]));
                    i++;
                    break;
            }
        }
        return tokens;
    }

    private double ConvertToRadians(double angle)
    {
        return (Math.PI / 180) * angle;
    }

    private Vector3 CalculateEnd2D(int rot, Vector3 start, int dist)
    {
        var x = start.x + dist * (float)Math.Cos(ConvertToRadians(rot));
        var y = start.y + dist * (float)Math.Sin(ConvertToRadians(rot));
        return new Vector3(x, y, 0);
    }

    private Vector3 CalculateEnd3D(Vector3 v, Vector3 start, int dist)
    {
        return start + dist * v;
    }

    public List<Tuple<Vector3, Vector3>> Turtle3D(List<Tuple<char, int[]>> tokens)
    {
        Vector3 scarlet_rot = Vector3.down;
        Vector3 current = Vector3.zero;
        List<Tuple<Vector3, Vector3>> tuples = new();
        //LSystemTree<Tuple<Vector3, Vector3>> tuples = new(new(current,current));
        //var currentNode = tuples;
        // List<Tuple<Vector3, Vector3>> marika = new() { new(Vector3.zero, new(0f, 10f, 0f)), new(new(0f, 10f, 0f), new(0f, 20f, 0f)), new(new(0f, 20f, 0f), new(-10f, 20f, 0f)), new(new(0f, 20f, 0f), new(10f, 20f, 0f)), new(new(0f, 20f, 0f), new(-10f, 20f, 0f)), new(new(0f, 20f, 0f), new(0f, 30f, 0f)), new(new(0f, 30f, 0f), new(-5f, 38.66f, 0f)), new(new(0f, 30f, 0f), new(5f, 38.66f, 0f)) };
        Stack<Tuple<Vector3, Vector3>> st = new();
        //Stack <Tuple<Vector3, Vector3,LSystemTree<Tuple<Vector3, Vector3>>>> st = new();
        foreach (var e in tokens)
        {
            switch (e.Item1)
            {
                case 'F':
                    var end = CalculateEnd3D(scarlet_rot, current, e.Item2[0]);// new endpoint
                    tuples.Add(new(current, end));// last endpoint = new startpoint
                    //currentNode = currentNode.addChild(new(current, end));
                    current = end;
                    break;
                case '+': // turn right
                    scarlet_rot = Quaternion.Euler(0, 0, e.Item2[0]) * scarlet_rot;
                    break;
                case '-': // turn left
                    scarlet_rot = Quaternion.Euler(0, 0, -e.Item2[0]) * scarlet_rot;
                    break;
                case '&': // pitch up
                    scarlet_rot = Quaternion.Euler(e.Item2[0], 0, 0) * scarlet_rot;
                    break;
                case '^': // pitch down
                    scarlet_rot = Quaternion.Euler(-e.Item2[0], 0, 0) * scarlet_rot;
                    break;
                case '\\': // roll clockwise
                    scarlet_rot = Quaternion.Euler(0, e.Item2[0], 0) * scarlet_rot;
                    break;
                case '/': // roll counter-clockwise
                    scarlet_rot = Quaternion.Euler(0, -e.Item2[0], 0) * scarlet_rot;
                    break;
                case '|': // turn 180 deg
                    scarlet_rot = Quaternion.Euler(0, 0, 180) * scarlet_rot;
                    break;
                case '*': // roll 180 deg
                    scarlet_rot = Quaternion.Euler(0, 180, 0) * scarlet_rot;
                    break;
                case '[':
                    st.Push(new(scarlet_rot, current));
                    //st.Push(new(scarlet_rot, current,currentNode));
                    break;
                case ']':
                    var tmp = st.Pop();
                    scarlet_rot = tmp.Item1;
                    current = tmp.Item2;
                    //currentNode = tmp.Item3;
                    break;
                default:
                    break;
            }
        }
        return tuples;
    }
    public string Parse(string s, uint it, Dictionary<char, string> rules)
    {
        string w = s;
        for (int i = 0; i < it; i++)
        {
            string temp = "";
            foreach (var c in w)
            {
                if (rules.ContainsKey(c))
                {
                    // NT
                    temp += rules[c];
                }
                else
                {
                    // Terminal
                    temp += c;
                }
            }
            w = temp;
        }
        return w;
    }
    public List<Tuple<Vector3, Vector3>> Evaluate(string start, uint iterations, Dictionary<char, string> rules, bool printResult = false)
    {
        var expanded = Parse(start, iterations, rules);
        if (printResult) Debug.Log(expanded);
        var res = Turtle3D(Tokenize(expanded));
        if (printResult) PrintList(res);
        return res;
    }

    public static void PrintList(List<Tuple<Vector3, Vector3>> l)
    {
        string o = "[  ";
        foreach (var e in l)
        {
            o += e.ToString() + "  ";
        }
        Debug.Log(o + "]");
    }
}
